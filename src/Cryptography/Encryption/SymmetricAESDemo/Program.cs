






using System.Security.Cryptography;

class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("***************************");

        var unencryptedMessage = "Welcome to the world of the symmetric encryption...";
        Console.WriteLine($"Unencrypted message: {unencryptedMessage}");

        // 1. Create a key (shared key between sender and reciever).
        byte[] key = {
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16
            };
        byte[] iv;
        using (Aes aesAlg = Aes.Create())
        {
            //key = //aesAlg.Key;
            iv = aesAlg.IV;
        }

        // 2. Sender: Encrypt message using key
        var encryptedMessage = Encrypt(unencryptedMessage, key, iv);
        Console.WriteLine($"Sending encrypted message: {Convert.ToHexString( encryptedMessage)}");

        // 3. Receiver: Decrypt message using same key
        var decryptedMessage = Decrypt(encryptedMessage, key, iv);
        Console.WriteLine($"Recieved and decrypted message: {decryptedMessage}");

    }

    private static byte[] Encrypt(string message, byte[] key, byte[] iv)
    {
        using var aesAlg = Aes.Create();
        aesAlg.Key = key;
        aesAlg.IV = iv;

        var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(message); // Write all data to the stream.
        }
        return ms.ToArray();
    }

    private static string Decrypt(byte[] cipherText, byte[] key, byte[] iv)
    {
        using var aesAlg = Aes.Create();
        aesAlg.Key = key;
        aesAlg.IV = iv;

        var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        using var ms = new MemoryStream(cipherText);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        return sr.ReadToEnd();
    }
}