using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace anagramApplication
{
    class Program
    {
        /// <summary>Max string length to be checked using <see cref="ModPrimes"/></summary>
        /// <remarks>
        /// Mod check is faster, but only works for words that fit into ulong (64b) without overflow.
        /// As highest prime used is 157 and ulong64.MaxValue is 18446744073709551615, this makes 8 chars length.
        /// Use https://www.rapidtables.com/calc/math/Log_Calculator.html
        /// </remarks>
        internal const int MaxLengthForModPrimes = 8;

        static Program()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);    // Yes, as this is smth. specific to encoding, put in static class constructor.
        }

        static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            // Code goes here. 
            
            var dictionary = File.ReadAllLines(args[0], Encoding.GetEncoding(1257));    // Dictionary is "cp-1257"
            // Step 1: Learn Estonian
            //LearnEstonian(dictionary);
            // Step 2: FindAll to find best solution
            //var all = Analyser.FindAll(dictionary, dictionary);

            // Step 3: search anagrams as in task
            var input = args[1];
            var inputLength = input.Length;
            //var inputChars = CountChars(input);
            var inputPrimes = ComputePrimes(input);
            var anagrams = new List<string>();

            foreach (var word in dictionary)
            {
                if (
                    // IsAnagram(input, word) // First 
                    // inputLength == word.Length && HasEnoughChars(word, inputChars) // First, but "inline" check and compute input once.
                    // IsAnagramPrimes(input, word) // Second
                    // inputPrimes == ComputePrimes(word)    //  Second, process input once.
                    // inputLength == word.Length && inputPrimes == ComputePrimes(word)    // Second, process input once, inline length check. 
                    inputLength == word.Length && (inputLength < MaxLengthForModPrimes
                        ? ModPrimes(inputPrimes, word)
                        : inputPrimes == ComputePrimes(word)) // [463.983, 9.806]
                )
                {
                    anagrams.Add(word);
                }
            }
            Console.Write(stopwatch.ElapsedMilliseconds); // Yes, split ;)
            Console.Write("," + string.Join(",", anagrams));
        }

        #region Implementation 1:

        /// <summary>First simple implementation</summary>
        /// <param name="strA">String A</param>
        /// <param name="strB">String B</param>
        /// <returns>True, if {strA;strB} is anagram pair</returns>
        public static bool IsAnagram(string strA, string strB)
        {
            return
                strA.Length == strB.Length
                && HasEnoughChars(strA, CountChars(strB));
        }

        /// <summary>This is first primitive implementation - just count symbols in word.</summary>
        /// <param name="word">Word to split to chars.</param>
        /// <returns>Number of occurence in word by char (all to lower).</returns>
        internal static IDictionary<char, int> CountChars(string word)
        {
            var result = new Dictionary<char, int>();
            foreach (var ch in word.ToCharArray())
            {
                var lower = Char.ToLower(ch);
                if (!result.ContainsKey(lower))
                {
                    result.Add(lower, 1);
                }
                else
                {
                    result[lower]++;
                }
            }

            return result;
        }

        /// <summary></summary>
        /// <param name="word">Word to check</param>
        /// <param name="symbolCount">Symbol count, occurence by char (to lower)</param>
        /// <see cref="CountChars"/>
        /// <returns>True, if is anagram, but note, length equality is not checked!</returns>
        public static bool HasEnoughChars(string word, IDictionary<char, int> symbolCount)
        {
            // As this count shall be modified, I need to "clone" it (shallow-copy enough)
            var decounted = symbolCount.ToDictionary(i => i.Key, i => i.Value);
            foreach (var ch in word.ToCharArray())
            {
                var lower = char.ToLower(ch);
                if (!decounted.TryGetValue(lower, out var count))
                    return false;
                if (count == 0)
                    return false;
                decounted[lower] = count - 1;
            }

            return true;
        }

        #endregion

        #region Implementation 2
        
        #region Analyse + Generate multipliers

        /// <summary>Get list of prime numbers - google http://mathforum.org/dr.math/faq/faq.prime.num.html</summary>
        private static int[] _primes =
        {
            2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 103,
            107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199
        };

        /// <summary>This is used to examine dictionary and generate array.</summary>
        /// <remarks>
        /// Findings:
        /// - Totally 37 symbols used.
        /// - Max same symbol count is 8 for 'i' in 'diskrimineerimispoliitika'
        /// - Longest word is 'kergejoustiku-meistrivoistlused' with 31 symbols.
        /// - Max index of Symbol is 382/381, i.e. Ž/ž
        /// </remarks>
        /// <param name="dictionary"></param>
        private static void LearnEstonian(IEnumerable<string> dictionary)
        {
            var analyser = new Analyser();
            foreach (var word in dictionary)
            {
                analyser.Analyse(word);
            }

            analyser.PrintResults(Console.WriteLine);

            // Code-generation:
            // Create array of 382 elements (Ascii) and for each symbol, assign a 'new' prime number, both to Upper and Lower variations
            var primeEnumerator = _primes.GetEnumerator();
            var lookup = new int[383];
            foreach (var s in analyser.Symbols.OrderBy(s => s.Value.Occurences))
            {
                primeEnumerator.MoveNext();
                lookup[s.Value.Upper] = lookup[s.Value.Lower] = (int) primeEnumerator.Current;
            }

            Console.WriteLine(
                "\n\nprivate static readonly ulong[] _multipliers = { " + string.Join(",", lookup) + " };");
        }

        #endregion

        #region Primes
        /// <summary>This is generated from <see cref="LearnEstonian"/>.</summary>
        private static readonly ulong[] Multipliers =
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 29, 11, 0,
            0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 37, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 157, 53, 23, 83,
            149, 43, 73, 79, 151, 61, 113, 127, 103, 107, 101, 97, 5, 109, 139, 137, 131, 89, 7, 13, 17, 19, 0, 0, 0, 0,
            0,
            0, 157, 53, 23, 83, 149, 43, 73, 79, 151, 61, 113, 127, 103, 107, 101, 97, 5, 109, 139, 137, 131, 89, 7, 13,
            17, 19, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 71, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 67, 47, 0, 0, 0, 0, 0, 59, 0, 0, 0, 0, 0, 0, 0, 71, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 67, 47, 0, 0, 0, 0, 0, 59, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 41, 41, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 31, 31
        };

        /// <summary></summary>
        /// <param name="strA"></param>
        /// <param name="strB"></param>
        /// <returns></returns>
        internal static bool IsAnagramPrimes(string strA, string strB)
        {
            var s1L = strA.Length;
            if (s1L != strB.Length)
                return false;
            var s1 = ComputePrimes(strA);
            return s1L <= MaxLengthForModPrimes ? ModPrimes(s1, strB) : s1 == ComputePrimes(strB);
        }

        internal static ulong ComputePrimes(string word)
        {
            ulong result = 1;
            foreach (var l in word)
            {
                result *= Multipliers[l];
            }

            return result;
        }

        /// <summary>Check by modulo from primes calculation</summary>
        /// <param name="cmp">ComputePrimes result for</param>
        /// <param name="word"></param>
        /// <returns></returns>
        internal static bool ModPrimes(ulong cmp, string word)
        {
            foreach (var l in word)
            {
                var mul = Multipliers[l];
                if (cmp % mul == 0)
                {
                    cmp /= mul;
                }
                else
                {
                    return false;
                }
            }

            return cmp == 1;
        }

        #endregion
        #endregion
    }
}
