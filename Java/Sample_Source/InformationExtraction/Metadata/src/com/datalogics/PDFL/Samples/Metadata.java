package com.datalogics.pdfl.samples.InformationExtraction.Metadata;

import com.datalogics.PDFL.Container;
import com.datalogics.PDFL.Content;
import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.Image;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.SaveFlags;
import java.io.ByteArrayInputStream;
import java.util.EnumSet;
import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import org.w3c.dom.Element;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;

/*
 * 
 * This sample shows how to view and edit metadata for a PDF document. The metadata values appear on the Properties
 * window in Adobe Reader and Adobe Acrobat. Click File/Properties, and then click Additional Metadata.
 *
 * For more detail see the description of the Metadata sample on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/listing-information-about-values-and-objects-in-pdf-files#metadata
 * 
 *
 * Copyright (c) 2008-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
public class Metadata {

    /**
     * @param args the command line arguments
     */
    public static void main(String[] args) throws Throwable {
        Library lib = new Library();
        try {
            String sInput1 = Library.getResourceDirectory() + "Sample_Input/sample.pdf";
            String sInput2 = Library.getResourceDirectory() + "Sample_Input/Ducky_with_metadata.pdf";
            String sOutput1 = "Metadata-out.pdf";
            if ( args.length > 0 )
                sInput1 = args[0];
            if ( args.length > 1 )
                sInput2 = args[1];
            if ( args.length > 2 )
                sOutput1 = args[2];
            displayDocumentMetadata(sInput1, sOutput1);
            displayImageMetadata(sInput2);
        } finally {
            lib.delete(); // close library and clean up everything
        }
    }

    private static void displayDocumentMetadata(String input, String output) throws Throwable {
        Document doc = new Document(input);
        try {
            // Parse some data out of the document metadata string
            String metadata = doc.getXMPMetadata();
            System.out.format("Title: %s\n", getTitle(metadata));
            System.out.format("CreatorTool: %s\n", doc.getXMPMetadataProperty("http://ns.adobe.com/xap/1.0/", "CreatorTool"));
            System.out.format("format: %s\n", doc.getXMPMetadataProperty("http://purl.org/dc/elements/1.1/", "format"));
            int numAuthors = doc.countXMPMetadataArrayItems("http://ns.adobe.com/xap/1.0/", "Authors");
            System.out.format("Number of authors: %d\n", numAuthors);
            for (int i = 1; i <= numAuthors; i++) {
                System.out.format("Author: %s\n", doc.getXMPMetadataArrayItem("http://ns.adobe.com/xap/1.0/", "Authors", i));
            }
            // Demonstrate setting a property
            doc.setXMPMetadataArrayItem("http://ns.adobe.com/xap/1.0/", "tetractys", "Authors", 2, "Metadata Tester");
            doc.save(EnumSet.of(SaveFlags.FULL), output);
        } finally {
            doc.delete();
        }
    }

    private static void displayImageMetadata(String input) throws Throwable {
        Document doc = new Document(input);
        try {
            // Demonstrate getting data from an image
            Content content = doc.getPage(0).getContent();
            Container container = (Container) content.getElement(0);
            Image image = (Image) container.getContent().getElement(0);
            String metadata = image.getStream().getDict().getXMPMetadata();
            System.out.format("Ducky CreatorTool: %s\n", getCreatorToolAttribute(metadata));
        } finally {
            doc.delete();
        }
    }

    static String getTitle(String xmlstring) throws Throwable {
        DocumentBuilder builder = DocumentBuilderFactory.newInstance().newDocumentBuilder();
        org.w3c.dom.Document xmldoc = builder.parse(new ByteArrayInputStream(xmlstring.getBytes("UTF-8")));
        Element element = (Element) xmldoc.getElementsByTagName("dc:title").item(0);
        Node titlenode = element.getElementsByTagName("rdf:li").item(0);
        return getText(titlenode.getChildNodes());
    }

    static String getCreatorTool(String xmlstring) throws Throwable {
        DocumentBuilder builder = DocumentBuilderFactory.newInstance().newDocumentBuilder();
        org.w3c.dom.Document xmldoc = builder.parse(new ByteArrayInputStream(xmlstring.getBytes("UTF-8")));
        Element element = (Element) xmldoc.getElementsByTagName("xap:CreatorTool").item(0);
        return getText(element.getChildNodes());

    }

    static String getCreatorToolAttribute(String xmlstring) throws Throwable {
        DocumentBuilder builder = DocumentBuilderFactory.newInstance().newDocumentBuilder();
        org.w3c.dom.Document xmldoc = builder.parse(new ByteArrayInputStream(xmlstring.getBytes("UTF-8")));
        NodeList descriptions = xmldoc.getElementsByTagName("rdf:Description");
        for (int i = 0; i < descriptions.getLength(); i++) {
            Element e = (Element) descriptions.item(i);
            if (e.hasAttribute("xap:CreatorTool")) {
                return e.getAttribute("xap:CreatorTool");
            }
        }

        return null;
    }

    static String getText(NodeList nodelist) {
        StringBuffer rc = new StringBuffer();

        for (int i = 0; i < nodelist.getLength(); i++) {
            Node node = nodelist.item(i);
            if (node.getNodeType() == Node.TEXT_NODE) {
                rc.append(node.getNodeValue());
            }
        }

        return rc.toString();
    }
}
