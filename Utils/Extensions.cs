<<<<<<< HEAD
using System;
using System.Collections.Generic;

namespace Utils
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

        public static IEnumerable<List<T>> SplitList<T>(List<T> chunks, int nSize = 6)
        {
            var list = new List<List<T>>();

            for (int i = 0; i < chunks.Count; i += nSize)
            {
                list.Add(chunks.GetRange(i, Math.Min(nSize, chunks.Count - i)));
            }

            return list;
        }
    }
=======
using System;
using System.Collections.Generic;

namespace Utils
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

        public static IEnumerable<List<T>> SplitList<T>(List<T> chunks, int nSize = 6)
        {
            var list = new List<List<T>>();

            for (int i = 0; i < chunks.Count; i += nSize)
            {
                list.Add(chunks.GetRange(i, Math.Min(nSize, chunks.Count - i)));
            }

            return list;
        }
    }
>>>>>>> dcc9280ee55e4e19cb91acb156a096240b4797f7
}