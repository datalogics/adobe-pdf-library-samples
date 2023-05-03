using System;
using Datalogics.PDFL;

/*
 * 
 * This sample demonstrates converting the input PDF with the input Invoice XML to a Factur-X compliant PDF.
 *
 * For more detail see the description of the Factur-XConverter sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/converting-and-merging-pdf-content/#facturxconverter
 * 
 * Copyright (c) 2022-2023, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
namespace FacturXConverter
{
class FacturXConverter
    {
        // The input XML must be named this way for it to be compliant. If you replace this with an absolute path to the file, then that would be a violation of this conformity.
        const string sInputInoviceXML = "factur-x.xml";

        // The type of Associated File Relationship specified, it's Alternative in Germany, while Data or Source in France
        const string sRelationship = "Alternative";

        static void Main(string[] args)
        {
            Console.WriteLine("Factur-XConverter Sample:");


            // Initialize the Library
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                if (args.Length < 1)
                {
                    Console.WriteLine("You must specify an input PDF, e.g.:");
                    Console.WriteLine();
                    Console.WriteLine("Factur-XConverter input-file.pdf");

                    return;
                }

                String sInputPDF = args[0];

                String sOutput = "Factur-XConverter-out.pdf";

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

                            PDFDict rootObj = pdfaDoc.Root;
                            PDFArray associatedFilesArray = (PDFArray)rootObj.Get("AF");
                            PDFDict associatedFile = (PDFDict)associatedFilesArray.Get(0);

                            associatedFile.Put("AFRelationship", new PDFName(sRelationship, pdfaDoc, false));

                            //Step 4) Add the required XMP metadata entries
                            AddMetadataAndExtensionSchema(pdfaDoc);

                            //Step 5) Save the document
                            pdfaDoc.Save(pdfaResult.PDFASaveFlags, sOutput);
                        }
                    }
                }
            }
        }

        static void AddMetadataAndExtensionSchema(Document document)
        {
            string namespaceURI = "urn:factur-x:pdfa:CrossIndustryDocument:invoice:1p0#";
            string namespacePrefix = "fx";

            string pathDocumentType = "DocumentType";
            string pathDocumentTypeValue = "INVOICE";

            string pathDocumentFileName = "DocumentFileName";
            string pathDocumentFileNameValue = sInputInoviceXML;

            string pathConformanceLevel = "ConformanceLevel";
            string pathConformanceLevelValue = "BASIC";

            string pathVersion = "Version";
            string pathVersionValue = "1.0";

            //Set the XMP Factur-X properties
            document.SetXMPMetadataProperty(namespaceURI, namespacePrefix, pathDocumentType, pathDocumentTypeValue);
            document.SetXMPMetadataProperty(namespaceURI, namespacePrefix, pathDocumentFileName,
                pathDocumentFileNameValue);
            document.SetXMPMetadataProperty(namespaceURI, namespacePrefix, pathConformanceLevel,
                pathConformanceLevelValue);
            document.SetXMPMetadataProperty(namespaceURI, namespacePrefix, pathVersion, pathVersionValue);

            //Create the PDF/A Extension Schema for Factur-X since it's not part of the PDF/A standard.
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
                                                                + "<pdfaSchema:schema>Factur-X PDFA Extension Schema</pdfaSchema:schema>" +
                                                                Environment.NewLine
                                                                + "<pdfaSchema:namespaceURI>urn:factur-x:pdfa:CrossIndustryDocument:invoice:1p0#</pdfaSchema:namespaceURI>" +
                                                                Environment.NewLine
                                                                + "<pdfaSchema:prefix>fx</pdfaSchema:prefix>" +
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
