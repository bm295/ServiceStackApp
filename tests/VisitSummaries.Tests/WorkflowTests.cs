using System.Text;
using System.Text.Json;
using DbEncryption;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using WolverinePlayground.Features.VisitSummaries;
using WolverinePlayground.Storage;

namespace VisitSummaries.Tests;

public class WorkflowTests
{
    private string path = null!;
    private VisitSummaryStore store = null!;
    private readonly DemoVisitCatalog catalog = new();
    private readonly VisitSummaryValidator validator = new();
    private IDbEncryptionService encryption = null!;

    [SetUp]
    public void Setup()
    {
        path = Path.Combine(Path.GetTempPath(), $"visit-test-{Guid.NewGuid():N}.db");
        encryption = new DataProtectionDbEncryptionService(new EphemeralDataProtectionProvider());
        store = new VisitSummaryStore($"Data Source={path};Pooling=False", encryption);
    }
    [TearDown] public void Cleanup() => File.Delete(path);
    private Task<VisitSummary> Generate(string scenario, string request = "request-1") =>
        new VisitSummaryWorkflow(store, new DemoVisitSummaryGenerator(), validator).RunAsync(catalog.Visits[0], request, scenario, default);

    [Test]
    public async Task ValidDraftCanBeEditedAndApprovedAndSurvivesReload()
    {
        var draft = await Generate("valid");
        Assert.That(draft.Status, Is.EqualTo(SummaryStatus.PendingReview));
        var edited = draft.Content! with { ReasonForVisit = [new("Headache, confirm at visit", [new("note-1", 1)])] };
        var approved = new ApproveVisitSummaryHandler(store, validator, catalog).Handle(draft.Id, new(draft.Revision, edited), "doctor-1");
        var reloaded = new VisitSummaryStore($"Data Source={path};Pooling=False", encryption).Get(draft.Id)!;
        Assert.Multiple(() => {
            Assert.That(reloaded.Status, Is.EqualTo(SummaryStatus.Approved));
            Assert.That(reloaded.ApprovedBy, Is.EqualTo("doctor-1"));
            Assert.That(reloaded.Content!.ReasonForVisit[0].Text, Is.EqualTo("Headache, confirm at visit"));
            Assert.That(approved.History.Last().Outcome, Is.EqualTo("ApprovedByDoctor"));
            Assert.That(Encoding.UTF8.GetString(File.ReadAllBytes(path)), Does.Not.Contain("patient-1"));
        });
        Assert.Throws<InvalidOperationException>(() => new ApproveVisitSummaryHandler(store, validator, catalog).Handle(draft.Id, new(draft.Revision, edited), "doctor-2"));
    }
    [Test]
    public async Task ForeignCitationRequiresManualReviewWithoutRetry()
    {
        var result = await Generate("foreign-source");
        Assert.Multiple(() => {
            Assert.That(result.Status, Is.EqualTo(SummaryStatus.NeedsManualReview));
            Assert.That(result.Content, Is.Null);
            Assert.That(result.History, Has.Length.EqualTo(1));
            Assert.That(store.Get(result.Id)!.History[0].Reasons[0], Does.Contain("note-2"));
        });
    }
    [TestCase("timeout")]
    [TestCase("invalid-json")]
    public async Task FailureRetriesOnlyOnceAndRepeatedRequestDoesNotDuplicate(string scenario)
    {
        var result = await Generate(scenario);
        var repeated = await Generate(scenario);
        Assert.Multiple(() => {
            Assert.That(result.Status, Is.EqualTo(SummaryStatus.Failed));
            Assert.That(result.Content, Is.Null);
            Assert.That(result.History, Has.Length.EqualTo(2));
            Assert.That(repeated.Id, Is.EqualTo(result.Id));
            Assert.That(repeated.History, Has.Length.EqualTo(2));
        });
    }
    [Test] public async Task MalformedFirstResponseCanRecover() =>
        Assert.That((await Generate("recover")).Status, Is.EqualTo(SummaryStatus.PendingReview));

    [TestCase("null")]
    [TestCase("{}")]
    [TestCase("{\"unexpected\":1}")]
    public async Task InvalidShapesFailAfterTwoAttempts(string raw)
    {
        var result = await new VisitSummaryWorkflow(store, new FixedGenerator(raw), validator).RunAsync(catalog.Visits[0], "shape", "valid", default);
        Assert.That(result.Status, Is.EqualTo(SummaryStatus.Failed));
        Assert.That(result.History, Has.Length.EqualTo(2));
    }
    [Test]
    public async Task ApprovalRejectsChangedCitationAndRecordsReason()
    {
        var draft = await Generate("valid");
        var invalid = draft.Content! with { MissingData = [new("Unknown", [new("note-1", 99)])] };
        var handler = new ApproveVisitSummaryHandler(store, validator, catalog);
        Assert.Throws<ArgumentException>(() => handler.Handle(draft.Id, new(draft.Revision, invalid), "doctor-1"));
        var persisted = store.Get(draft.Id)!;
        Assert.That(persisted.Status, Is.EqualTo(SummaryStatus.PendingReview));
        Assert.That(persisted.History.Last().Outcome, Is.EqualTo("ApprovalRejected"));
        Assert.Throws<InvalidOperationException>(() => handler.Handle(draft.Id, new(draft.Revision, draft.Content!), "doctor-1"));
    }
    [Test]
    public async Task PatientAndVersionAreCheckedIndependently()
    {
        var draft = await Generate("valid");
        var wrongPatient = catalog.Visits[0] with { PatientId = "other-patient" };
        Assert.That(validator.Validate(draft.Content, wrongPatient).Errors, Is.Not.Empty);
        var stale = draft.Content! with { ReasonForVisit = [new("Reason", [new("note-1", 2)])] };
        Assert.That(validator.Validate(stale, catalog.Visits[0]).Errors, Is.Not.Empty);
    }
    [Test]
    public async Task ConcurrentRequestsShareSingleGeneration()
    {
        var fake = new BlockingGenerator();
        var workflow = new VisitSummaryWorkflow(store, fake, validator);
        var first = workflow.RunAsync(catalog.Visits[0], "same", "valid", default);
        var second = await workflow.RunAsync(catalog.Visits[0], "same", "valid", default);
        fake.Release.SetResult(await new DemoVisitSummaryGenerator().GenerateAsync(catalog.Visits[0], "valid", 1, default));
        var completed = await first;
        Assert.That(second.Id, Is.EqualTo(completed.Id));
        Assert.That(fake.Calls, Is.EqualTo(1));
    }
    private sealed class FixedGenerator(string raw) : IVisitSummaryGenerator
    {
        public Task<string> GenerateAsync(Visit visit, string scenario, int attempt, CancellationToken ct) => Task.FromResult(raw);
    }
    private sealed class BlockingGenerator : IVisitSummaryGenerator
    {
        public int Calls;
        public TaskCompletionSource<string> Release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Task<string> GenerateAsync(Visit visit, string scenario, int attempt, CancellationToken ct) { Calls++; return Release.Task; }
    }
}
