package com.datalogics.PDFL.Samples;

import com.datalogics.PDFL.*;
import java.util.*;
/*
 * 
 * A sample which demonstrates the use of the DLE API to list
 * the set of available fonts.
 * 
 * This sample lists all of the fonts available for use on the machine where the ListFont program is run.
 * The font name appears with the type, such as Type0, Type1, or TrueType, and the encoding method,
 * such as WinAnsiEncoding.
 * 
 * For more detail see the description of the List sample programs, and ListFont, on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/listing-information-about-values-and-objects-in-pdf-files
 *
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
public class ListFonts {
	public static void main(String[] args) throws Throwable {
		Library lib = new Library();

		try {
			List<Font> fonts = Font.getFontList();

			for (Font font : fonts) {
				System.out.println(font.getName());
				System.out.println(font.getType());
				System.out.println(font.getEncoding());
			}
		} finally {
			lib.delete();
		}
	}

};
