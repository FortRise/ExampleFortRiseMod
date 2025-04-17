using System;

namespace Teuria.NewExampleMod;

// implement the methods with interface
public sealed class ApiImplementation : INewModApi
{
    public void SayHelloWorld()
    {
        Console.WriteLine("Say Hello World");
    }
}

