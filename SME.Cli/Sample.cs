using System;
namespace SME.Console
{
    public class Writer
    {
        public void Write(string message)
        {
            Console.WriteLine($"you said '{Sanitize(message)}!'");
        }

        public string Sanitize(string input)
        {
            return input;
        }
    }
}