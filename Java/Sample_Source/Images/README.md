## ***DocToImages***
Saves pages of a PDF document as image files.

## ***DrawSeparations***
Collects Inks present on the PDF page and then rasterizes the page using those Ink channels separately and saving them out as PNG image files.

## ***EPSSeparations***
Creates color separations from a PDF page and then saves out each plate as an EPS file.

## ***GetSeparatedImages***
Collects inks from a PDF page and then creates a collection of grayscale image separations and saves them out as a multi-page TIFF.

## ***ImageEmbedICCProfile***
Rasterizes PDF pages using a specified ICCProfile and saves them out as image files.

## ***ImageExport***
Explores the content of PDF pages and when images are found they are saved out as image files, in the same format as their internal stream.

## ***ImageExtraction***
Explores the content of PDF pages and when images are found they are saved out as Bitmap files.

## ***ImageFromBufferedImage***
Demonstrates creating an image object without binding to a physical file.  The graphic is drawn from a BufferedImage in memory, a common way for Java systems to represent graphics images.

## ***ImageFromByteArray***
This sample program offers an alternate method for drawing a graphic image from a PDF; rather than exporting the embedded image itself, this program interprets a description of the image from a Byte Array and uses that description to create the image as a page in a new PDF file.

## ***ImageImport***
Imports graphics from image files to a PDF page.

## ***ImageResampling***
Explores the content of PDF pages and when images are found they are resampled to have a new resolution.

## ***ImageSoftMask***
Adds an image file to be placed on a PDF page and adds another image file to use as its SoftMask image.

## ***RasterizePage***
Rasterizes the first page of a PDF document.
