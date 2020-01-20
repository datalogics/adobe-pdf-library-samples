
package com.datalogics.pdfl.samples.Forms.ImportFormsData;

/*
 *
 * The ImportFormsData sample demonstrates how to convert XFA into AcroForms.
 *
 * The ImportFormsData sample demonstrates how to Import forms data into XFA and AcroForms documents:
 *
 *  - Import data into a XFA (Dynamic or Static) document, the types supported include XDP, XML, or XFD
 *  - Import data into an AcroForms document, the types supported include XFDF, FDF, or XML
 *
 * For more detail see the description of the ImportFormsData sample program on our Developer’s site,
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-sample-programs/working-with-forms/ImportFormsData
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
public class ImportFormsData
{

    public static void main(String[] args) throws Throwable {
        System.out.println("ImportFormsData sample:");

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
            String sInputData = Library.getResourceDirectory() + "Sample_Input/DynamicXFA_data.xdp";
            String sOutput = "../ImportFormsDataXFA-out.pdf";

            if (args.length > 0)
            {
                sOutput = args[0];
            }

            Document doc = new Document(sInput);

            //Import the data, acceptable types include XDP, XML, and XFD
            boolean result = doc.importXFAFormsData(sInputData);

            if (result)
            {
                System.out.println("Forms data was imported!");

                doc.save(EnumSet.of(SaveFlags.FULL, SaveFlags.LINEARIZED), sOutput);
            }
            else
            {
                System.out.println("Importing of Forms data failed!");
            }

            doc.delete();

            //AcroForms document
            sInput = Library.getResourceDirectory() + "Sample_Input/AcroForm.pdf";
            sInputData = Library.getResourceDirectory() + "Sample_Input/AcroForm_data.xfdf";
            sOutput = "../ImportFormsDataAcroForms.xfdf";

            if (args.length > 1)
            {
                sOutput = args[1];
            }

            doc = new Document(sInput);

            //Import the data while specifying the type, in this case XFDF
            result = doc.importAcroFormsData(sInputData, AcroFormImportType.XFDF);

            if (result)
            {
                System.out.println("Forms data was imported!");

                doc.save(EnumSet.of(SaveFlags.FULL, SaveFlags.LINEARIZED), sOutput);
            }
            else
            {
                System.out.println("Importing of Forms data failed!");
            }

            doc.delete();
        }
        finally {
            lib.delete();
        }
   }
}

