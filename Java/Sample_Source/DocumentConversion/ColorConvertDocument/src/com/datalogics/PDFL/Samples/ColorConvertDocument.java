package com.datalogics.pdfl.samples.DocumentConversion.ColorConvertDocument;

import java.util.List;
import java.util.ArrayList;
import java.util.EnumSet;
import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.LibraryFlags;
import com.datalogics.PDFL.SaveFlags;
import com.datalogics.PDFL.ColorConvertActions;
import com.datalogics.PDFL.ColorConvertObjAttrs;
import com.datalogics.PDFL.ColorConvertCSpaceType;
import com.datalogics.PDFL.ColorConvertActionType;
import com.datalogics.PDFL.RenderIntent;
import com.datalogics.PDFL.ColorProfile;
import com.datalogics.PDFL.ColorConvertParams;

/*
 *
 * The ColorConvertDocument sample program demonstrates working with color conversion in PDF documents.
 * The color conversion process allows you to apply a different color profile to an object in a PDF document,
 * and thus effectively change the colors found in that object. This process applies a set of colors to an
 * image or other object within a PDF and embeds that information in the document, so that the right set of
 * colors will be rendered when the PDF document is sent to a printer or to another output device.
 * 
 * Note that the color profile is not embedded by default; rather, the default is not to embed the color profile.
 * The user must set the option to embed to True.
 * 
 * For more detail see the description of the ColorConvertDocument sample program on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/color-conversion
 * 
 * Copyright (c) 2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
 
public class ColorConvertDocument {

	/**
	 * @param args
	 */
        public static void main(String[] args) throws Throwable {
        System.out.println("ColorConvertDocument sample:");

        String resourceDirectory = Library.getResourceDirectory();
        List<String> fontPaths = new ArrayList<String>();
        fontPaths.add(resourceDirectory + "Fonts");

        Library lib = new Library(fontPaths, resourceDirectory + "CMap", resourceDirectory + "Unicode", resourceDirectory + "Color", EnumSet.of(LibraryFlags.DISABLE_MEMORY_SUBALLOCATOR));
        
        try {
            String sInput = resourceDirectory + "Sample_Input/ducky.pdf";
            String sOutput = "ColorConvertDocument-out.pdf";
            if (args.length > 0)
                sInput = args[0];
            if (args.length > 1)
                sOutput = args[1];
            System.out.println("Input file: " + sInput + ", will write to " + sOutput);

            Document doc = new Document(sInput);

            /* Create the list of color conversion actions to be applied to the document. Each object in the document is compared
            * against the selection criteria for each of the actions until a matching action is found. Actions do not chain, 
            * except in the case of aliased ink definitions
            */
            List<ColorConvertActions> colorConvActions = new ArrayList<ColorConvertActions>();
            ColorConvertActions action = new ColorConvertActions();

            /* In this example, make any object in the document a candidate for color conversion. Also allow for any kind of Color Space. 
            * The ColorConvertObjAttrs values can be combined together for more specific matching patterns using the | operator. 
            * This is also true for Color Spaces.
            */
            action.setMustMatchAnyAttrs(ColorConvertObjAttrs.COLOR_CONV_ANY_OBJECT);
            action.setMustMatchAnyCSAttrs(ColorConvertCSpaceType.COLOR_CONV_ANY_SPACE);
            action.setIntentToMatch(RenderIntent.USE_PROFILE_INTENT);
            action.setConvertIntent(RenderIntent.PERCEPTUAL);
            action.setAction(ColorConvertActionType.CONVERT_COLOR);
            action.setConvertProfile(ColorProfile.DOT_GAIN_10);

            /*Actions that should be applied to inks need to have its IsInkAction property set to true before being added to the list*/
            colorConvActions.add(action);

            /*Once all the actions to be performed are on the list, a ColorConvertParams object is created to hold them. Defaults for 
            *Render Intent and Device Color Profiles can also be set here. 
            */
            ColorConvertParams parms = new ColorConvertParams(colorConvActions);

            boolean success = doc.colorConvertPages(parms);
            
            
            doc.save(EnumSet.of(SaveFlags.FULL, SaveFlags.COMPRESSED), sOutput);
        }
        catch (Exception e) {

                System.out.println(e.toString());
        }
        finally {
            lib.delete();
        }
    }
}
