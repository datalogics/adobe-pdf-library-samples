package com.datalogics.PDFL.Samples;

import com.datalogics.PDFL.Action;
import com.datalogics.PDFL.AddPageFlags;
import com.datalogics.PDFL.Annotation;
import com.datalogics.PDFL.AnnotationFlags;
import com.datalogics.PDFL.Content;
import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.Form;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.PDFArray;
import com.datalogics.PDFL.PDFDict;
import com.datalogics.PDFL.PDFInteger;
import com.datalogics.PDFL.PDFName;
import com.datalogics.PDFL.PDFStream;
import com.datalogics.PDFL.PDFString;
import com.datalogics.PDFL.Page;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.SaveFlags;
import java.io.FileInputStream;
import java.util.EnumSet;

/*
 *
 * The sample program shows how to add an annotation to a PDF file that features a flash video embedded under an image.
 * 
 * For more detail see the description of the FlashAnnotCreate sample program on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/working-with-annotations-in-pdf-files#flashannotcreate
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
public class FlashAnnotCreate {

    public static void main(String[] args) throws java.io.IOException{
        // Names of the flash-format file to embed & output file to create
        String flashFileName = "../../Resources/Sample_Input/JobReady-info.swf";
        String outputFileName = "FlashAnnotCreate-out.pdf";

        // Name of the PDF file to use for the annotation appearance - the
        // first page of the PDF is imported
        String appearancePDFName = "../../Resources/Sample_Input/cottage.pdf";
        Library lib = new Library();

        try {
            if (args.length > 0)
                flashFileName = args[0];
            if (args.length > 1)
                appearancePDFName = args[1];
            if (args.length > 2)
                outputFileName = args[2];
            System.out.println("Flash file: " + flashFileName + " Output: " + outputFileName +
                " appearance PDF: " + appearancePDFName );
            
            // Create a document and a 5" x 4" page for the Flash annotation
            Document doc = new Document();
            Rect mediaBox = new Rect(0, 0, 5 * 72, 4 * 72);
            Page page = doc.createPage(Document.BEFORE_FIRST_PAGE, mediaBox);

            // Set the annotation to be 3" x 2", with its lower-left corner 1" over and 1" up
            // from the bottom-left corner of the page.
            //
            // The size of the annotation should match the design size of your flash file.
            // If the two are drastically disparate, oddities with object placement in the
            // flash file may result.
            Rect annotRect = new Rect(1 * 72, 3 * 72, 4 * 72, 1 * 72);

            // Create the annotation and acquire the COS-layer representation.
            // Most of the specific functionality for annotations is set via
            // direct manipulation of the COS-layer representation of the annotation
            Annotation newAnnot = new Annotation(page, "Screen", annotRect);
            PDFDict cosAnnot = newAnnot.getPDFDict();

            // Set the annotation title and flags
            newAnnot.setTitle("Flash annotation sample");
            newAnnot.setFlags(EnumSet.of(AnnotationFlags.PRINT, AnnotationFlags.READ_ONLY));

            // There are several layers of objects to be created and nested in order to make
            // an annotation that plays Flash file content. The 2 major tasks are creating
            // the appearance for the annotation, and creating the annotation's action:
            // the appearance is what's shown before the Flash file is clicked on & invoked,
            // whereas the action states what to do in order to play the Flash file. 

            // The annotation action has many subdictionaries that are required.
            // This example starts by creating the topmost item:
            // set the action type to Rendition for SWF (Flash) files
            Action action = new Action(doc, "Rendition");
            PDFDict actionDict = action.getPDFDict();

            // Make a back-link to the annotation
            actionDict.put("AN", cosAnnot);

            // Specify what to do: in this case, the OP value of 0 means
            // if no rendition is associated with the annotation specified
            // by AN, play the rendition specified by R, associating it 
            // with the annotation. If a rendition is already associated
            // with the annotation, it is stopped, and the new rendition
            // is associated with the annotation.
            actionDict.put("OP",new PDFInteger(0, doc, false));

            // Make a rendition object to use for playing:
            PDFDict rendObj = new PDFDict(doc, true);

            // Rendition object is of the "media rendition" type
            rendObj.put("S", new PDFName("MR", doc, false));

            // Add a name for the rendition (optional)
            rendObj.put("N",new PDFString("Rendition for Flash embedding sample", doc, false, false));

            // Make the media clip dictionary:
            PDFDict mediaClipObj = new PDFDict(doc, true);

            // Required: this is a media clip data object:
            mediaClipObj.put("S",new PDFName("MCD", doc, false));

            // Specify what sort of media clip this is (MIME type):
            mediaClipObj.put("CT", new PDFString("application/x-shockwave-flash", doc, false, false));

            // Add a permissions dictionary (for making temp files for playback)
            PDFDict permObj = new PDFDict(doc, false);

            // Indicate a temp file may be made to play the rendition:
            permObj.put("TF", new PDFString("TEMPACCESS", doc, false, false));

            // Add the permissions dictionary to the rendition
            mediaClipObj.put("P", permObj);

            PDFStream fileStmObj = new PDFStream(new FileInputStream(flashFileName), doc, new PDFDict(doc, false), new PDFArray(doc, false));

            // Make a new file reference
            PDFDict fileRefObj = new PDFDict(doc, true);

            // Which needs an embedded file
            PDFDict efObj = new PDFDict(doc, false);

            // Add the filestream above to the embedded file dict
            efObj.put("F", fileStmObj);

            // Add the embedded file to the file reference
            fileRefObj.put("EF",efObj);

            // Set the type of this object
            fileRefObj.put("Type", new PDFName("Filespec", doc, false));

            // And add the actual file name this was based upon
            fileRefObj.put("F", new PDFString(flashFileName, doc, false, false));

            // Place the file specification in the media clip dictionary:
            mediaClipObj.put("D", fileRefObj);

            // Associate the media clip dictionary with the rendition:
            rendObj.put("C", mediaClipObj);

            // Associate the rendition with the action:
            actionDict.put("R", rendObj);

            // Associate the action with the annotation:
            cosAnnot.put("A",actionDict);

            try {
                // The other major part is creating the appearance for the annotation.
                // This example will import a PDF page and use it's first page as the appearance.
                Document importPDDoc = new Document(appearancePDFName);

                Page importPDPage = importPDDoc.getPage(0);
                Content tempContent = new Content();

                // The page is added to the PDE Content in order to acquire the PDEForm
                // created in the process. This PDEForm will become the basis of the
                // annotation appearance.
                //
                // NOTE: the actual appearance of the page in the annotation will be scaled to fit
                // within the boundaries of the annotation - so, if the page being used is of drastically
                // different x/y proportions from the annotation, it will appear distorted.
                tempContent.addPage(Content.BEFORE_FIRST, doc, importPDPage, null, null, EnumSet.noneOf(AddPageFlags.class), null);

                if (tempContent.getNumElements() == 1 && tempContent.getElement(0) instanceof Form)
                {
                    Form annotAPForm = (Form)tempContent.getElement(0);

                    // Place the form XObject as the "normal" appearance in an appearance dictionary
                    PDFDict apDict = new PDFDict(doc, false);
                    apDict.put("N", annotAPForm.getStream());

                    // And place the appearance dictionary in the annotation
                    cosAnnot.put("AP", apDict);
                }
                else
                    System.out.println("Unexpected page import result. Annotation will have no appearance.");

                importPDDoc.delete();
            }
            catch (RuntimeException ex) {
                if (ex.getMessage().startsWith("PDF"))
                {
                    System.out.println("Exception while importing annotation appearance:");
                    System.out.println(ex.getMessage());
                    System.out.println("* Annotation will not have a visible appearance but is still in PDF file");
                }
                else
                    throw ex;
            }

            // Create a backlink to the page this annotation appears on
            // in the annotation. This is required for Screen annotations
            // with rendition actions.
            cosAnnot.put("P", page.getPDFDict());

            page.delete();

            doc.save(EnumSet.of(SaveFlags.FULL), outputFileName);
        }
        finally {
            lib.delete();
        }
    }

}
