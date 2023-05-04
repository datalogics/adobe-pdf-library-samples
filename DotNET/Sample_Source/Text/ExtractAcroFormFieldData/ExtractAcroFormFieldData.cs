using System;
using System.Collections.Generic;
using System.Text.Json;
using Datalogics.PDFL;
using ExtractTextNameSpace;
/*
 * This sample extracts text from the AcroForm fields in a PDF
 * document and saves the text to a file.
 *
 * Copyright (c) 2022-2023, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace ExtractAcroFormFieldData
{
    class ExtractAcroFormFieldData
    {
        // Set Defaults
        static String sInput = Library.ResourceDirectory + "Sample_Input/ExtractAcroFormFieldData.pdf";
        static String sOutput = "../ExtractAcroFormFieldData-out.json";

        static void Print(AcroFormTextFieldObject t)
        {
            Console.WriteLine("AcroForm Field Name > " + t.AcroFormFieldName);
            Console.WriteLine("AcroForm Field Text > " + t.AcroFormFieldText);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("ExtractAcroFormFieldData Sample:");


            using (new Library())
            {
                Console.WriteLine("Initialized the library.");
                using (Document doc = new Document(sInput))
                {
                    Console.WriteLine("Input file: " + sInput);

                    using (ExtractText docFormText = new ExtractText(doc))
                    {
                        List<AcroFormTextFieldObject> result = docFormText.GetAcroFormFieldData();

                        // Print the output to the console
                        result.ForEach(Print);

                        // Save the output to a JSON file.
                        Console.WriteLine("Writing JSON to " + sOutput);
                        JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
                        string json = JsonSerializer.Serialize(result, options);
                        System.IO.File.WriteAllText(sOutput, json);
                    }
                }
            }
        }
    }
}
