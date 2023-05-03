using System;
using System.Collections.Generic;
using Datalogics.PDFL;

/*
 * 
 * This sample demonstrates how to covnert a PDF document into a series of graphic images files,
 * one per page. You can also create a mult-page TIFF file. This program requires that you enter 
 * formatting values manually at the command line. 
 *
 * For more detail, and information about the command line parameters, see the description of the DocToImages
 * sample program on our Developer's site,
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/converting-pdf-pages-to-images/#doctoimages
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace DocToImages
{
    public class DocToImagesOptions
    {
        ImageType outputformat = ImageType.Invalid;
        ColorSpace color = ColorSpace.DeviceRGB;
        bool grayhalftone;
        bool firstpageonly;
        int quality;
        double hres = 300.0;
        double vres = 300.0;
        List<string> fontdirs = new List<string>(0);
        int hpixelsize;
        int vpixelsize;
        CompressionCode compress = CompressionCode.Default;
        string pageregion = "crop";
        List<int> PageList = new List<int>(0);
        int evenoddpages; // 1 = all odd pages, 2 = all even pages.
        string outputfilename = "";
        string outputdirname = "";
        SmoothFlags smoothingflags = SmoothFlags.None;
        bool reversegray;
        bool blackisone;
        bool multipage;
        int ZeroSuffix;
        bool asprinted;

        public void setoutputformat(ImageType format)
        {
            outputformat = format;
        }

        public ImageType getoutputformat()
        {
            return (outputformat);
        }

        public void setcolor(ColorSpace outputcolor)
        {
            color = outputcolor;
        }

        public ColorSpace getcolor()
        {
            return (color);
        }

        public void setgrayhalftone(bool halftone)
        {
            grayhalftone = halftone;
        }

        public bool getgrayhalftone()
        {
            return (grayhalftone);
        }

        public void setfirst(bool firstonly)
        {
            firstpageonly = firstonly;
        }

        public bool getfirst()
        {
            return (firstpageonly);
        }

        public void setquality(int q)
        {
            quality = q;
        }

        public int getquality()
        {
            return (quality);
        }

        public void sethres(double h)
        {
            hres = h;
        }

        public double gethres()
        {
            return (hres);
        }

        public void setvres(double v)
        {
            vres = v;
        }

        public double getvres()
        {
            return (vres);
        }

        public void setfontdirs(string fd)
        {
            string[] fontdirarray = fd.Split(';');
            for (int i = 0; i < fontdirarray.Length; i++)
            {
                fontdirs.Add(fontdirarray[i]);
            }
        }

        public List<string> getfontdirs()
        {
            return (fontdirs);
        }

        public void sethpixels(int h)
        {
            hpixelsize = h;
        }

        public int gethpixels()
        {
            return (hpixelsize);
        }

        public void setvpixels(int v)
        {
            vpixelsize = v;
        }

        public int getvpixels()
        {
            return (vpixelsize);
        }

        public void setcompress(CompressionCode cc)
        {
            compress = cc;
        }

        public CompressionCode getcompress()
        {
            return (compress);
        }

        public void setpageregion(string region)
        {
            pageregion = region;
        }

        public string getpageregion()
        {
            return (pageregion);
        }

        public void appendpagelist(int pageno)
        {
            PageList.Add(pageno);
        }

        /*
         * If this list is empty and the evenoddpages flag is set to 0 then the entire document
         * will be used.
         */

        public List<int> getpagelist()
        {
            return (PageList);
        }

        /*
         * Once the PDF page is opened the pagecount will be derived and then the evenoddpages flag will be used
         * to determine which pages are to be used.  If this flag is 0, and the Pagelist is empty, then the entire
         * document will be used.
         */

        public void setevenpages()
        {
            evenoddpages = 2;
        }

        public void setoddpages()
        {
            evenoddpages = 1;
        }

        public bool onlyodd()
        {
            return (evenoddpages == 1);
        }

        public bool onlyeven()
        {
            return (evenoddpages == 2);
        }

        public void setoutputfile(string outputfile)
        {
            outputfilename = outputfile;
        }

        public string getoutputfile()
        {
            return (outputfilename);
        }

        public void setoutputdir(string outputdir)
        {
            outputdirname = outputdir;
        }

        public string getoutputdir()
        {
            return (outputdirname);
        }

        public void setsmooth(SmoothFlags flags)
        {
            smoothingflags = flags;
        }

        public SmoothFlags getsmooth()
        {
            return (smoothingflags);
        }

        public void setreversegray(bool reverse)
        {
            reversegray = reverse;
        }

        public bool getreversegray()
        {
            return (reversegray);
        }

        public void setblackisone(bool isblackone)
        {
            blackisone = isblackone;
        }

        public bool getblackisone()
        {
            return (blackisone);
        }

        public void setmultipage(bool multi)
        {
            multipage = multi;
        }

        public bool getmultipage()
        {
            return (multipage);
        }

        public void setZeroSuffix(int numdigs)
        {
            ZeroSuffix = numdigs;
        }

        public int getZeroSuffix()
        {
            return (ZeroSuffix);
        }

        public void setasprinted(bool asp)
        {
            asprinted = asp;
        }

        public bool getasprinted()
        {
            return (asprinted);
        }

        public bool checkformattype()
        {
            return (outputformat != ImageType.Invalid);
        }

        public bool checkcolorspacegrayhalftone()
        {
            if (grayhalftone)
                return (color == ColorSpace.DeviceGray);
            else
                return true;
        }

        public bool checkformatcompressiontiff()
        {
            if (compress == CompressionCode.G3 || compress == CompressionCode.G4 || compress == CompressionCode.LZW)
                return (outputformat == ImageType.TIFF);
            else
                return true;
        }

        public bool checkformatcompressionpng()
        {
            if (compress == CompressionCode.FLATE)
                return (outputformat == ImageType.PNG);
            else
                return true;
        }

        public bool checkformatcompressionjpg()
        {
            if (compress == CompressionCode.DCT)
                return (outputformat == ImageType.JPEG);
            else
                return true;
        }

        public bool checkcompressionnone()
        {
            if (compress == CompressionCode.NONE)
                return (outputformat == ImageType.BMP || outputformat == ImageType.PNG ||
                        outputformat == ImageType.TIFF);
            else
                return true;
        }

        public bool checkreversegray()
        {
            if (reversegray)
                return (color == ColorSpace.DeviceGray);
            else
                return true;
        }

        public bool checkblackisoneformat()
        {
            if (blackisone)
                return (outputformat == ImageType.TIFF);
            else
                return true;
        }

        public bool checktiffcompressgrayhalftone()
        {
            if (compress == CompressionCode.G3 || compress == CompressionCode.G4)
                return (color == ColorSpace.DeviceGray && grayhalftone);
            else if (color == ColorSpace.DeviceGray && grayhalftone && outputformat == ImageType.TIFF)
                return (compress == CompressionCode.G3 || compress == CompressionCode.G4);
            else
                return true;
        }

        public bool checkqualityimagetype()
        {
            if (quality != 0)
                return (outputformat == ImageType.JPEG);
            else
                return true;
        }

        public bool checkgrayhalftoneformat()
        {
            if (grayhalftone)
                return (outputformat == ImageType.TIFF);
            else
                return true;
        }

        public bool checkfirstonlypagerange()
        {
            if (firstpageonly)
                return (PageList.Count == 0);
            else
                return true;
        }

        public bool checkfirstonlyevenodd()
        {
            if (firstpageonly)
                return (evenoddpages == 0);
            else
                return true;
        }

        public bool checkmultiformat()
        {
            if (multipage)
                return (outputformat == ImageType.TIFF);
            else
                return true;
        }

        public bool checkpagelist(int pagemax)
        {
            bool rc = true;
            if (PageList.Count != 0)
            {
                for (int i = 0; i < PageList.Count; i++)
                {
                    if (PageList[i] > pagemax)
                    {
                        rc = false;
                        break;
                    }
                }
            }

            return (rc);
        }

        public bool checkfontdirs()
        {
            bool rc = true;
            for (int i = 0; i < fontdirs.Count; i++)
            {
                if (System.IO.Directory.Exists(fontdirs[i]) == false)
                {
                    Console.WriteLine("The directory path " + fontdirs[i] +
                                      " listed in the fontlist options is not a valid directory.");
                    rc = false;
                }
            }

            return (rc);
        }
    }

    class DocToImages
    {
        static void Usage()
        {
            Console.WriteLine("DocToImages Usage:");
            Console.WriteLine("DocToImages [options] inputPDF");
            Console.WriteLine("inputPDF is the name of the PDF file to open");
            Console.WriteLine("Options are one or more of:");
            Console.WriteLine("-format=[tif, jpg, bmp, png, gif], No default");
            Console.WriteLine("-color=[gray|cmyk|rgb], default=rgb");
            Console.WriteLine(
                "-grayhalftone = [n|y] - is a grayscale image halftone? Only valid for format=tif and color=gray.");
            Console.WriteLine("-first=[y|n] Only convert the first PDFL page, default=n");
            Console.WriteLine("-quality=1-100. Only valid for an output type of jpg, default for jpg is 75");
            Console.WriteLine("-resolution=[horiz x vert] ( target DPI, [12-1200], default=300");
            Console.WriteLine("\tA single value sets both horizontal and vertical the same.");
            Console.WriteLine("\tex: resolution=300 -or - resolution=480x640");
            Console.WriteLine("-fontlist=\"dir1;dir2;dirN\"");
            Console.WriteLine("\t(A quoted list of directories for font resources, semicolon delimited, 16 dirs max.");
            Console.WriteLine("-pixels=[width x height] (absolute picture size, no default)");
            Console.WriteLine("\t(If set, output image will be 'width' pixels wide, 'height' pixels tall)");
            Console.WriteLine("\t(Otherwise it will be autoscaled from the PDF page.");
            Console.WriteLine("-compression=[default|no|flatelzw|g3|g4|dct]");
            Console.WriteLine("\tg3 and g4 are only valid for gray images with grayhalftone=y");
            Console.WriteLine("\tflate is only valid for an output format of png");
            Console.WriteLine("\tdct is only valid for an output format of jpg");
            Console.WriteLine("\tnone is only valid for an output format of bmp, png, or tif");
            Console.WriteLine("\tlzw is only valid for an output format of tif");
            Console.WriteLine("-region=[crop|media|art|trim|bleed|bounding]");
            Console.WriteLine("\t(region of PDF page to rasterize, default=crop");
            Console.WriteLine(
                "-pages=[comma separated list or range, or even or odd], i.e., pages=2,4,7-9,14. Default is all.");
            Console.WriteLine("-output=[filename], (default=input filename)");
            Console.WriteLine("-smoothing=[none|text|all], (default=none");
            Console.WriteLine("-reverse=[y|n], Reverse black/white, for gray images only.");
            Console.WriteLine("-blackisone=[y|n], Reverse black/white, for gray tif images only.");
            Console.WriteLine("-multi=[y|n],create one multipage tif file. Only valid for format=tif.");
            Console.WriteLine("-digits=[0-9], size of the fixed digit suffix added to the file name.");
            Console.WriteLine("-asprinted=[y|n], default=n, Renders only printable annotations.");
        }

        static string CreateFileSuffix(string filename, ImageType imagetype)
        {
            string outputfile = "";
            switch (imagetype)
            {
                case ImageType.BMP:
                    outputfile = filename + ".bmp";
                    break;

                case ImageType.GIF:
                    outputfile = filename + ".gif";
                    break;

                case ImageType.JPEG:
                    outputfile = filename + ".jpg";
                    break;

                case ImageType.PNG:
                    outputfile = filename + ".png";
                    break;

                case ImageType.TIFF:
                    outputfile = filename + ".tif";
                    break;
            }

            return (outputfile);
        }

        static string formatdigits(int numdigits, int counter)
        {
            string counterstring = counter.ToString();
            string ZeroSuffix = "";
            if (counterstring.Length >= numdigits)
            {
                return (counterstring);
            }

            for (int x = 0; x < numdigits - counterstring.Length; x++)
            {
                ZeroSuffix += "0";
            }

            counterstring = ZeroSuffix + counterstring;
            return (counterstring);
        }

        static void saveTheImage(Image saveimage, int imageIndex, DocToImagesOptions options, ImageSaveParams isp)
        {
            string outputfilepath;
            if (options.getoutputdir() != "")
                outputfilepath = options.getoutputdir() + "/" + options.getoutputfile() +
                                 formatdigits(options.getZeroSuffix(), imageIndex);
            else
                outputfilepath = options.getoutputfile() + formatdigits(options.getZeroSuffix(), imageIndex);

            outputfilepath = CreateFileSuffix(outputfilepath, options.getoutputformat());
            try
            {
                saveimage.Save(outputfilepath, options.getoutputformat(), isp);
                saveimage.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot write an image to a file: " + ex.Message);
            }
        }

        static void Main(string[] args)
        {
            string docpath;
            Console.WriteLine("PDF Document to Images Sample:");
            if (args.Length < 2)
            {
                Usage();
                Environment.Exit(1);
            }

            DocToImagesOptions options = new DocToImagesOptions();
            if (args[args.Length - 1].StartsWith("-") || args[args.Length - 1].Contains("="))
            {
                Console.WriteLine("The last option must be the path to a PDF file.");
                Usage();
                Environment.Exit(1);
            }

            for (int i = 0; i < args.Length - 1; i++)
            {
                String arg = args[i];
                if (arg.StartsWith("-") && arg.Contains("="))
                {
                    String opt = arg.Substring(arg.IndexOf("=", StringComparison.Ordinal) + 1);
                    if (arg.StartsWith("-format="))
                    {
                        // Process output format option
                        if (opt.Equals("jpg"))
                        {
                            options.setoutputformat(ImageType.JPEG);
                        }
                        else if (opt.Equals("tif"))
                        {
                            options.setoutputformat(ImageType.TIFF);
                        }
                        else if (opt.Equals("bmp"))
                        {
                            options.setoutputformat(ImageType.BMP);
                        }
                        else if (opt.Equals("png"))
                        {
                            options.setoutputformat(ImageType.PNG);
                        }
                        else if (opt.Equals("gif"))
                        {
                            options.setoutputformat(ImageType.GIF);
                        }
                        else
                        {
                            Console.WriteLine(
                                "Invalid value for the format option. Valid values are jpg, tif, bmp, png, gif");
                            Environment.Exit(1);
                        }

                        continue;
                    }

                    if (arg.StartsWith("-color="))
                    {
                        // Process output color option
                        if (opt.Equals("gray"))
                        {
                            options.setcolor(ColorSpace.DeviceGray);
                        }
                        else if (opt.Equals("rgb"))
                        {
                            options.setcolor(ColorSpace.DeviceRGB);
                        }
                        else if (opt.Equals("cmyk"))
                        {
                            options.setcolor(ColorSpace.DeviceCMYK);
                        }
                        else
                        {
                            Console.WriteLine("Invalid value for the color option. Valid values are gray, rgb, cmyk");
                            Environment.Exit(1);
                        }

                        continue;
                    }

                    if (arg.StartsWith("-grayhalftone="))
                    {
                        // Process grayscale half tone            
                        if (opt.Equals("y"))
                            options.setgrayhalftone(true);
                        else if (opt.Equals("n"))
                            options.setgrayhalftone(false);
                        else
                        {
                            Console.WriteLine("Invalid value for the grayhalftone option.  Valid values are n or y");
                            Environment.Exit(1);
                        }

                        continue;
                    }

                    if (arg.StartsWith("-first="))
                    {
                        // Process first page only option
                        if (opt.Equals("y"))
                        {
                            options.setfirst(true);
                        }
                        else if (opt.Equals("n"))
                        {
                            options.setfirst(false);
                        }
                        else
                        {
                            Console.WriteLine("Invalid value for the first option.  Valid values are 'y' and 'n'.");
                            Environment.Exit(1);
                        }

                        continue;
                    }

                    if (arg.StartsWith("-quality="))
                    {
                        // Process jpeg quality option
                        int quality = int.Parse(opt);
                        if (quality < 1)
                        {
                            Console.WriteLine("Invalid value for the quality option.  Valid values are 1 through 100.");
                            Environment.Exit(1);
                        }

                        options.setquality(quality);
                        continue;
                    }

                    if (arg.Contains("resolution="))
                    {
                        // Process horizontal and/or vertical resolution option
                        if (opt.Contains("x"))
                        {
                            string hopt = opt.Substring(0, opt.IndexOf("x", StringComparison.Ordinal));
                            String vopt = opt.Substring(opt.IndexOf("x", StringComparison.Ordinal) + 1);
                            options.sethres(double.Parse(hopt));
                            options.setvres(double.Parse(vopt));
                        }
                        else
                        {
                            double res = double.Parse(opt);
                            options.sethres(res);
                            options.setvres(res);
                        }

                        continue;
                    }

                    if (arg.StartsWith("-fontlist="))
                    {
                        // process font search directory list option
                        options.setfontdirs(opt);
                        continue;
                    }

                    if (arg.StartsWith("-pixels="))
                    {
                        // process size in pixels option
                        if (opt.Contains("x"))
                        {
                            string hopt = opt.Substring(0, opt.IndexOf("x", StringComparison.Ordinal));
                            String vopt = opt.Substring(opt.IndexOf("x", StringComparison.Ordinal) + 1);
                            options.sethpixels(int.Parse(hopt));
                            options.setvpixels(int.Parse(vopt));
                        }
                        else
                        {
                            int pixels = int.Parse(opt);
                            options.sethpixels(pixels);
                            options.setvpixels(pixels);
                        }

                        continue;
                    }

                    if (arg.StartsWith("-compression="))
                    {
                        // process TIFF compression option
                        if (opt.Equals("no"))
                        {
                            options.setcompress(CompressionCode.NONE);
                        }
                        else if (opt.Equals("lzw"))
                        {
                            options.setcompress(CompressionCode.LZW);
                        }
                        else if (opt.Equals("g3"))
                        {
                            options.setcompress(CompressionCode.G3);
                        }
                        else if (opt.Equals("g4"))
                        {
                            options.setcompress(CompressionCode.G4);
                        }
                        else if (opt.Equals("flate"))
                        {
                            options.setcompress(CompressionCode.FLATE);
                        }
                        else if (opt.Equals("dct"))
                        {
                            options.setcompress(CompressionCode.DCT);
                        }
                        else if (opt.Equals("default"))
                        {
                            options.setcompress(CompressionCode.Default);
                        }
                        else if (opt.Equals("none"))
                        {
                            options.setcompress(CompressionCode.NONE);
                        }
                        else
                        {
                            Console.WriteLine(
                                "Invalid value for the compression option.  Valid values are: no|lzw|g3|g4|jpg");
                            Environment.Exit(1);
                        }

                        continue;
                    }

                    if (arg.StartsWith("-region="))
                    {
                        // process page output region option
                        if (!(opt.Equals("crop") || opt.Equals("media") || opt.Equals("art") ||
                              opt.Equals("trim") || opt.Equals("bleed") || opt.Equals("bounding")))
                        {
                            Console.WriteLine(
                                "Invalid value for the region option.  Value values are: crop|media|art|trim|bleed|bounding");
                            Environment.Exit(1);
                        }

                        options.setpageregion(opt);
                        continue;
                    }

                    if (arg.Contains("pages="))
                    {
                        // process pages to be rasterized list option
                        if (opt.Equals("even"))
                        {
                            options.setevenpages();
                        }
                        else if (opt.Equals("odd"))
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
                            string[] pagegroups = opt.Split(',');
                            for (int ix = 0; ix < pagegroups.Length; ix++)
                            {
                                string pagegroup = pagegroups[ix];
                                if (pagegroup.Contains("-"))
                                {
                                    string[] pagerange = pagegroup.Split('-');
                                    if (pagerange.Length != 2 || pagerange[0].Equals("") || pagerange[1].Equals(""))
                                    {
                                        Console.WriteLine("Misformatted page range: " + pagegroup);
                                        Environment.Exit(1);
                                    }
                                    else
                                    {
                                        for (int z = int.Parse(pagerange[0]); z <= int.Parse(pagerange[1]); z++)
                                        {
                                            options.appendpagelist(z);
                                        }
                                    }
                                }
                                else
                                {
                                    int printpage = int.Parse(pagegroup);
                                    options.appendpagelist(printpage);
                                }
                            }
                        }

                        continue;
                    }

                    if (arg.StartsWith("-output="))
                    {
                        // process output filename option
                        options.setoutputfile(opt);
                        continue;
                    }

                    if (arg.StartsWith("-smoothing="))
                    {
                        // process smoothing option none|text|all
                        if (opt.Equals("none"))
                        {
                            options.setsmooth(SmoothFlags.None);
                        }
                        else if (opt.Equals("text"))
                        {
                            options.setsmooth(SmoothFlags.Text);
                        }
                        else if (opt.Equals("all"))
                        {
                            options.setsmooth(SmoothFlags.Image | SmoothFlags.LineArt | SmoothFlags.Text);
                        }
                        else
                        {
                            Console.WriteLine(
                                "Invalid value for the smoothing option.  Valid values are none|text|all");
                            Environment.Exit(1);
                        }

                        continue;
                    }

                    if (arg.StartsWith("-reverse="))
                    {
                        // process gray reverse option
                        if (opt.Equals("y"))
                        {
                            options.setreversegray(true);
                        }
                        else if (opt.Equals("n"))
                        {
                            options.setreversegray(false);
                        }
                        else
                        {
                            Console.WriteLine("Invalid value for the reverse option.  Valid values are 'y' and 'n'.");
                            Environment.Exit(1);
                        }

                        continue;
                    }

                    if (arg.StartsWith("-blackisone="))
                    {
                        // Photometrically reverse Tiff  option
                        if (opt.Equals("y"))
                        {
                            options.setblackisone(true);
                        }
                        else if (opt.Equals("n"))
                        {
                            options.setblackisone(false);
                        }
                        else
                        {
                            Console.WriteLine(
                                "Invalid value for the blackisone option.  Valid values are 'y' and 'n'.");
                            Environment.Exit(1);
                        }

                        continue;
                    }

                    if (arg.StartsWith("-multi="))
                    {
                        // process multipage tif option
                        if (opt.Equals("y"))
                        {
                            options.setmultipage(true);
                        }
                        else if (opt.Equals("n"))
                        {
                            options.setmultipage(false);
                        }
                        else
                        {
                            Console.WriteLine("Invalid value for the multi option.  Valid values are 'y' and 'n'.");
                            Environment.Exit(1);
                        }

                        continue;
                    }

                    if (arg.StartsWith("-digits="))
                    {
                        // process fixed digit suffix length option
                        int numdigits = int.Parse(opt);
                        if (numdigits < 0 || numdigits > 9)
                        {
                            Console.WriteLine("Invalid value for the digits option. Valid values are 0-9.");
                        }

                        options.setZeroSuffix(numdigits);
                        continue;
                    }

                    if (arg.StartsWith("-asprinted="))
                    {
                        // process render only printable annotations option
                        if (opt.Equals("y"))
                        {
                            options.setasprinted(true);
                        }
                        else if (opt.Equals("n"))
                        {
                            options.setasprinted(false);
                        }
                        else
                        {
                            Console.WriteLine("Invalid value for the asprinted option.  Valid values are 'y' and 'n'.");
                            Environment.Exit(1);
                        }

                        continue;
                    }

                    Console.WriteLine("Invalid option: " + arg);
                    Usage();
                    Environment.Exit(1);
                }
                else
                {
                    Console.WriteLine("Invalid option: " + arg);
                    Usage();
                    Environment.Exit(1);
                }
            }

            docpath = args[args.Length - 1];
            // Now crosscheck and verify the combinations of the options we have entered.
            int errorcount = 0;

            if (options.checkformattype() == false)
            {
                Console.WriteLine("format must be set to tif, jpg, bmp, png, or gif");
                errorcount++;
            }

            if (options.checkcolorspacegrayhalftone() == false)
            {
                Console.WriteLine("grayhalftone can only be set to 1 for a value of 'gray' for color");
                errorcount++;
            }

            if (options.checkformatcompressiontiff() == false)
            {
                Console.WriteLine("Compression can only be this value for a format value of 'tif'");
                errorcount++;
            }

            if (options.checkformatcompressionpng() == false)
            {
                Console.WriteLine("Compression can only be this value for a format value of 'png'");
                errorcount++;
            }

            if (options.checkformatcompressionjpg() == false)
            {
                Console.WriteLine("Compression can only be this value for a format value of 'jpg'");
                errorcount++;
            }

            if (options.checkcompressionnone() == false)
            {
                Console.WriteLine("Compression can only be this value for a format value of 'bmp' or 'png' or 'tif'");
                errorcount++;
            }

            if (options.checkreversegray() == false)
            {
                Console.WriteLine("reverse can only be set to 'y' for a value of 'gray' for color");
                errorcount++;
            }

            if (options.checkblackisoneformat() == false)
            {
                Console.WriteLine("blackisone can only be set to 'y' for a format value of 'tif'");
                errorcount++;
            }

            if (options.checktiffcompressgrayhalftone() == false)
            {
                Console.WriteLine(
                    "compression can only be set to 'g3' or 'g4' if grayhalftone is set to 'y' and color is set to 'gray'");
                errorcount++;
            }


            if (options.checkgrayhalftoneformat() == false)
            {
                Console.WriteLine("grayhalftone can only be set to 'y' for format set to tif");
                errorcount++;
            }

            if (options.checkqualityimagetype() == false)
            {
                Console.WriteLine("quality can only be set to a value other than 0 for a format value of 'jpg'");
                errorcount++;
            }

            if (options.checkfirstonlypagerange() == false)
            {
                Console.WriteLine("The pages option cannot be specified if firstonly is set to 'y'");
                errorcount++;
            }

            if (options.checkfirstonlyevenodd() == false)
            {
                Console.WriteLine("The pages option cannot be set to 'even' or 'odd' if firstonly is set to 'y'");
                errorcount++;
            }

            if (options.checkmultiformat() == false)
            {
                Console.WriteLine("The multi option can only be set to 'y' if format is set to 'tif'");
                errorcount++;
            }

            if (options.checkfontdirs() != true)
            {
                errorcount++;
            }

            if (errorcount > 0)
            {
                Console.WriteLine("Because of command line option errors, no image processing will be performed.");
                Environment.Exit(1);
            }


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library(options.getfontdirs()))
            {
                Document pdfdocument = null;
                int numpages = 0;
                try
                {
                    pdfdocument = new Document(docpath);
                    numpages = pdfdocument.NumPages;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error opening PDF document " + docpath + ":" + ex.Message);
                    Environment.Exit(1);
                }

                /*
                 * Now that we know we can open the PDF file, use the filename to create what will be the basis
                 * of the output filename and directory name.
                 */

                string outputfilename;
                if (options.getoutputfile() == "")
                {
                    outputfilename = docpath;
                }
                else
                {
                    outputfilename = options.getoutputfile();
                }

                options.setoutputdir(System.IO.Path.GetDirectoryName(outputfilename));
                string[] basefilename =
                    System.IO.Path.GetFileName(outputfilename).Split('.'); // split off the .pdf suffix.
                options.setoutputfile(basefilename[0]);


                if (options.checkpagelist(numpages) == false)
                {
                    Console.WriteLine(
                        "The list of pages given in the 'pages' option is outside the number of pages in this PDF document: " +
                        pdfdocument.NumPages);
                    Environment.Exit(1);
                }

                /*
                 * If the list of pages were not populated from the command line options, or if the options have specified
                 * all odd or all even, then populate the list here and then reget the list.
                 */

                List<int> pagelist = options.getpagelist();
                if (pagelist.Count == 0)
                {
                    if (options.getfirst()) // First page only.
                    {
                        numpages = 1; // Will modify the operation of the loop below.
                    }

                    for (int i = 0; i < numpages; i++)
                    {
                        // Remember in the Doc object page #'s start with 0, but physically they start with 1.
                        if ((options.onlyodd() == false && options.onlyeven() == false) || // all pages
                            (options.onlyodd() && (((i + 1) % 2) == 1)) || // this is an odd page
                            (options.onlyeven() && (((i + 1) % 2) == 0))) // this is an even page
                        {
                            options.appendpagelist(i);
                        }
                    }

                    pagelist = options.getpagelist();
                }

                PageImageParams pip = new PageImageParams(options.getcolor(), options.getsmooth(), options.gethpixels(),
                    options.getvpixels(), options.gethres(), options.getvres(),
                    (DrawFlags.UseAnnotFaces | DrawFlags.DoLazyErase));
                if (options.getasprinted())
                    pip.PageDrawFlags |= DrawFlags.IsPrinting;

                ImageCollection Pageimagecollection = new ImageCollection();

                if (options.getoutputformat() == ImageType.TIFF && options.getcompress() == CompressionCode.NONE)
                {
                    options.setcompress(CompressionCode.LZW); // Default for TIF
                }

                ImageSaveParams isp = new ImageSaveParams();
                isp.HalftoneGrayImages = options.getgrayhalftone();
                isp.Compression = options.getcompress();
                if (options.getoutputformat() == ImageType.JPEG)
                {
                    isp.JPEGQuality = options.getquality();
                }

                isp.ReverseGray = options.getreversegray();
                isp.TIFFBlackIsOne = options.getblackisone();

                for (int i = 0;
                    i < pagelist.Count;
                    i++) // Get the images of the PDF pages to create an image collection.
                {
                    Page docpage = pdfdocument.GetPage(pagelist[i]);
                    Rect PageRect = null;
                    if (options.getpageregion().Equals("crop"))
                    {
                        PageRect = docpage.CropBox;
                    }
                    else if (options.getpageregion().Equals("media"))
                    {
                        PageRect = docpage.MediaBox;
                    }
                    else if (options.getpageregion().Equals("art"))
                    {
                        PageRect = docpage.ArtBox;
                    }
                    else if (options.getpageregion().Equals("bounding"))
                    {
                        PageRect = docpage.BBox;
                    }
                    else if (options.getpageregion().Equals("bleed"))
                    {
                        PageRect = docpage.BleedBox;
                    }
                    else if (options.getpageregion().Equals("trim"))
                    {
                        PageRect = docpage.TrimBox;
                    }
                    else
                    {
                        Console.WriteLine("Unknown page region option.");
                        Environment.Exit(1);
                    }

                    try
                    {
                        Image pageimage = docpage.GetImage(PageRect, pip);
                        if (options.getmultipage())
                        {
                            Pageimagecollection.Append(pageimage);
                        }
                        else
                        {
                            int x = i + 1;
                            saveTheImage(pageimage, x, options, isp);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Cannot rasterize page to an image: " + ex.Message);
                        Environment.Exit(1);
                    }
                }

                /* 
                 * Pageimagecollection now Contains, as the name states,
                 * a collection of images created from the PDF pages according
                 * to the user's options.  Now to post process them to the image.
                 * type according to the user's options.
                 */
                if (options.getmultipage())
                {
                    string outputfilepath;
                    if (options.getoutputdir() != "")
                        outputfilepath = options.getoutputdir() + "/" + options.getoutputfile();
                    else
                        outputfilepath = options.getoutputfile();

                    outputfilepath = CreateFileSuffix(outputfilepath, options.getoutputformat());
                    try
                    {
                        Pageimagecollection.Save(outputfilepath, options.getoutputformat(), isp);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Cannot save images to a multi-page TIF file: " + ex.Message);
                    }
                }
            }
        }
    }
}
