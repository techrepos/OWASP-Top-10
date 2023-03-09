## Encryption

Encryption is the process of converting information from one form to another using an encryption key. Encryption also allows us to recover the original information back using the corresponding encryption key.

The process of converting a normal readable message known as plain text into a garbage message or not readable message is know as Cipher Text. The cipher text obtained from the encryption can easily be transformed into plaintext using the encryption key. 
Some of the examples of encryption algorithms are `RSA`, `AES`, and `Blowfish`.

- Encryption is always done with the intention of recovering the original information back.
- Encryption is a two-way process. It allows us to encrypt as well as decrypt the information.
- Encryption & decryption key remains the same in the case of Symmetric Encryption while in the case of Asymmetric Encryption, they both are different.


**Symmetric Encryption**

Symmetric algorithms require the creation of a key and an initialization vector (IV). You must keep this key secret from anyone who shouldn't decrypt your data. The IV doesn't have to be secret but should be changed for each session

> same key is used for encryption and decryption,


**Symmetric Keys **

    - symmetric encryption classes supplied by .NET require a key and a new IV to encrypt and decrypt data
    - A new key and IV is automatically created when you create a new instance of one of the managed symmetric cryptographic classes using the parameterless Create() method.
    - Anyone that you allow to decrypt your data must possess the same key and IV and use the same algorithm
    - a new key and IV should be created for every session, and neither the key nor the IV should be stored for use in a later sessio

C# Example using AES algorithm

`Encrypt` Method

Takes three parameters, 
```c#


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
   

```

`Decrypt` Method

```c#
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
```

Initiating encryption and decryption

```c#
private static void Main(string[] args)
{
    Console.WriteLine("***************************");

    var unencryptedMessage = "Welcome to the world of the symmetric encryption...";
    Console.WriteLine($"Unencrypted message: {unencryptedMessage}");

    // 1. Create a key (shared key between sender and reciever).
    byte[] key, iv;
    using (Aes aesAlg = Aes.Create())
    {
        key = aesAlg.Key;
        iv = aesAlg.IV;
    }

    // 2. Sender: Encrypt message using key
    var encryptedMessage = Encrypt(unencryptedMessage, key, iv);
    Console.WriteLine($"Sending encrypted message: {Convert.ToHexString( encryptedMessage)}");

    // 3. Receiver: Decrypt message using same key
    var decryptedMessage = Decrypt(encryptedMessage, key, iv);
    Console.WriteLine($"Recieved and decrypted message: {decryptedMessage}");

}

```

**Output**

```bash
Unencrypted message: Welcome to the world of the symmetric encryption...
Sending encrypted message: B5CAF76BD302D867707EC0BDFC8C796F8A0D754768951A51A06487F3124344028471D70D599B23D68CF831047BAF2B4CDD561BB69C3B76F3417289DFA7AD70C7
Recieved and decrypted message: Welcome to the world of the symmetric encryption...
```

**Asymmetric Encryption**

Asymmetric algorithms require the creation of a public key and a private key. The public key can be made known to anyone, but the decrypting party must only know the corresponding private key

Widely used algorithms include
    - RSA
    - EC (Elliptic Curve)

These algorithms can be used for encryption and decryption, as well as digital signatures.

- Asymmetric keys can be either stored for use in multiple sessions or generated for one session only. 
- While you can make the public key available, you must closely guard the private key.


`Encrypt` Method

```c#
private static byte[] Encrypt(string message, RSAParameters rsaParameters)
{
    using var rsaAlg = RSA.Create(rsaParameters);
    return rsaAlg.Encrypt(Encoding.UTF8.GetBytes(message), RSAEncryptionPadding.Pkcs1);
}
```

`Decrypt` Method

```c#
private static string Decrypt(byte[] cipherText, RSAParameters rsaParameters)
{
    using var rsaAlg = RSA.Create(rsaParameters);
    var decryptedMessage = rsaAlg.Decrypt(cipherText, RSAEncryptionPadding.Pkcs1);
    return Encoding.UTF8.GetString(decryptedMessage);
}
```



we create a public and a private key pair, we encrypt the message using the public key and we decrypt message using the private key.


Initiating encryption and decryption

```c#
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
```

**Output**

```bash
Unencrypted message: To be or not to be, that is the question, whether tis nobler in the...
Sending encrypted message: 44478CA550A3D9D0AFCCC3954C85FBE281A16F8A4335AF982E5E2BCEB71E11E98D9B002C1565F7EE4390D849621EECBBA93DC57C3F4AE602E2D9E709F0132C8DD8BDBEEEF6420203AFC2289D36E9CA60166B94FFA7DF505292403E005F46DB8104E5AF17CD8752B6F778D14F15621B6F9A8AE549852330C054CD540E292467492D5E4BD2408469F6C3CB1F26594322FA9E8140B09D559611862563BFCA7C960711B22E9D70AD352C52B800130396E302B3FA3648F7D9C10D238E3E064BBA46C57BAAABA151830184B1AB8BA996625B9A7C6E44E083009B4AEB4EE6ADC9420A2D6FEEB492AEDEA753F1491ADBF90CB95B9C0968F0D4823DD1C095EA724EFE8DBB
Recieved and decrypted message: To be or not to be, that is the question, whether tis nobler in the...
```



## Hashing


Hash functions can get input and return unique output creating a singular (sort of) “signature” for that input. Hashing is a common requirement for storing passwords over Databases but you need to be very careful when implementing security logic for yourself.

Some of the examples of a hashing algorithm are `MD5`, `SHA256`.

 In the case of hashing, information is not recoverable. Once a value is hashed, we can not get the original value from its hashed value.

- Hashing is always done with the intention when we do not want to get the original data back.
- Hashing is a one-way process. We can only encrypt a value but can’t decrypt it.
- Hashing is mostly used to store passwords. We store the hashed password in the database & whenever the user enters his password, we hash it & compare it with the hashed password stored in the database. If both match, only then a user is considered authenticated.

`C# Example`

1. Import the below namespace

```c#
using System.Security.Cryptography;
```

Create a new method for hashing the text, this sample uses SHA-256 algorithm

```c#
private static string ComputeHash(string message)
{
    using var sha256 = SHA256.Create();
    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(message));
    return Convert.ToHexString(hashedBytes);
}

```
Invoke hashing operation

```c#
private static void Main(string[] args)
{
    Console.WriteLine("******************************************");
    var items = new[] {
            
            "The red biker jumps over the black cone",
            "The red biker jumps ouer the black cone",
            "The red biker jumps oevr the black cone",
            "The red biker jumps oer the black cone"};
    foreach (var message in items)
    {
        Console.WriteLine($"{message} => {ComputeHash(message)}");
    }

    Console.WriteLine("******************************************");
}

```

**Output**

If you look at the output carefully you can see that the hashed string in entirely different for even the slightest of changes in the input string

```bash
******************************************
The red biker jumps over the black cone => 4FBC3D9A49654E8676EA82FC59155AA8EE2B8B6C50589DB63A78BBAB80051412
The red biker jumps ouer the black cone => FE3B108AC6F94231ADFB0F0D838211224A8D30F311E494B5693745EC2807B58A
The red biker jumps oevr the black cone => 6E23670064CCCE78E084E76AA3319CB9FB2680C9EA38CEE59A8D2D48869945AB
The red biker jumps oer the black cone => 55AA4F9071BBAFB8F9928BAF84A30C2DC54ED1234E28D5F094EB2722F21F5ABD
******************************************

```


## Salting

Salting is the process of adding additional data to the information before hashing it. Salting with hashing makes your information more confidential & not easily hackable.

Your Salt should always be very confidential. Though even if someone knows your salt cannot hack your information as hashing is itself very secure. But if an attacker tries various possible passwords & append your salt, then there is a possibility that he might end up cracking the password.

```c#
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

```

Invoking

```c#
const int keySize = 64;
const int iterations = 350000;
HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;

   
var hash = HashPasword("NewP#D2345~~", out var salt);

Console.WriteLine($"Password hash: {hash}");
Console.WriteLine($"Generated salt: {Convert.ToHexString(salt)}");

Console.WriteLine("******************************************");
    
```


**Output**

```bash
Password hash: DFC048CE3106B8B2C7E26FD8EFD4BAD0FA821C330B0CDA0FF1FAB7DE098BAB15D7B18971BC33DEF99AA02C073D4EC72AE1E5197D86909B45DDDE7A301D6CA407
Generated salt: 5CD53AAA5557F3EF77806A6BB994D1FF4D1981F301AA7E7F67AE2D5D463B1856298F7A9EEBB770B64DBFB44194D1581BF7F7ABD15166A43ADEA97C071614ED9F
```