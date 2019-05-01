using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ParkInfo.Models
{
    [Serializable, XmlRoot("Location")]
    public class Location
    {
        public int LocationId { get; set; }

        public string LocationName { get; set; }

        public string Address { get; set; }

        public string PostalCode { get; set; }

        [XmlArrayItem(Type = typeof(Facility))]
        public Facility[] Facilities { get; set; }

        [XmlArrayItem(Type = typeof(Intersection))]
        public Intersection[] Intersections { get; set; }

    }

    public class Facility
    {
        public string FacilityID { get; set; }

        public string FacilityType { get; set; }

        public string FacilityName { get; set; }

        public string FacilityDisplayName { get; set; }
    }

    public class Intersection
    {
        [XmlElement(ElementName = "Intersection")]
        public string StreetIntersection { get; set; }
    }
}
