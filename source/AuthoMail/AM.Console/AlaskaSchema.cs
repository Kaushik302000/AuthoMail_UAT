using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AM.Console
{
    [Serializable]
    [XmlRootAttribute(IsNullable = false, ElementName = "RespaControl")]
    public partial class ALKRespaControl
    {
        public string Address { get; set; }

        public string Address2 { get; set; }

        public string BorrowerFirstName { get; set; }

        public string BorrowerLastName { get; set; }

        public string City { get; set; }

        public string LoanNbr { get; set; }

        public string State { get; set; }

        public string StatusID { get; set; }

        public string Zip { get; set; }

        public ALKReturnInfo ReturnInfo { get; set; }

        public string File { get; set; }
    }

    /// <remarks/>
    [Serializable]
    [XmlType(TypeName = "ReturnInfo")]
    public partial class ALKReturnInfo
    {
        public ALKAddress Address;
    }

    [Serializable]
    [XmlType(TypeName = "Address")]
    public partial class ALKAddress
    {
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
    }
}
