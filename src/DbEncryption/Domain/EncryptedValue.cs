namespace DbEncryption;

/// <summary>
/// Represents a database value protected by the Microsoft Data Protection API.
/// </summary>
/// <param name="ProtectedData">The protected payload to persist in the database.</param>
/// <param name="Purpose">The Data Protection purpose used to derive the protector.</param>
public sealed record EncryptedValue(
    byte[] ProtectedData,
    string Purpose)
{
    public string ProtectedDataBase64 => Convert.ToBase64String(ProtectedData);
}
