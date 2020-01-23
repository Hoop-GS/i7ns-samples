/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2020 iText Group NV
    Authors: iText Software.

    For more information, please contact iText Software at this address:
    sales@itextpdf.com
 */
/*
 * This example was written in the context of the following book:
 * https://leanpub.com/itext7_pdfHTML
 * Go to http://developers.itextpdf.com for more info.
 */

using System;
using System.IO;
using iText.Html2pdf;
using iText.Kernel.Pdf;
using iText.License;

namespace iText.Samples.Htmlsamples.Chapter07
{
    public class C07E08_Metadata
    {
        /// <summary>
        /// The path to the resulting PDF file.
        /// </summary>
        public static readonly String DEST = "./target/htmlsamples/ch07/metadata.pdf";

        /// <summary>
        /// The path to the source HTML file.
        /// </summary>
        public static readonly String SRC = "../../resources/htmlsamples/html/metadata.html";

        /// <summary>
        /// The main method of this example.
        /// </summary>
        /// <param name="args">no arguments are needed to run this example.</param>
        public static void Main(String[] args)
        {
            LicenseKey.LoadLicenseFile(Environment.GetEnvironmentVariable("ITEXT7_LICENSEKEY") +
                                       "/itextkey-html2pdf_typography.xml");
            FileInfo file = new FileInfo(DEST);
            file.Directory.Create();

            C07E08_Metadata app = new C07E08_Metadata();
            app.CreatePdf(SRC, DEST);
        }

        /// <summary>
        /// Creates the PDF file.
        /// </summary>
        /// <param name="src">the path to the source HTML file</param>
        /// <param name="dest">the path to the resulting PDF</param>
        public void CreatePdf(String src, String dest)
        {
            PdfWriter writer = new PdfWriter(dest, new WriterProperties().AddXmpMetadata());
            HtmlConverter.ConvertToPdf(new FileStream(src, FileMode.Open, FileAccess.Read), writer);
        }
    }
}