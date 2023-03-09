


using System.Security.Cryptography;
using System.Text;




const int keySize = 64;
const int iterations = 350000;
HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;

   
var hash = HashPasword("NewP#D2345~~", out var salt);

Console.WriteLine($"Password hash: {hash}");
Console.WriteLine($"Generated salt: {Convert.ToHexString(salt)}");

Console.WriteLine("******************************************");
    

string HashPasword(string password, out byte[] salt)
{
    salt = RandomNumberGenerator.GetBytes(keySize);
    var hash = Rfc2898DeriveBytes.Pbkdf2(
        Encoding.UTF8.GetBytes(password),
        salt,
        iterations,
        hashAlgorithm,
        keySize);
    return Convert.ToHexString(hash);
}
