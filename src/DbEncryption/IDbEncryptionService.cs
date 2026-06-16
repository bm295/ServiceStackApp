namespace DbEncryption;

/// <summary>
/// Protects and unprotects database values before they are persisted or after they are read.
/// </summary>
public interface IDbEncryptionService
{
    EncryptedValue Encrypt(ReadOnlySpan<byte> plainText, string? purpose = null);

    byte[] Decrypt(EncryptedValue encryptedValue);
}
