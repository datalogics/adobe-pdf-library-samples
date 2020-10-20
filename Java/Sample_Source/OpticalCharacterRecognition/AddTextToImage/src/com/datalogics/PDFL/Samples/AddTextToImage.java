package com.datalogics.pdfl.samples.OpticalCharacterRecognition.AddTextToImage;

import java.util.EnumSet;
import java.util.List;
import java.util.ArrayList;

import com.datalogics.PDFL.Content;
import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.Form;
import com.datalogics.PDFL.Image;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.Matrix;
import com.datalogics.PDFL.OCREngine;
import com.datalogics.PDFL.OCRParams;
import com.datalogics.PDFL.Page;
import com.datalogics.PDFL.LanguageSetting;
import com.datalogics.PDFL.Language;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.SaveFlags;

/*
 * The sample uses an image as input which will be processed by the optical character recognition engine.
 * We will then place the image and the processed text in an output pdf
 * 
 * For more detail see the description of AddTextToImage, on our Developers site, 
 * https://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/optical-character-recognition/
 * 
 * Copyright (c) 2007-2019, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

public class AddTextToImage {

    /**
     * @param args
     */
    public static void main(String[] args) {
        System.out.println("AddTextToImage sample:");

        String sInput = Library.getResourceDirectory() + "Sample_Input/text_as_image.jpg";
        String sOutput = "AddTextToImage-out.pdf";

        if (args.length > 0)
            sInput = args[0];
        if (args.length > 1)
            sOutput = args[1];

        System.out.println("Input file: " + sInput + ", will write to " + sOutput);

        Library lib = new Library();

        try {
            OCRParams ocrParams = new OCRParams();
            //The OCRParams.setLanguages method controls which languages the OCR engine attempts
            //to detect. By default the OCR engine searches for English.
            List<LanguageSetting> langList = new ArrayList<LanguageSetting>();
            LanguageSetting languageOne = new LanguageSetting(Language.ENGLISH, false);
            langList.add(languageOne);
            //You could add additional languages for the OCR engine to detect by adding 
            //more entries to the LanguageSetting list.
            //LanguageSetting languageTwo = new LanguageSetting(Language.JAPANESE, false);
            //langList.add(languageTwo);
            ocrParams.setLanguages(langList);

            OCREngine ocrEngine = new OCREngine(ocrParams);
            // Open a document with a single page.
            Document doc = new Document();
            // Create a PDF page around this image. The design width and height
            // for the image are carried in the Image's Matrix's A and D fields, respectively.
            Image newImage = new Image(sInput, doc);
            Matrix imageMatrix = newImage.getMatrix();
            Rect pageRect = new Rect(0, 0, imageMatrix.getA(), imageMatrix.getD());
            Page docpage = doc.createPage(Document.BEFORE_FIRST_PAGE, pageRect);
            Page page = doc.getPage(0);
            try {
                //PlaceTextUnder creates a form with the image and the generated text
                //under the image. The original image in the page is then replaced by
                //by the form.
                Form form = ocrEngine.placeTextUnder(newImage, doc);
                page.getContent().addElement(form, Content.BEFORE_FIRST);
                page.updateContent();
            }
            finally {
                page.delete();
                ocrEngine.delete();
            }
            doc.save(EnumSet.of(SaveFlags.FULL), sOutput);
            doc.delete();
        }
         finally {
            lib.delete();
        }
    }

}
