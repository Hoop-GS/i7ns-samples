/*
This file is part of the iText (R) project.
Copyright (c) 1998-2020 iText Group NV
Authors: iText Software.

For more information, please contact iText Software at this address:
sales@itextpdf.com
*/

using System;
using System.IO;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;
using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.License;
using iText.Signatures;
using Org.BouncyCastle.Pkcs;

namespace iText.Samples.Sandbox.Typography.Latin
{
    public class LatinSignature
    {
        public const String DEST = "results/sandbox/typography/LatinSignature.pdf";
        public const String FONTS_FOLDER = "../../../resources/font/";
        public const String RESOURCE_FOLDER = "../../../resources/pdfs/";
        public const String CERTIFICATE_FOLDER = "../../../resources/cert/";
        private static readonly char[] PASSWORD = "testpass".ToCharArray();

        public static void Main(String[] args)
        {
            // Load the license file to use typography features
            LicenseKey.LoadLicenseFile(Environment.GetEnvironmentVariable("ITEXT7_LICENSEKEY") +
                                       "/itextkey-typography.xml");

            FileInfo file = new FileInfo(DEST);
            file.Directory.Create();

            new LatinSignature().CreatePDF(DEST);
        }

        public virtual void CreatePDF(String dest)
        {
            String line1 = "Text # 1";
            String line2 = "Text # 2";
            String line3 = "Text # 3 / ";

            PdfFont font = PdfFontFactory.CreateFont(FONTS_FOLDER + "FreeSans.ttf", PdfEncodings.IDENTITY_H);

            // Define certificate
            String certFileName = CERTIFICATE_FOLDER + "signCertRsa01.p12";

            // Prerequisite for signing
            ICipherParameters signPrivateKey = ReadFirstKey(certFileName, PASSWORD, PASSWORD);
            IExternalSignature pks = new PrivateKeySignature(signPrivateKey, DigestAlgorithms.SHA256);
            X509Certificate[] signChain = ReadFirstChain(certFileName, PASSWORD);

            PdfSigner signer = new PdfSigner(new PdfReader(RESOURCE_FOLDER + "simpleDocument.pdf"),
                    new FileStream(dest, FileMode.Create), new StampingProperties());

            Rectangle rect = new Rectangle(30, 500, 500, 100);

            // Set the name indicating the field to be signed
            signer.SetFieldName("Field1");

            // Get Signature Appearance and set some of its properties
            signer.GetSignatureAppearance()
                    .SetPageRect(rect)
                    .SetReason(line1)
                    .SetLocation(line2)
                    .SetReasonCaption(line3)
                    .SetLayer2Font(font);

            // Sign the document
            signer.SignDetached(pks, signChain, null, null, null, 0,
                    PdfSigner.CryptoStandard.CADES);
        }

        // Read pkcs12 file first private key
        private static X509Certificate[] ReadFirstChain(String p12FileName, char[] ksPass)
        {
            X509Certificate[] chain;
            String alias = null;
            Pkcs12Store pk12 = new Pkcs12Store(new FileStream(p12FileName, FileMode.Open, FileAccess.Read), ksPass);

            foreach (var a in pk12.Aliases)
            {
                alias = ((String) a);
                if (pk12.IsKeyEntry(alias))
                {
                    break;
                }
            }

            X509CertificateEntry[] ce = pk12.GetCertificateChain(alias);
            chain = new X509Certificate[ce.Length];
            for (int k = 0; k < ce.Length; ++k)
            {
                chain[k] = ce[k].Certificate;
            }

            return chain;
        }

        // Read first public certificate chain
        private static ICipherParameters ReadFirstKey(String p12FileName, char[] ksPass, char[] keyPass)
        {
            String alias = null;
            Pkcs12Store pk12 = new Pkcs12Store(new FileStream(p12FileName, FileMode.Open, FileAccess.Read), ksPass);

            foreach (var a in pk12.Aliases)
            {
                alias = ((String) a);
                if (pk12.IsKeyEntry(alias))
                {
                    break;
                }
            }

            ICipherParameters pk = pk12.GetKey(alias).Key;
            return pk;
        }
    }
}