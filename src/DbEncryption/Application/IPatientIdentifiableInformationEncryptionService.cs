namespace DbEncryption;

/// <summary>
/// Encrypts and decrypts patient identifiable information fields before database persistence and after retrieval.
/// </summary>
public interface IPatientIdentifiableInformationEncryptionService
{
    EncryptedPatientIdentifiableInformation Encrypt(PatientIdentifiableInformation patientInformation);

    PatientIdentifiableInformation Decrypt(EncryptedPatientIdentifiableInformation encryptedPatientInformation);
}
