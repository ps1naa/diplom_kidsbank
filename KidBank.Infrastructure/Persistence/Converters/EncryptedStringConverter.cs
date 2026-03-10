using KidBank.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace KidBank.Infrastructure.Persistence.Converters;

public class EncryptedStringConverter : ValueConverter<string, string>
{
    public EncryptedStringConverter(IDataEncryptor encryptor)
        : base(
            v => encryptor.Encrypt(v),
            v => encryptor.Decrypt(v))
    {
    }
}

public class EncryptedNullableStringConverter : ValueConverter<string?, string?>
{
    public EncryptedNullableStringConverter(IDataEncryptor encryptor)
        : base(
            v => v == null ? null : encryptor.Encrypt(v),
            v => v == null ? null : encryptor.Decrypt(v))
    {
    }
}
