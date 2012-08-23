using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FuzzyMatching
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() != 3)
            {
                Console.WriteLine("FuzzyMatching <products file> <listings file> <output file>");
                return;
            }
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Error. Could not find products file.");
                return;
            }
            if (!File.Exists(args[1]))
            {
                Console.WriteLine("Error. Could not find listings file.");
                return;
            }

            try
            {
                //Measure current time, for further execution time calculation
                var currTime = DateTime.Now;
                //Deserialize products and listings from json file
                var products = Util.DeserializeObjects<Product>(args[0]);
                products.ForEach(c => c.Listings = new List<Listing>());
                var listings = Util.DeserializeObjects<Listing>(args[1]);

                //There are products with Family EasyShare that will be splitted in ["easy", "share"]
                //and products with easyshare family that will be ["easyshare"]
                //By using Util.CamelCombinations that we populated in Util.Split() transform ["easyshare"] in ["easy", "share"]
                Parallel.ForEach(Util.CamelCombinations, comb =>
                                                             {
                                                                 var lComb = string.Join("", comb.Take(2));
                                                                 var rComb = string.Join("", comb.Skip(2).Take(2));
                                                                 foreach (var prod in products)
                                                                 {
                                                                     if (prod.SplittedFamily != null)
                                                                         for (int i = 0; i < prod.SplittedFamily.Count; i++)
                                                                         {
                                                                             var m = prod.SplittedFamily[i];
                                                                             if (m.Contains(comb))
                                                                             {
                                                                                 var spl = m.Split(new[] { comb },
                                                                                                   StringSplitOptions.None);
                                                                                 var firstFam = spl[0] + lComb;
                                                                                 var secFam = rComb + rComb;
                                                                                 prod.SplittedFamily.RemoveAt(i);
                                                                                 prod.SplittedFamily.Add(firstFam);
                                                                                 prod.SplittedFamily.Add(secFam);
                                                                             }
                                                                         }
                                                                 }
                                                             });
                //Making things running in parallel using PLinq
                Parallel.ForEach(listings, listing =>
                                               {
                                                   int bestProductIndex = 0;
                                                   int bestProductScore = int.MinValue;
                                                   for (int i = 0; i < products.Count; i++)
                                                   {
                                                       var prod = products[i];
                                                       var score = Util.Test(listing, prod);
                                                       if (score > bestProductScore)
                                                       {
                                                           bestProductScore = score;
                                                           bestProductIndex = i;
                                                       }
                                                   }
                                                   if (bestProductScore > 10)
                                                       products[bestProductIndex].Listings.Add(listing);
                                               });
                //Serialize products to file
                Util.SerializeObjects(args[2], products);
                Console.WriteLine("Time used in ms: " + (DateTime.Now - currTime).TotalMilliseconds);
                int k = products.Sum(product => product.Listings.Count);
                Console.WriteLine("Matched listings: " + k);
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}