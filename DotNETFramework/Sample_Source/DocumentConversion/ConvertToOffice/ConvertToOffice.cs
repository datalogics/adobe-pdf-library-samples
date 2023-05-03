using System;
using Datalogics.PDFL;

/*
 * 
 * ConvertToOffice converts sample PDF documents to Office Documents.
 *
 * For more detail see the description of the ConvertToOffice sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/converting-and-merging-pdf-content
 * 
 * Copyright (c) 2023, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
namespace ConvertToOffice
{
    class ConvertToOffice
    {
        enum OfficeType
        {
            Word = 0,
            Excel = 1,
            PowerPoint = 2
        };

        static void Main(string[] args)
        {
            Console.WriteLine("ConvertToOffice Sample:");

            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                string inputPathWord = Library.ResourceDirectory + "Sample_Input/Word.pdf";
                string outputPathWord = "word-out.docx";
                string inputPathExcel = Library.ResourceDirectory + "Sample_Input/Excel.pdf";
                string outputPathExcel = "excel-out.xlsx";
                string inputPathPowerPoint = Library.ResourceDirectory + "Sample_Input/PowerPoint.pdf";
                string outputPathPowerPoint = "powerpoint-out.pptx";

                ConvertPDFToOffice(inputPathWord, outputPathWord, OfficeType.Word);
                ConvertPDFToOffice(inputPathExcel, outputPathExcel, OfficeType.Excel);
                ConvertPDFToOffice(inputPathPowerPoint, outputPathPowerPoint, OfficeType.PowerPoint);
            }
        }

        static void ConvertPDFToOffice(string inputPath, string outputPath, OfficeType officeType)
        {
            Console.WriteLine("Converting " + inputPath + ", output file is " + outputPath);

            bool result = false;

            if (officeType == OfficeType.Word)
            {
                result = Document.ConvertToWord(inputPath, outputPath);
            }
            else if (officeType == OfficeType.Excel)
            {
                result = Document.ConvertToExcel(inputPath, outputPath);
            }
            else if (officeType == OfficeType.PowerPoint)
            {
                result = Document.ConvertToPowerPoint(inputPath, outputPath);
            }

            if (result)
            {
                Console.WriteLine("Successfully converted " + inputPath + " to " + outputPath);
            }
            else
            {
                Console.WriteLine("ERROR: Could not convert " + inputPath);
            }
        }
    }
}
