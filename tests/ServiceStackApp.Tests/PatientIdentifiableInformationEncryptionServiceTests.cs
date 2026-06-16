using System.Text;
using DbEncryption;
using Microsoft.AspNetCore.DataProtection;
using NUnit.Framework;

namespace ServiceStackApp.Tests;

public sealed class PatientIdentifiableInformationEncryptionServiceTests
{
    [Test]
    public void Encrypt_protects_patient_identifiable_information_fields()
    {
        var service = CreateService();
        var patientInformation = CreatePatientInformation();

        var encrypted = service.Encrypt(patientInformation);

        Assert.That(encrypted.PatientId, Is.EqualTo(patientInformation.PatientId));
        Assert.That(encrypted.FirstName.ProtectedData, Is.Not.EqualTo(Encoding.UTF8.GetBytes(patientInformation.FirstName)));
        Assert.That(encrypted.LastName.ProtectedData, Is.Not.EqualTo(Encoding.UTF8.GetBytes(patientInformation.LastName)));
        Assert.That(encrypted.DateOfBirth.ProtectedData, Is.Not.EqualTo(Encoding.UTF8.GetBytes("1984-02-29")));
        Assert.That(encrypted.MedicalRecordNumber, Is.Not.Null);
        Assert.That(encrypted.EmailAddress, Is.Not.Null);
        Assert.That(encrypted.PhoneNumber, Is.Not.Null);
        Assert.That(encrypted.StreetAddress, Is.Not.Null);
    }

    [Test]
    public void Decrypt_restores_patient_identifiable_information()
    {
        var service = CreateService();
        var patientInformation = CreatePatientInformation();

        var encrypted = service.Encrypt(patientInformation);
        var decrypted = service.Decrypt(encrypted);

        Assert.That(decrypted, Is.EqualTo(patientInformation));
    }

    [Test]
    public void Encrypt_uses_distinct_patient_field_purposes()
    {
        var service = CreateService();
        var patientInformation = CreatePatientInformation();

        var encrypted = service.Encrypt(patientInformation);

        Assert.That(encrypted.FirstName.Purpose, Does.Contain($".{patientInformation.PatientId}.FirstName"));
        Assert.That(encrypted.LastName.Purpose, Does.Contain($".{patientInformation.PatientId}.LastName"));
        Assert.That(encrypted.DateOfBirth.Purpose, Does.Contain($".{patientInformation.PatientId}.DateOfBirth"));
        Assert.That(encrypted.MedicalRecordNumber!.Purpose, Does.Contain($".{patientInformation.PatientId}.MedicalRecordNumber"));
        Assert.That(encrypted.EmailAddress!.Purpose, Does.Contain($".{patientInformation.PatientId}.EmailAddress"));
        Assert.That(encrypted.PhoneNumber!.Purpose, Does.Contain($".{patientInformation.PatientId}.PhoneNumber"));
        Assert.That(encrypted.StreetAddress!.Purpose, Does.Contain($".{patientInformation.PatientId}.StreetAddress"));
    }

    [Test]
    public void Encrypt_omits_optional_empty_patient_identifiable_information_fields()
    {
        var service = CreateService();
        var patientInformation = new PatientIdentifiableInformation(
            "patient-456",
            "Grace",
            "Hopper",
            new DateOnly(1906, 12, 9));

        var encrypted = service.Encrypt(patientInformation);
        var decrypted = service.Decrypt(encrypted);

        Assert.That(encrypted.MedicalRecordNumber, Is.Null);
        Assert.That(encrypted.EmailAddress, Is.Null);
        Assert.That(encrypted.PhoneNumber, Is.Null);
        Assert.That(encrypted.StreetAddress, Is.Null);
        Assert.That(decrypted, Is.EqualTo(patientInformation));
    }

    private static PatientIdentifiableInformationEncryptionService CreateService()
    {
        var dataProtectionProvider = new EphemeralDataProtectionProvider();
        var dbEncryptionService = new DataProtectionDbEncryptionService(dataProtectionProvider);

        return new PatientIdentifiableInformationEncryptionService(dbEncryptionService);
    }

    private static PatientIdentifiableInformation CreatePatientInformation()
    {
        return new PatientIdentifiableInformation(
            "patient-123",
            "Ada",
            "Lovelace",
            new DateOnly(1984, 2, 29),
            "MRN-12345",
            "ada@example.test",
            "+1-555-0100",
            "123 Example Street");
    }
}
