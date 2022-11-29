using System;
using Datalogics.PDFL;
using System.Collections.Generic;

/*
 * 
 * This sample demonstrates merging one PDF document into another. The program
 * prompts the user to enter the names of two PDF files, and then inserts the content 
 * of the second PDF file into the first PDF file and saves the result in a third PDF file.
 *
 * Copyright (c) 2007-2021, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace MergePDF
{
    class MergePDF
    {
        static void Main(string[] args)
        {
            Console.WriteLine("MergePDF with Attachments or Portfolio sample:");

            using (Library lib = new Library())
            {
                // attachments.pdf -- contains 2 attachments, one regular, one attached to an annotation. doc.Attachments only returns 1
                // merge_pdf2.pdf -- no attachments
                // Portfolio1.pdf -- DL made portfolio with 2 PDFs - doc.Attachments returns 2

                String sInput1 = "mergepdf2.pdf"; 
                String sMainPDF = "Attachments.pdf";
                String sOutput = "MergePDF-outComplete.pdf";

                if (args.Length > 0)
                    sInput1 = args[0];

                if (args.Length > 1)
                    sMainPDF = args[1];

                if (args.Length > 2)
                    sOutput = args[2];

                // Issue 1: merge_pdf2.pdf has no attachment or embedded files. Using 
                // doc1.Attachments results in "The document doesn't contain a EmbeddedFiles dictionary" exception
                // which then has to be handled.  Shouldn't it just return a 0 length IList?  
                // Or perhaps DLE should have methods that return the # of attachements 
                // (e.g. Document.numAttachments and Document.numAnnotationAttachments))
                // so that the application can check first?
                
                // Issue 2: attachments.pdf has contains 2 attachments, one regular, one attached to an annotation.
                //   Acrobat lists both, but doc1.Attachments only retrieves 1. 
                // 

                using (Document doc1 = new Document(sInput1))
                {
                    try
                    {
                        Document docMain = new Document(sMainPDF);

                        // Issue1: Don't try to get the attachments unless you know there is an attachment
                        IList<FileAttachment> attachments = doc1.Attachments;
                        Console.WriteLine("\nDocument " + sInput1 + " has " + attachments.Count + " attachments");

                        if (doc1.Root.Contains("Names")) // 
                        {
                            PDFDict names = (PDFDict)doc1.Root.Get("Names");
                            if (names.Contains("EmbeddedFiles"))
                            {
                                IList<FileAttachment> attachmentsTemp = doc1.Attachments;
                                Console.WriteLine("\nDocument " + sInput1 + " has " + attachmentsTemp.Count + " attachments");
                                foreach (FileAttachment fileA in attachmentsTemp)
                                {
                                    // Not all attachments are PDFs.
                                    Console.WriteLine("Attachement is "+ fileA.FileName );
                                    if (fileA.FileName.EndsWith(".pdf") )

                                    {
                                        // APDFL does not currently have a mechanism to convert the fileAttachment to a Document.
                                        // Need to save to a temp file and then re-open?
                                        fileA.SaveToFile("temp.pdf");
                                        Document tempPDF = new Document("temp.pdf");
                                        docMain.InsertPages(Document.LastPage, tempPDF, 0, Document.AllPages, PageInsertFlags.Bookmarks | PageInsertFlags.Threads |
                                           PageInsertFlags.DoNotMergeFonts | PageInsertFlags.DoNotResolveInvalidStructureParentReferences | PageInsertFlags.DoNotRemovePageInheritance);
                                        tempPDF.Close();
                                        tempPDF.Dispose();                        
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("Document does not have EmbeddedFiles entry in Names dict");
                            }
                        }
                        else
                        {
                             Console.WriteLine("Document does not have Names entry in catalog");
                        }

                        // For best performance processing large documents, set the following flags.
                        Console.WriteLine("Saving to " + sOutput);
                        docMain.Save(SaveFlags.Full | SaveFlags.SaveLinearizedNoOptimizeFonts | SaveFlags.Compressed, sOutput);

                    }
                    catch(LibraryException ex)
                    {
                        Console.Out.WriteLine(ex.Message);
                    }

                }
            }
        }
    }
}

