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
                    $"{Occurences}, {MaxInWord} in '{WordWithMax}' ({WordWithMax.Length}), '{LongestWord}' ({LongestWord.Length}) is '{Lower}'/'{Upper}'/'{Ascii}/{Lower == (Upper | 32)}', bitsize {(int) Math.Log(MaxInWord, 2)+1}";
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
        internal static void CodeGen(IEnumerable<string> dictionary)
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
            int bitNumber = 0;
            var primesLookup = new long[383];
            var bitmaskLookup = new long[383];
            foreach (var s in analyser.Symbols.OrderByDescending(s => s.Value.MaxInWord))
            {
                primeEnumerator.MoveNext();
                primesLookup[s.Value.Upper] = primesLookup[s.Value.Lower] = (int) primeEnumerator.Current;
                bitmaskLookup[s.Value.Upper] = bitmaskLookup[s.Value.Lower] = 1L << bitNumber;
                // Guess, why? :) 
                bitNumber += bitNumber < 36 ? ((int) Math.Log(s.Value.MaxInWord, 2) + 1) : 1;
            }

            Console.WriteLine("Max prime {0}, bitmask-bit {1}", primeEnumerator.Current, bitNumber);
            
            Console.WriteLine("\n\nprivate static readonly ulong[] Primes = { " +
                              string.Join(",", primesLookup) + " };");
            Console.WriteLine("\n\nprivate static readonly ulong[] Bitmasks = { " +
                              string.Join(",", bitmaskLookup) + " };");
        }
    }
}