using System;
using System.Collections.Generic;
using System.Text.Json;
using Datalogics.PDFL;
using ExtractTextNameSpace;

/*
 * 
 * This sample extracts text from the Annotations in a PDF
 * document and saves the text to a file.
 *
 * Copyright (c) 2022-2023, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/.
 *
 */
namespace ExtractTextFromAnnotations
{
    class ExtractTextFromAnnotations
    {

        // Set Defaults
        static String sInput = Library.ResourceDirectory + "Sample_Input/sample_annotations.pdf";
        static String sOutput = "../ExtractTextFromAnnotations-out.json";

        static void Print(AnnotationTextObject t)
        {
            Console.WriteLine("Annotation Type > " + t.AnnotationType);
            Console.WriteLine("Annotation Text > " + t.AnnotationText);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Annotations Sample:");


            using (new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput = Library.ResourceDirectory + "Sample_Input/sample_annotations.pdf";

                Console.WriteLine("Input file: " + sInput);

                using (Document doc = new Document(sInput))
                {
                    using (ExtractText docAnnotText = new ExtractText(doc))
                    {
                        List<AnnotationTextObject> result = docAnnotText.GetAnnotationText();

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
