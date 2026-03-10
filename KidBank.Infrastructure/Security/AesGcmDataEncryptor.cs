using System.Security.Cryptography;
using System.Text;
using KidBank.Application.Common.Interfaces;

namespace KidBank.Infrastructure.Security;

public class AesGcmDataEncryptor : IDataEncryptor
{
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private readonly byte[] _key;

    public AesGcmDataEncryptor(byte[] key)
    {
        if (key.Length != 32)
            throw new ArgumentException("Encryption key must be 256 bits (32 bytes)");
        _key = key;
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var nonce = new byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);

        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

        var result = new byte[NonceSize + cipherBytes.Length + TagSize];
        Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
        Buffer.BlockCopy(cipherBytes, 0, result, NonceSize, cipherBytes.Length);
        Buffer.BlockCopy(tag, 0, result, NonceSize + cipherBytes.Length, TagSize);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        var fullCipher = Convert.FromBase64String(cipherText);

        if (fullCipher.Length < NonceSize + TagSize)
            throw new CryptographicException("Invalid cipher text");

        var nonce = new byte[NonceSize];
        var cipherLength = fullCipher.Length - NonceSize - TagSize;
        var cipherBytes = new byte[cipherLength];
        var tag = new byte[TagSize];

        Buffer.BlockCopy(fullCipher, 0, nonce, 0, NonceSize);
        Buffer.BlockCopy(fullCipher, NonceSize, cipherBytes, 0, cipherLength);
        Buffer.BlockCopy(fullCipher, NonceSize + cipherLength, tag, 0, TagSize);

        var plainBytes = new byte[cipherLength];

        using var aes = new AesGcm(_key, TagSize);
        aes.Decrypt(nonce, cipherBytes, tag, plainBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
