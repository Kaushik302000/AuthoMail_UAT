using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCI.Service
{
    class ScaleEvent : PdfPageEventHelper
    {
        protected float? hscale = null;
        protected float? wscale = null;

        public float HScale
        {
            get { return hscale.Value; }
        }

        public float WScale
        {
            get { return wscale.Value; }
        }

        protected PdfRectangle targetSize;
        protected PdfDictionary pageDict;

        private void InitScale(float wscale, float hscale)
        {
            this.wscale = wscale;
            this.hscale = hscale;
        }

        public ScaleEvent(float scale)
        {
            InitScale(scale, scale);
        }

        public ScaleEvent(float wscale, float hscale)
        {
            InitScale(wscale, hscale);
        }

        public ScaleEvent(PdfRectangle size)
        {
            this.targetSize = size;
        }

        public void SetPageDict(PdfDictionary pageDict)
        {
            this.pageDict = pageDict;

            if (targetSize != null)
            {
                this.hscale = null;
                this.wscale = null;

                var original = pageDict.GetAsArray(PdfName.MEDIABOX);

                if (original != null && (original.GetWidth() <= original.GetHeight()))
                {
                    wscale = targetSize.Width / original.GetWidth();
                    hscale = targetSize.Height / original.GetHeight();
                }
                else
                {
                    wscale = targetSize.Width / original.GetHeight();
                    hscale = targetSize.Height / original.GetWidth();
                }
            }
            else//This code added for convert odd size into letter or legal
            {
                this.hscale = null;
                this.wscale = null;

                var original = pageDict.GetAsArray(PdfName.MEDIABOX);
                float o_width = original.GetWidth();
                float o_height = original.GetHeight();

                if (original != null && (o_width <= o_height))
                {
                    if (o_height < 792f)
                    {
                        wscale = 612f / original.GetWidth();
                        hscale = 792f / original.GetHeight();
                    }
                    else if (o_height > 792f)
                    {
                        wscale = 612f / original.GetWidth();
                        hscale = 1008f / original.GetHeight();
                    }
                    else if ((o_width < 612f && o_height == 792f) || (o_width > 612f && o_height == 792f))
                    {
                        wscale = 612f / original.GetWidth();
                        hscale = 792f / original.GetHeight();
                    }
                    else if ((o_width > 612f && o_height == 1008f) || (o_width < 612f && o_height == 1008f))
                    {
                        wscale = 612f / original.GetWidth();
                        hscale = 1008f / original.GetHeight();
                    }
                    else
                    {
                        wscale = 1.0f;
                        hscale = 1.0f;
                    }
                }
                else
                {
                    if (o_width < 792f)
                    {
                        wscale = 612f / original.GetHeight();
                        hscale = 792f / original.GetWidth();
                    }
                    else if (o_width > 792f)
                    {
                        wscale = 612f / original.GetHeight();
                        hscale = 1008f / original.GetWidth();
                    }
                    else if ((o_height < 612f && o_width == 792f) || (o_height > 612f && o_width == 792f))
                    {
                        wscale = 612f / original.GetHeight();
                        hscale = 1008f / original.GetWidth();
                    }
                    else if ((o_height > 612f && o_width == 1008f) || (o_height < 612f && o_width == 1008f))
                    {
                        wscale = 612f / original.GetHeight();
                        hscale = 1008f / original.GetWidth();
                    }
                    else
                    {
                        wscale = 1.0f;
                        hscale = 1.0f;
                    }
                }
            }
        }

        public override void OnStartPage(PdfWriter writer, Document document)
        {
            var originalRotate = pageDict.GetAsNumber(PdfName.ROTATE);
            var originalPageSize = pageDict.GetAsArray(PdfName.MEDIABOX);

            if (originalPageSize.GetWidth() > originalPageSize.GetHeight())
            {
                if (originalRotate != null && originalRotate.IntValue == 90)
                {
                    pageDict.Put(PdfName.ROTATE, new PdfNumber(90));
                }
                else if (originalRotate != null && originalRotate.IntValue == 180)
                {
                    pageDict.Put(PdfName.ROTATE, new PdfNumber(90));
                }
                else
                {
                    pageDict.Put(PdfName.ROTATE, new PdfNumber(270));
                }
            }
            else if (originalRotate != null && originalRotate.IntValue == 90)
            {
                pageDict.Put(PdfName.ROTATE, new PdfNumber(0));
            }
            else if (originalRotate != null && originalRotate.IntValue == 270)
            {
                pageDict.Put(PdfName.ROTATE, new PdfNumber(180));
            }

            writer.AddPageDictEntry(PdfName.ROTATE, pageDict.GetAsNumber(PdfName.ROTATE));
            writer.AddPageDictEntry(PdfName.MEDIABOX, ScaleDown(pageDict.GetAsArray(PdfName.MEDIABOX), wscale, hscale));
            writer.AddPageDictEntry(PdfName.CROPBOX, ScaleDown(pageDict.GetAsArray(PdfName.CROPBOX), wscale, hscale));
        }

        public PdfArray ScaleDown(PdfArray original, float? wScale, float? hScale)
        {
            if (original == null || !wscale.HasValue || !hscale.HasValue)
                return null;

            if (original.GetWidth() > original.GetHeight())
            {
                return new PdfRectangle(original.GetWidth() * hscale.Value, original.GetHeight() * wscale.Value);
            }
            else
            {
                return new PdfRectangle(original.GetWidth() * wscale.Value, original.GetHeight() * hscale.Value);
            }
        }
    }
}
