using System;

namespace MJ.CodeGenerator.Demo.Source
{
    internal class Program
    {
        static void Main()
        {
            ISomeType1 wrapped = new SomeType1Wrapper(100, "Hello MSBuild");

            Console.WriteLine($"{nameof(wrapped.SomeField1)}      -> \t{wrapped.SomeField1}");
            Console.WriteLine($"{nameof(wrapped.SomeField2)}      -> \t{wrapped.SomeField2}");
            Console.WriteLine($"{nameof(wrapped.SomeMethod1)}(10) -> \t{wrapped.SomeMethod1(10)}");
        }
    }
}