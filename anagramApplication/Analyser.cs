using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace anagramApplication
{
    internal class Analyser
    {
        internal string LongestWord = "";

        internal class Symbol
        {
            /// <summary>Total occurrence during all analysis</summary>
            internal int Occurences = 0;

            /// <summary>Is this byte, i.e. 8-bit?</summary>
            internal byte Ascii;

            /// <summary>Symbol for lower-case</summary>
            internal char Lower;

            /// <summary>Symbol for upper-case</summary>
            internal char Upper;

            /// <summary>Max number in single word</summary>
            /// <see cref="WordWithMax"/>
            internal int MaxInWord = 0;

            /// <summary>Word with maximum number of occurence</summary>
            /// <see cref="MaxInWord"/>
            internal string WordWithMax = "";

            /// <summary>Longest word containing this letter</summary>
            internal string LongestWord = "";

            internal Symbol(char ch)
            {
                Lower = Char.ToLower(ch);
                Upper = Char.ToUpper(ch);
                Ascii = (byte) (ch & 255);
            }

            public override string ToString()
            {
                return
                    $"{Occurences}, {MaxInWord} in '{WordWithMax}' ({WordWithMax.Length}), '{LongestWord}' ({LongestWord.Length}) is '{Lower}'/'{Upper}'/'{Ascii}/{Lower == (Upper | 32)}'";
            }
        }

        internal readonly IDictionary<char, Symbol> Symbols = new Dictionary<char, Symbol>();

        internal void Analyse(string word)
        {
            if (LongestWord.Length < word.Length)
            {
                LongestWord = word;
            }

            foreach (var kv in Program.CountChars(word))
            {
                Symbol s;
                if (!Symbols.TryGetValue(kv.Key, out s))
                {
                    s = new Symbol(kv.Key);
                    Symbols.Add(kv.Key, s);
                }

                s.Occurences += kv.Value;
                if (s.MaxInWord < kv.Value)
                {
                    s.MaxInWord = kv.Value;
                    s.WordWithMax = word;
                }

                if (s.LongestWord.Length < word.Length)
                {
                    s.LongestWord = word;
                }
            }
        }

        internal void PrintResults(Action<string> action)
        {
            action(
                $"Longest word '{LongestWord}' ({LongestWord.Length}) Ascii Mi/Ma '{Symbols.Max(kv => (int) kv.Value.Lower)}/{Symbols.Max(kv => (int) kv.Value.Upper)}', Symbols {Symbols.Count}");
            // Sort symbols by max in word.
            foreach (var kv in Symbols.OrderByDescending(kv => kv.Value.MaxInWord))
            {
                action($"{kv.Key} : {kv.Value}");
            }
        }
        
        /// <summary>Get global list of anagrams.</summary>
        /// <param name="listA"></param>
        /// <param name="listB"></param>
        /// <returns></returns>
        internal static Dictionary<string, string[]> FindAll(IEnumerable<string> listA, IEnumerable<string> listB)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var anagrams = new List<KeyValuePair<string, string>>();
            foreach (var w1 in listA)
            {
                var w1Len = w1.Length;
                var w1Chars = Program.CountChars(w1);
                var w1Primes = Program.ComputePrimes(w1);
                foreach (var w2 in listB)
                {
                    if (string.Compare(w1, w2, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        continue; // Too simple, do not anagram yourself
                    }

                    if (
                        //IsAnagram(w1, w2)
                        //w1Len == w2.Length && HasEnoughChars(w2, w1Chars)
                        //IsAnagramPrimes(w1, w2)
                        //w1Primes == ComputePrimes(w2)
                        //w1Len == w2.Length && w1Primes == Program.ComputePrimes(w2)
                        w1Len == w2.Length && (w1Len < Program.MaxLengthForModPrimes
                            ? Program.ModPrimes(w1Primes, w2)
                            : w1Primes == Program.ComputePrimes(w2))
                    )
                    {
                        anagrams.Add(new KeyValuePair<string, string>(w1, w2));
                    }
                }
            }

            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            Console.WriteLine(anagrams.Count);
            var result = anagrams.GroupBy(kv => kv.Key, kv => kv.Value).ToDictionary(i => i.Key, i => i.ToArray());
            foreach (var ap in result.OrderBy(kv => kv.Key))
            {
                Console.WriteLine("{0}: {1}", ap.Key, string.Join(",", ap.Value));
            }

            return result;
        }
    }
}