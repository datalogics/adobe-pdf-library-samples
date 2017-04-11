package com.datalogics.PDFL.Samples;

import java.awt.image.BufferedImage;
import java.io.File;
import java.io.IOException;
import java.util.ArrayList;
import java.util.EnumSet;
import java.util.List;

import javax.imageio.ImageIO;

import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.DrawFlags;
import com.datalogics.PDFL.DrawParams;
import com.datalogics.PDFL.Ink;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.Matrix;
import com.datalogics.PDFL.Page;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.SeparationColorSpace;

public class DrawSeparations {
	 
	/*
	 * This sample demonstrates drawing a list of grayscale separations from a PDF file.
	 *
	 * For more detail see the description of the DrawSeparations sample program on our Developer's site, 
	 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/manipulating-graphics-and-separating-colors-for-images
	 * 
	/**
     * @param args the command line arguments
     */
    public static void main(String[] args) {
        System.out.println("DrawSeparations Sample:");
        
        System.setProperty("java.awt.headless", "true");

        Library lib = new Library();

        try {
            String sInput = "../../Resources/Sample_Input/ducky.pdf";
            String sOutput = "DrawSeparations-out-";
            if (args.length > 0)
                sInput = args[0];
            if (args.length > 1)
                sOutput = args[1];
            System.out.println("Initialized the library.");
            System.out.println("PDF Document to Images Sample:");
            System.out.println("Input file: " + sInput + ", will write to " + sOutput);

            
            Document doc = new Document(sInput);
            Page pg = doc.getPage(0);
            
            // Get all inks tha are present on the page
            List<Ink> inks = pg.listInks();
            List<SeparationColorSpace> colorants = new ArrayList<SeparationColorSpace>();
            
            // Here we decide, which inks should be drawn
            for (Ink ink : inks) {
            	colorants.add(new SeparationColorSpace(pg, ink));
            }
            
            double width = pg.getMediaBox().getRight() - pg.getMediaBox().getLeft();
            double height = pg.getMediaBox().getTop() - pg.getMediaBox().getBottom();
            

            // Must invert the page to get from PDF with origin at lower left,
            // to a bitmap with the origin at upper right.
            Matrix matrix = new Matrix().scale(1, -1).translate(0, -height);
            
            DrawParams params = new DrawParams();
            params.setMatrix(matrix);
            params.setDestRect(new Rect(0, 0, width, height));
            params.setFlags(EnumSet.of(DrawFlags.DO_LAZY_ERASE, DrawFlags.USE_ANNOT_FACES));
            
            // Acquiring list of separations
            List<BufferedImage> separatedImages = pg.drawContents(params, colorants);
            
            try {
                int number = 0;
                for (BufferedImage image : separatedImages) {
	                File outputfile = new File(sOutput + number + "-" + inks.get(number).getColorantName() + ".png" );
	                ImageIO.write(image, "png", outputfile);
	                number++;
                }
            } catch (IOException e) {
            }
        }      
        finally {
            lib.delete();
        }
    }
}
