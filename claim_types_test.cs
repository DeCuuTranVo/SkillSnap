using System.Security.Claims;
using System;

class Program 
{
    static void Main()
    {
        Console.WriteLine($"ClaimTypes.NameIdentifier: {ClaimTypes.NameIdentifier}");
        Console.WriteLine($"ClaimTypes.Name: {ClaimTypes.Name}");
        Console.WriteLine($"ClaimTypes.Email: {ClaimTypes.Email}");
        Console.WriteLine($"ClaimTypes.Role: {ClaimTypes.Role}");
    }
}
