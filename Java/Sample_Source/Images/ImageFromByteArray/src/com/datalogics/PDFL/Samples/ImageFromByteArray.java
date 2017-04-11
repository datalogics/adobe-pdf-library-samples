package com.datalogics.PDFL.Samples;


import java.util.EnumSet;

import com.datalogics.PDFL.ColorSpace;
import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.DrawParams;
import com.datalogics.PDFL.Image;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.Matrix;
import com.datalogics.PDFL.Page;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.SaveFlags;

/*
*
* This sample program is an alternate method for drawing a graphic image from a PDF.
* Rather than exporting the embedded image itself, this program draws the page of an
* input document to a Byte Array, creates an export image with that array, and then 
* saves the contents to a graphics file.
* 
* A Byte Array is a contiguous section of memory, expressed as raw data, a series of bytes.
* 
* When you run the program it will generate a single PDF as an output file.
*
* Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
*
* For complete copyright information, refer to:
* http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
*
*/

public class ImageFromByteArray {

/**
 * @param args
 */
public static void main(String[] args) throws Throwable {
    System.out.println("ImageFromByteArray sample:");

    Library lib = new Library();

    try {
        Document docWithContent = null;
        Page page = null;

        Image img = null;

        Document emptyDoc = null;
        Page docpage = null;

        String sInput = "../../Resources/Sample_Input/cottage.pdf";
        String sOutput = "ImageFromByteArray-out.pdf";
        if (args.length > 0)
            sInput = args[0];
        if (args.length > 1)
            sOutput = args[1];
        System.out.println("Input file: " + sInput + ", will write to " + sOutput);

        try {
            docWithContent = new Document(sInput);
            page = docWithContent.getPage(0);
            Rect cropBox = page.getCropBox();

            final int width = (int)cropBox.getRight() - (int)cropBox.getLeft();
            final int height = (int)cropBox.getTop() - (int)cropBox.getBottom();

            // Shift the page display based on the location of the CropBox
            final int shiftx = (int)cropBox.getLeft();
            final int shifty = (int)cropBox.getBottom();

            // Must invert the page to get from PDF with origin at lower left,
            // to a bitmap with the origin at upper right.
            // Also, we use the shiftx and shifty to take the location of the CropBox
            // into account.
            Matrix matrix = new Matrix().scale(1, -1).translate(-shiftx, -height - shifty);

            DrawParams dp = new DrawParams();
            dp.setMatrix(matrix);
            dp.setDestRect(page.getCropBox());
            dp.setColorSpace(ColorSpace.DEVICE_RGB);

            //Here bytes array obtained from the page
            //but user can create his own byte array.
            byte arr[]  = page.drawContents(dp);

            //The page.drawContents() function returns 4 bytes aligned data buffer
            //however image constructor expected non-padded buffer
            //In native code we check if it need to strip padding
            img = new Image(arr, width, height, dp.getColorSpace());

            emptyDoc = new Document();
            Rect pageRect = page.getCropBox();

            docpage = emptyDoc.createPage(Document.BEFORE_FIRST_PAGE, pageRect);

            docpage.getContent().addElement(img);
            docpage.updateContent();

            emptyDoc.save(EnumSet.of(SaveFlags.FULL), sOutput);
            System.runFinalization();
        }
        finally {
            if (docWithContent != null) {
                docWithContent.delete();
            }
            if (page != null) {
                page.delete();
            }
            if (img != null) {
                img.delete();
            }
            if (emptyDoc != null) {
                emptyDoc.delete();
            }
            if (docpage != null) {
                docpage.delete();
            }
        }
    }
    finally {
        lib.delete();
    }
}
}
