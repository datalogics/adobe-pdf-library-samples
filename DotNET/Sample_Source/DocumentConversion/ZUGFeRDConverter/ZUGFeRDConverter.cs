using System;
using Datalogics.PDFL;

/*
 * 
 * This sample demonstrates converting the input PDF with the input Invoice ZUGFeRD XML to a ZUGFeRD compliant PDF.
 *
 * For more detail see the description of the ZUGFeRDConverter sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/converting-and-merging-pdf-content/#zugferdconverter
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
namespace ZUGFeRDConverter
{
    class ZUGFeRDConverter
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ZUGFeRDConverter Sample:");

            //Initialize the Library

           // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                if (args.Length < 2)
                {
                    Console.WriteLine("You must specify an input PDF and an input ZUGFeRD Invoice XML file, e.g.:");
                    Console.WriteLine();
                    Console.WriteLine("ConvertToZUGFeRD input-file.pdf input-ZUGFeRD-invoice.xml");

                    return;
                }

                String sInputPDF = args[0];
                String sInputInoviceXML = args[1];
                String sOutput = "ZUGFeRDConverter-out.pdf";

                Console.WriteLine("Converting " + sInputPDF + " with " + sInputInoviceXML + ", output file is " +
                                  sOutput);

                // Step 1) Open the input PDF
                using (Document doc = new Document(sInputPDF))
                {
                    // Step 2) Open the input Invoice XML and attach it to the PDF
                    using (new FileAttachment(doc, sInputInoviceXML))
                    {
                        // Make a conversion parameters object
                        PDFAConvertParams pdfaParams = new PDFAConvertParams();
                        pdfaParams.IgnoreFontErrors = false;
                        pdfaParams.NoValidationErrors = false;
                        pdfaParams.ValidateImplementationLimitsOfDocument = true;

                        // Step 3) Convert the input PDF to be a PDF/A-3 document.
                        PDFAConvertResult pdfaResult = doc.CloneAsPDFADocument(PDFAConvertType.RGB3b, pdfaParams);

                        // The conversion may have failed: we must check if the result has a valid Document
                        if (pdfaResult.PDFADocument == null)
                        {
                            Console.WriteLine("ERROR: Could not convert " + sInputPDF + " to PDF/A.");
                        }
                        else
                        {
                            Console.WriteLine("Successfully converted " + sInputPDF + " to PDF/A.");

                            Document pdfaDoc = pdfaResult.PDFADocument;

                            //Step 4) Add the required XMP metadata entries
                            AddMetadataAndExtensionSchema(pdfaDoc, sInputInoviceXML);

                            //Step 5) Save the document
                            pdfaDoc.Save(pdfaResult.PDFASaveFlags, sOutput);
                        }
                    }
                }
            }
        }

        static void AddMetadataAndExtensionSchema(Document document, string sInputInoviceXML)
        {
            string namespaceURI = "urn:ferd:pdfa:CrossIndustryDocument:invoice:2p0#";
            string namespacePrefix = "zf";

            string pathDocumentType = "DocumentType";
            string pathDocumentTypeValue = "INVOICE";

            string pathDocumentFileName = "DocumentFileName";
            string pathDocumentFileNameValue = sInputInoviceXML;

            string pathConformanceLevel = "ConformanceLevel";
            string pathConformanceLevelValue = "BASIC";

            string pathVersion = "Version";
            string pathVersionValue = "2p0";

            //Set the XMP ZUGFeRD properties
            document.SetXMPMetadataProperty(namespaceURI, namespacePrefix, pathDocumentType, pathDocumentTypeValue);
            document.SetXMPMetadataProperty(namespaceURI, namespacePrefix, pathDocumentFileName,
                pathDocumentFileNameValue);
            document.SetXMPMetadataProperty(namespaceURI, namespacePrefix, pathConformanceLevel,
                pathConformanceLevelValue);
            document.SetXMPMetadataProperty(namespaceURI, namespacePrefix, pathVersion, pathVersionValue);

            //Create the PDF/A Extension Schema for ZUGFeRD since it's not part of the PDF/A standard.
            string extensionSchema;
            extensionSchema = "<rdf:Description rdf:about=\"\"" + Environment.NewLine
                                                                + "xmlns:pdfaExtension=\"http://www.aiim.org/pdfa/ns/extension/\"" +
                                                                Environment.NewLine
                                                                + "xmlns:pdfaSchema=\"http://www.aiim.org/pdfa/ns/schema#\"" +
                                                                Environment.NewLine
                                                                + "xmlns:pdfaProperty=\"http://www.aiim.org/pdfa/ns/property#\">" +
                                                                Environment.NewLine
                                                                + "<pdfaExtension:schemas>" + Environment.NewLine
                                                                + "<rdf:Bag>" + Environment.NewLine
                                                                + "<rdf:li rdf:parseType=\"Resource\">" +
                                                                Environment.NewLine
                                                                + "<pdfaSchema:schema>ZUGFeRD PDFA Extension Schema</pdfaSchema:schema>" +
                                                                Environment.NewLine
                                                                + "<pdfaSchema:namespaceURI>urn:ferd:pdfa:CrossIndustryDocument:invoice:2p0#</pdfaSchema:namespaceURI>" +
                                                                Environment.NewLine
                                                                + "<pdfaSchema:prefix>zf</pdfaSchema:prefix>" +
                                                                Environment.NewLine
                                                                + "<pdfaSchema:property>" + Environment.NewLine
                                                                + "<rdf:Seq>" + Environment.NewLine
                                                                + "<rdf:li rdf:parseType=\"Resource\">" +
                                                                Environment.NewLine
                                                                + "<pdfaProperty:name>DocumentFileName</pdfaProperty:name>" +
                                                                Environment.NewLine
                                                                + "<pdfaProperty:valueType>Text</pdfaProperty:valueType>" +
                                                                Environment.NewLine
                                                                + "<pdfaProperty:category>external</pdfaProperty:category>" +
                                                                Environment.NewLine
                                                                + "<pdfaProperty:description>name of the embedded XML invoice file</pdfaProperty:description>" +
                                                                Environment.NewLine
                                                                + "</rdf:li>" + Environment.NewLine
                                                                + "<rdf:li rdf:parseType=\"Resource\">" +
                                                                Environment.NewLine
                                                                + "<pdfaProperty:name>DocumentType</pdfaProperty:name>" +
                                                                Environment.NewLine
                                                                + "<pdfaProperty:valueType>Text</pdfaProperty:valueType>" +
                                                                Environment.NewLine
                                                                + "<pdfaProperty:category>external</pdfaProperty:category>" +
                                                                Environment.NewLine
                                                                + "<pdfaProperty:description>INVOICE</pdfaProperty:description>" +
                                                                Environment.NewLine
                                                                + "</rdf:li>" + Environment.NewLine
                                                                + "<rdf:li rdf:parseType=\"Resource\">" +
                                                                Environment.NewLine
                                                                + "<pdfaProperty:name>Version</pdfaProperty:name>" +
                                                                Environment.NewLine
                                                                + "<pdfaProperty:valueType>Text</pdfaProperty:valueType>" +
                                                                Environment.NewLine
                                                                + "<pdfaProperty:category>external</pdfaProperty:category>" +
                                                                Environment.NewLine
                                                                + "<pdfaProperty:description>The actual version of the ZUGFeRD XML schema</pdfaProperty:description>" +
                                                                Environment.NewLine
                                                                + "</rdf:li>" + Environment.NewLine
                                                                + "<rdf:li rdf:parseType=\"Resource\">" +
                                                                Environment.NewLine
                                                                + "<pdfaProperty:name>ConformanceLevel</pdfaProperty:name>" +
                                                                Environment.NewLine
                                                                + "<pdfaProperty:valueType>Text</pdfaProperty:valueType>" +
                                                                Environment.NewLine
                                                                + "<pdfaProperty:category>external</pdfaProperty:category>" +
                                                                Environment.NewLine
                                                                + "<pdfaProperty:description>The conformance level of the embedded ZUGFeRD data</pdfaProperty:description>" +
                                                                Environment.NewLine
                                                                + "</rdf:li>" + Environment.NewLine
                                                                + "</rdf:Seq>" + Environment.NewLine
                                                                + "</pdfaSchema:property>" + Environment.NewLine
                                                                + "</rdf:li>" + Environment.NewLine
                                                                + "</rdf:Bag>" + Environment.NewLine
                                                                + "</pdfaExtension:schemas>" + Environment.NewLine
                                                                + "</rdf:Description>" + Environment.NewLine
                                                                + "</rdf:RDF>" + Environment.NewLine;

            string xmpMetadata = document.XMPMetadata;

            //We're going to look for the ending of our typical XMP Metadata to replace it with our Extension Schema
            string newXMPMetadata = xmpMetadata.Replace("</rdf:RDF>", extensionSchema);

            //Update with our new XMP Metadata
            document.XMPMetadata = newXMPMetadata;
        }
    }
}
