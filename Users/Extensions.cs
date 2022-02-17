using System;
using System.Collections.Generic;

namespace BBBUsers
{
    public static class Extensions
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

        public static List<List<T>> SplitList<T>(List<T> users, int nSize = 6)
        {
            var list = new List<List<T>>();

            for (int i = 0; i < users.Count; i += nSize)
            {
                list.Add(users.GetRange(i, Math.Min(nSize, users.Count - i)));
            }

            return list;
        }
    }
}