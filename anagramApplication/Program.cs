using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Encoding.RegisterProvider(CodePagesEncodingProvider
                .Instance); // Yes, as this is smth. specific to encoding, put in static class constructor.
        }

        static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            // Code goes here. 
            var dictionary = File.ReadAllLines(args[0], Encoding.GetEncoding(1257)); // Dictionary is "cp-1257"

            #region Analise + Find All
            /*
            // Step 1: Learn some Estonian and Code-Gen
            Analyser.CodeGen(dictionary);
            // Step 2: FindAll to find best solution
            FindAllParallel(dictionary, s => s, IsAnagram); // 2.131.393, 9.806
            FindAllParallel(dictionary, s => s, IsAnagramPrimes); //   486.151, 9.806
            FindAllParallel(dictionary, ComputePrimes, (p, w) => p == ComputePrimes(w)); //   465.423, 9.806
            FindAllParallel(dictionary, ComputeXor,(p, w) => p == ComputeXor(w)); //   465.385, but 89.130 => Too many false positives!
            FindAllParallel(dictionary, ComputeAdd,(p, w) => p == ComputeAdd(w)); //   460.294, but 16.824/14.090/9.820 => False positives!
            //foreach (var ap in FindAll(,,).OrderBy(kv => kv.Key)) { Console.WriteLine("{0}: {1}", ap.Key, string.Join(",", ap.Value)); }
            // Step 3: search anagrams as in task
            */
            #endregion

            var input = args[1];
            var inputLength = input.Length;
            var anagrams = new List<string>();
            // Here: parallel-foreach does not help, only overhead. // Parallel.ForEach(dictionary, word=>
            if (inputLength <= MaxLengthForModPrimes)
            {
                var inputRes = ComputePrimes(input);
                foreach (var word in dictionary)
                {
                    if (
                        inputLength == word.Length
                        && inputRes == ComputePrimes(word)
                        && 0 != string.Compare(input, word,
                            StringComparison.InvariantCultureIgnoreCase) // Exclude "self"
                    )
                    {
                        anagrams.Add(word);
                    }
                }
            }
            else
            {
                var inputRes = ComputeAdd(input);
                foreach (var word in dictionary)
                {
                    if (
                        inputLength == word.Length
                        && inputRes == ComputeAdd(word)
                        && ComputePrimes(input) == ComputePrimes(word) // Filter out 7 collision pairs.
                        && 0 != string.Compare(input, word,
                            StringComparison.InvariantCultureIgnoreCase) // Exclude "self"
                    )
                    {
                        anagrams.Add(word);
                    }
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

        #endregion

        #region Primes

        /// <summary>This is generated from <see cref="Analyser.CodeGen"/>.</summary>
        private static readonly ulong[] Primes =
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 13, 5, 0, 0,
            0, 0, 0, 3, 0, 0, 0, 0, 0, 19, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 137, 43, 37, 59,
            127, 97, 41, 113, 157, 47, 107, 103, 101, 109, 151, 67, 2, 61, 139, 131, 149, 79, 29, 7, 17, 31, 0, 0, 0, 0,
            0, 0, 137, 43, 37, 59, 127, 97, 41, 113, 157, 47, 107, 103, 101, 109, 151, 67, 2, 61, 139, 131, 149, 79, 29,
            7, 17, 31, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 89, 0, 0, 0, 0, 11, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 73, 71, 0, 0, 0, 0, 0, 83, 0, 0, 0, 0,
            0, 0, 0, 89, 0, 0, 0, 0, 11, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 73, 71, 0, 0, 0, 0, 0, 83, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 53, 53, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 23, 23
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
                result *= Primes[l];
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
                var mul = Primes[l];
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

        #region Implementation 3

        /// <summary>This is generated from <see cref="Analyser.CodeGen"/>.</summary>
        /// <remarks>
        /// For each symbol I have one bit set in lookup
        /// Theoretically, this means, if I Xor all symbols, for two words, and results are equal
        /// - This is anagram.
        /// - The difference is 2 letters.
        /// - If length of words is same, I may only have false positives like ('nöör'/'noor' or 'papa'/'mama')
        /// => Once this check is true, a real <see cref="ComputePrimes"/> should be performed. 
        /// </remarks>
        private static readonly ulong[] Bitmasks =
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            2251799813685248, 576460752303423488, 0, 0, 0, 0, 0, 288230376151711744, 0, 0, 0, 0, 0, 9007199254740992, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 16, 281474976710656, 70368744177664, 137438953472,
            524288, 35184372088832, 140737488355328, 65536, 1, 562949953421312, 2147483648, 268435456, 33554432,
            17179869184, 8192, 549755813888, 144115188075855872, 274877906944, 128, 4194304, 1024, 4398046511104,
            36028797018963968, 1152921504606846976, 4503599627370496, 72057594037927936, 0, 0, 0, 0, 0, 0, 16,
            281474976710656, 70368744177664, 137438953472, 524288, 35184372088832, 140737488355328, 65536, 1,
            562949953421312, 2147483648, 268435456, 33554432, 17179869184, 8192, 549755813888, 144115188075855872,
            274877906944, 128, 4194304, 1024, 4398046511104, 36028797018963968, 1152921504606846976, 4503599627370496,
            72057594037927936, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 17592186044416, 0, 0, 0, 0, 2305843009213693952, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            2199023255552, 1099511627776, 0, 0, 0, 0, 0, 8796093022208, 0, 0, 0, 0, 0, 0, 0, 17592186044416, 0, 0, 0, 0,
            2305843009213693952, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2199023255552, 1099511627776, 0, 0, 0, 0, 0,
            8796093022208, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 1125899906842624, 1125899906842624, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 18014398509481984, 18014398509481984
        };


        /// <summary>Computing XorBitmask</summary>
        /// <remarks>Note,</remarks>
        /// <param name="word"></param>
        /// <returns></returns>
        internal static ulong ComputeXor(string word)
        {
            ulong result = 1;
            foreach (var l in word)
            {
                result ^= Bitmasks[l];
            }

            return result;
        }

        /// <summary>This is generated from <see cref="Analyser.CodeGen"/>.</summary>
        /// <remarks>
        /// For each symbol I have one bit set in lookup
        /// Theoretically, this means, if I Add all symbols, for two words, and results are equal
        /// - This is anagram.
        /// - I can have some collisions, but I cannot think out an example, may be there is none!
        /// </remarks>
        internal static ulong ComputeAdd(string word)
        {
            ulong result = 1;
            foreach (var l in word)
            {
                result += Bitmasks[l];
            }

            return result;
        }

        #endregion

        #region Performance

        /// <summary>Get global list of anagrams.</summary>
        /// <remarks>
        /// Shall output total number found and time in millis. 
        /// </remarks>
        /// <param name="dictionary">Dictionary to look N*N for.</param>
        /// <param name="prepare">Prepare word for search.</param>
        /// <param name="compare">Compare</param>
        /// <returns>List of anagrams found by word.</returns>
        internal static Dictionary<string, string[]> FindAll<T>(IEnumerable<string> dictionary, Func<string, T> prepare,
            Func<T, string, bool> compare)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var anagrams = new List<KeyValuePair<string, string>>();
            foreach (var w1 in dictionary)
            {
                var w1Len = w1.Length;
                T w1p = prepare(w1);
                foreach (var w2 in dictionary)
                {
                    if (
                        w1Len == w2.Length
                        && compare(w1p, w2)
                        && 0 != string.Compare(w1, w2, StringComparison.InvariantCultureIgnoreCase)
                    )
                    {
                        anagrams.Add(new KeyValuePair<string, string>(w1, w2));
                    }
                }
            }

            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            Console.WriteLine(anagrams.Count);
            var result = anagrams.GroupBy(kv => kv.Key, kv => kv.Value).ToDictionary(i => i.Key, i => i.ToArray());
            return result;
        }

        /// <summary>Get global list of anagrams.</summary>
        /// <remarks>
        /// Shall output total number found and time in millis. 
        /// </remarks>
        /// <param name="dictionary">Dictionary to look N*N for.</param>
        /// <param name="prepare">Prepare word for search.</param>
        /// <param name="compare">Compare</param>
        /// <returns>List of anagrams found by word.</returns>
        internal static Dictionary<string, string[]> FindAllParallel<T>(IEnumerable<string> dictionary,
            Func<string, T> prepare,
            Func<T, string, bool> compare)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var anagrams = new ConcurrentBag<KeyValuePair<string, string>>();
            Parallel.ForEach(dictionary, w1 =>
            {
                var w1Len = w1.Length;
                T w1p = prepare(w1);
                Parallel.ForEach(dictionary, w2 =>
                {
                    if (
                        w1Len == w2.Length
                        && compare(w1p, w2)
                        && 0 != string.Compare(w1, w2, StringComparison.InvariantCultureIgnoreCase)
                    )
                    {
                        anagrams.Add(new KeyValuePair<string, string>(w1, w2));
                    }
                });
            });

            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            Console.WriteLine(anagrams.Count);
            var result = anagrams.GroupBy(kv => kv.Key, kv => kv.Value).ToDictionary(i => i.Key, i => i.ToArray());
            return result;
        }

        #endregion

        #endregion
    }
}