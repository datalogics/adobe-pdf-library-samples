package com.datalogics.PDFL.Samples;


import java.util.ArrayList;
import java.util.EnumSet;
import java.util.List;

import com.datalogics.PDFL.Color;
import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.HighlightAnnotation;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.Page;
import com.datalogics.PDFL.Quad;
import com.datalogics.PDFL.SaveFlags;
import com.datalogics.PDFL.UnderlineAnnotation;
import com.datalogics.PDFL.Word;
import com.datalogics.PDFL.WordAttributeFlags;
import com.datalogics.PDFL.WordFinder;
import com.datalogics.PDFL.WordFinderConfig;
import com.datalogics.PDFL.WordFinderVersion;

/*
 *
 * This program shows how to add annotations to an existing PDF file that will highlight and underline words.
 * When you run it, the program generates a PDF output file. The output sample annotates a PDF file showing
 * a National Weather Service web page, highlighting the word "Cloudy" wherever it appears and underlining
 * the word "Rain."
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
public class UnderlinesAndHighlights {

	public static void main(String[] args) {
		System.out.println("HighlightAndUnderlineAnnotations sample:");

		Library lib = new Library();

		try {
			String filename = "../../Resources/Sample_Input/sample.pdf";
                        String sOutput = "HighlightAndUnderlineAnnotations-out.pdf";
                        if ( args.length > 0 )
                            filename = args[0];
                        if ( args.length > 1 )
                            sOutput = args[1];
                        System.out.println("Reading " + filename + ", writing " + sOutput);

			Document doc = new Document(filename);

			System.out.println("Opened document " + filename);

			Page docpage = doc.getPage(0);

			//
			// Highlight occurrences of the word "cloudy" on the page.
			// Underline occurrences of the word "rain" on the page.
			//
			// For a more in-depth example of using the WordFinder, see the TextExtraction sample.
			//
			List<Quad> cloudyQuads = new ArrayList<Quad>();
			List<Quad> rainQuads = new ArrayList<Quad>();
			WordFinderConfig wfc = new WordFinderConfig();
			WordFinder wf = new WordFinder(doc, WordFinderVersion.LATEST, wfc);
			List<Word> words = wf.getWordList(docpage.getPageNumber());
			for (Word w : words)
			{
				// Store the Quads of all "Cloudy" words in a list for later use in
				// creating the annotation.
				if ("cloudy".equalsIgnoreCase(w.getText()) ||
					(w.getAttributes().contains(WordAttributeFlags.HAS_TRAILING_PUNCTUATION) && w.getText().toLowerCase().startsWith("cloudy")))
					cloudyQuads.addAll(w.getQuads());

				// Store the Quads of all "Rain" words
				if ("rain".equalsIgnoreCase(w.getText()) ||
						(w.getAttributes().contains(WordAttributeFlags.HAS_TRAILING_PUNCTUATION) && w.getText().toLowerCase().startsWith("rain")))
					rainQuads.addAll(w.getQuads());

			}

			HighlightAnnotation highlights = new HighlightAnnotation(docpage, cloudyQuads);
			highlights.setColor(new Color(1.0, 0.75, 1.0));
			highlights.setNormalAppearance(highlights.generateAppearance());

			UnderlineAnnotation underlines = new UnderlineAnnotation(docpage, rainQuads);
			underlines.setColor(new Color(0.0, 0.0, 0.0));
			underlines.setNormalAppearance(underlines.generateAppearance());

			// Read back the text that was annotated.
			System.out.println("Cloudy text:" + highlights.getAnnotatedText(true));
			System.out.println("Rainy text: " + underlines.getAnnotatedText(false));

			doc.save(EnumSet.of(SaveFlags.FULL), sOutput);

		} finally {
			lib.delete();
		}
	}
}
