using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCI.Service
{
    class PCIFieldData
    {
        public int PieceId { get; set; }//These will be used for sorting data for matching page counts pieceId wise
        public List<PCIField> PCIFieldColl { get; set; }
    }
}
