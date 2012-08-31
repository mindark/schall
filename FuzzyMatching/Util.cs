using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System;
using Newtonsoft.Json;

namespace FuzzyMatching
{
    public static class Util
    {
        public static List<string> CamelCombinations = new List<string>();

        public static int Test(Listing listing, Product product)
        {
            int similarityPoints = 0;

            //Tests for Product#Model
            int maxDigit;

            //Get max digit from Product#SplittedModel
            //check if listing title contains that digit
            if (Int32.TryParse(product.SplittedModel.OrderByDescending(Digits).FirstOrDefault(), out maxDigit) && listing.JoinedTitle.Any(p => p.Contains(maxDigit.ToString(CultureInfo.InvariantCulture))))
            {
                //This is a strong match, assign 15 points for >= 4 digits number match
                if (maxDigit >= 1000)
                    similarityPoints += 15;

                else if (maxDigit >= 100)
                    similarityPoints += 10;

                else if (maxDigit >= 10)
                    similarityPoints += 5;
            }
            //Get longest string from Product#SplittedModel
            //check if listing title contains that string
            var maxString = product.SplittedModel.OrderByDescending(p => p.Length).First();
            int temp;
            if (Int32.TryParse(maxString, out temp) == false && listing.JoinedTitle.Any(t => t.Contains(maxString)))
            {
                var msl = maxString.Length;
                if (msl >= 6)
                    similarityPoints += 10;
                else if (msl >= 5)
                    similarityPoints += 8;
                else if (msl >= 4)
                    similarityPoints += 6;
            }
            //Asign points for manufacturers if they're equal 
            if (listing.Manufacturer.Contains(product.Manufacturer.ToLower()) || listing.Manufacturer.ToLower().Contains(product.Manufacturer.ToLower()))
            {
                similarityPoints += 4;
            }
            //Assign points for price if it's setted
            if (listing.Price != 0)
            {
                similarityPoints += 2;
            }
            //Assign points if product family is contained in listing family
            if (product.Family != null && ((product.SplittedFamilyCount > 3 && product.SplittedFamily.Where(listing.JoinedTitle.Contains).Count() > product.SplittedFamilyCount - 3) || product.SplittedFamily.All(listing.JoinedTitle.Contains)))
            {
                similarityPoints += 4;
            }
            return similarityPoints;
        }

        public static List<T> DeserializeObjects<T>(string file)
        {
            var ser = new DataContractJsonSerializer(typeof(T));
            return (from jsObj in File.ReadAllLines(file) select Encoding.ASCII.GetBytes(jsObj) into byteArray select new MemoryStream(byteArray) { Position = 0 } into stream select (T)ser.ReadObject(stream)).ToList();
        }

        public static List<string> Split(string name)
        {
            const string pattern = @"\s|-|\(|\)|_|,|\.|(?<=[a-z]{2})(?=[A-Z][a-z])|(\d+)";

            const string camelPattern = @"([a-z]{2})([A-Z][a-z])";
            foreach (Match match in Regex.Matches(name, camelPattern))
            {
                string comb = (match.Groups[1].ToString() + match.Groups[2]).ToLower();
                if (!CamelCombinations.Contains(comb))
                    CamelCombinations.Add(comb);
            }

            var substrings = Regex.Split(name, pattern);
            return substrings.Select(match => match.ToLower()).Where(m => m != "").ToList();
        }

        private static int Digits(string candidate)
        {
            int temp;
            Int32.TryParse(candidate, out temp);
            return Int32.TryParse(candidate, out temp) == false ? 0 : temp;
        }
        public static void SerializeObjects(string fileName, List<Product> products)
        {
            var json = new List<string>();
            foreach (var product in products)
            {
                dynamic serializeObj = new { product_name = product.ProductName, listings = new List<dynamic>() };
                foreach (var l in product.Listings)
                {
                    dynamic listing =
                        new { title = l.Title, currency = l.Currency, price = l.Price, manufacturer = l.Manufacturer };
                    serializeObj.listings.Add(listing);
                }
                json.Add(JsonConvert.SerializeObject(product));
            }
            File.WriteAllLines(fileName, json);
        }

    }

}