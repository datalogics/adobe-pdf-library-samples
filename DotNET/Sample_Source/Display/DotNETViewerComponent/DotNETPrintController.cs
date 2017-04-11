/*
Copyright (C) 2008-2009, Datalogics, Inc. All rights reserved.
 
The information and code in this sample is for the exclusive use
of Datalogics customers and evaluation users only.  Datalogics 
permits you to use, modify and distribute this file in accordance 
with the terms of your license agreement. Sample code is for 
demonstrative purposes only and is not intended for production use.
*/

/*
 * DotNETPrintController :
 * 
 * DotNETPrintController should be created and used when printing with DotNETViewer.
 * It is designed to use the custom print dialog packaged with DotNETViewer
 * called DotNETPrintDialog that has an added option to "Shrink to fit printable
 * area".
 */

// define this if you want to draw with bitmaps
//#define DRAWWITHBITMAPS

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using Datalogics.PDFL;

namespace DotNETViewerComponent
{
    class DotNETPrintController
    {
        // members

        Document iDoc;                      // reference to the PDF open in DotNETViewer
        PrintDocument printDocument;        // object used to send information to printer
        PrinterSettings printerSettings;    
        DrawParams drawParams;

        int currentPage;
        double offset;
        
        double horizontalSquish;
        double verticalSquish;

        bool singlePage;
        bool shrinkToFit;

        // support for manual collation
        int currentCopies;
        int localCopies;
        bool notCollating;

        // constructor

        public DotNETPrintController(Document doc, PrintDocument printDoc, DrawParams dp, bool single, bool shrink)
        {
            iDoc = doc;
            printDocument = printDoc;
            printerSettings = printDocument.PrinterSettings;
            drawParams = dp;

            currentPage = printerSettings.FromPage - 1;
            offset = 0.0;
            horizontalSquish = verticalSquish = 1.0;
            singlePage = single;
            shrinkToFit = shrink;

            currentCopies = 1;
            localCopies = 1;

            // CHANGE THIS LINE for printers that do their own collation
            notCollating = (!printerSettings.Collate && singlePage); 
        }

        // methods/events

        /*
         * printDocument_QueryPageSetting - 
         * If the Printer has a Custom Paper recalculate the Paper Height
         */
        void printDocument_QueryPageSetting(object sender, QueryPageSettingsEventArgs e)
        {
            if (e.PageSettings.PaperSize.Kind == PaperKind.Custom)
            {
                double height = 0;
                for (int i = printerSettings.FromPage - 1; i <= printerSettings.ToPage - 1; ++i)
                {
                    using (Page page = iDoc.GetPage(i)) height += page.CropBox.Height * 100 / GlobalConsts.pdfDPI;
                }
                e.PageSettings.PaperSize.Height = (int)height;// (int)(iDoc.heightOfPageRange(printerSettings.FromPage - 1, printerSettings.ToPage - 1) * 100.0 / (double)GlobalConsts.pdfDPI);
            }
        }

        /*
         * printDocument_PrintPage - 
         * Is an event handler that gets called each time a 
         * page is to be printed. This handles the settings to
         * use while printing.
         */
        private void printDocument_PrintPage(object sender, PrintPageEventArgs ev)
        {
            RectangleF bounds = ev.PageSettings.PrintableArea;
            double currentPos = bounds.Top;

            if (singlePage)
            {
                Rect cbox;
                using (Page page = iDoc.GetPage(currentPage)) cbox = page.CropBox;

                // add to the width to correct for math errors
                double maxPageWidth = (cbox.Right - cbox.Left + 1.0) * 100.0 / (double)GlobalConsts.pdfDPI;
                double maxPageHeight = (cbox.Top - cbox.Bottom) * 100.0 / (double)GlobalConsts.pdfDPI;

                // some print drivers are misleading about their printable area
                // corrected by doubling the reported offset from the top left corner of the physical sheet of paper
                // and make it a "buffer zone" around the print image
                maxPageWidth += (bounds.X * 2.0);
                maxPageHeight += (bounds.Y * 2.0);

                if (shrinkToFit && (maxPageWidth > bounds.Width || maxPageHeight > bounds.Height))
                {
                    horizontalSquish = verticalSquish = bounds.Width / maxPageWidth;

                    if ((bounds.Height / maxPageHeight) < horizontalSquish)
                        horizontalSquish = verticalSquish = bounds.Height / maxPageHeight;
                }
                else
                {
                    horizontalSquish = 1.0;
                    verticalSquish = 1.0;
                }
            }

            while (currentPos < bounds.Bottom)
            {
                using (Page p = iDoc.GetPage(currentPage))
                {
                    Rect cb = p.CropBox;

                    // Draw the page at the currentPosition here
                    double pageWidthInPDFUserUnits = cb.Right - cb.Left + 1.0;
                    double pageHeightInPDFUserUnits = cb.Top - cb.Bottom;
                    int pageWidthInPrinterRezUnits = (int)(pageWidthInPDFUserUnits * horizontalSquish * (double)ev.Graphics.DpiX / (double)GlobalConsts.pdfDPI);
                    int pageHeightInPrinterRezUnits = (int)(pageHeightInPDFUserUnits * verticalSquish * (double)ev.Graphics.DpiY / (double)GlobalConsts.pdfDPI);

                    // DLADD dtom 10Jul2009: Set up CropBox offsets.
                    int shiftx = (int)cb.LLx;
                    int shifty = (int)cb.LLy;

                    // Invert the page to get from PDFL coordinates to .Net coordinates
                    Datalogics.PDFL.Matrix matrix = new Datalogics.PDFL.Matrix().Scale((horizontalSquish * (double)ev.Graphics.DpiX / (double)GlobalConsts.pdfDPI),
                        -(verticalSquish * (double)ev.Graphics.DpiY / (double)GlobalConsts.pdfDPI)).Translate(-shiftx, -pageHeightInPDFUserUnits - shifty);

                    Rect updateRect = new Rect(cb.LLx, cb.LLy, cb.LLx + pageWidthInPDFUserUnits, cb.LLy + pageHeightInPDFUserUnits);

                    drawParams.UpdateRect = updateRect;
                    drawParams.SmoothFlags = SmoothFlags.Image | SmoothFlags.LineArt | SmoothFlags.Text;
                    drawParams.Flags = DrawFlags.UseAnnotFaces;

#if DRAWWITHBITMAPS
                    drawParams.Matrix = matrix;

                    using(Bitmap bitmap = new Bitmap(pageWidthInPrinterRezUnits, pageHeightInPrinterRezUnits, ev.Graphics))
                    {
                        Graphics graphics = Graphics.FromImage(bitmap);

                        graphics.Clear(System.Drawing.Color.White);

                        p.DrawContents(bitmap, drawParams);
                        ev.Graphics.DrawImageUnscaled(bitmap, 0, (int)(currentPos - offset));
                    }
#else
                    Datalogics.PDFL.Matrix matrix2 = new Datalogics.PDFL.Matrix();
                    matrix2 = matrix2.Translate(0, (offset - currentPos) * (double)GlobalConsts.pdfDPI / 100.0);
                    matrix2 = matrix2.Concat(matrix);

                    drawParams.Matrix = matrix2;
                    p.DrawContents(ev.Graphics, drawParams);
#endif

                    currentPos += (double)pageHeightInPrinterRezUnits * 100.0 / (double)ev.Graphics.DpiX - offset;
                    offset = 0;

                    if (++currentPage >= printerSettings.ToPage)
                        break;

                    if (singlePage)
                        break;
                }
            }

            if(notCollating && currentPos <= bounds.Bottom)
            {
                if(currentCopies < localCopies)
                {
                    currentPage--;
                    currentCopies++;
                }
                else
                    currentCopies = 1;
            }

            if(currentPage >= printerSettings.ToPage)
                ev.HasMorePages = false;
            else
                ev.HasMorePages = true;
        }

        /*
         * Print -
         * The Print function adds the event handler created above to
         * the print document to specify how each page should be handled
         * and then calls printDocument.Print(), which starts the printing
         * process.
         */
        public void Print()
        {
            printDocument.DocumentName = iDoc.FileName;
            printDocument.PrintPage += new PrintPageEventHandler(printDocument_PrintPage);

            // The notCollating flag was set in the constructor--it indicates that the user wants
            // copies, but doesn't want them collated (i.e. wants: AABBCC instead of ABCABC).
            // Most printers don't handle this automatically, so we include a workaround:
            // if notCollating is set, we set printerSettings.Copies to 1 (prevent the driver
            // from making copies) and loop by oursevles in the PrintPage handler.
            //
            // Note that we only support this for single page mode--in continuous mode we are
            // conceptually printing one very long 'page', and collation is meaningless.
            if (notCollating)
            {
                localCopies = printerSettings.Copies;
                printerSettings.Copies = 1; // Disables automated copying
            }

            // print the document
            printDocument.Print();
        }
    }
}
