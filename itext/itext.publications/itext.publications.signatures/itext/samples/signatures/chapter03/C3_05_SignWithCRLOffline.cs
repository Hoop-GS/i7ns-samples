/*

This file is part of the iText (R) project.
Copyright (c) 1998-2019 iText Group NV

*/
/*
* This class is part of the white paper entitled
* "Digital Signatures for PDF documents"
* written by Bruno Lowagie
*
* For more info, go to: http://itextpdf.com/learn
*/
using System;
using System.Collections.Generic;
using System.IO;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;
using iText.IO.Util;
using iText.Kernel.Geom;
using iText.Signatures;
using NUnit.Framework;
using Org.BouncyCastle.Pkcs;

namespace iText.Samples.Signatures.Chapter03
{
	public class C3_05_SignWithCRLOffline : C3_01_SignWithCAcert
	{
	    public static readonly string CRLURL = NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/../../resources/encryption/revoke.crl";

	    public static readonly string SRC = NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/../../resources/pdfs/hello.pdf";

	    public static readonly string DEST = NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/test/resources/signatures/chapter03/hello_cacert_crl_offline.pdf";

	    public static readonly string PROPERTIES = NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/../../resources/encryption/signkey.properties";

	    /// <exception cref="System.IO.IOException"/>
		/// <exception cref="Org.BouncyCastle.Security.GeneralSecurityException"/>
		public static void Main(String[] args)
		{
            Properties properties = new Properties();
            properties.Load(new FileStream(PROPERTIES, FileMode.Open, FileAccess.Read));
            String path = NUnit.Framework.TestContext.CurrentContext.TestDirectory + properties.GetProperty("PRIVATE");
            char[] pass = properties.GetProperty("PASSWORD").ToCharArray();
            string alias = null;
            Pkcs12Store pk12;

            pk12 = new Pkcs12Store(new FileStream(path, FileMode.Open, FileAccess.Read), pass);
            foreach (var a in pk12.Aliases)
            {
                alias = ((string)a);
                if (pk12.IsKeyEntry(alias))
                    break;
            }
            ICipherParameters pk = pk12.GetKey(alias).Key;
            X509CertificateEntry[] ce = pk12.GetCertificateChain(alias);
            X509Certificate[] chain = new X509Certificate[ce.Length];
            for (int k = 0; k < ce.Length; ++k)
                chain[k] = ce[k].Certificate;
			FileStream @is = new FileStream(CRLURL, FileMode.Open, FileAccess.Read);
			MemoryStream baos = new MemoryStream();
			byte[] buf = new byte[1024];
			while (@is.Read(buf, 0, buf.Length) != 0)
			{
                baos.Write(buf, 0, buf.Length);
			}
			ICrlClient crlClient = new CrlClientOffline(baos.ToArray());
			X509Crl crl = new X509CrlParser().ReadCrl(new FileStream(CRLURL, FileMode.Open, FileAccess.Read));
			System.Console.Out.WriteLine("CRL valid until: " + crl.NextUpdate);
			System.Console.Out.WriteLine("Certificate revoked: " + crl.IsRevoked(chain[0]));
			IList<ICrlClient> crlList = new List<ICrlClient>();
			crlList.Add(crlClient);
			C3_05_SignWithCRLOffline app = new C3_05_SignWithCRLOffline();
			app.Sign(SRC, DEST, chain, pk, DigestAlgorithms.SHA256, PdfSigner.CryptoStandard
				.CMS, "Test", "Ghent", crlList, null, null, 0);
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="System.Exception"/>
		/// <exception cref="Org.BouncyCastle.Security.GeneralSecurityException"/>
		[NUnit.Framework.Test]
		public override void RunTest()
        {
            Directory.CreateDirectory(NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/test/resources/signatures/chapter03/");
			C3_05_SignWithCRLOffline.Main(null);
			String[] resultFiles = new String[] { "hello_cacert_crl_offline.pdf" };
			String destPath = String.Format(outPath, "chapter03");
			String comparePath = String.Format(cmpPath, "chapter03");
			String[] errors = new String[resultFiles.Length];
			bool error = false;
            Dictionary<int, IList<Rectangle>> ignoredAreas = new Dictionary<int, IList<Rectangle>> { { 1, JavaUtil.ArraysAsList(new Rectangle(36, 648, 200, 100)) } };

			for (int i = 0; i < resultFiles.Length; i++)
			{
				String resultFile = resultFiles[i];
				String fileErrors = CheckForErrors(destPath + resultFile, comparePath + "cmp_" + 
					resultFile, destPath, ignoredAreas);
				if (fileErrors != null)
				{
					errors[i] = fileErrors;
					error = true;
				}
			}
			if (error)
			{
				NUnit.Framework.Assert.Fail(AccumulateErrors(errors));
			}
		}
	}
}
