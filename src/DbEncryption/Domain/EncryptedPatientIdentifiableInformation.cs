namespace DbEncryption;

/// <summary>
/// Protected patient identifiable information fields that are safe to persist in a database.
/// </summary>
public sealed record EncryptedPatientIdentifiableInformation(
    string PatientId,
    EncryptedValue FirstName,
    EncryptedValue LastName,
    EncryptedValue DateOfBirth,
    EncryptedValue? MedicalRecordNumber = null,
    EncryptedValue? EmailAddress = null,
    EncryptedValue? PhoneNumber = null,
    EncryptedValue? StreetAddress = null);
