using System;

namespace BigBlueButtonUsers
{
    public static class StringExtensions
    {
        public static string FirstCharToUpperNextToLower(this string input)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
                default: return input[0].ToString().ToUpper() + input.Substring(1).ToLower();
            }
        }
    }
}