using System.Security.Cryptography;
using FluentAssertions;
using KidBank.Infrastructure.Security;

namespace KidBank.Infrastructure.Tests.Security;

public class AesGcmDataEncryptorTests
{
    private readonly byte[] _validKey = new byte[32];

    public AesGcmDataEncryptorTests()
    {
        RandomNumberGenerator.Fill(_validKey);
    }

    [Fact]
    public void Constructor_WithInvalidKeyLength_ThrowsArgumentException()
    {
        var shortKey = new byte[16];

        var act = () => new AesGcmDataEncryptor(shortKey);

        act.Should().Throw<ArgumentException>().WithMessage("*256 bits*");
    }

    [Fact]
    public void EncryptDecrypt_RoundTrip_ReturnsOriginalText()
    {
        var encryptor = new AesGcmDataEncryptor(_validKey);
        var original = "Hello, World! Привет, мир! 12345";

        var encrypted = encryptor.Encrypt(original);
        var decrypted = encryptor.Decrypt(encrypted);

        decrypted.Should().Be(original);
    }

    [Fact]
    public void Encrypt_ProducesDifferentCiphertextEachTime()
    {
        var encryptor = new AesGcmDataEncryptor(_validKey);
        var original = "same text";

        var encrypted1 = encryptor.Encrypt(original);
        var encrypted2 = encryptor.Encrypt(original);

        encrypted1.Should().NotBe(encrypted2);
    }

    [Fact]
    public void Encrypt_EmptyString_ReturnsEmptyString()
    {
        var encryptor = new AesGcmDataEncryptor(_validKey);

        encryptor.Encrypt("").Should().Be("");
        encryptor.Encrypt(null!).Should().BeNull();
    }

    [Fact]
    public void Decrypt_EmptyString_ReturnsEmptyString()
    {
        var encryptor = new AesGcmDataEncryptor(_validKey);

        encryptor.Decrypt("").Should().Be("");
        encryptor.Decrypt(null!).Should().BeNull();
    }

    [Fact]
    public void Decrypt_WithDifferentKey_ThrowsCryptographicException()
    {
        var key1 = new byte[32];
        var key2 = new byte[32];
        RandomNumberGenerator.Fill(key1);
        RandomNumberGenerator.Fill(key2);

        var encryptor1 = new AesGcmDataEncryptor(key1);
        var encryptor2 = new AesGcmDataEncryptor(key2);

        var encrypted = encryptor1.Encrypt("secret data");

        var act = () => encryptor2.Decrypt(encrypted);

        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Decrypt_TamperedCiphertext_ThrowsCryptographicException()
    {
        var encryptor = new AesGcmDataEncryptor(_validKey);
        var encrypted = encryptor.Encrypt("secret data");

        var bytes = Convert.FromBase64String(encrypted);
        bytes[15] ^= 0xFF;
        var tampered = Convert.ToBase64String(bytes);

        var act = () => encryptor.Decrypt(tampered);

        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Decrypt_TooShortCiphertext_ThrowsCryptographicException()
    {
        var encryptor = new AesGcmDataEncryptor(_validKey);
        var tooShort = Convert.ToBase64String(new byte[10]);

        var act = () => encryptor.Decrypt(tooShort);

        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void EncryptDecrypt_LargeText_Succeeds()
    {
        var encryptor = new AesGcmDataEncryptor(_validKey);
        var largeText = new string('A', 100_000);

        var encrypted = encryptor.Encrypt(largeText);
        var decrypted = encryptor.Decrypt(encrypted);

        decrypted.Should().Be(largeText);
    }

    [Fact]
    public void EncryptDecrypt_UnicodeText_Succeeds()
    {
        var encryptor = new AesGcmDataEncryptor(_validKey);
        var unicode = "日本語 한국어 العربية emoji: 🎉🔐";

        var encrypted = encryptor.Encrypt(unicode);
        var decrypted = encryptor.Decrypt(encrypted);

        decrypted.Should().Be(unicode);
    }

    [Fact]
    public void Encrypt_OutputIsBase64()
    {
        var encryptor = new AesGcmDataEncryptor(_validKey);
        var encrypted = encryptor.Encrypt("test");

        var act = () => Convert.FromBase64String(encrypted);

        act.Should().NotThrow();
    }
}
