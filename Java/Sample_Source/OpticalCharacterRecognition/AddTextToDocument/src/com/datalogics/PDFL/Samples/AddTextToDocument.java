package com.datalogics.pdfl.samples.OpticalCharacterRecognition.AddTextToDocument;


import java.util.EnumSet;

import com.datalogics.PDFL.Content;
import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.Element;
import com.datalogics.PDFL.Form;
import com.datalogics.PDFL.Image;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.OCREngine;
import com.datalogics.PDFL.OCRParams;
import com.datalogics.PDFL.Page;
import com.datalogics.PDFL.PageSegmentationMode;
import com.datalogics.PDFL.Performance;
import com.datalogics.PDFL.SaveFlags;
import com.datalogics.PDFL.Container;
import com.datalogics.PDFL.Group;

/*
 * Process a document using the optical character recognition engine.
 * Then place the image and the processed text in an output pdf
 * 
 * For more detail see the description of AddTextToDocument on our Developers site, 
 * https://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/optical-character-recognition/
 * 
 * Copyright (c) 2007-2019, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

public class AddTextToDocument {

    public static void addTextToImage(Document doc, Content content, OCREngine engine) {
        for (int index = 0; index < content.getNumElements(); index++) {
            Element e = content.getElement(index);
            if (e instanceof Image) {

                Image img = (Image)e;
                //PlaceTextUnder creates a form with the image and the generated text
                //under the image. The original image in the page is then replaced by
                //by the form.
                Form form = engine.placeTextUnder((Image)e, doc);
                content.removeElement(index);
                content.addElement(form, Document.BEFORE_FIRST_PAGE);
            } else if (e instanceof Container) {
                addTextToImage(doc, ((Container)e).getContent(), engine);
            } else if (e instanceof Group) {
                addTextToImage(doc, ((Group)e).getContent(), engine);
            } else if (e instanceof Form) {
                addTextToImage(doc, ((Form)e).getContent(), engine);
            }
        }
    }

    /**
     * @param args
     */
    public static void main(String[] args) {
        System.out.println("AddTextToDocument sample:");

        String sInput = Library.getResourceDirectory() + "Sample_Input/scanned_images.pdf";
        String sOutput = "AddTextToDocument-out.pdf";

        if (args.length > 0)
            sInput = args[0];
        if (args.length > 1)
            sOutput = args[1];
        
        System.out.println("Input file: " + sInput + ", will write to " + sOutput);
        Library lib = new Library();

        try {
            OCRParams ocrParams = new OCRParams();
            // Setting the segmentation mode to AUTOMATIC lets the OCR engine
            // choose how to segment the page for text detection.
            ocrParams.setPageSegmentationMode(PageSegmentationMode.AUTOMATIC);
            // This tells the selected engine to improve accuracy at the expense
            // of increased run time. For Tesseract 3, it runs two different
            // algorithms, and chooses the one that has the most confidence.
            ocrParams.setPerformance(Performance.BEST_ACCURACY);
            OCREngine ocrEngine = new OCREngine(ocrParams);

            // Open a document with a single page.
            Document doc = new Document(sInput);

            for (int pageNumber = 0; pageNumber < doc.getNumPages(); pageNumber++) {
                Page page = doc.getPage(pageNumber);
                try {
                    Content content = page.getContent();
                    addTextToImage(doc, content, ocrEngine);
                    page.updateContent();
                }
                finally {
                    page.delete();
                }
            } 
            ocrEngine.delete();
            doc.save(EnumSet.of(SaveFlags.FULL), sOutput);
            doc.delete();
        }
        finally {
            lib.delete();
        }
    }

}
