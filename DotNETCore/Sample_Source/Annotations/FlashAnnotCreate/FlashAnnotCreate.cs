using System;
using System.Collections.Generic;
using System.Text;
using Datalogics.PDFL;

/*
 *
 * The sample program shows how to add an annotation to a PDF file that features a flash video embedded under an image.
 * 
 * For more detail see the description of the FlashAnnotCreate sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/working-with-annotations#flashannotcreate
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
namespace FlashAnnotCreate
{
    class FlashAnnotCreate
    {

        static void Main(string[] args)
        {
            Console.WriteLine("FlashAnnotCreate Sample:");

            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                // Names of the flash-format file to embed.
                String sInput1 = Library.ResourceDirectory + "Sample_Input/JobReady-info.swf";
                // Name of the PDF file to use for the annotation appearance - the
                // first page of the PDF is imported
                String sInput2 = Library.ResourceDirectory + "Sample_Input/cottage.pdf";
                String sOutput = "FlashAnnotCreate-out.pdf";

                if (args.Length > 0)
                    sInput1 = args[0];

                if (args.Length > 1)
                    sInput2 = args[1];

                if (args.Length > 2)
                    sOutput = args[2];

                Console.WriteLine("Using flash file " + sInput1 + " and generating appearance from " + sInput2 + ", saving output file : " + sOutput);

                // Create a document and a 5" x 4" page for the Flash annotation
                using (Document doc = new Document())
                {
                    Rect mediaBox = new Rect(0, 0, 5 * 72, 4 * 72);
                    using (Page page = doc.CreatePage(Document.BeforeFirstPage, mediaBox))
                    {
                        // Set the annotation to be 3" x 2", with its lower-left corner 1" over and 1" up
                        // from the bottom-left corner of the page.
                        //
                        // The size of the annotation should match the design size of your flash file.
                        // If the two are drastically disparate, oddities with object placement in the
                        // flash file may result.
                        Rect annotRect = new Rect(1 * 72, 3 * 72, 4 * 72, 1 * 72);

                        // Create the annotation and acquire the COS-layer representation.
                        // Most of the specific functionality for annotations is set via
                        // direct manipulation of the COS-layer representation of the annotation */
                        Annotation newAnnot = new Annotation(page, "Screen", annotRect);
                        PDFDict cosAnnot = newAnnot.PDFDict;

                        // Set the annotation title and flags
                        newAnnot.Flags = AnnotationFlags.Print | AnnotationFlags.ReadOnly;
                        newAnnot.Title = "Flash annotation sample";

                        // There are several layers of objects to be created and nested in order to make
                        // an annotation that plays Flash file content. The 2 major tasks are creating
                        // the appearance for the annotation, and creating the annotation's action:
                        // the appearance is what's shown before the Flash file is clicked on & invoked,
                        // whereas the action states what to do in order to play the Flash file. */

                        // The annotation action has many subdictionaries that are required.
                        // This example starts by creating the topmost item:
                        // set the action type to Rendition for SWF (Flash) files
                        Datalogics.PDFL.Action action = new Datalogics.PDFL.Action(doc, "Rendition");
                        PDFDict actionDict = action.PDFDict;
                        // Make a back-link to the annotation
                        actionDict.Put("AN", cosAnnot);

                        // Specify what to do: in this case, the OP value of 0 means
                        // if no rendition is associated with the annotation specified
                        // by AN, play the rendition specified by R, associating it
                        // with the annotation. If a rendition is already associated
                        // with the annotation, it is stopped, and the new rendition
                        // is associated with the annotation.
                        actionDict.Put("OP", new PDFInteger(0, doc, false));

                        // Make a rendition object to use for playing:
                        PDFDict rendObj = new PDFDict(doc, true);
                        // Rendition object is of the "media rendition" type
                        rendObj.Put("S", new PDFName("MR", doc, false));

                        // Add a name for the rendition (optional)
                        rendObj.Put("N", new PDFString("Rendition for Flash embedding sample", doc, false, false));

                        // Make the media clip dictionary:
                        PDFDict mediaClipObj = new PDFDict(doc, true);

                        // Required: this is a media clip data object:
                        mediaClipObj.Put("S", new PDFName("MCD", doc, false));

                        // Specify what sort of media clip this is (MIME type):
                        mediaClipObj.Put("CT", new PDFString("application/x-shockwave-flash", doc, false, false));

                        // Add a permissions dictionary (for making temp files for playback)
                        PDFDict permObj = new PDFDict(doc, false);
                        // Indicate a temp file may be made to play the rendition:
                        permObj.Put("TF", new PDFString("TEMPACCESS", doc, false, false));
                        // Add the permissions dictionary to the rendition
                        mediaClipObj.Put("P", permObj);

                        using (System.IO.FileStream fileStream = new System.IO.FileStream(sInput1, System.IO.FileMode.Open))
                        {
                            using (PDFStream fileStmObj = new PDFStream(fileStream, doc, new PDFDict(doc, false), new PDFArray(doc, false)))
                            {
                                // Make a new file reference
                                PDFDict fileRefObj = new PDFDict(doc, true);
                                // Which needs an embedded file
                                PDFDict efObj = new PDFDict(doc, false);
                                // Add the filestream above to the embedded file dict
                                efObj.Put("F", fileStmObj);
                                // Add the embedded file to the file reference
                                fileRefObj.Put("EF", efObj);
                                // Set the type of this object
                                fileRefObj.Put("Type", new PDFName("Filespec", doc, false));
                                // And add the actual file name this was based upon
                                fileRefObj.Put("F", new PDFString(sInput1, doc, false, false));

                                // Place the file specification in the media clip dictionary:
                                mediaClipObj.Put("D", fileRefObj);

                                // Associate the media clip dictionary with the rendition:
                                rendObj.Put("C", mediaClipObj);

                                // Associate the rendition with the action:
                                actionDict.Put("R", rendObj);

                                // Associate the action with the annotation:
                                cosAnnot.Put("A", actionDict);

                                try
                                {
                                    // The other major part is creating the appearance for the annotation.
                                    // This example will import a PDF page and use it's first page as the appearance.
                                    using (Document importPDDoc = new Document(sInput2))
                                    {
                                        Page importPDPage = importPDDoc.GetPage(0);
                                        Content tempContent = new Content();

                                        // The page is added to the PDE Content in order to acquire the PDEForm
                                        // created in the process. This PDEForm will become the basis of the
                                        // annotation appearance.
                                        //
                                        // NOTE: the actual appearance of the page in the annotation will be scaled to fit
                                        // within the boundaries of the annotation - so, if the page being used is of drastically
                                        // different x/y proportions from the annotation, it will appear distorted. */
                                        tempContent.AddPage(Content.BeforeFirst, doc, importPDPage, null, null, 0, null);

                                        if (tempContent.NumElements == 1 && tempContent.GetElement(0) is Form)
                                        {
                                            Form annotAPForm = tempContent.GetElement(0) as Form;
                                            // Place the form XObject as the "normal" appearance in an appearance dictionary
                                            PDFDict apDict = new PDFDict(doc, false);
                                            apDict.Put("N", annotAPForm.Stream);
                                            // And place the appearance dictionary in the annotation
                                            cosAnnot.Put("AP", apDict);
                                        }
                                        else
                                            Console.WriteLine("Unexpected page import result. Annotation will have no appearance.");
                                    }
                                }
                                catch (ApplicationException ex)
                                {
                                    if (ex.Message.StartsWith("PDF"))
                                    {
                                        Console.WriteLine("Exception %x (%s) while importing annotation appearance:");
                                        Console.WriteLine(ex.Message);
                                        Console.WriteLine("* Annotation will not have a visible appearance but is still in PDF file");
                                    }
                                    else
                                        throw;
                                }

                                // Create a backlink to the page this annotation appears on
                                // in the annotation. This is required for Screen annotations
                                // with rendition actions.
                                cosAnnot.Put("P", page.PDFDict);

                                doc.Save(SaveFlags.Full, sOutput);
                            }
                        }
                    }
                }
            }
        }
    }
}
