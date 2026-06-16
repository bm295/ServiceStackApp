using System.Text;

namespace DbEncryption;

/// <summary>
/// Protects patient identifiable information fields with distinct Data Protection purposes per field.
/// </summary>
public sealed class PatientIdentifiableInformationEncryptionService : IPatientIdentifiableInformationEncryptionService
{
    private const string DateOnlyFormat = "yyyy-MM-dd";

    private readonly IDbEncryptionService dbEncryptionService;

    public PatientIdentifiableInformationEncryptionService(IDbEncryptionService dbEncryptionService)
    {
        ArgumentNullException.ThrowIfNull(dbEncryptionService);

        this.dbEncryptionService = dbEncryptionService;
    }

    public EncryptedPatientIdentifiableInformation Encrypt(PatientIdentifiableInformation patientInformation)
    {
        ArgumentNullException.ThrowIfNull(patientInformation);

        if (string.IsNullOrWhiteSpace(patientInformation.PatientId))
        {
            throw new ArgumentException("Patient identifiable information must include a patient identifier.", nameof(patientInformation));
        }

        return new EncryptedPatientIdentifiableInformation(
            patientInformation.PatientId,
            ProtectRequired(patientInformation.FirstName, patientInformation.PatientId, PatientInformationField.FirstName),
            ProtectRequired(patientInformation.LastName, patientInformation.PatientId, PatientInformationField.LastName),
            ProtectRequired(patientInformation.DateOfBirth.ToString(DateOnlyFormat), patientInformation.PatientId, PatientInformationField.DateOfBirth),
            ProtectOptional(patientInformation.MedicalRecordNumber, patientInformation.PatientId, PatientInformationField.MedicalRecordNumber),
            ProtectOptional(patientInformation.EmailAddress, patientInformation.PatientId, PatientInformationField.EmailAddress),
            ProtectOptional(patientInformation.PhoneNumber, patientInformation.PatientId, PatientInformationField.PhoneNumber),
            ProtectOptional(patientInformation.StreetAddress, patientInformation.PatientId, PatientInformationField.StreetAddress));
    }

    public PatientIdentifiableInformation Decrypt(EncryptedPatientIdentifiableInformation encryptedPatientInformation)
    {
        ArgumentNullException.ThrowIfNull(encryptedPatientInformation);

        if (string.IsNullOrWhiteSpace(encryptedPatientInformation.PatientId))
        {
            throw new ArgumentException("Encrypted patient identifiable information must include a patient identifier.", nameof(encryptedPatientInformation));
        }

        return new PatientIdentifiableInformation(
            encryptedPatientInformation.PatientId,
            UnprotectRequired(encryptedPatientInformation.FirstName, encryptedPatientInformation.PatientId, PatientInformationField.FirstName),
            UnprotectRequired(encryptedPatientInformation.LastName, encryptedPatientInformation.PatientId, PatientInformationField.LastName),
            DateOnly.ParseExact(UnprotectRequired(encryptedPatientInformation.DateOfBirth, encryptedPatientInformation.PatientId, PatientInformationField.DateOfBirth), DateOnlyFormat),
            UnprotectOptional(encryptedPatientInformation.MedicalRecordNumber, encryptedPatientInformation.PatientId, PatientInformationField.MedicalRecordNumber),
            UnprotectOptional(encryptedPatientInformation.EmailAddress, encryptedPatientInformation.PatientId, PatientInformationField.EmailAddress),
            UnprotectOptional(encryptedPatientInformation.PhoneNumber, encryptedPatientInformation.PatientId, PatientInformationField.PhoneNumber),
            UnprotectOptional(encryptedPatientInformation.StreetAddress, encryptedPatientInformation.PatientId, PatientInformationField.StreetAddress));
    }

    private EncryptedValue ProtectRequired(string value, string patientId, PatientInformationField field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"Patient {field} is required.", nameof(value));
        }

        return Protect(value, patientId, field);
    }

    private EncryptedValue? ProtectOptional(string? value, string patientId, PatientInformationField field)
    {
        return string.IsNullOrWhiteSpace(value) ? null : Protect(value, patientId, field);
    }

    private EncryptedValue Protect(string value, string patientId, PatientInformationField field)
    {
        var purpose = BuildPurpose(patientId, field);
        return dbEncryptionService.Encrypt(Encoding.UTF8.GetBytes(value), purpose);
    }

    private string UnprotectRequired(EncryptedValue encryptedValue, string patientId, PatientInformationField field)
    {
        ArgumentNullException.ThrowIfNull(encryptedValue);

        var value = Unprotect(encryptedValue, patientId, field);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Patient {field} decrypted to an empty value.");
        }

        return value;
    }

    private string? UnprotectOptional(EncryptedValue? encryptedValue, string patientId, PatientInformationField field)
    {
        return encryptedValue is null ? null : Unprotect(encryptedValue, patientId, field);
    }

    private string Unprotect(EncryptedValue encryptedValue, string patientId, PatientInformationField field)
    {
        var expectedPurpose = BuildPurpose(patientId, field);
        if (!string.Equals(encryptedValue.Purpose, expectedPurpose, StringComparison.Ordinal))
        {
            throw new ArgumentException($"Encrypted patient {field} was protected with an unexpected purpose.", nameof(encryptedValue));
        }

        return Encoding.UTF8.GetString(dbEncryptionService.Decrypt(encryptedValue));
    }

    private static string BuildPurpose(string patientId, PatientInformationField field)
    {
        return $"{DataProtectionDbEncryptionService.DefaultPurpose}.PatientIdentifiableInformation.{patientId}.{field}";
    }

    private enum PatientInformationField
    {
        FirstName,
        LastName,
        DateOfBirth,
        MedicalRecordNumber,
        EmailAddress,
        PhoneNumber,
        StreetAddress,
    }
}
