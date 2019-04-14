using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SimHashBuckets
{
    class SimHashBuckets
    {
        private static void Main(string[] args)
        {
            #region Debugging LSH algorithm

            //const Int32 BufferSize = 128;
            //using (var fileStream = File.OpenRead("S:\\Projekti\\SimHashBuckets\\SimHashBuckets\\bin\\Debug\\R.in"))
            //using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
            //{
            //    var textNumber = Convert.ToInt32(streamReader.ReadLine());
            //    var listOfHashes = new List<string>();

            //    for (var i = 0; i < textNumber; i++)
            //    {
            //        var line = streamReader.ReadLine().Trim();
            //        if (string.IsNullOrWhiteSpace(line))
            //        {
            //            continue;
            //        }
            //        var hash = SimHashAlgorithm(line);
            //        listOfHashes.Add(hash);
            //    }
            //    var listOfCandidates = LSHAlgorithm(listOfHashes);

            //    var queryNumber = Convert.ToInt32(streamReader.ReadLine());

            //    for (var j = 0; j < queryNumber; j++)
            //    {

            //        var line = streamReader.ReadLine().Split();
            //        if (line == null || line.Length != 2)
            //        {
            //            continue;
            //        }
            //        var textIndex = Convert.ToInt32(line[0]);
            //        var maxDistance = Convert.ToInt32(line[1]);

            //        var numberOfSimilarTexts = RunQuery(listOfHashes, listOfCandidates, textIndex, maxDistance);
            //        Console.WriteLine(numberOfSimilarTexts);
            //    }

            #endregion


            #region Reading and hashing the text

            var textNumber = Convert.ToInt32(Console.ReadLine());
            var listOfHashes = new List<string>();

            for (var i = 0; i < textNumber; i++)
            {
                var line = Console.ReadLine().Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                var hash = SimHashAlgorithm(line);
                listOfHashes.Add(hash);
            }

            #endregion

            //za svaki tekst dobivamo pripadajući skup tekstova koji su potencijalno kandidati za sličnost
            var listOfCandidates = LSHAlgorithm(listOfHashes);

            #region Running every query using the list of candidates

            var queryNumber = Convert.ToInt32(Console.ReadLine());
            for (var j = 0; j < queryNumber; j++)
            {

                var line = Console.ReadLine().Split();
                if (line == null || line.Length != 2)
                {
                    continue;
                }
                var textIndex = Convert.ToInt32(line[0]);
                var maxDistance = Convert.ToInt32(line[1]);

                //skraćena verzija od koda napisanog u regiji "Running the query in a simpler, more understanding way
                var candidatesForTextIndex = listOfCandidates[textIndex];
                var numberOfSimilarTexts = candidatesForTextIndex.Select(candidate => HammingDistance(listOfHashes[candidate], listOfHashes[textIndex])).Count(hammingDistance => maxDistance >= hammingDistance);

                #region Running the query in a simpler, more understanding way

                //var numberOfSimilarTexts = 0;
                //var candidatesForTextIndex = listOfCandidates[textIndex];
                //foreach (var candidate in candidatesForTextIndex)
                //{
                //    var hammingDistance = HammingDistance(listOfHashes[candidate], listOfHashes[textIndex]);
                //    if (maxDistance >= hammingDistance)
                //    {
                //        numberOfSimilarTexts++;
                //    }
                //}
                //return numberOfSimilarTexts;

                #endregion

                Console.WriteLine(numberOfSimilarTexts);
            }

            #endregion

        }

        //algoritam je napisan prema pseudokodu napisanom u zadatku 1. laboratorijske vježbe
        private static Dictionary<int, HashSet<int>> LSHAlgorithm(List<string> listOfHashes)
        {
            var listOfCandidates = InitializeDictionary(listOfHashes.Count);
            for (var band = 0; band < 8; band++)
            {
                var buckets = new Dictionary<int, HashSet<int>>();
                for (var currentID = 0; currentID < listOfHashes.Count; currentID++)
                {
                    var value = Hash2Int(listOfHashes[currentID], band);
                    var textsInBucket = new HashSet<int>();

                    if (buckets.ContainsKey(value))
                    {
                        textsInBucket = buckets[value];
                        foreach (var textID in textsInBucket)
                        {
                            listOfCandidates[currentID].Add(textID);
                            listOfCandidates[textID].Add(currentID);
                        }
                    }
                    else
                    {
                        textsInBucket = new HashSet<int>();
                    }
                    textsInBucket.Add(currentID);
                    buckets[value] = textsInBucket;
                }
            }
            return listOfCandidates;
        }

        private static Dictionary<int, HashSet<int>> InitializeDictionary(int lenght)
        {
            //prilikom inicijalizacije korstimo HashSet jer korištenjem List uzimati će nam se i duplikati
            //ovako korištenjem HashSet, to je sve napravljeno za nas, i duplikati se neće ponovno dodavati u listu
            var listOfCandidates = new Dictionary<int, HashSet<int>>();
            for (var i = 0; i < lenght; i++)
            {
                listOfCandidates[i] = new HashSet<int>();
            }
            return listOfCandidates;
        }

        //algoritam je napisan prema pseudokodu napisanom u zadatku 1. laboratorijske vježbe
        public static string SimHashAlgorithm(string text)
        {
            MD5 md5 = MD5.Create();

            //the hash generated by md5.ComputeHash() is always 128 bits
            var sh = new int[128];
            var words = text.Split();
            foreach (var word in words)
            {
                var wordToBytes = Encoding.ASCII.GetBytes(word);
                var hash = md5.ComputeHash(wordToBytes);
                for (int i = 0; i < hash.Length; i++)
                {
                    //    128  ,    64   ,    32   ,    16   ,     8   ,     4   ,    2    ,    1
                    // 10000000, 01000000, 00100000, 00010000, 00001000, 00000100, 00000010, 00000001
                    //j in the following for loop serves as a mask to check if hash[index] is 1 or 0
                    var br = 0;
                    for (int j = 128; j > 0; j = j / 2)
                    {
                        //pomoću index želimo znati koji je toćno bit jednak 1, zato ga provjeramo s maskama koje imaju jedinicu na različitim pozicijama
                        var index = i * 8 + br;
                        br++;

                        if ((hash[i] & j) != 0)
                        {
                            sh[index] += 1;
                        }
                        else
                        {
                            sh[index] -= 1;
                        }
                    }

                }
            }

            for (int k = 0; k < sh.Length; k++)
            {
                if (sh[k] >= 0)
                {
                    sh[k] = 1;
                }
                else
                {
                    sh[k] = 0;
                }
            }
            return string.Join("", sh);

            #region Check to see if SimHash works with hex example

            //var binaryString = string.Join("", sh);
            //StringBuilder result = new StringBuilder();
            //for (int i = 0; i < binaryString.Length; i += 8)
            //{
            //    string eightBits = binaryString.Substring(i, 8);
            //    result.AppendFormat("{0:x2}", Convert.ToByte(eightBits, 2));
            //}

            #endregion

        }

        public static int HammingDistance(string hash1, string hash2)
        {
            //skraćena verzija od koda napisanog u regiji "Hamming distance in a simpler, more understanding way"
            return hash1.Where((t, i) => t != hash2[i]).Count();

            #region Hamming distance in a simpler, more understanding way

            //var result = 0;
            //for (int i = 0; i < hash1.Length; i++)
            //{
            //    if (hash1[i] != hash2[i])
            //    {
            //        result++;
            //    }
            //}
            //return result;

            #endregion

        }

        private static int Hash2Int(string hash, int band)
        {
            //s obzirom na pojas(band) uzimamo određeni substring hash-a
            //pojas 1(naš index je za to 0) => 0:15
            //pojas 2(naš index je za to 1) => 16:31 itd... sve do pojasa 8
            //zatim tih 16 bita pretvaramo u integer
            var result = hash.Substring(band * 16, 16);
            return Convert.ToInt32(result, 2);
        }

    }
}
