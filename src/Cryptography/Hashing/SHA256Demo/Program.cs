


using System.Security.Cryptography;
using System.Text;



class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("******************************************");

        foreach (var message in new[] {
                
                "The red biker jumps over the black cone",
                "The red biker jumps ouer the black cone",
                "The red biker jumps oevr the black cone",
                "The red biker jumps oer the black cone"})
        {
            Console.WriteLine($"{message} => {ComputeHash(message)}");
        }


        Console.WriteLine("******************************************");
    }

    private static string ComputeHash(string message)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(message));
        return Convert.ToHexString(hashedBytes);
    }
}