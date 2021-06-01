package com.datalogics.pdfl.samples.Text.RegexExtractText;

import com.datalogics.PDFL.DocTextFinder;
import com.datalogics.PDFL.DocTextFinderMatch;
import com.datalogics.PDFL.DocTextFinderQuadInfo;
import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.Page;
import com.datalogics.PDFL.Point;
import com.datalogics.PDFL.Quad;
import com.datalogics.PDFL.SaveFlags;
import com.datalogics.PDFL.WordFinderConfig;

import java.util.EnumSet;
import java.io.FileWriter;
import java.util.List;

import org.json.JSONArray;
import org.json.JSONObject;

/*
 * 
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

// This Datalogics sample uses the org.json (JSON-Java) library to generate JSON output. Below is the JSON license for the org.json software:
/*
Copyright (c) 2002 JSON.org
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
The Software shall be used for Good, not Evil.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
IN THE SOFTWARE.
*/

public class RegexExtractText {
	public static void main (String[] args) throws Throwable
    {
        System.out.println("RegexExtractText sample:");

        Library lib = new Library();

        try {

            final String sInput = (args.length > 0) ? args[0] : Library.getResourceDirectory() + "Sample_Input/RegexExtractText.pdf";
            final String sOutput = "RegexExtractText-out.json";

            // Phone numbers
            final String sRegex = "((1-)?(\\()?\\d{3}(\\))?(\\s)?(-)?\\d{3}-\\d{4})";
            // Email addresses
            //String sRegex = "(\\b[\\w.!#$%&'*+\\/=?^`{|}~-]+@[\\w-]+(?:\\.[\\w-]+)*\\b)";
            // URLs
            //String sRegex = "((https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s]{2,}|www\\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s]{2,}|https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9]+\\.[^\\s]{2,}|www\\.[a-zA-Z0-9]+\\.[^\\s]{2,}))";
            
            System.out.println("Reading " + sInput);

            Document doc = new Document(sInput);

            // This array will hold the JSON stream that we will print to the output JSON file.
            JSONArray result = new JSONArray();
            
            final int nPages = doc.getNumPages();

            System.out.println("Opened document " + sInput);
            
            WordFinderConfig wordConfig = new WordFinderConfig();
            
            // Need to set this to true so phrases will be concatenated properly.
            wordConfig.setNoHyphenDetection(true);
            
            DocTextFinder docTextFinder = new DocTextFinder(doc, wordConfig);
            
            List<DocTextFinderMatch> docMatches = docTextFinder.getMatchList(0, nPages - 1, sRegex);

            for (DocTextFinderMatch wInfo : docMatches) {
                // This JSON object will store the match phrase and an array of quads for the match.
                JSONObject matchObject = new JSONObject();

                // This JSON array will store the page number and quad location for each match quad.
                JSONArray matchQuadInformation = new JSONArray();

                // Set the match phrase in the JSON object.
                matchObject.put("match-phrase", wInfo.getMatchString());

                // Get the word quads
                List<DocTextFinderQuadInfo> quadList = wInfo.getQuadInfo();

                // Iterate through the quad info
                for (DocTextFinderQuadInfo qInfo : quadList) {
                    for(Quad quad : qInfo.getQuads()){
                        // Get the coordinates of the quad.
                        double top_left_x = quad.getTopLeft().getH();
                        double top_left_y = quad.getTopLeft().getV();

                        double bottom_left_x = quad.getBottomLeft().getH();
                        double bottom_left_y = quad.getBottomLeft().getV();

                        double top_right_x = quad.getTopRight().getH();
                        double top_right_y = quad.getTopRight().getV();

                        double bottom_right_x = quad.getBottomRight().getH();
                        double bottom_right_y = quad.getBottomRight().getV();

                        // Set the quad coordinates in JSON objects.
                        JSONObject topLeft = new JSONObject();
                        topLeft.put("x", top_left_x);
                        topLeft.put("y", top_left_y);
                        
                        JSONObject bottomLeft = new JSONObject();
                        bottomLeft.put("x", bottom_left_x);
                        bottomLeft.put("y", bottom_left_y);
                        
                        JSONObject topRight = new JSONObject();
                        topRight.put("x", top_right_x);
                        topRight.put("y", top_right_y);
                        
                        JSONObject bottomRight = new JSONObject();
                        bottomRight.put("x", bottom_right_x);
                        bottomRight.put("y", bottom_right_y);

                        // Use the quad coordinate JSON objects to form a single JSON object that holds match quad location information.
                        JSONObject quadLocation = new JSONObject();
                        quadLocation.put("bottom-left", bottomLeft);
                        quadLocation.put("bottom-right", bottomRight);
                        quadLocation.put("top-left", topLeft);
                        quadLocation.put("top-right", topRight);

                        JSONObject quadInformationObject = new JSONObject();
                        quadInformationObject.put("page-number", qInfo.getPageNum());
                        quadInformationObject.put("quad-location", quadLocation);

                        // Insert the match's page number and quad location(s) in the matchQuadInformation JSON array.
                        matchQuadInformation.put(quadInformationObject);
                    }
                }
                // Set the match's quad information in the matchObject.
                matchObject.put("match-quads", matchQuadInformation);

                result.put(matchObject);
            }
            // Write the match information to the output JSON file.
            System.out.println("Writing JSON to " + sOutput);
            FileWriter file = new FileWriter(sOutput);
            file.write(result.toString(4));
            file.close();

            doc.close();
        }
        finally {
            lib.delete();
        }
    }
}
