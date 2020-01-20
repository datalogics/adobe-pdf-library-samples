
package com.datalogics.pdfl.samples.Forms.ConvertXFAToAcroForms;

/*
 *
 * The ConvertXFAToAcroForms sample demonstrates how to convert XFA into AcroForms.
 *
 * Converts XFA (Dynamic or Static) fields to AcroForms fields and removes XFA fields.
 *
 * For more detail see the description of the ConvertXFAToAcroForms sample program on our Developer’s site,
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/working-with-forms/ConvertXFAToAcroForms
 *
 * NOTE: The APDFL Forms Extension is available separately from APDFL.  Please contact Datalogics directly for more information about this extension.
 *
 * Copyright (c) 2019, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
import com.datalogics.PDFL.*;
import java.util.EnumSet;
public class ConvertXFAToAcroForms
{

    public static void main(String[] args) throws Throwable {
        System.out.println("ConvertXFAToAcroForms sample:");

        Library lib = new Library(EnumSet.of(LibraryFlags.INIT_FORMS_EXTENSION));

        try
        {
            if (!lib.isFormsExtensionAvailable())
            {
                System.out.println("Forms Plugins were not properly loaded!");
                return;
            }

            //Must be set to true to prevent default legacy behavior of PDFL
            lib.setAllowOpeningXFA(true);

            System.out.println("Initialized the library.");

            //XFA document
            String sInput = Library.getResourceDirectory() + "Sample_Input/DynamicXFA.pdf";
            String sOutput = "../ConvertXFAToAcroForms-out.pdf";

            if (args.length > 0)
            {
                sInput = args[0];
            }

            if (args.length > 1)
            {
                sOutput = args[1];
            }

            Document doc = new Document(sInput);
            long pagesOutput = doc.convertXFAFieldsToAcroFormFields();

            System.out.println("XFA document was converted into AcroForms document with " + pagesOutput + " pages.");

            doc.save(EnumSet.of(SaveFlags.FULL, SaveFlags.LINEARIZED), sOutput);

            doc.delete();
        }
        finally {
            lib.delete();
        }
   }
}

