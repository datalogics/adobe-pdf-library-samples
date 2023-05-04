using System;
using System.IO;
using Datalogics.PDFL;


/*
 * 
 * Run this program to extract content from a PDF file from a stream. The system will prompt you to enter
 * the name of a PDF file to use for input. The program generates two output PDF files.
 *
 * A stream is a string of bytes of any length, embedded in a PDF document with a dictionary
 * that is used to interpret the values in the stream. 
 * 
 * This program is similar to ImagefromStream, but in this example the PDF file streams hold text.
 *
 * For more detail see the description of the StreamIO sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/exporting-images-from-pdf-files/#streamio
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
namespace StreamIO
{
    class StreamIOSample
    {
        /// <summary>
        /// Demonstrate reading a PDF Document from a .NET Stream object.
        /// </summary>
        /// <param name="path">The filename of the PDF document to read.</param>
        /// <param name="output">A string to receive the contents of the PDF document.</param>
        void ReadFromStream(String path, String output)
        {
            // Create a .NET FileStream object, opened using the path argument.
            // A FileStream is used here for demonstration only, but the technique
            // works just as well for MemoryStream, or other streams which support
            // seeking.  In practice, when dealing with files it is usually more
            // appropriate to pass the path directly to the Document constructor.
            FileStream fs = new FileStream(path, FileMode.Open);

            // A document is then opened, using the FileStream as its data source.
            using (Document d = new Document(fs))
            {
                // Add a watermark to have some visible change to the PDF
                WatermarkParams wp = new WatermarkParams();
                wp.TargetRange.PageSpec = PageSpec.AllPages;
                WatermarkTextParams wtp = new WatermarkTextParams();
                wtp.Text = "This PDF was opened\nfrom a Stream";
                d.Watermark(wtp, wp);

                // The document can not simply be saved at this point.
                // d.Save(SaveFlags.Incremental);  // This will throw an exception.

                // Instead, the document can be saved to a file. Saving the document
                // to a file causes the document to use the file as its data source.
                d.Save(SaveFlags.Full, output);

                // Make another minor change.
                d.Creator = "PDFL StreamIO Sample";

                // Since the document is now backed by a file, an incremental save is okay.
                d.Save(SaveFlags.Incremental);
            }
        }

        /// <summary>
        /// Demonstrate writing a PDF Document to a .NET Stream object.
        /// </summary>
        void WriteToStream(String output)
        {
            // A MemoryStream will be used as the destination.
            MemoryStream ms = new MemoryStream();

            using (Document d = new Document())
            {
                // Add some content to the Document so there will be something to see.
                d.Creator = "PDFL StreamIO Sample";
                Page p = d.CreatePage(Document.BeforeFirstPage, new Rect(0, 0, 612, 792));
                AddContentToPage(p);

                // Save the file to the MemoryStream.  The document is saved as a
                // copy, so the full contents will be written.
                d.Save(SaveFlags.Full, ms);
            }

            // Though the document has been disposed, we can open a new document
            // using the saved stream.
            using (Document d = new Document(ms))
            {
                Console.WriteLine("creator: " + d.Creator);
            }

            // Another possibility would be to write the stream to a file, and open
            // the document from the file.
            FileStream fs = new FileStream(output, FileMode.Create);
            ms.WriteTo(fs);
            fs.Close();
            using (Document d = new Document(output))
            {
                Console.WriteLine("creator: " + d.Creator);
            }
        }

        void AddContentToPage(Page p)
        {
            Datalogics.PDFL.Path rect = new Datalogics.PDFL.Path();
            rect.AddRect(new Point(100, 100), 100, 100);
            GraphicState gs = new GraphicState();
            gs.StrokeColor = new Color(0, 1.0, 0);
            gs.Width = 3.0;
            rect.GraphicState = gs;
            rect.PaintOp = PathPaintOpFlags.Stroke;
            p.Content.AddElement(rect);
            p.UpdateContent();
        }

        static void Main(string[] args)
        {
            StreamIOSample sample = new StreamIOSample();
            Console.WriteLine("StreamIO Sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                String sInput = Library.ResourceDirectory + "Sample_Input/sample.pdf";
                String sOutput1 = "StreamIO-out1.pdf";
                String sOutput2 = "StreamIO-out2.pdf";

                if (args.Length > 0)
                    sInput = args[0];

                if (args.Length > 1)
                    sOutput1 = args[1];

                if (args.Length > 2)
                    sOutput2 = args[2];

                Console.WriteLine("Input file: " + sInput + ". Writing to output " + sOutput1 + " and " + sOutput2);

                sample.ReadFromStream(sInput, sOutput1);

                sample.WriteToStream(sOutput2);
            }
        }
    }
}
