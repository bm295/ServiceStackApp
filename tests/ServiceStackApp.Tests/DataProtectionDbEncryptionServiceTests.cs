using System.Text;
using DbEncryption;
using Microsoft.AspNetCore.DataProtection;
using NUnit.Framework;

namespace ServiceStackApp.Tests;

public sealed class DataProtectionDbEncryptionServiceTests
{
    [Test]
    public void Encrypt_uses_the_stable_database_purpose_by_default()
    {
        var service = CreateService();

        var encrypted = service.Encrypt("sensitive"u8);

        Assert.That(encrypted.Purpose, Is.EqualTo("ServiceStackApp.DbEncryption.DatabaseValue"));
        Assert.That(Encoding.UTF8.GetString(service.Decrypt(encrypted)), Is.EqualTo("sensitive"));
    }

    [Test]
    public void Encrypt_preserves_an_explicit_purpose()
    {
        var service = CreateService();

        var encrypted = service.Encrypt("sensitive"u8, "test.explicit-purpose");

        Assert.That(encrypted.Purpose, Is.EqualTo("test.explicit-purpose"));
        Assert.That(Encoding.UTF8.GetString(service.Decrypt(encrypted)), Is.EqualTo("sensitive"));
    }

    private static DataProtectionDbEncryptionService CreateService() =>
        new(new EphemeralDataProtectionProvider());
}
