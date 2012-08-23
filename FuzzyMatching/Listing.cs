using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FuzzyMatching
{
    [DataContract]
    public class Listing
    {
        public bool Done { get; set; }
        private string _title;

        [DataMember(Name = "title")]
        public string Title
        {
            get { return _title; }
            set
            {
                JoinedTitle = Util.Split(value);
                _title = value;
            }
        }

        [DataMember(Name = "manufacturer")]
        public string Manufacturer { get; set; }

        [DataMember(Name = "currency")]
        public string Currency { get; set; }
        [DataMember(Name = "price")]
        public decimal Price { get; set; }
        public List<string> JoinedTitle { get; set; }
    }
}