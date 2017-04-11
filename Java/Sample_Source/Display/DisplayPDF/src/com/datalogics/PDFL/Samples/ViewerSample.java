package com.datalogics.PDFL.Samples;

/*
 * 
 * A sample which demonstrates the use of the DLE API to view a PDF
 * file and search for text in the file and highlight them using the
 * C# drawing library.
 *
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
import javax.swing.*;

public class ViewerSample {

	static ViewerFrame frame;
	
	public static void main(String[] args) {
		// Do OS specific setup.
		platformSetup();
        java.awt.EventQueue.invokeLater(new Runnable() {
            public void run() {
                frame = new ViewerFrame();
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
