package com.datalogics.pdfl.samples.DocumentConversion.ConvertToOffice;

import java.util.EnumSet;

import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.Library;

/*
 *
 * ConvertToOffice converts sample PDF documents to Office Documents.
 *
 * For more detail see the description of the ConvertToOffice sample program on our Developer’s site,
 * https://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/converting-and-merging-pdf-content/
 *
 * Copyright (c) 2023, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
public class ConvertToOffice
{
    enum OfficeType
    {
        WORD,
        EXCEL,
        POWERPOINT
    };

    public static void ConvertPDFToOffice(String inputPath, String outputPath, OfficeType officeType)
    {
        System.out.println("Converting " + inputPath + ", output file is " + outputPath);

        Boolean result = false;

        if (officeType == OfficeType.WORD)
        {
            result = Document.convertToWord(inputPath, outputPath);
        }
        else if (officeType == OfficeType.EXCEL)
        {
            result = Document.convertToExcel(inputPath, outputPath);
        }
        else if (officeType == OfficeType.POWERPOINT)
        {
            result = Document.convertToPowerPoint(inputPath, outputPath);
        }

        if (result)
        {
             System.out.println("Successfully converted " + inputPath + " to " + outputPath);
        }
        else
        {
             System.out.println("ERROR: Could not convert " + inputPath);
        }
    }

    public static void main(String[] args) throws Exception {
        System.out.println("ConvertToOffice Sample:");

        Library lib = new Library();
        try {
            System.out.println("Initialized the library.");

            String inputPathWord = Library.getResourceDirectory() + "Sample_Input/Word.pdf";
            String outputPathWord = "word-out.docx";
            String inputPathExcel = Library.getResourceDirectory() + "Sample_Input/Excel.pdf";
            String outputPathExcel = "excel-out.xlsx";
            String inputPathPowerPoint = Library.getResourceDirectory() + "Sample_Input/PowerPoint.pdf";
            String outputPathPowerPoint = "powerpoint-out.pptx";

            ConvertPDFToOffice(inputPathWord, outputPathWord, OfficeType.WORD);
            ConvertPDFToOffice(inputPathExcel, outputPathExcel, OfficeType.EXCEL);
            ConvertPDFToOffice(inputPathPowerPoint, outputPathPowerPoint, OfficeType.POWERPOINT);
        }
        finally
        {
            lib.delete();
        }
    }
}