using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCI.Service
{
    public static class BarcodeInfo
    {

        public static int TemplateSize { get; private set; }
        public static float ActualSizeInch { get; private set; }
        public static int WhiteSpace { get; private set; }
        public static float ScaledWhiteSpace
        {
            get
            {
                return WhiteSpace * ScalePercent / 100f;
            }
        }
        public static float ActualSize
        {
            get
            {
                return ActualSizeInch * 72;
            }
        }
        public static float ScalePercent
        {
            get
            {
                return (ActualSize * 100f) / TemplateSize;
            }
        }
        public static float TopMargin { get; private set; }
        public static float BottomMargin { get; private set; }
        public static float LeftMargin { get; private set; }
        public static float RightMargin { get; private set; }

        public static float BarcodeTopMargin { get; private set; }
        public static float BarcodeBottomMargin { get; private set; }
        public static float BarcodeLeftMargin { get; private set; }
        public static float BarcodeRightMargin { get; private set; }

        static BarcodeInfo()
        {
            TopMargin = float.Parse(ConfigurationManager.AppSettings["TopMargin"]);
            BottomMargin = float.Parse(ConfigurationManager.AppSettings["BottomMargin"]);
            LeftMargin = float.Parse(ConfigurationManager.AppSettings["LeftMargin"]);
            RightMargin = float.Parse(ConfigurationManager.AppSettings["RightMargin"]);
            TemplateSize = Convert.ToInt32(ConfigurationManager.AppSettings["TemplateSize"]);
            ActualSizeInch = float.Parse(ConfigurationManager.AppSettings["ActualSizeInch"]);
            WhiteSpace = Convert.ToInt32(ConfigurationManager.AppSettings["WhiteSpace"]);

            BarcodeTopMargin = float.Parse(ConfigurationManager.AppSettings["BarcodeTopMargin"]);
            BarcodeBottomMargin = float.Parse(ConfigurationManager.AppSettings["BarcodeBottomMargin"]);
            BarcodeLeftMargin = float.Parse(ConfigurationManager.AppSettings["BarcodeLeftMargin"]);
            BarcodeRightMargin = float.Parse(ConfigurationManager.AppSettings["BarcodeRightMargin"]);
        }
    }
}

namespace iTextSharp.text.pdf
{
    public static class Extension
    {
        public static float ToPoint(this float value)
        {
            return value * 72;
        }

        public static float ToInch(this int value)
        {
            return value / 72f;
        }

        public static float ToInch(this float value)
        {
            return value / 72f;
        }
    }
}
