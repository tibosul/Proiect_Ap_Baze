
using System;
using BCrypt.Net;

class Program
{
    static void Main()
    {
        string password = "Password123!";
        string hash = BCrypt.Net.BCrypt.EnhanceHash(password);
        Console.WriteLine(hash);
    }
}
