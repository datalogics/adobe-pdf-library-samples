using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Datalogics.PDFL;

/*
 * This sample demonstrates using DocTextFinder to find instances of a phrase
 * that matches a user-supplied regular expression. The output is a JSON file that
 * has the match information.
 * 
 * Copyright (c) 2021, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

// This Datalogics sample uses the Newtonsoft.Json library to generate JSON output. Below is the MIT license for the Newtonsoft.Json software:
/*
Copyright (c) 2007 James Newton-King

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

namespace RegexExtractText
{
    // This class represents a match quad's bottom left coordinate.
    public class BottomLeft {
        public double x { get; set; }
        public double y { get; set; }
    }

    // This class represents a match quad's bottom right coordinate.
    public class BottomRight {
        public double x { get; set; }
        public double y { get; set; }
    }

    // This class represents a match quad's top left coordinate.
    public class TopLeft {
        public double x { get; set; }
        public double y { get; set; }
    }

    // This class represents a match quad's top right coordinate.
    public class TopRight {
        public double x { get; set; }
        public double y { get; set; }
    }

    // This class represents the 4 coordinates that make up a match quad.
    public class QuadLocation {
        [JsonProperty("bottom-left")]
        public BottomLeft bottomLeft { get; set; }

        [JsonProperty("bottom-right")]
        public BottomRight bottomRight { get; set; }

        [JsonProperty("top-left")]
        public TopLeft topLeft { get; set; }

        [JsonProperty("top-right")]
        public TopRight topRight { get; set; }
    }

    // This class represents a match quad's location (the quad coordinates and page number that quad is located on).
    public class MatchQuadInformation {
        [JsonProperty("page-number")]
        public int pageNumber { get; set; }

        [JsonProperty("quad-location")]
        public QuadLocation quadLocation { get; set; }
    }

    // This class represents the information that is associated with a match (match phrase and match quads).
    public class MatchObject {
        [JsonProperty("match-phrase")]
        public string matchPhrase { get; set; }

        [JsonProperty("match-quads")]
        public List<MatchQuadInformation> matchQuads { get; set; }
    }

    // This class represents the final JSON that will be written to the output JSON file.
    public class DocTextFinderJson {
        public List<MatchObject> documentJson;
    }

    class RegexExtractText
    {
        static void Main(string[] args)
        {
            Console.WriteLine("RegexExtractText Sample:");

            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput = Library.ResourceDirectory + "Sample_Input/RegexExtractText.pdf";
                String sOutput = "../RegexExtractText-out.json";

                // Uncomment only one regular expression you are interested in seeing the match information of (as a JSON file).
                // Phone numbers
                String sRegex = "((1-)?(\\()?\\d{3}(\\))?(\\s)?(-)?\\d{3}-\\d{4})";
                // Email addresses
                //String sRegex = "(\\b[\\w.!#$%&'*+\\/=?^`{|}~-]+@[\\w-]+(?:\\.[\\w-]+)*\\b)";
                // URLs
                //String sRegex = "((https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s]{2,}|www\\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s]{2,}|https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9]+\\.[^\\s]{2,}|www\\.[a-zA-Z0-9]+\\.[^\\s]{2,}))";

                if (args.Length > 0)
                    sInput = args[0];

                using (Document doc = new Document(sInput))
                {
                    int nPages = doc.NumPages;

                    Console.WriteLine("Input file:  " + sInput);

                    // This will hold the JSON stream that we will print to the output JSON file.
                    DocTextFinderJson result = new DocTextFinderJson();
                    result.documentJson = new List<MatchObject>();

                    WordFinderConfig wordConfig = new WordFinderConfig();

                    // Need to set this to true so phrases will be concatenated properly.
                    wordConfig.NoHyphenDetection = true;

                    // Create a DocTextFinder with the default wordfinder parameters.
                    using (DocTextFinder docTextFinder =
                        new DocTextFinder(doc, wordConfig))
                    {
                        // Retrieve the phrases matching a regular expression.
                        IList<DocTextFinderMatch> docMatches =
                            docTextFinder.GetMatchList(0, nPages - 1, sRegex);

                        // Iterate through the matches and add match information to the DocTextFinderJson object.
                        foreach (DocTextFinderMatch wInfo in docMatches)
                        {
                            // This object will store the match phrase and an array of quads for the match.
                            MatchObject matchObject = new MatchObject();
                            
                            // This list will store the page number and quad location for each match quad.
                            List<MatchQuadInformation> matchQuadInformationList = new List<MatchQuadInformation>();

                            // Set the match phrase in the matchObject.
                            matchObject.matchPhrase = wInfo.MatchString;

                            // Get the quads.
                            IList<DocTextFinderQuadInfo> QuadInfo = wInfo.QuadInfo;

                            foreach (DocTextFinderQuadInfo qInfo in QuadInfo)
                            {
                                MatchQuadInformation temp = new MatchQuadInformation();
                                temp.pageNumber = qInfo.PageNum;

                                // Iterate through the quads and insert the quad information into the matchQuadInformation object.
                                foreach (Quad quad in qInfo.Quads)
                                {
                                    QuadLocation quadLocation = new QuadLocation();
                                    quadLocation.topLeft = new TopLeft();
                                    quadLocation.bottomLeft = new BottomLeft();
                                    quadLocation.topRight = new TopRight();
                                    quadLocation.bottomRight = new BottomRight();

                                    quadLocation.topLeft.x = quad.TopLeft.H;
                                    quadLocation.topLeft.y = quad.TopLeft.V;

                                    quadLocation.bottomLeft.x = quad.BottomLeft.H;
                                    quadLocation.bottomLeft.y = quad.BottomLeft.V;

                                    quadLocation.topRight.x = quad.TopRight.H;
                                    quadLocation.topRight.y = quad.TopRight.V;

                                    quadLocation.bottomRight.x = quad.BottomRight.H;
                                    quadLocation.bottomRight.y = quad.BottomRight.V;

                                    temp.quadLocation = quadLocation;
                                    matchQuadInformationList.Add(temp);
                                }
                            }
                            matchObject.matchQuads = matchQuadInformationList;
                            result.documentJson.Add(matchObject);
                        }
                        // Save the output JSON file.
                        Console.WriteLine("Writing JSON to " + sOutput);
                        string json = JsonConvert.SerializeObject(result.documentJson, Formatting.Indented);
                        System.IO.File.WriteAllText(sOutput, json);
                    }
                }
            }
        }
    }
}
