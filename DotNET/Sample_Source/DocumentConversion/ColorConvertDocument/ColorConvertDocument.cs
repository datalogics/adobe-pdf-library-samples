using System;
using System.Collections.Generic;
using Datalogics.PDFL;

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
 * For more detail see the description of the ColorConvertDocument sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/color-conversion
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
namespace ColorConvertDocument
{
    class ColorConvertDocument
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ColorConvertDocument Sample:");


            List<string> paths = new List<string>();
            paths.Add(Library.ResourceDirectory + "Fonts/");

            // ReSharper disable once UnusedVariable
            using (Library lib = new Library(paths, Library.ResourceDirectory + "CMap/",
                Library.ResourceDirectory + "Unicode/", Library.ResourceDirectory + "Color",
                LibraryFlags.DisableMemorySuballocator))
            {
                Console.WriteLine("Initialized the library.");

                String sInput = Library.ResourceDirectory + "Sample_Input/ducky.pdf";
                String sOutput = "ColorConvertDocument-out.pdf";

                if (args.Length > 0)
                    sInput = args[0];

                if (args.Length > 1)
                    sOutput = args[1];

                Console.WriteLine("Input file: " + sInput + ", writing to " + sOutput);

                Document doc = new Document(sInput);

                /* Create the list of color conversion actions to be applied to the document. Each object in the document is compared
                 * against the selection criteria for each of the actions until a matching action is found. Actions do not chain,
                 * except in the case of aliased ink definitions
                 */
                List<ColorConvertActions> colorConvActions = new List<ColorConvertActions>();
                ColorConvertActions action = new ColorConvertActions();

                /* In this example, make any object in the document a candidate for color conversion. Also allow for any kind of Color Space.
                 * The ColorConvertObjAttrs values can be combined together for more specific matching patterns using the | operator.
                 * This is also true for Color Spaces.
                 */
                action.MustMatchAnyAttrs = ColorConvertObjAttrs.ColorConvAnyObject;
                action.MustMatchAnyCSAttrs = ColorConvertCSpaceType.ColorConvAnySpace;
                action.IntentToMatch = RenderIntent.UseProfileIntent;
                action.ConvertIntent = RenderIntent.Perceptual;
                action.Action = ColorConvertActionType.ConvertColor;
                action.ConvertProfile = ColorProfile.DotGain10;

                /*Actions that should be applied to inks need to have its IsInkAction property set to true before being added to the list*/
                colorConvActions.Add(action);

                /*Once all the actions to be performed are on the list, a ColorConvertParams object is created to hold them. Defaults for 
                 *Render Intent and Device Color Profiles can also be set here. 
                 */
                ColorConvertParams parms = new ColorConvertParams(colorConvActions);

                // ReSharper disable once UnusedVariable
                bool success = doc.ColorConvertPages(parms);

                doc.Save(SaveFlags.Full | SaveFlags.Compressed, sOutput);
            }
        }
    }
}
