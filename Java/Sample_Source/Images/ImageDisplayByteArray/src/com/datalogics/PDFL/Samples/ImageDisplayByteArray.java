package com.datalogics.pdfl.samples.Images.ImageDisplayByteArray;

/*
 * 
 * This sample makes a rasterized image of the first page of a PDF document and 
 * displays it. The program defines an optional input document, and the entire first 
 * page is converted to a graphic for display.
 * 
 * This program is related to the ImageDisplay.java sample, and they both produce the
 * same result. But the source code differs. This program builds the image using a ByteArray,
 * a Java feature. A Byte Array is a contiguous section of memory, expressed as raw data,
 * a series of bytes.
 *
 * ImageDisplayByteArray takes the content for the first page of a PDF file and stores it
 * in a Byte Array, and then exports that content from the Byte Array to the display viewer.
 *
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

import javax.swing.*;

import java.io.BufferedReader;
import java.io.InputStreamReader;

import com.datalogics.PDFL.*;

public class ImageDisplayByteArray
{
    public static void main(String[] args) throws Exception
    {
        // Do OS specific setup.
        platformSetup();

        String inputFileName = Library.getResourceDirectory() + "Sample_Input/ducky.pdf";
        if (args.length > 0)
        {
            inputFileName = args[0];
        }

        final String fileNameToOpen = inputFileName;

        java.awt.EventQueue.invokeLater(new Runnable()
        {
            public void run()
            {
                ImageDisplayByteArrayFrame frame = new ImageDisplayByteArrayFrame(fileNameToOpen);
                frame.setVisible(true);
            }
        });
    }

    private static void platformSetup()
    {
        if (System.getProperty("os.name").toLowerCase().startsWith("mac os x"))
            System.setProperty("apple.laf.useScreenMenuBar", "true");
        try
        {
            UIManager.setLookAndFeel(UIManager.getSystemLookAndFeelClassName());
        }
        catch (Exception e)
        {
            e.printStackTrace();
        }
    }
}
