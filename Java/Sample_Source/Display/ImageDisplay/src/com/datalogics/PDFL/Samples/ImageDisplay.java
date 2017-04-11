package com.datalogics.PDFL.Samples;

/*
 * 
 * ImageDisplay makes a rasterized image of the first page of a PDF file and displays it
 * in a special window on your computer. The program defines an optional input document to display.
 * The entire first page is converted to a graphic.
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

public class ImageDisplay {

    public static void main(String[] args) throws Exception {
        // Do OS specific setup.
        platformSetup();

        String inputFileName = "../../Resources/Sample_Input/ducky.pdf";
        if (args.length > 0)
        {
            inputFileName = args[0];
        }

        final String fileNameToOpen = inputFileName;

        java.awt.EventQueue.invokeLater(new Runnable() {
            public void run() {
                ImageDisplayFrame frame = new ImageDisplayFrame(fileNameToOpen);
                frame.setVisible(true);
            }
        });
    }

    private static void platformSetup() {
        if (System.getProperty("os.name").toLowerCase().startsWith("mac os x"))
            System.setProperty("apple.laf.useScreenMenuBar", "true");
        try {
            UIManager.setLookAndFeel(UIManager.getSystemLookAndFeelClassName());
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
