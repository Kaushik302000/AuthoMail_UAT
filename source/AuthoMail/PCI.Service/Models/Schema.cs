using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCI.Service
{
    class PCIField
    {
        public int Id { get; set; }
        public string FieldName { get; set; }
        public int StartPosition { get; set; }
        public int Length { get; set; }
        public bool IsActive { get; set; }
        public SqlDbTypeCustom DataType { get; set; }
        public string DataFormatTemplate { get; set; }
        public string DefaultValue { get; set; }

        public string DataFormat
        {
            get { return DataFormatTemplate.Replace("<len>", Length.ToString()); }
        }

        public string FormattedValue
        {
            get { return DataFormat.Fill(Value); }
        }

        private string strValue;

        public string StrValue
        {
            get { return strValue; }
            set
            {
                strValue = value;
                if (DataType == SqlDbTypeCustom.DateTime || DataType == SqlDbTypeCustom.Date || DataType == SqlDbTypeCustom.DateTime2)
                    clrValue = strValue.ToClrType(DataType, DataFormatTemplate.Replace("{0:", "").Replace("}", ""));
                else
                    clrValue = strValue.ToClrType(DataType);
            }
        }

        private object clrValue;

        public object Value
        {
            get { return clrValue; }
            //set
            //{
            //    clrValue = value;
            //    strValue = value.ToString();
            //}
        }

        public PCIField Clone()
        {
            return (PCIField)this.MemberwiseClone();
        }

        public override string ToString()
        {
            return StrValue;
        }
    }

    public class DocumentPageSizes
    {
        public string PageSize { get; set; }
        public int TotalPages { get; set; }
        public string PageNumbers { get; set; }
    }
}
