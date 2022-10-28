/*
 * ImageDisplayFrame.java
 *
 * Copyright (c) 2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 */

package com.datalogics.pdfl.samples.Display.ImageDisplay;

import apple.dts.samplecode.osxadapter.OSXAdapter;
import com.datalogics.PDFL.*;

@SuppressWarnings("serial")
public class ImageDisplayFrame extends javax.swing.JFrame  {
    private Document currentDoc = null;
    private  String filename = null;
    private Library library;
    private final ImageDisplayer imageDisplayer;

    // Check that we are on Mac OS X.  This is crucial to loading and using the OSXAdapter class.
    public static boolean MAC_OS_X = (System.getProperty("os.name").toLowerCase().startsWith("mac os x"));

    /** Creates new form ImageDisplayFrame */
    public ImageDisplayFrame(String aFileName) {
        registerForMacOSXEvents();

    	filename = aFileName;
        library = new Library();
        imageDisplayer = new ImageDisplayer();
        initComponents();
        try
        {
            currentDoc = new Document(filename);
        }
        catch (Exception ex)
        {
            System.out.println("Cannot open PDF file " + filename);
            this.dispose();
        }
        imageDisplayer.setDocument(currentDoc);
        
        jScrollPane1.setViewportView(imageDisplayer);
        imageDisplayer.displayPage();
    }
    

    private void initComponents() {

        setDefaultCloseOperation(javax.swing.WindowConstants.DISPOSE_ON_CLOSE);
        setTitle("Image Display Sample");
        jScrollPane1 = new javax.swing.JScrollPane();
        setMinimumSize(new java.awt.Dimension(450, 46));
        jScrollPane1.setMinimumSize(new java.awt.Dimension(550, 23));
        jScrollPane1.setPreferredSize(new java.awt.Dimension(550, 502));
        getContentPane().add(jScrollPane1, java.awt.BorderLayout.CENTER);
        pack();
    }

    public void windowActivated(java.awt.event.WindowEvent evt) {
    }

    public void windowClosed(java.awt.event.WindowEvent evt) {
    }

    public void windowClosing(java.awt.event.WindowEvent evt) {
        if (evt.getSource() == ImageDisplayFrame.this) {
            ImageDisplayFrame.this.formWindowClosing(evt);
        }
    }

    public void windowDeactivated(java.awt.event.WindowEvent evt) {
    }

    public void windowDeiconified(java.awt.event.WindowEvent evt) {
    }

    public void windowIconified(java.awt.event.WindowEvent evt) {
    }

    public void windowOpened(java.awt.event.WindowEvent evt) {
    }

    private void formWindowClosing(java.awt.event.WindowEvent evt) {
        cleanup();
        this.dispose();
    }

    public void cleanup() {
        if (library != null)
            library.delete();
        library = null;
    }

    // Generic registration with the Mac OS X application menu
    // Checks the platform, then attempts to register with the Apple EAWT
    // See OSXAdapter.java to see how this is done without directly referencing any Apple APIs
    public void registerForMacOSXEvents() {
        if (MAC_OS_X) {
            try {
                // Generate and register the OSXAdapter, passing it a hash of all the methods we wish to
                // use as delegates for various com.apple.eawt.ApplicationListener methods
                OSXAdapter.setQuitHandler(this, getClass().getDeclaredMethod("quit", (Class[])null));
            } catch (Exception e) {
                System.err.println("Error while loading the OSXAdapter:");
                e.printStackTrace();
            }
        }
    }

    // General quit handler; fed to the OSXAdapter as the method to call when a system quit event occurs
    // A quit event is triggered by Cmd-Q, selecting Quit from the application or Dock menu, or logging out
    public boolean quit() {
        cleanup();
        return true;
    }

    private javax.swing.JScrollPane jScrollPane1;

}
