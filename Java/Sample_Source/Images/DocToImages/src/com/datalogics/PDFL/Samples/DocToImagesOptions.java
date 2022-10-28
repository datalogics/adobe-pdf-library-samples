package com.datalogics.pdfl.samples.Images.DocToImages;

import java.io.File;
import java.util.ArrayList;
import java.util.EnumSet;
import java.util.List;

import com.datalogics.PDFL.ColorSpace;
import com.datalogics.PDFL.CompressionCode;
import com.datalogics.PDFL.ImageType;
import com.datalogics.PDFL.SmoothFlags;

public class DocToImagesOptions
{
    public enum ImportedImageType
    {
        DL,     //com.datalogics.PDFL.Image
        JAVA    //java.awt.BufferedImage
    }

    ImageType       outputformat = ImageType.INVALID;
    ColorSpace      color = ColorSpace.DEVICE_RGB;
    ImportedImageType  importedimagetype = ImportedImageType.DL;
    boolean         grayhalftone = false;
    boolean         firstpageonly = false;
    int             quality=0;
    double          hres = 300.0;
    double          vres = 300.0;
    ArrayList<String>    fontdirs = new ArrayList<String>(0);
    int             hpixelsize = 0;
    int             vpixelsize = 0;
    CompressionCode compress = CompressionCode.DEFAULT;
    String          pageregion = "crop";
    List<Integer>   PageList = new ArrayList<Integer>(0);
    int             evenoddpages = 0; // 1 = all odd pages, 2 = all even pages.
    String          outputfilename = null;
    String          outputdirname = null;
    EnumSet<SmoothFlags>     smoothingflags = EnumSet.of(SmoothFlags.NONE);  
    boolean         reversegray = false;
    boolean         blackisone = false;
    boolean         multipage = false;
    int             ZeroSuffix = 0;
    boolean         asprinted = false;
    
    public void setoutputformat(ImageType format)
    {
        outputformat = format;
    }
    public ImageType getoutputformat()
    {
        return(outputformat);
    }
    
    public void setcolor(ColorSpace outputcolor)
    {
        color = outputcolor;
    }
    public ColorSpace getcolor()
    {
        return (color);
    }

    public void setimportedimagetype(ImportedImageType imType)
    {
        importedimagetype = imType;
    }

    public ImportedImageType getimportedimagetype()
    {
        return importedimagetype;
    }

    public void setgrayhalftone(boolean halftone)
    {
        grayhalftone = halftone;
    }
    public boolean getgrayhalftone()
    {
        return (grayhalftone);
    }

    public void setfirst(boolean firstonly)
    {
        firstpageonly = firstonly;
    }
    public boolean getfirst()
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

    public void setfontdirs(String fd)
    {
        String[] fontdirarray = fd.split(";");
        for (int i = 0; i < fontdirarray.length; i++)
            fontdirs.add(fontdirarray[i]);
    }

    public ArrayList<String> getfontdirs()
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

    public void setpageregion(String region)
    {
        pageregion = region;
    }
    public String getpageregion()
    {
        return (pageregion);
    }

    public void appendpagelist(int pageno)
    {
        PageList.add(pageno);
    }

    /*
     * If this list is empty and the evenoddpages flag is set to 0 then the entire document
     * will be used.
     */

    public List<Integer> getpagelist()
    {
        return(PageList);
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

    public boolean onlyodd()
    {
        return (evenoddpages == 1);
    }

    public boolean onlyeven()
    {
        return (evenoddpages == 2);
    }

    public void setoutputfile(String outputfile)
    {
        outputfilename = outputfile;
    }
    public String getoutputfile()
    {
        return (outputfilename);
    }

    public void setoutputdir(String outputdir)
    {
        outputdirname = outputdir;
    }
    public String getoutputdir()
    {
        return (outputdirname);
    }

    public void setsmooth(EnumSet<SmoothFlags> flags)
    {
        smoothingflags = flags;
    }
    public EnumSet<SmoothFlags> getsmooth()
    {
        return (smoothingflags);
    }

    public void setreversegray(boolean reverse)
    {
        reversegray = reverse;
    }
    public boolean getreversegray()
    {
        return (reversegray);
    }

    public void setblackisone(boolean isblackone)
    {
        blackisone = isblackone;
    }
    public boolean getblackisone()
    {
        return (blackisone);
    }

    public void setmultipage(boolean multi)
    {
        multipage = multi;
    }
    public boolean getmultipage()
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

    public void setasprinted(boolean asp)
    {
        asprinted = asp;
    }
    public boolean getasprinted()
    {
        return (asprinted);
    }

    public boolean checkSavedImageType()
    {
        if(importedimagetype == ImportedImageType.JAVA && outputformat == ImageType.TIFF)
            return false;
        else
            return true;
    }

    public boolean checkformattype()
    {
        return ( outputformat != ImageType.INVALID );
    }

    public boolean checkcolorspacegrayhalftone()
    {
        if (grayhalftone == true)
            return (color == ColorSpace.DEVICE_GRAY);
        else
            return true;
    }

    public boolean checkformatcompressiontiff()
    {
        if (compress == CompressionCode.G3 || 
                compress == CompressionCode.G4 ||
                compress == CompressionCode.LZW)
            return (outputformat == ImageType.TIFF);
        else
            return true;
    }
    public boolean checkformatcompressionpng()
    {
        if (compress == CompressionCode.FLATE)
            return (outputformat == ImageType.PNG);
        else
            return true;
    }

    public boolean checkformatcompressionjpg()
    {
        if (compress == CompressionCode.DCT)
            return (outputformat == ImageType.JPEG);
        else
            return true;
    }
    
    public boolean checkcompressionnone()
    {
        if (compress == CompressionCode.NONE)
            return (outputformat == ImageType.BMP || outputformat == ImageType.PNG || outputformat == ImageType.TIFF);
        else
            return true;
    }

    public boolean checkreversegray()
    {
        if (reversegray == true)
            return (color == ColorSpace.DEVICE_GRAY);
        else
            return true;
    }

    public boolean checkblackisoneformat()
    {
        if (blackisone == true)
            return (outputformat == ImageType.TIFF);
        else
            return true;
    }

    public boolean checktiffcompressgrayhalftone()
    {
        if (compress == CompressionCode.G3 || compress == CompressionCode.G4)
            return (color == ColorSpace.DEVICE_GRAY && grayhalftone == true);
        else if (color == ColorSpace.DEVICE_GRAY && grayhalftone == true && outputformat == ImageType.TIFF)
            return (compress == CompressionCode.G3 || compress == CompressionCode.G4);
        else
            return true;
    }
    
    public boolean checkgrayhalftoneformat()
    {
        if (grayhalftone == true)
            return (outputformat == ImageType.TIFF);
        else
            return true;
    }

    public boolean checkqualityimagetype()
    {
        if (quality != 0)
            return (outputformat == ImageType.JPEG);
        else
            return true;
    }

    public boolean checkfirstonlypagerange()
    {
        if (firstpageonly == true)
            return (PageList.size() == 0);
        else
            return true;
    }

    public boolean checkfirstonlyevenodd()
    {
        if (firstpageonly == true)
            return (evenoddpages == 0);
        else
            return true;
    }

    public boolean checkmultiformat()
    {
        if (multipage == true)
            return (outputformat == ImageType.TIFF);
        else
            return true;
    }

    public boolean checkpagelist( int pagemax )
    {
        boolean rc = true;
        if (PageList.size() != 0)
        {
            for (int i = 0; i < PageList.size(); i++)
            {
                if (PageList.get(i) > pagemax)
                {
                    rc = false;
                    break;
                }
            }
        }
        return(rc);
    }
    
    public boolean checkfontdirs()
    {
        boolean rc = true;
        for (int i = 0; i < fontdirs.size(); i++ )
        {
        	File d = new File(fontdirs.get(i));
        	if ( !d.exists())            {
                System.out.println("The directory path " + fontdirs.get(i) + " listed in the fontlist options is not a valid directory.");
                rc = false;
            }
        }
        return (rc);
    }
}
