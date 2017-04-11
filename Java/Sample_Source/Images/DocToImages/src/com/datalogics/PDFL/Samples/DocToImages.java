package com.datalogics.PDFL.Samples;

/*
 * 
 * This sample demonstrates how to covnert a PDF document into a series of graphic images files,
 * one per page. You can also create a multi-page TIFF file. This program requires that you enter 
 * formatting values manually at the command line. 
 *
 * For more detail, and information about the command line parameters, see the description of the DocToImages
 * sample program on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/converting-pdf-pages-to-images
 * 
 *
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

import java.awt.image.BufferedImage;
import java.io.File;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collection;
import java.util.EnumSet;
import java.util.Iterator;
import java.util.List;
import java.util.ListIterator;

import javax.imageio.ImageIO;

import com.datalogics.PDFL.ColorSpace;
import com.datalogics.PDFL.CompressionCode;
import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.DrawFlags;
import com.datalogics.PDFL.Image;
import com.datalogics.PDFL.ImageCollection;
import com.datalogics.PDFL.ImageSaveParams;
import com.datalogics.PDFL.ImageType;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.Page;
import com.datalogics.PDFL.PageImageParams;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.SmoothFlags;

public class DocToImages
{
    //The wrapper for an ImageCollection class
    //Such wrapper allow us to use same mechanism for datalogics and java images
    private static class ImageList implements List
    {
        public ImageList() {
            imageCollection = new ImageCollection();
        }

        public int size() {
            return imageCollection.getCount();
        }

        public boolean isEmpty() {
            return imageCollection.getCount() == 0;
        }

        public Object get(int index) {
            return imageCollection.getImage(index);
        }

        public boolean add(Object o) {
            imageCollection.append((Image) o);
            return true;
        }

        public void save( String outputfilepath, ImageType imageformat, ImageSaveParams isp) {
            imageCollection.save(outputfilepath, imageformat, isp);
    }

        public boolean contains(Object o) {
            throw new UnsupportedOperationException();
        }

        public Iterator iterator() {
            throw new UnsupportedOperationException();
        }

        public Object[] toArray() {
            throw new UnsupportedOperationException();
        }

        public Object[] toArray(Object[] a) {
            throw new UnsupportedOperationException();
        }

        public boolean remove(Object o) {
            throw new UnsupportedOperationException();
        }

        public boolean containsAll(Collection c) {
            throw new UnsupportedOperationException();
        }

        public boolean addAll(Collection c) {
            throw new UnsupportedOperationException();
        }

        public boolean addAll(int index, Collection c) {
            throw new UnsupportedOperationException();
        }

        public boolean removeAll(Collection c) {
            throw new UnsupportedOperationException();
        }

        public boolean retainAll(Collection c) {
            throw new UnsupportedOperationException();
        }

        public void clear() {
            throw new UnsupportedOperationException();
        }

        public Object set(int index, Object element) {
            throw new UnsupportedOperationException();
        }

        public void add(int index, Object element) {
            throw new UnsupportedOperationException();
        }

        public Object remove(int index) {
            throw new UnsupportedOperationException();
        }

        public int indexOf(Object o) {
            throw new UnsupportedOperationException();
        }

        public int lastIndexOf(Object o) {
            throw new UnsupportedOperationException();
        }

        public ListIterator listIterator() {
            throw new UnsupportedOperationException();
        }

        public ListIterator listIterator(int index) {
            throw new UnsupportedOperationException();
        }

        public List subList(int fromIndex, int toIndex) {
            throw new UnsupportedOperationException();
        }

        @Override
        public boolean equals(Object obj) {
            return super.equals(obj);
        }

        private ImageCollection imageCollection;
    }

    private static void Usage()
    {
        System.out.println("DocToImages Usage:");
        System.out.println("DocToImages [options] inputPDF");
        System.out.println("inputPDF is the name of the PDF file to open");
        System.out.println("Options are one or more of:");
        System.out.println("-importedimage=[dl, java] default=dl");
        System.out.println("-format=[tif, jpg, bmp, png, gif], No default");
        System.out.println("-color=[gray|cmyk|rgb], default=rgb");
        System.out.println("-grayhalftone = [n|y] - is a grayscale image halftone? Only valid for format=tif and color=gray.");
        System.out.println("-first=[y|n] Only convert the first PDFL page, default=n");
        System.out.println("-quality=1-100. Only valid for an output type of jpg, default for jpg is 75");
        System.out.println("resolution=[horiz x vert] ( target DPI, [12-1200], default=300");
        System.out.println("\tA single value sets both horizontal and vertical the same.");
        System.out.println("\tex: resolution=300 -or - resolution=480x640");
        System.out.println("-fontlist=\"dir1;dir2;dirN\"");
        System.out.println("\t(A quoted list of directories for font resources, semicolon delimited, 16 dirs max.");
        System.out.println("-pixels=[width x height] (absolute picture size, no default)");
        System.out.println("\t(If set, output image will be 'width' pixels wide, 'height' pixels tall)");
        System.out.println("\t(Otherwise it will be autoscaled from the PDF page.");
        System.out.println("-compression=[default|no|flatelzw|g3|g4|dct]");
        System.out.println("\tg3 and g4 are only valid for gray images with grayhalftone=y");
        System.out.println("\tflate is only valid for an output format of png");
        System.out.println("\tdct is only valid for an output format of jpg");
        System.out.println("\tnone is only valid for an output format of bmp, png, or tif");
        System.out.println("\tlzw is only valid for an output format of tif");
        System.out.println("-region=[crop|media|art|trim|bleed|bounding]");
        System.out.println("\t(region of PDF page to rasterize, default=crop");
        System.out.println("-pages=[comma separated list or range, or even or odd], i.e., pages=2,4,7-9,14. Default is all.");
        System.out.println("-output=[filename], (default=input filename)");
        System.out.println("-smoothing=[none|text|all], (default=none");
        System.out.println("-reverse=[y|n], Reverse black/white, for gray images only.");
        System.out.println("-blackisone=[y|n], Reverse black/white, for gray tif images only.");
        System.out.println("-multi=[y|n],create one multipage tif file. Only valid for format=tif.");
        System.out.println("-digits=[0-9], size of the fixed digit suffix added to the file name.");
        System.out.println("-asprinted=[y|n], default=n, Renders only printable annotations.");
    }

    static String formatdigits( int numdigits, int counter)
    {
        String counterstring = Integer.toString(counter);
        String ZeroSuffix = "";
        if ( counterstring.length() >= numdigits)
        {
            return( counterstring);
        }

        for (int x = 0; x < numdigits - counterstring.length(); x++)
        {
            ZeroSuffix += "0";
        }

        counterstring = ZeroSuffix + counterstring;
        return (counterstring);
    }

    static String CreateFileSuffix( String filename, ImageType imagetype)
    {
        String  outputfile= "";
        if ( imagetype == ImageType.BMP)
        {
            outputfile = filename + ".bmp";
        }
        else if ( imagetype == ImageType.GIF)
        {
           outputfile = filename + ".gif";
        }
        else if ( imagetype == ImageType.JPEG)
        {
            outputfile = filename + ".jpg";
        }
        else if ( imagetype == ImageType.PNG)
        {
            outputfile = filename + ".png";
        }
        else if ( imagetype == ImageType.TIFF)
        {
            outputfile = filename + ".tif";
        }
        else
        {
            ;  // We should never get here
        }
        return (outputfile);
    }
    
    private static void saveTheImage(Object img, final boolean isDLImageType, final int imageIndex, DocToImagesOptions options, ImageSaveParams isp)
    {
    	 String outputfilepath;
         if (options.getoutputdir() != null)
             outputfilepath = options.getoutputdir() + "/" + options.getoutputfile()+ formatdigits(options.getZeroSuffix(), imageIndex);
         else
             outputfilepath = options.getoutputfile() + formatdigits(options.getZeroSuffix(), imageIndex);

         outputfilepath = CreateFileSuffix(outputfilepath, options.getoutputformat());

         if (isDLImageType)
         {
             ((Image)img).save(outputfilepath, options.getoutputformat(), isp);
             ((Image)img).delete();
         }
         else
         {
             String fileFormat = options.getoutputformat().name();
             File file = new File(outputfilepath);
             List<String> supportedImageFormats = Arrays.asList(ImageIO.getWriterFormatNames());
             if(supportedImageFormats.indexOf(fileFormat) != -1)
             {
                 try
                 {
                     BufferedImage image = (BufferedImage)img;
                     ImageIO.write(image, fileFormat, file);  // ignore returned boolean
                     image.flush();
                 }
                 catch(IOException e)
                 {
                     System.out.println("Write error for " + file.getPath() + ": " + e.getMessage());
                 }
             }
             else
             {
                 System.out.println("Current java version can't write " + fileFormat + " file format");
                 return;
             }
         }
    }
    
    public static void main(String[] args)
    {
        String docpath;
        int i = 0;
        System.out.println("PDF Document to Images Sample:");

        if (args.length < 2 )
        {
            Usage();
            System.exit(1);
        }

        Library lib = new Library();

        DocToImagesOptions options = new DocToImagesOptions();

        if ( args[args.length -1].startsWith("-") || args[args.length - 1].contains("="))
        {
            System.out.println("The last option must be the path to a PDF file.");
            Usage();
            System.exit(1);
        }

        for ( i = 0; i < args.length-1; i++)
        {
            String arg = args[i];
            if ( arg.startsWith("-") && arg.contains("="))
            {
                String opt = arg.substring(arg.indexOf("=") + 1);
                if(arg.startsWith("-importedimage="))
                {
                    if (opt.equals("dl"))
                    {
                        options.setimportedimagetype(DocToImagesOptions.ImportedImageType.DL);
                    } else if (opt.equals("java"))
                    {
                        options.setimportedimagetype(DocToImagesOptions.ImportedImageType.JAVA);
                    }
                    continue;
                }

                if (arg.startsWith("-format="))
                {
                    // Process output format option
                    if (opt.equals("jpg"))
                    {
                        options.setoutputformat(ImageType.JPEG);
                    }
                    else if (opt.equals("tif"))
                    {
                        options.setoutputformat(ImageType.TIFF);
                    }
                    else if (opt.equals("bmp"))
                    {
                        options.setoutputformat(ImageType.BMP);
                    }
                    else if (opt.equals("png"))
                    {
                        options.setoutputformat(ImageType.PNG);
                    }
                    else if (opt.equals("gif"))
                    {
                        options.setoutputformat(ImageType.GIF);
                    }
                    else
                    {
                        System.out.println("Invalid value for the format option. Valid values are jpg, tif, bmp, png, gif");
                        System.exit(1);
                    }
                    continue;
                }

                if (arg.startsWith("-color="))
                {
                    // Process output color option
                    if (opt.equals("gray"))
                    {
                        options.setcolor(ColorSpace.DEVICE_GRAY);
                    }
                    else if (opt.equals("rgb"))
                    {
                        options.setcolor(ColorSpace.DEVICE_RGB);
                    }
                    else if (opt.equals("cmyk"))
                    {
                        options.setcolor(ColorSpace.DEVICE_CMYK);
                    }
                    else
                    {
                        System.out.println("Invalid value for the color option. Valid values are gray, rgb, cmyk");
                        System.exit(1);
                    }
                    continue;
                }

                if (arg.startsWith("-grayhalftone="))
                {
                    // Process grayscale half tone
                    if (opt.equals("y"))
                    	options.setgrayhalftone(true);
                    else if ( opt.equals("n"))
                    	options.setgrayhalftone(false);
                    else
                    {
                        System.out.println("Invalid value for the grayhalftone option.  Valid values are n or y");
                        System.exit(1);
                    }
                    continue;
                }

                if (arg.startsWith("-first="))
                {
                    // Process first page only option
                    if (opt.equals("y"))
                    {
                        options.setfirst(true);
                    }
                    else if (opt.equals("n"))
                    {
                        options.setfirst(false);
                    }
                    else
                    {
                        System.out.println("Invalid value for the first option.  Valid values are 'y' and 'n'.");
                        System.exit(1);
                    }
                    continue;
                }

                if (arg.startsWith("-quality="))
                {
                    // Process jpeg quality option
                    int quality = Integer.parseInt(opt);
                    if (quality < 1 || 1 > 100)
                    {
                        System.out.println("Invalid value for the quality option.  Valid values are 1 through 100.");
                        System.exit(1);
                    }
                    options.setquality(quality);
                    continue;
                }

                if (arg.startsWith("-resolution="))
                {
                    // Process horizontal and/or vertical resolution option
                    if (opt.contains("x"))
                    {
                        String hopt = opt.substring(0, opt.indexOf("x"));
                        String vopt = opt.substring(opt.indexOf("x") + 1);
                        options.sethres(Double.parseDouble(hopt));
                        options.setvres(Double.parseDouble(vopt));
                    }
                    else
                    {
                        double res = Double.parseDouble(opt);
                        options.sethres(res);
                        options.setvres(res);
                    }
                    continue;
                }

                if (arg.startsWith("-fontlist="))
                {
                    // process font search directory list option
                    options.setfontdirs(opt);
                    continue;
                }

                if (arg.startsWith("-pixels="))
                {
                    // process size in pixels option
                    if (opt.contains("x"))
                    {
                        String hopt = opt.substring(0, opt.indexOf("x"));
                        String vopt = opt.substring(opt.indexOf("x") + 1);
                        options.sethpixels(Integer.parseInt(hopt));
                        options.setvpixels(Integer.parseInt(vopt));
                    }
                    else
                    {
                        int pixels = Integer.parseInt(opt);
                        options.sethpixels(pixels);
                        options.setvpixels(pixels);
                    }
                    continue;
                }

                if (arg.startsWith("-compression="))
                {
                    // process TIFF compression option
                	if (opt.equals("default"))
                    {
                        options.setcompress(CompressionCode.DEFAULT);
                    }
                    if (opt.equals("no"))
                    {
                        options.setcompress(CompressionCode.NONE);
                    }
                    else if (opt.equals("lzw"))
                    {
                        options.setcompress(CompressionCode.LZW);
                    }
                    else if (opt.equals("g3"))
                    {
                        options.setcompress(CompressionCode.G3);
                    }
                    else if (opt.equals("dct"))
                    {
                        options.setcompress(CompressionCode.DCT);
                    }
                    else if (opt.equals("flate"))
                    {
                        options.setcompress(CompressionCode.FLATE);
                    }
                    else
                    {
                        System.out.println("Invalid value for the compression option.  Valid values are: default|no|lzw|g3|g4|dct|flate");
                        System.exit(1);
                    }
                    continue;
                }

                if (arg.startsWith("-region="))
                {
                    // process page output region option
                    if (!(opt.equals("crop") || opt.equals("media") || opt.equals("art")) ||
                        opt.equals("trim") || opt.equals("bleed") || opt.equals("bounding"))
                    {
                        System.out.println("Invalid value for the region option.  Value values are: crop|media|art|trim|bleed|bounding");
                        System.exit(1);
                    }
                    options.setpageregion(opt);
                    continue;
                }

                if (arg.startsWith("-pages="))
                {
                    // process pages to be rasterized list option
                    if (opt.equals("even"))
                    {
                        options.setevenpages();
                    }
                    else if (opt.equals("odd"))
                    {
                        options.setoddpages();
                    }
                    else
                    {
                        /*
                         * Get the comma separated page groups and check each
                         * for page ranges.  If page ranges exist then create the
                         * range using the page numbers on either side of the '-' as
                         * the lower and upper bound.
                         */
                        String[] pagegroups = opt.split(",");
                        for (int ix = 0; ix < pagegroups.length; ix++)
                        {
                            String pagegroup = pagegroups[ix];
                            if (pagegroup.contains("-"))
                            {
                                String[] pagerange = pagegroup.split("-");
                                if (pagerange.length != 2 || pagerange[0].equals("") || pagerange[1].equals(""))
                                {
                                    System.out.println("Misformatted page range: " + pagegroup);
                                    System.exit(1);
                                }
                                else
                                {
                                    for (int z = Integer.parseInt(pagerange[0]); z <= Integer.parseInt(pagerange[1]); z++)
                                    {
                                        options.appendpagelist(z);
                                    }
                                }
                            }
                            else
                            {
                                int printpage = Integer.parseInt(pagegroup);
                                options.appendpagelist(printpage);
                            }
                        }
                    }
                    continue;
                }

                if (arg.startsWith("-output="))
                {
                    // process output filename option
                    options.setoutputfile(opt);
                    continue;
                }

                if (arg.startsWith("-smoothing="))
                {
                    // process smoothing option none|text|all
                    if (opt.equals("none"))
                    {
                        options.setsmooth(EnumSet.of(SmoothFlags.NONE));
                    }
                    else if (opt.equals("text"))
                    {
                        options.setsmooth(EnumSet.of(SmoothFlags.TEXT));
                    }
                    else if (opt.equals("all"))
                    {
                        options.setsmooth(EnumSet.of(SmoothFlags.IMAGE, SmoothFlags.LINE_ART, SmoothFlags.TEXT));
                    }
                    else
                    {
                        System.out.println("Invalid value for the smoothing option.  Valid values are none|text|all");
                        System.exit(1);
                    }
                    continue;
                }

                if (arg.startsWith("-reverse="))
                {
                    // process gray reverse option
                    if (opt.equals("y"))
                    {
                        options.setreversegray(true);
                    }
                    else if (opt.equals("n"))
                    {
                        options.setreversegray(false);
                    }
                    else
                    {
                        System.out.println("Invalid value for the reverse option.  Valid values are 'y' and 'n'.");
                        System.exit(1);
                    }
                    continue;
                }

                if (arg.startsWith("-blackisone="))
                {
                    // Photometrically reverse Tiff  option
                    if (opt.equals("y"))
                    {
                        options.setblackisone(true);
                    }
                    else if (opt.equals("n"))
                    {
                        options.setblackisone(false);
                    }
                    else
                    {
                        System.out.println("Invalid value for the blackisone option.  Valid values are 'y' and 'n'.");
                        System.exit(1);
                    }
                    continue;
                }

                if (arg.startsWith("-multi="))
                {
                    // process multipage tiff option
                    if (opt.equals("y"))
                    {
                        options.setmultipage(true);
                    }
                    else if (opt.equals("n"))
                    {
                        options.setmultipage(false);
                    }
                    else
                    {
                        System.out.println("Invalid value for the multi option.  Valid values are 'y' and 'n'.");
                        System.exit(1);
                    }
                    continue;
                }

                if (arg.startsWith("-digits="))
                {
                    // process fixed digit suffix length option
                    int numdigits = Integer.parseInt(opt);
                    if (numdigits < 0 || numdigits > 9)
                    {
                        System.out.println("Invalid value for the digits option. Valid values are 0-9.");
                    }
                    options.setZeroSuffix(numdigits);
                    continue;
                }

                if (arg.startsWith("-asprinted="))
                {
                    // process render only printable annotations option
                    if (opt.equals("y"))
                    {
                        options.setasprinted(true);
                    }
                    else if (opt.equals("n"))
                    {
                        options.setasprinted(false);
                    }
                    else
                    {
                        System.out.println("Invalid value for the asprinted option.  Valid values are 'y' and 'n'.");
                        System.exit(1);
                    }
                    continue;
                }
                // Fall through to this case.
            	System.out.println("Invalid option: " + arg);
            	Usage();
        		System.exit(1);
            }
            else
            {
            	System.out.println("Invalid option: " + arg);
            	Usage();
            	System.exit(1);
            }
        }
        docpath = args[args.length - 1];
        // Now crosscheck and verify the combinations of the options we have entered.
        int errorcount = 0;

        if (options.checkSavedImageType() == false)
        {
            System.out.println("java image can not be saved as tiff image. To save image in tiff format use DL image type");
            errorcount++;
        }

        if (options.checkformattype() == false)
        {
            System.out.println("format must be set to tif, jpg, bmp, png, or gif");
            errorcount++;
        }

        if (options.checkcolorspacegrayhalftone() == false)
        {
            System.out.println("grayhalftone can only be set to 1 for a value of 'gray' for color");
            errorcount++;
        }

        if (options.checkformatcompressiontiff() == false)
        {
            System.out.println("Compression can only be this value for a format value of 'tif'");
            errorcount++;
        }

        if (options.checkformatcompressionpng() == false)
        {
            System.out.println("Compression can only be this value for a format value of 'png'");
            errorcount++;
        }

        if (options.checkformatcompressionjpg() == false)
        {
        	System.out.println("Compression can only be this value for a format value of 'jpg'");
            errorcount++;
        }

        if (options.checkcompressionnone() == false)
        {
            System.out.println("Compression can only be this value for a format value of 'bmp' or 'png' or 'tif'");
            errorcount++;
        }

        if (options.checkreversegray() == false)
        {
            System.out.println("reverse can only be set to 'y' for a value of 'gray' for color");
            errorcount++;
        }

        if (options.checkblackisoneformat() == false)
        {
            System.out.println("blackisone can only be set to 'y' for a format value of 'tif'");
            errorcount++;
        }

        if (options.checktiffcompressgrayhalftone() == false)
        {
            System.out.println("compression can only be set to 'g3' or 'g4' if grayhalftone is set to 'y' and color is set to 'gray'");
            errorcount++;
        }

        if (options.checkgrayhalftoneformat() == false)
        {
            System.out.println("grayhalftone can only be set to 'y' for format set to tif");
            errorcount++;
        }

        if (options.checkqualityimagetype() == false)
        {
            System.out.println("quality can only be set to a value other than 0 for a format value of 'jpg'");
            errorcount++;
        }

        if (options.checkfirstonlypagerange() == false)
        {
            System.out.println("The pages option cannot be specified if firstonly is set to 'y'");
            errorcount++;
        }

        if (options.checkfirstonlyevenodd() == false)
        {
            System.out.println("The pages option cannot be set to 'even' or 'odd' if firstonly is set to 'y'");
            errorcount++;
        }

        if (options.checkmultiformat() == false)
        {
            System.out.println("The multi option can only be set to 'y' if format is set to 'tif'");
            errorcount++;
        }

        if ( options.checkfontdirs() == false )
        {
        	errorcount++;
        }

        if (errorcount > 0)
        {
            System.out.println("Because of command line option errors, no image processing will be performed.");
            System.exit(1);
        }

        lib.delete();
        lib = new Library(options.getfontdirs());

        try {
            Document pdfdocument = null;
            int numpages = 0;
            List<Integer>   pagelist = new ArrayList<Integer>(0);
            try
            {
                pdfdocument = new Document(docpath);
                numpages = pdfdocument.getNumPages();
            }
            catch (Exception ex)
            {
                System.out.println("Error opening PDF document " + docpath + ":" + ex.getMessage());
                System.exit(1);
            }

            /*
             * Now that we know we can open the PDF file, use the filename to create what will be the basis
             * of the output filename and directory name.
             */

            File	outputfile;
            if (options.getoutputfile() == null)
            {
                outputfile = new File(docpath);
            }
            else
            {
                outputfile = new File(options.getoutputfile());
            }
            options.setoutputdir(outputfile.getParent());
            String filename = outputfile.getName();
            int ix = filename.lastIndexOf('.'); // If the filename has a suffix, strip it off.
            if ( ix > 0 )
            {
                String    basefilename = filename.substring(0, filename.indexOf('.'));
                options.setoutputfile(basefilename);
            }
            else
                options.setoutputfile(filename);

            if (options.checkpagelist(numpages) == false)
            {
                System.out.println("The list of pages given in the 'pages' option is outside the number of pages in this PDF document: " + pdfdocument.getNumPages());
                System.exit(1);
            }

            /*
             * If the list of pages were not populated from the command line options, or if the options have specified
             * all odd or all even, then populate the list here and then reget the list.
             */

            pagelist = options.getpagelist();
            if (pagelist.isEmpty())
            {
                if (options.getfirst() == true) // First page only.
                {
                    numpages = 1; // Will modify the operation of the loop below.
                }
                for (i = 0; i < numpages; i++)
                {
                    // Remember in the Doc object page #'s start with 0, but physically they start with 1.
                    if ((options.onlyodd() == false && options.onlyeven() == false) ||  // all pages
                        (options.onlyodd() == true && (((i + 1) % 2) == 1)) ||          // this is an odd page
                        (options.onlyeven() == true && (((i + 1) % 2) == 0)))           // this is an even page
                    {
                        options.appendpagelist(i);
                    }
                }
                pagelist = options.getpagelist();
            }

            PageImageParams pip = new PageImageParams();
            pip.setImageColorSpace( options.getcolor());
            pip.setSmoothing(options.getsmooth());
            pip.setPixelWidth(options.gethpixels());
            pip.setPixelHeight(options.getvpixels());
            pip.setHorizontalResolution(options.gethres());
            pip.setVerticalResolution(options.getvres());

            if( options.getasprinted() == true )
            {
                pip.setPageDrawFlags(EnumSet.of(DrawFlags.DO_LAZY_ERASE, DrawFlags.USE_ANNOT_FACES, DrawFlags.IS_PRINTING));
            }
            else
            {
                pip.setPageDrawFlags(EnumSet.of(DrawFlags.DO_LAZY_ERASE, DrawFlags.USE_ANNOT_FACES));
            }

            final boolean isDLImageType = options.getimportedimagetype() == DocToImagesOptions.ImportedImageType.DL;
            List Pageimagecollection = isDLImageType ? new ImageList() : new ArrayList<BufferedImage>();

            if (options.getoutputformat() == ImageType.TIFF && options.getcompress() == CompressionCode.NONE)
            {
                options.setcompress(CompressionCode.LZW); // Default for TIF
            }

            ImageSaveParams isp = new ImageSaveParams();
            isp.setHalftoneGrayImages(options.getgrayhalftone());
            isp.setCompression(options.getcompress());
            if ( options.getoutputformat() == ImageType.JPEG )
            {
                isp.setJPEGQuality(options.getquality());
            }
            isp.setReverseGray(options.getreversegray());
            isp.setTIFFBlackIsOne(options.getblackisone());
            
            for (i = 0; i < pagelist.size(); i++) // Get the images of the PDF pages to create an image collection.
            {
                Page docpage = pdfdocument.getPage(pagelist.get(i));
                Rect PageRect = null;
                if (options.getpageregion().equals("crop"))
                {
                    PageRect = docpage.getCropBox();
                }
                else if (options.getpageregion().equals("media"))
                {
                    PageRect = docpage.getMediaBox();
                }
                else if (options.getpageregion().equals("art"))
                {
                    PageRect = docpage.getArtBox();
                }
                else if (options.getpageregion().equals("bounding"))
                {
                    PageRect = docpage.getBBox();
                }
                else if (options.getpageregion().equals("bleed"))
                {
                    PageRect = docpage.getBleedBox();
                }
                else if (options.getpageregion().equals("trim"))
                {
                    PageRect = docpage.getTrimBox();
                }
                else
                {
                    System.out.println("Unknown page region option.");
                    System.exit(1);
                }

                if (options.getmultipage() == true)
                {
                	Pageimagecollection.add(isDLImageType ? docpage.getImage(PageRect, pip) : docpage.makeBufferedImage(PageRect, pip));
                } else {
                	int x = i + 1;
                	if (isDLImageType)
                	{
                		saveTheImage(docpage.getImage(PageRect, pip), isDLImageType, x, options, isp);
                	} else
                	{
                		saveTheImage(docpage.makeBufferedImage(PageRect, pip), isDLImageType, x, options, isp);
                	}
                }
            }
            /*
             * Pageimagecollection now contains, as the name states,
             * a collection of images created from the PDF pages according
             * to the user's options.  Now to post process them to the image.
             * type according to the user's options.
             */

            if (options.getmultipage() == true)
            {
                String outputfilepath;
                if (options.getoutputdir() != null)
                    outputfilepath = options.getoutputdir()+"/"+options.getoutputfile();
                else
                    outputfilepath = options.getoutputfile();

                outputfilepath = CreateFileSuffix(outputfilepath, options.getoutputformat());
                ((ImageList)Pageimagecollection).save(outputfilepath, options.getoutputformat(), isp);
            }
        }
        finally
        {
            lib.delete();
        }
    }
}
