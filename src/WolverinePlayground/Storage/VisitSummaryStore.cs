using System.Text.Json;
using DbEncryption;
using Microsoft.Data.Sqlite;
using WolverinePlayground.Features.VisitSummaries;

namespace WolverinePlayground.Storage;

public sealed class VisitSummaryStore
{
    private readonly string connectionString;
    private readonly IDbEncryptionService encryption;
    public VisitSummaryStore(string connectionString, IDbEncryptionService encryption)
    {
        this.connectionString = connectionString;
        this.encryption = encryption;
        using var db = Open();
        using var cmd = db.CreateCommand();
        cmd.CommandText = "CREATE TABLE IF NOT EXISTS VisitSummaries (Id TEXT PRIMARY KEY, RequestId TEXT UNIQUE NOT NULL, Revision INTEGER NOT NULL, Payload TEXT NOT NULL)";
        cmd.ExecuteNonQuery();
    }
    private SqliteConnection Open() { var db = new SqliteConnection(connectionString); db.Open(); return db; }
    private string Protect(VisitSummary value) => JsonSerializer.Serialize(encryption.Encrypt(JsonSerializer.SerializeToUtf8Bytes(value), "VisitSummaries.Payload.v1"));
    private VisitSummary Read(string payload) => JsonSerializer.Deserialize<VisitSummary>(encryption.Decrypt(JsonSerializer.Deserialize<EncryptedValue>(payload)!))!;
    public VisitSummary? Get(string id, bool byRequest = false)
    {
        using var db = Open(); using var cmd = db.CreateCommand();
        cmd.CommandText = byRequest ? "SELECT Payload FROM VisitSummaries WHERE RequestId=$id" : "SELECT Payload FROM VisitSummaries WHERE Id=$id";
        cmd.Parameters.AddWithValue("$id", id);
        return cmd.ExecuteScalar() is string payload ? Read(payload) : null;
    }
    public bool Insert(VisitSummary summary)
    {
        using var db = Open(); using var cmd = db.CreateCommand();
        cmd.CommandText = "INSERT OR IGNORE INTO VisitSummaries VALUES ($id,$request,$revision,$payload)";
        cmd.Parameters.AddWithValue("$id", summary.Id); cmd.Parameters.AddWithValue("$request", summary.RequestId);
        cmd.Parameters.AddWithValue("$revision", summary.Revision); cmd.Parameters.AddWithValue("$payload", Protect(summary));
        return cmd.ExecuteNonQuery() == 1;
    }
    public bool Save(VisitSummary summary, int expectedRevision)
    {
        using var db = Open(); using var cmd = db.CreateCommand();
        cmd.CommandText = "UPDATE VisitSummaries SET Payload=$payload, Revision=$revision WHERE Id=$id AND Revision=$expected";
        cmd.Parameters.AddWithValue("$id", summary.Id); cmd.Parameters.AddWithValue("$revision", summary.Revision);
        cmd.Parameters.AddWithValue("$expected", expectedRevision); cmd.Parameters.AddWithValue("$payload", Protect(summary));
        return cmd.ExecuteNonQuery() == 1;
    }
}
