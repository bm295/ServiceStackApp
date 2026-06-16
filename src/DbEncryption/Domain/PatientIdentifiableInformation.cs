namespace DbEncryption;

/// <summary>
/// Patient identifiable information that should be protected before database persistence.
/// </summary>
public sealed record PatientIdentifiableInformation(
    string PatientId,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string? MedicalRecordNumber = null,
    string? EmailAddress = null,
    string? PhoneNumber = null,
    string? StreetAddress = null);
