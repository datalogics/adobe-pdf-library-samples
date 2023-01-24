using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Datalogics.PDFL;
using ExtractTextNameSpace;

/*
 * 
 * This sample extracts text and details of that text in a PDF
 * document, prints to console, and saves the text to a JSON file.
 *
 * Copyright (c) 2022-2023, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/.
 *
 */
namespace ExtractTextPreservingStyleAndPositionInfo
{
    class ExtractTextPreservingStyleAndPositionInfo
    {
        // Set Defaults
        static String sInput = Library.ResourceDirectory + "Sample_Input/sample.pdf";
        static String sOutput = "../ExtractTextPreservingStyleAndPositionInfo-out.json";

        static void Main(string[] args)
        {
            Console.WriteLine("ExtractTextPreservingStyleAndPositionInfo Sample:");

            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                Console.WriteLine("Input file: " + sInput);

                using (Document doc = new Document(sInput))
                {
                    using (ExtractText docText = new ExtractText(doc))
                    {
                        List<TextAndDetailsObject> result = docText.GetTextAndDetails();

                        // Save the output to a JSON file.
                        Console.WriteLine("Writing JSON to " + sOutput);
                        SaveJson(result);
                    }
                }
            }
        }

        static void SaveJson(List<TextAndDetailsObject> result)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;
                writer.WriteStartArray();
                foreach (TextAndDetailsObject resultText in result)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("text");
                    writer.WriteValue(resultText.Text);
                    writer.WritePropertyName("quads");
                    writer.WriteStartArray();
                    foreach (Quad quad in resultText.Quads)
                    {
                        writer.WriteStartObject();
                        writer.WritePropertyName("top-left");
                        writer.WriteValue(quad.TopLeft.ToString());
                        writer.WritePropertyName("top-right");
                        writer.WriteValue(quad.TopRight.ToString());
                        writer.WritePropertyName("bottom-left");
                        writer.WriteValue(quad.BottomLeft.ToString());
                        writer.WritePropertyName("bottom-right");
                        writer.WriteValue(quad.BottomRight.ToString());
                        writer.WriteEndObject();
                    }
                    writer.WriteEndArray();
                    writer.WritePropertyName("styles");
                    writer.WriteStartArray();
                    foreach (DLStyleTransition st in resultText.StyleList)
                    {
                        writer.WriteStartObject();
                        writer.WritePropertyName("char-index");
                        writer.WriteValue(st.CharIndex.ToString());
                        writer.WritePropertyName("font-name");
                        writer.WriteValue(st.Style.FontName.ToString());
                        writer.WritePropertyName("font-size");
                        writer.WriteValue(Math.Round(st.Style.FontSize, 2).ToString());
                        writer.WritePropertyName("color-space");
                        writer.WriteValue(st.Style.Color.Space.Name);
                        writer.WritePropertyName("color-values");
                        writer.WriteStartArray();
                        foreach (double cv in st.Style.Color.Value)
                        {
                            writer.WriteValue(Math.Round(cv, 3).ToString());
                        }
                        writer.WriteEndArray();
                        writer.WriteEndObject();
                    }
                    writer.WriteEndArray();
                    writer.WriteEndObject();
                }
            }
            System.IO.File.WriteAllText(sOutput, sb.ToString());
        }
    }
}
