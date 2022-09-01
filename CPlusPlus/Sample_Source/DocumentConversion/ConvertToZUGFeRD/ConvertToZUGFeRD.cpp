//
// Copyright (c) 2019-2021, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// ConvertToZUGFeRD converts the input PDF with the input Invoice ZUGFeRD XML to a ZUGFeRD compliant PDF.
//
// Command-line:  <input-pdf> <input-ZUGFeRD-invoice-xml>
//
//        For example, you might enter a command line statement that looks like this:
//
//        ConvertToZUGFeRD input-file.pdf input-ZUGFeRD-invoice.xml
//
//        This statement provides the name of an input file and the name of an input ZUGFeRD invoice XML file.
//
// NOTE: The input Invoice ZUGFeRD XML file needs to be ZUGFeRD-compliant, PDFL does not create this for you and
// does not ensure the XML is compliant either.
//
// For more detail see the description of the ConvertToZUGFeRD sample program on our Developer’s site, 
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#converttozugferd

#include <sstream>

#include "InitializeLibrary.h"
#include "APDFLDoc.h"
#include "CosCalls.h"
#include "PEExpT.h"
#include "PDMetadataCalls.h"
#include "PDFProcessorCalls.h"

#define DEF_OUTPUT "ConvertToZUGFeRD-out.pdf"

//forward declarations
void SetupPDFAProcessorParams(PDFProcessorPDFAConvertParams userParams);
void AddInvoiceXMLAsAnAttachment(PDDoc doc, CosDoc cosDoc, ASFileSys fileSys, ASPathName xmlInvoiceFilePath, ASFile inputXMLInvoiceFile);
void AddMetadataAndExtensionSchema(PDDoc outDoc, char* invoiceXML);

int main(int argc, char **argv)
{
    if (argc < 2)
    {
        std::cout << "You must specify an input PDF and an input ZUGFeRD Invoice XML file, e.g.:" << std::endl << std::endl
                  << "ConvertToZUGFeRD input-file.pdf input-ZUGFeRD-invoice.xml" << std::endl;

        return -1;
    }

    char *outputPathName = DEF_OUTPUT;

    APDFLib lib;
    ASErrorCode errCode = 0;
    if (lib.isValid() == false)
    {
        errCode = lib.getInitError();
        std::cout << "Initialization failed with code " << errCode << std::endl;
        return lib.getInitError();
    }

    std::string csInputFileName(argv[1]);
    std::string csInputInvoiceXMLFileName(argv[2]);
    std::string csOutputFileName(DEF_OUTPUT);
    std::cout << "Will convert " << csInputFileName.c_str() << " with " << csInputInvoiceXMLFileName.c_str() << " to a ZUGFeRD-compliant PDF named "
              << csOutputFileName.c_str() << std::endl;

DURING
    gPDFProcessorHFT = InitPDFProcessorHFT;

    //Initialize PDFProcessor plugin
    if(PDFProcessorInitialize())
    {
        ASInt32 result = 0;

        ASPathName destFilePath = NULL;
        ASPathName xmlInvoiceFilePath = NULL;
        ASFileSys fileSys = ASGetDefaultFileSys();

        // Step 1) Open the input PDF
        APDFLDoc inAPDoc(csInputFileName.c_str(), true);  // Open the input document, repairing it if necessary.

        PDDoc doc = inAPDoc.getPDDoc();
        CosDoc cosDoc = PDDocGetCosDoc(doc);

#if !MAC_ENV
        destFilePath = ASFileSysCreatePathName(NULL, ASAtomFromString("Cstring"), csOutputFileName.c_str(), NULL);
        xmlInvoiceFilePath = ASFileSysCreatePathName(NULL, ASAtomFromString("Cstring"), csInputInvoiceXMLFileName.c_str(), NULL);
#else
        destFilePath = APDFLDoc::makePath (csOutputFileName.c_str() ;
        xmlInvoiceFilePath = APDFLDoc::makePath(csInputInvoiceXMLFileName.c_str());
#endif

        ASFile inputXMLInvoiceFile = NULL;

        // Step 2) Open the input Invoice XML
        if (ASFileSysOpenFile(fileSys, xmlInvoiceFilePath, ASFILE_READ, &inputXMLInvoiceFile) == 0)
        {
            // Step 3) Attach the input Invoice XML to the PDF
            AddInvoiceXMLAsAnAttachment(doc, cosDoc, fileSys, xmlInvoiceFilePath, inputXMLInvoiceFile);

            PDFProcessorPDFAConvertParamsRec userParamsA;
            SetupPDFAProcessorParams(&userParamsA);

            // Step 4) Convert the input PDF to be a PDF/A-3 document.
            result = PDFProcessorConvertAndSaveToPDFA(doc, destFilePath, fileSys, kPDFProcessorConvertToPDFA3bRGB, &userParamsA);

            //Open output PDF/A document
            APDFLDoc outAPDoc(csOutputFileName.c_str(), false);
            PDDoc outDoc = outAPDoc.getPDDoc();

            //Step 5) Add the required XMP metadata entries
            AddMetadataAndExtensionSchema(outDoc, argv[2]);

            //Step 6) Save the document
            PDDocSave(outDoc, PDSaveFull, destFilePath, fileSys, NULL, NULL);

            if(result)
            {
                std::cout << "File " << csInputFileName.c_str() << " has been successfully Converted." << std::endl;
            }
            else
            {
                std::cout << "Conversion of file " << csInputFileName.c_str() << " has failed..." << std::endl;
            }
        }

        // Cleanup resources
        if (destFilePath)
        {
            ASFileSysReleasePath(fileSys, destFilePath);
        }

        if (xmlInvoiceFilePath)
        {
            ASFileSysReleasePath(fileSys, xmlInvoiceFilePath);
        }

        ASFileClose(inputXMLInvoiceFile);
    }

    //Terminate PDFProcessor plugin
    PDFProcessorTerminate();
HANDLER
    errCode = ERRORCODE;
    lib.displayError(errCode);

    //Terminate PDFProcessor plugin
    PDFProcessorTerminate();
END_HANDLER

    return errCode;// APDFLib's destructor terminates APDFL
}

void AddInvoiceXMLAsAnAttachment(PDDoc doc, CosDoc cosDoc, ASFileSys fileSys, ASPathName xmlInvoiceFilePath, ASFile inputXMLInvoiceFile)
{
    const ASAtom filterNames[1] = { ASAtomFromString("FlateDecode") };

    //Create a File Attachment from the input Invoice XML
    PDFileAttachment attachmentInvoiceXML = PDFileAttachmentNewFromFile(cosDoc, inputXMLInvoiceFile, filterNames, 1, CosNewNull(), NULL, NULL, NULL);
    CosObj attachmentInvoiceObj = PDFileAttachmentGetCosObj(attachmentInvoiceXML);

    if (CosObjGetType(attachmentInvoiceObj) != CosNull)
    {
        ASAtom embeddedFilesAtom = ASAtomFromString("EmbeddedFiles");
        PDNameTree nameTree = PDDocGetNameTree(doc, embeddedFilesAtom);

        //If there isn't a Name Tree, add one.
        if (!PDNameTreeIsValid(nameTree))
        {
            nameTree = PDDocCreateNameTree(doc, embeddedFilesAtom);
        }

        if (PDNameTreeIsValid(nameTree))
        {
            ASText fileNameText = ASTextNew();

            if (0 == ASFileSysGetNameFromPathAsASText(fileSys, xmlInvoiceFilePath, fileNameText))
            {
                if (fileNameText && !ASTextIsEmpty(fileNameText))
                {
                    ASTArraySize nameLen = 0;
                    char *fileNameString = ASTextGetPDTextCopy(fileNameText, &nameLen);

                    if (fileNameString && nameLen)
                    {
                        //Create a key for the NameTree.
                        CosObj key = CosNewString(cosDoc, false, fileNameString, nameLen);

                        //Add the Attachment using the created key to the Name Tree.
                        PDNameTreePut(nameTree, key, attachmentInvoiceObj);
                    }
                }
            }

            //Cleanup resource
            ASTextDestroy(fileNameText);
        }
    }
}

void AddMetadataAndExtensionSchema(PDDoc outDoc, char* invoiceXML)
{
    ASText asNamespaceURI = ASTextFromPDText("urn:ferd:pdfa:CrossIndustryDocument:invoice:2p0#");
    ASText asNamespacePrefix = ASTextFromPDText("zf");

    ASText asPathDocumentType = ASTextFromPDText("DocumentType");
    ASText asPathDocumentTypeValue = ASTextFromPDText("INVOICE");

    ASText asPathDocumentFileName = ASTextFromPDText("DocumentFileName");
    ASText asPathDocumentFileNameValue = ASTextFromPDText(invoiceXML);

    ASText asPathConformanceLevel = ASTextFromPDText("ConformanceLevel");
    ASText asPathConformanceLevelValue = ASTextFromPDText("BASIC");

    ASText asPathVersion = ASTextFromPDText("Version");
    ASText asPathVersionValue = ASTextFromPDText("2p0");

    //Set the XMP ZUGFeRD properties
    PDDocSetXAPMetadataProperty(outDoc, asNamespaceURI, asNamespacePrefix, asPathDocumentType, asPathDocumentTypeValue);
    PDDocSetXAPMetadataProperty(outDoc, asNamespaceURI, asNamespacePrefix, asPathDocumentFileName, asPathDocumentFileNameValue);
    PDDocSetXAPMetadataProperty(outDoc, asNamespaceURI, asNamespacePrefix, asPathConformanceLevel, asPathConformanceLevelValue);
    PDDocSetXAPMetadataProperty(outDoc, asNamespaceURI, asNamespacePrefix, asPathVersion, asPathVersionValue);

    //Create the PDF/A Extension Schema for ZUGFeRD since it's not part of the PDF/A standard.
    std::stringstream extensionSchema;
        extensionSchema << "<rdf:Description rdf:about=\"\"" << std::endl
        << "xmlns:pdfaExtension=\"http://www.aiim.org/pdfa/ns/extension/\"" << std::endl
        << "xmlns:pdfaSchema=\"http://www.aiim.org/pdfa/ns/schema#\"" << std::endl
        << "xmlns:pdfaProperty=\"http://www.aiim.org/pdfa/ns/property#\">" << std::endl
        << "<pdfaExtension:schemas>" << std::endl
        << "<rdf:Bag>" << std::endl
        << "<rdf:li rdf:parseType=\"Resource\">" << std::endl
        << "<pdfaSchema:schema>ZUGFeRD PDFA Extension Schema</pdfaSchema:schema>" << std::endl
        << "<pdfaSchema:namespaceURI>urn:ferd:pdfa:CrossIndustryDocument:invoice:2p0#</pdfaSchema:namespaceURI>" << std::endl
        << "<pdfaSchema:prefix>zf</pdfaSchema:prefix>" << std::endl
        << "<pdfaSchema:property>" << std::endl
        << "<rdf:Seq>" << std::endl
        << "<rdf:li rdf:parseType=\"Resource\">" << std::endl
        << "<pdfaProperty:name>DocumentFileName</pdfaProperty:name>" << std::endl
        << "<pdfaProperty:valueType>Text</pdfaProperty:valueType>" << std::endl
        << "<pdfaProperty:category>external</pdfaProperty:category>" << std::endl
        << "<pdfaProperty:description>name of the embedded XML invoice file</pdfaProperty:description>" << std::endl
        << "</rdf:li>" << std::endl
        << "<rdf:li rdf:parseType=\"Resource\">" << std::endl
        << "<pdfaProperty:name>DocumentType</pdfaProperty:name>" << std::endl
        << "<pdfaProperty:valueType>Text</pdfaProperty:valueType>" << std::endl
        << "<pdfaProperty:category>external</pdfaProperty:category>" << std::endl
        << "<pdfaProperty:description>INVOICE</pdfaProperty:description>" << std::endl
        << "</rdf:li>" << std::endl
        << "<rdf:li rdf:parseType=\"Resource\">" << std::endl
        << "<pdfaProperty:name>Version</pdfaProperty:name>" << std::endl
        << "<pdfaProperty:valueType>Text</pdfaProperty:valueType>" << std::endl
        << "<pdfaProperty:category>external</pdfaProperty:category>" << std::endl
        << "<pdfaProperty:description>The actual version of the ZUGFeRD XML schema</pdfaProperty:description>" << std::endl
        << "</rdf:li>" << std::endl
        << "<rdf:li rdf:parseType=\"Resource\">" << std::endl
        << "<pdfaProperty:name>ConformanceLevel</pdfaProperty:name>" << std::endl
        << "<pdfaProperty:valueType>Text</pdfaProperty:valueType>" << std::endl
        << "<pdfaProperty:category>external</pdfaProperty:category>" << std::endl
        << "<pdfaProperty:description>The conformance level of the embedded ZUGFeRD data</pdfaProperty:description>" << std::endl
        << "</rdf:li>" << std::endl
        << "</rdf:Seq>" << std::endl
        << "</pdfaSchema:property>" << std::endl
        << "</rdf:li>" << std::endl
        << "</rdf:Bag>" << std::endl
        << "</pdfaExtension:schemas>" << std::endl
        << "</rdf:Description>" << std::endl
        << "</rdf:RDF>" << std::endl;

    ASText metadataXMP = PDDocGetXAPMetadata(outDoc);

    ASText rdfEnding = ASTextFromPDText("</rdf:RDF>");
    ASText extensionSchemaText = ASTextFromPDText(extensionSchema.str().c_str());

    //We're going to look for the ending of our typical XMP Metadata to replace it with our Extension Schema
    ASTextReplace(metadataXMP, rdfEnding, extensionSchemaText);

    //Update with our new XMP Metadata
    PDDocSetXAPMetadata(outDoc, metadataXMP);

    //Cleanup resources
    ASTextDestroy(rdfEnding);

    ASTextDestroy(asNamespaceURI);
    ASTextDestroy(asNamespacePrefix);

    ASTextDestroy(asPathDocumentType);
    ASTextDestroy(asPathDocumentTypeValue);

    ASTextDestroy(asPathDocumentFileName);
    ASTextDestroy(asPathDocumentFileNameValue);

    ASTextDestroy(asPathConformanceLevel);
    ASTextDestroy(asPathConformanceLevelValue);

    ASTextDestroy(asPathVersion);
    ASTextDestroy(asPathVersionValue);
}

void SetupPDFAProcessorParams(PDFProcessorPDFAConvertParams userParams)
{
    memset (userParams, 0, sizeof (PDFProcessorPDFAConvertParamsRec));
    userParams->size = sizeof(PDFProcessorPDFAConvertParamsRec);
    userParams->progMon = NULL;
    userParams->noRasterizationOnFontErrors = false;
    userParams->removeAllAnnotations = false;
    userParams->colorCompression = kPDFProcessorColorJpegCompression;
    userParams->noValidationErrors = false;
    userParams->validateImplementationLimitsOfDocument = true;
}
