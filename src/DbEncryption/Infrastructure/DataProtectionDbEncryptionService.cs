using Microsoft.AspNetCore.DataProtection;

namespace DbEncryption;

/// <summary>
/// Database encryption service backed by the Microsoft Data Protection API.
/// </summary>
public sealed class DataProtectionDbEncryptionService : IDbEncryptionService
{
    public const string DefaultPurpose = "ServiceStackApp.DbEncryption.DatabaseValue";

    private readonly IDataProtectionProvider dataProtectionProvider;
    private readonly string defaultPurpose;

    public DataProtectionDbEncryptionService(
        IDataProtectionProvider dataProtectionProvider,
        string defaultPurpose = DefaultPurpose)
    {
        ArgumentNullException.ThrowIfNull(dataProtectionProvider);

        if (string.IsNullOrWhiteSpace(defaultPurpose))
        {
            throw new ArgumentException("A non-empty Data Protection purpose is required.", nameof(defaultPurpose));
        }

        this.dataProtectionProvider = dataProtectionProvider;
        this.defaultPurpose = defaultPurpose;
    }

    public EncryptedValue Encrypt(ReadOnlySpan<byte> plainText, string? purpose = null)
    {
        var resolvedPurpose = ResolvePurpose(purpose);
        var protector = dataProtectionProvider.CreateProtector(resolvedPurpose);
        var protectedData = protector.Protect(plainText.ToArray());

        return new EncryptedValue(protectedData, resolvedPurpose);
    }

    public byte[] Decrypt(EncryptedValue encryptedValue)
    {
        ArgumentNullException.ThrowIfNull(encryptedValue);

        if (string.IsNullOrWhiteSpace(encryptedValue.Purpose))
        {
            throw new ArgumentException("Encrypted values must include the Data Protection purpose used to protect them.", nameof(encryptedValue));
        }

        var protector = dataProtectionProvider.CreateProtector(encryptedValue.Purpose);

        return protector.Unprotect(encryptedValue.ProtectedData);
    }

    private string ResolvePurpose(string? purpose)
    {
        if (purpose is null)
        {
            return defaultPurpose;
        }

        if (string.IsNullOrWhiteSpace(purpose))
        {
            throw new ArgumentException("A non-empty Data Protection purpose is required.", nameof(purpose));
        }

        return purpose;
    }
}
