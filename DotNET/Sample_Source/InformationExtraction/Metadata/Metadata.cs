using System;
using System.Text;
using Datalogics.PDFL;
using System.Xml;

/*
 *
 * This sample shows how to view and edit metadata for a PDF document. The metadata values appear on the Properties
 * window in Adobe Reader and Adobe Acrobat. Click File/Properties, and then click Additional Metadata.
 *
 * For more detail see the description of the Metadata sample on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/listing-information-about-values-and-objects-in-pdf-files#metadata
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace Metadata
{
    class Metadata
    {
        static void Main(string[] args)
        {

            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                String sInput1 = Library.ResourceDirectory + "Sample_Input/sample.pdf";
                String sInput2 = Library.ResourceDirectory + "Sample_Input/Ducky_with_metadata.pdf";
                String sOutput = "sample-metadata-out.pdf";

                if (args.Length > 0)
                    sInput1 = args[0];

                if (args.Length > 1)
                    sInput2 = args[1];

                if (args.Length > 2)
                    sOutput = args[2];

                Console.WriteLine("Input files " + sInput1 + " and  " + sInput2 + ". Writing to output file " +
                                  sOutput);

                DisplayDocumentMetadata(sInput1, sOutput);
                DisplayImageMetadata(sInput2);
            }
        }

        private static void DisplayDocumentMetadata(String input, String output)
        {
            using (Document doc = new Document(input))
            {
                // Parse some data out of the document metadata string
                string metadata = doc.XMPMetadata;
                Console.WriteLine("Title: {0}", GetTitle(metadata));
                Console.WriteLine("CreatorTool: {0}",
                    doc.GetXMPMetadataProperty("http://ns.adobe.com/xap/1.0/", "CreatorTool"));
                Console.WriteLine("format: {0}",
                    doc.GetXMPMetadataProperty("http://purl.org/dc/elements/1.1/", "format"));
                int numAuthors = doc.CountXMPMetadataArrayItems("http://ns.adobe.com/xap/1.0/", "Authors");
                Console.WriteLine("Number of authors: {0}", numAuthors);
                for (int i = 1; i <= numAuthors; i++)
                {
                    Console.WriteLine("Author: {0}",
                        doc.GetXMPMetadataArrayItem("http://ns.adobe.com/xap/1.0/", "Authors", i));
                }

                // Demonstrate setting a property
                doc.SetXMPMetadataArrayItem("http://ns.adobe.com/xap/1.0/", "tetractys", "Authors", 2,
                    "Metadata Tester");
                doc.Save(SaveFlags.Full, output);
            }
        }

        private static void DisplayImageMetadata(String input)
        {
            using (Document doc = new Document(input))
            {
                // Demonstrate getting data from an image
                Content content = doc.GetPage(0).Content;
                Container container = (Container) content.GetElement(0);
                Datalogics.PDFL.Image image = (Datalogics.PDFL.Image) container.Content.GetElement(0);
                String metadata = image.Stream.Dict.XMPMetadata;
                Console.WriteLine("Ducky CreatorTool: {0}\n", GetCreatorToolAttribute(metadata));
            }
        }

        static string GetTitle(string xmlstring)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(xmlstring);
            XmlElement element = (XmlElement) xmldoc.GetElementsByTagName("dc:title")[0];
            XmlNode titleNode = element.GetElementsByTagName("rdf:li")[0];
            return GetText(titleNode.ChildNodes);
        }

        // ReSharper disable once UnusedMember.Local
        static string GetCreatorTool(string xmlstring)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(xmlstring);
            XmlElement element = (XmlElement) xmldoc.GetElementsByTagName("xap:CreatorTool")[0];
            return GetText(element.ChildNodes);
        }

        static string GetCreatorToolAttribute(string xmlstring)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(xmlstring);
            foreach (XmlNode node in xmldoc.GetElementsByTagName("rdf:Description"))
            {
                XmlElement e = (XmlElement) node;
                if (e.HasAttribute("xap:CreatorTool"))
                    return e.GetAttribute("xap:CreatorTool");
            }

            return null;
        }

        static string GetText(XmlNodeList nodeList)
        {
            StringBuilder sb = new StringBuilder();

            foreach (XmlNode node in nodeList)
            {
                if (node.NodeType == XmlNodeType.Text)
                    sb.Append(node.Value);
            }

            return sb.ToString();
        }
    }
}
