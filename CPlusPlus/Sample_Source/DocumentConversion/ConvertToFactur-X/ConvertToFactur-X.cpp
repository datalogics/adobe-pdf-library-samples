//
// Copyright (c) 2023, Datalogics, Inc. All rights reserved.
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// ConvertToFactur-X converts the input PDF with the input Invoice XML to a Factur-X (also known as ZUGFeRD v2.2) compliant PDF.
//
// Command-line:  <input-pdf> [<relationship>]
//
//        For example, you might enter a command line statement that looks like this:
//
//        ConvertToFactur-X input-file.pdf Alternative
//
//        This statement provides the name of an input file and optionally the relationship type.
//
// NOTE: The input Invoice XML file needs to be Factur-X-compliant, APDFL does not create this for you and
// does not ensure the XML is compliant either.
//
// For more detail see the description of the ConvertToFactur-X sample program on our Developer's site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#converttofacturx

#include <sstream>

#include "InitializeLibrary.h"
#include "APDFLDoc.h"
#include "CosCalls.h"
#include "PEExpT.h"
#include "PDMetadataCalls.h"
#include "PDFProcessorCalls.h"

#define DEF_OUTPUT "ConvertToFactur-X-out.pdf"

//The Input XML must be named this way for it to be compliant
#define DEF_INPUT_XML "factur-x.xml"

//The type of Associated File Relationship specified, it's Alternative in Germany, while Data or Source in France
#define DEF_RELATIONSHIP "Alternative"

//forward declarations
void SetupPDFAProcessorParams(PDFProcessorPDFAConvertParams userParams);
void AddInvoiceXMLAsAnAttachment(PDDoc doc, CosDoc cosDoc, ASFileSys fileSys, ASPathName xmlInvoiceFilePath, ASFile inputXMLInvoiceFile);
void AddMetadataAndExtensionSchema(PDDoc outDoc);

int main(int argc, char **argv)
{
    if (argc < 2)
    {
        std::cout << "You must specify an input PDF, e.g.:" << std::endl << std::endl
                  << "ConvertToFactur-X input-file.pdf" << std::endl;

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
    std::string csInputRelationshipSpecifciation(argc > 2 ? argv[2] : DEF_RELATIONSHIP);
    std::string csOutputFileName(DEF_OUTPUT);
    std::cout << "Will convert " << csInputFileName.c_str() << " with " << DEF_INPUT_XML << " to a Factur-X-compliant PDF named "
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
        xmlInvoiceFilePath = ASFileSysCreatePathName(NULL, ASAtomFromString("Cstring"), DEF_INPUT_XML, NULL);
#else
        destFilePath = APDFLDoc::makePath (csOutputFileName.c_str() ;
        xmlInvoiceFilePath = APDFLDoc::makePath(DEF_INPUT_XML);
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

            // Open output PDF/A document
            APDFLDoc outAPDoc(csOutputFileName.c_str(), false);
            PDDoc outDoc = outAPDoc.getPDDoc();

            CosObj rootObj = CosDocGetRoot(PDDocGetCosDoc(outDoc));
            CosObj associatedFilesArray = CosDictGet(rootObj, ASAtomFromString("AF"));
            CosObj associatedFile = CosArrayGet(associatedFilesArray, 0);

            // PDF/A-3 conversion will set the /AFRelationship key to Unspecified, Factur-X requires it be specific
            CosDictPut(associatedFile, ASAtomFromString("AFRelationship"), CosNewName(cosDoc, false, ASAtomFromString(csInputRelationshipSpecifciation.c_str())));

            //Step 5) Add the required XMP metadata entries
            AddMetadataAndExtensionSchema(outDoc);

            //Step 6) Save the document
            PDDocSave(outDoc, PDSaveFull, destFilePath, fileSys, NULL, NULL);

            if(result)
            {
                std::cout << "File " << csInputFileName.c_str() << " has been successfully Converted." << std::endl;
            }
            else
            {
                std::cout << "Conversion of file " << csInputFileName.c_str() << " has failed..." << std::endl;

                errCode = -1;
            }
        }
        else
        {
            std::cout << "The Factur-X input XML file factur-x.xml could not be found." << std::endl;

            errCode = -1;
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

            if (ASFileSysGetNameFromPathAsASText(fileSys, xmlInvoiceFilePath, fileNameText) == 0)
            {
                if (!ASTextIsEmpty(fileNameText))
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

void AddMetadataAndExtensionSchema(PDDoc outDoc)
{
    ASText asNamespaceURI = ASTextFromPDText("urn:factur-x:pdfa:CrossIndustryDocument:invoice:1p0#");
    ASText asNamespacePrefix = ASTextFromPDText("fx");

    ASText asPathDocumentType = ASTextFromPDText("DocumentType");
    ASText asPathDocumentTypeValue = ASTextFromPDText("INVOICE");

    ASText asPathDocumentFileName = ASTextFromPDText("DocumentFileName");
    ASText asPathDocumentFileNameValue = ASTextFromPDText(DEF_INPUT_XML);

    ASText asPathConformanceLevel = ASTextFromPDText("ConformanceLevel");
    ASText asPathConformanceLevelValue = ASTextFromPDText("BASIC");

    ASText asPathVersion = ASTextFromPDText("Version");
    ASText asPathVersionValue = ASTextFromPDText("1.0");

    //Set the XMP Factur-X properties
    PDDocSetXAPMetadataProperty(outDoc, asNamespaceURI, asNamespacePrefix, asPathDocumentType, asPathDocumentTypeValue);
    PDDocSetXAPMetadataProperty(outDoc, asNamespaceURI, asNamespacePrefix, asPathDocumentFileName, asPathDocumentFileNameValue);
    PDDocSetXAPMetadataProperty(outDoc, asNamespaceURI, asNamespacePrefix, asPathConformanceLevel, asPathConformanceLevelValue);
    PDDocSetXAPMetadataProperty(outDoc, asNamespaceURI, asNamespacePrefix, asPathVersion, asPathVersionValue);

    //Create the PDF/A Extension Schema for Factur-X since it's not part of the PDF/A standard.
    std::stringstream extensionSchema;
        extensionSchema << "<rdf:Description rdf:about=\"\"" << std::endl
        << "xmlns:pdfaExtension=\"http://www.aiim.org/pdfa/ns/extension/\"" << std::endl
        << "xmlns:pdfaSchema=\"http://www.aiim.org/pdfa/ns/schema#\"" << std::endl
        << "xmlns:pdfaProperty=\"http://www.aiim.org/pdfa/ns/property#\">" << std::endl
        << "<pdfaExtension:schemas>" << std::endl
        << "<rdf:Bag>" << std::endl
        << "<rdf:li rdf:parseType=\"Resource\">" << std::endl
        << "<pdfaSchema:schema>Factur-X PDFA Extension Schema</pdfaSchema:schema>" << std::endl
        << "<pdfaSchema:namespaceURI>urn:factur-x:pdfa:CrossIndustryDocument:invoice:1p0#</pdfaSchema:namespaceURI>" << std::endl
        << "<pdfaSchema:prefix>fx</pdfaSchema:prefix>" << std::endl
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
