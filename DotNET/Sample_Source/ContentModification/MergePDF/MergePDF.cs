using System;
using Datalogics.PDFL;

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
            Console.WriteLine("MergePDF Sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                String sInput1 = Library.ResourceDirectory + "Sample_Input/merge_pdf1.pdf";
                String sInput2 = Library.ResourceDirectory + "Sample_Input/merge_pdf2.pdf";
                String sOutput = "MergePDF-out.pdf";

                if (args.Length > 0)
                    sInput1 = args[0];

                if (args.Length > 1)
                    sInput2 = args[1];

                if (args.Length > 2)
                    sOutput = args[2];

                Console.WriteLine("MergePDF: adding " + sInput1 + " and " + sInput2 + " and writing to " + sOutput);

                using (Document doc1 = new Document(sInput1))
                {
                    using (Document doc2 = new Document(sInput2))
                    {
                        try
                        {
                            doc1.InsertPages(Document.LastPage, doc2, 0, Document.AllPages, PageInsertFlags.Bookmarks | PageInsertFlags.Threads |
                                             // For best performance processing large documents, set the following flags.
                                             PageInsertFlags.DoNotMergeFonts | PageInsertFlags.DoNotResolveInvalidStructureParentReferences | PageInsertFlags.DoNotRemovePageInheritance);
                        }
                        catch (LibraryException ex)
                        {
                            if (!ex.Message.Contains(
                                "An incorrect structure tree was found in the PDF file but operation continued"))
                            {
                                throw;
                            }

                            Console.Out.WriteLine(ex.Message);
                        }

                        // For best performance processing large documents, set the following flags.
                        doc1.Save(SaveFlags.Full | SaveFlags.SaveLinearizedNoOptimizeFonts | SaveFlags.Compressed, sOutput);
                    }
                }
            }
        }
    }
}
