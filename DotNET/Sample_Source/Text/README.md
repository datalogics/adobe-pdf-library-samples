## ***AddGlyphs***
Adds specific glyphs as text to a PDF page.

## ***AddUnicodeText***
Adds Unicode text to a PDF page.

## ***AddVerticalText***
Adds text in vertical writing mode to a PDF page.

## ***ExtractAcroformFieldData***
Extracts text from the AcroForm fields in a PDF document.  It displays the output in the console window and also saves the text to a JSON file. It is useful for those who work with fillable forms in PDFs and need to extract the text within the form to use in a different format.

## ***ExtractCJKTextByPatternMatch***
This sample allows searching for Unicode characters such as Chinese, Japanese, and Korean (CJK). It is the same as ExtractTextByPatternMatch, but searches for a specific string of text in Korean.

## ***ExtractTextByPatternMatch***
Searches for patterns that match text based on a given regular expression. For this sample, we search for phone numbers and save the results in a text file.

## ***ExtractTextByRegion***
Extracts text from a specific region of a page in a PDF document and saves the text to a file. Let’s say you or one of your clients has thousands of invoices with the same format and you need to pull data out of a specific region all at once (i.e., all of the invoice numbers), this sample can help with that.

## ***ExtractTextFromAnnotations***
Extracts text from the annotations in a PDF document and saves the text to a JSON file indicating what type of annotation and the text within it. If you need consolidate all the notes and feedback that were added as annotations in your PDFs, this function can help you pull out that data for easy viewing and/or printing.

## ***ExtractTextFromMultiRegions***
Processes example invoice PDF files in a folder that share the same page layout and extracts text from specific regions of its pages and saves the text to a CSV file.  All the invoice numbers, dates, order numbers, customer IDs, and totals from the invoices in the folder are saved in convenient CSV format.

## ***ExtractTextPreservingStyleAndPositionInfo***
Extracts text along with details about the text such as quads, font, font-size, color, and styles from a PDF document and saves the text to a JSON file.

## ***ListWords***
Extracts text from a PDF document and displays the extracted words.

## ***RegexExtractText***
Uses a regular expression to find a specified phrase of text in a PDF document.

## ***RegexTextSearch***
Searches for phrases or text patterns in a PDF input document. It supplies sample regular expressions to use in searching for phone numbers, email addresses, or URLs, and you can use them or create your own. You can search the entire PDF document or provide a page range for your search. The program generates an output PDF document that matches the input file except that the search content appears highlighted.  You can enter the name of the input file you plan to use, and the name of the output file. The sample uses [PDDocTextFinder](https://dev.datalogics.com/adobe-pdf-library/adobe-pdf-library-c-language-interface/working-with-the-adobe-pdf-library/searching-for-content-in-a-pdf-document/) to find instances of a phrase or pattern in a PDF input document.

The sample normally highlights search text with a box that surrounds the entire phrase found. But if the search text is on multiple lines, or if the font changes within the phrase, the content appears in multiple boxes.

## ***TextExtract***
Extracts text from a tagged and untagged PDF document.
