






using System.Security.Cryptography;
using System.Text;

class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("***************************");

        var unencryptedMessage = "To be or not to be, that is the question, whether tis nobler in the...";
        Console.WriteLine($"Unencrypted message: {unencryptedMessage}");

        // 1. Create a public / private key pair.
        RSAParameters privateAndPublicKeys, publicKeyOnly;
        using (var rsaAlg = RSA.Create())
        {
            privateAndPublicKeys = rsaAlg.ExportParameters(includePrivateParameters: true);
            publicKeyOnly = rsaAlg.ExportParameters(includePrivateParameters: false);
        }

        // 2. Sender: Encrypt message using public key
        var encryptedMessage = Encrypt(unencryptedMessage, publicKeyOnly);
        Console.WriteLine($"Sending encrypted message: {Convert.ToHexString(encryptedMessage)}");

        // 3. Receiver: Decrypt message using private key
        var decryptedMessage = Decrypt(encryptedMessage, privateAndPublicKeys);
        Console.WriteLine($"Recieved and decrypted message: {decryptedMessage}");


    }

    private static byte[] Encrypt(string message, RSAParameters rsaParameters)
    {
        using var rsaAlg = RSA.Create(rsaParameters);
        return rsaAlg.Encrypt(Encoding.UTF8.GetBytes(message), RSAEncryptionPadding.Pkcs1);
    }

    private static string Decrypt(byte[] cipherText, RSAParameters rsaParameters)
    {
        using var rsaAlg = RSA.Create(rsaParameters);
        var decryptedMessage = rsaAlg.Decrypt(cipherText, RSAEncryptionPadding.Pkcs1);
        return Encoding.UTF8.GetString(decryptedMessage);
    }
}