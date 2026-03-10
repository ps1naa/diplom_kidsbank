namespace KidBank.Application.Common.Interfaces;

public interface IDataEncryptor
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
