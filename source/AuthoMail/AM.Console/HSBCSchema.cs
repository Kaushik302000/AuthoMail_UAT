using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AM.Console
{
    [Serializable]
    [XmlRootAttribute(IsNullable = false, ElementName = "BcgRespa")]
    public partial class HSBCBcgRespa
    {
        [XmlElementAttribute("RespaControl", IsNullable = false)]
        public virtual List<HSBCRespaControl> RespaControl { get; set; }
    }

    [Serializable]
    [XmlType(TypeName = "RespaControl")]
    public partial class HSBCRespaControl
    {
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string BorrowerFirstName { get; set; }
        public string BorrowerLastName { get; set; }
        public string BorrowerMiddleName { get; set; }
        public string BorrowerSuffix { get; set; }
        public string City { get; set; }
        public string LoanNbr { get; set; }
        public string PackageName { get; set; }
        public string State { get; set; }
        public string StatusID { get; set; }
        public string Zip { get; set; }
    }
}
