using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace FuzzyMatching
{
    [DataContract]
    public class Product
    {
        private string _family;
        private string _model;

        [DataMember(Name = "product_name")]
        public string ProductName { get; set; }

        [DataMember(Name = "manufacturer")]
        public string Manufacturer { get; set; }

        [DataMember(Name = "family")]
        public string Family
        {
            get { return _family; }
            set
            {
                var split = Util.Split(value);
                SplittedFamily = split;
                SplittedFamilyCount = SplittedFamily.Count;
                _family = value;
            }
        }

        [DataMember(Name = "model")]
        public string Model
        {
            get { return _model; }
            set
            {
                SplittedModel = new List<string>();
                var split = Util.Split(value);
                SplittedModel = split;
                _model = value;
            }
        }
        public List<Listing> Listings { get; set; }
        public List<string> SplittedModel { get; set; }
        public List<string> SplittedFamily { get; set; }
        public int SplittedFamilyCount { get; set; }
    }
}