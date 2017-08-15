using System;
using System.IO;
using HiQPdf;
using LFE.Core.Utils;

namespace LFE.Application.Services.ExternalProviders
{

    public class StandardPdfRenderer
    {
      //  private const int HorizontalMargin = 40;
     //   private const int VerticalMargin = 40;

        private const string SERIAL_NUMBER = "************************************************"; //HiQPdf license serial number

        public byte[] Html2Pdf(string html)
        {
            try
            {
                // create the HTML to PDF converter
                var htmlToPdfConverter = new HtmlToPdf { SerialNumber = SERIAL_NUMBER };
                
                // convert a HTML code to a PDF buffer in memory
                // the buffer can be saved to a file, a stream or a HTTP response
                var pdfBuffer = htmlToPdfConverter.ConvertHtmlToMemory(html, null);

                return pdfBuffer;

            }
            catch (Exception)
            {
                return new byte[0];
            }
        }

        public MemoryStream Html2PdfStream(string html)
        {
            try
            {
                // create the HTML to PDF converter
                var htmlToPdfConverter = new HtmlToPdf { SerialNumber = SERIAL_NUMBER };

                var outputStream = new MemoryStream();
                
                htmlToPdfConverter.ConvertHtmlToStream(html, null,outputStream);

                return outputStream;

            }
            catch (Exception)
            {
                return null;
            }
        }

        public PdfDocument Html2PdfDoc(string html,PdfPageSize size)
        {
            try
            {
                var document = new PdfDocument {SerialNumber = SERIAL_NUMBER,Security = { AllowEditContent = false,AllowCopyContent = false}};
                var page = document.AddPage(size,new PdfDocumentMargins(0,0,0,0),PdfPageOrientation.Landscape);
                page.DisplayFooter = false;
                page.DisplayHeader = false;
                //var page = document.AddPage(new PdfPageSize(640,455), new PdfDocumentMargins(4,4, 4,4));
                var pageHtml = new PdfHtml(html, Utils.BaseUrl());
                page.Layout(pageHtml);
                return document;
            }
            catch (Exception)
            {
                return null;
            }
        }

        //private void buttonCreatePdf_Click(object sender, EventArgs e)
        //{
        //    // create an empty PDF document
        //    var document = new PdfDocument();
            
        //    // add a page to document
        //    var page1 = document.AddPage(PdfPageSize.A4, new PdfDocumentMargins(5), PdfPageOrientation.Portrait);
        //    // set the document header and footer before adding any objects to document
        //    SetHeader(document);
        //    SetFooter(document);
        //    // add some content to first page
        //    var html1 = new PdfHtml("<b>First Page Body</b>", null);
        //    var html1LayoutInfo = page1.Layout(html1);
        //    // add a second page to document
        //    var page2 = document.AddPage(PdfPageSize.A4, new PdfDocumentMargins(5), PdfPageOrientation.Portrait);
        //    // add some content to second page
        //    var html2 = new PdfHtml("<b>Second Page Body</b>", null);
        //    var html2LayoutInfo = page2.Layout(html2);
        //    // set a custom header in first page
        //    page1 = document.Pages[0];
        //    // override the document header in first page
        //    page1.Header = document.CreateHeaderCanvas(document.Header.Width, document.Header.Height);
        //    // draw the custom page header
        //    DrawCustomHeader(page1.Header);
        //    // set a custom footer in second page
        //    page2 = document.Pages[1];
        //    // override the document footer in second page
        //    page2.Footer = document.CreateFooterCanvas(document.Footer.Width, document.Footer.Height);
        //    // draw the custom page footer
        //    DrawCustomFooter(page2.Footer);
        //    // write the output PDF file
        //    document.WriteToFile("CustomHeaderAndFooter.pdf");
        //}
        //private void SetHeader(PdfDocument document)
        //{
        //    // create the document header
        //    document.Header = document.CreateHeaderCanvas(document.Pages[0].DrawableRectangle.Width, 50);
        //    // layout HTML in header
        //    var headerHtml = new PdfHtml(0, 0, 0, document.Header.Height, "<span style=\"color:Blue\">Default Header Content</span>", null);
        //    headerHtml.FitDestHeight = true;
        //    headerHtml.FontEmbedding = true;
        //    document.Header.Layout(headerHtml);
        //}
        //private void SetFooter(PdfDocument document)
        //{
        //    //create the document footer
        //    document.Footer = document.CreateFooterCanvas(document.Pages[0].DrawableRectangle.Width, 50);
        //    // layout HTML in footer
        //    var footerHtml = new PdfHtml(0, 0, 0, document.Footer.Height, "<span style=\"color:Green\">Default Footer Content</span>", null);
        //    footerHtml.FitDestHeight = true;
        //    footerHtml.FontEmbedding = true;
        //    document.Footer.Layout(footerHtml);
        //}
        //private void DrawCustomHeader(PdfDocumentHeader pageHeader)
        //{
        //    // layout HTML in custom header
        //    var headerHtml = new PdfHtml(0, 0, 0, pageHeader.Height, "<span style=\"color:Red\">Custom Header Content</span>", null);
        //    headerHtml.FitDestHeight = true;
        //    headerHtml.FontEmbedding = true;
        //    pageHeader.Layout(headerHtml);
        //}
        //private void DrawCustomFooter(PdfDocumentFooter pageFooter)
        //{
        //    // layout HTML in custom footer
        //    var footerHtml = new PdfHtml(0, 0, 0, pageFooter.Height, "<span style=\"color:Red\">Custom Footer Content</span>", null);
        //    footerHtml.FitDestHeight = true;
        //    footerHtml.FontEmbedding = true;
        //    pageFooter.Layout(footerHtml);
        //}
        
    }
    
    
}
