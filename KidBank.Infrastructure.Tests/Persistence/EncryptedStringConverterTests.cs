using System.Security.Cryptography;
using FluentAssertions;
using KidBank.Application.Common.Interfaces;
using KidBank.Infrastructure.Persistence.Converters;
using KidBank.Infrastructure.Security;

namespace KidBank.Infrastructure.Tests.Persistence;

public class EncryptedStringConverterTests
{
    private readonly IDataEncryptor _encryptor;

    public EncryptedStringConverterTests()
    {
        var key = new byte[32];
        RandomNumberGenerator.Fill(key);
        _encryptor = new AesGcmDataEncryptor(key);
    }

    [Fact]
    public void EncryptedStringConverter_RoundTrip_PreservesValue()
    {
        var converter = new EncryptedStringConverter(_encryptor);
        var original = "test@example.com";

        var toDb = converter.ConvertToProvider(original);
        var fromDb = converter.ConvertFromProvider(toDb);

        fromDb.Should().Be(original);
    }

    [Fact]
    public void EncryptedNullableStringConverter_Null_ReturnsNull()
    {
        var converter = new EncryptedNullableStringConverter(_encryptor);

        var toDb = converter.ConvertToProvider(null);
        var fromDb = converter.ConvertFromProvider(null);

        toDb.Should().BeNull();
        fromDb.Should().BeNull();
    }

    [Fact]
    public void EncryptedNullableStringConverter_RoundTrip_PreservesValue()
    {
        var converter = new EncryptedNullableStringConverter(_encryptor);
        var original = "some exception trace";

        var toDb = converter.ConvertToProvider(original);
        var fromDb = converter.ConvertFromProvider(toDb);

        fromDb.Should().Be(original);
    }

    [Fact]
    public void EncryptedStringConverter_DifferentValues_ProduceDifferentCiphertexts()
    {
        var converter = new EncryptedStringConverter(_encryptor);

        var encrypted1 = converter.ConvertToProvider("value1");
        var encrypted2 = converter.ConvertToProvider("value2");

        encrypted1.Should().NotBe(encrypted2);
    }
}
