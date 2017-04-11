/*
 * This sample is a utility that demonstrates a PDF viewing tool. You can use it to open, display, and edit PDF files.
 * 
 * For more detail see the description of the JavaViewer on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/viewing-pdf-files
 * 
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.PDFL.JavaViewer;

import java.awt.event.WindowAdapter;
import java.awt.event.WindowEvent;

import javax.swing.JFrame;
import javax.swing.JOptionPane;
import javax.swing.SwingUtilities;
import javax.swing.UIManager;

import com.datalogics.PDFL.JavaViewer.Presentation.ApplicationController;
import com.datalogics.PDFL.JavaViewer.Views.JavaViewerComponent;
import com.datalogics.PDFL.JavaViewer.Views.Interfaces.Dialogs.MessageType;

/**
 * JavaViewer - main entry point of the application. Creates instances of
 * ApplicationController and JavaViewerComponent. Sets the instance of
 * JavaViewerComponent as a child component of frame's content pane.
 */
public class JavaViewer {
    public static void main(String[] args) {
        // Do OS specific setup
        platformSetup();
        SwingUtilities.invokeLater((new Runnable() {
            public void run() {
                new JavaViewer().run();
            }
        }));
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

    private void run() {
        final JFrame jf = new JFrame("JavaViewer");
        try {
            jf.setDefaultCloseOperation(JFrame.DO_NOTHING_ON_CLOSE);
            final ApplicationController appController = new ApplicationController();
            final JavaViewerComponent component = new JavaViewerComponent(appController);

            jf.setContentPane(component);
            jf.setSize(1300, 700);
            jf.setVisible(true);

            jf.addWindowListener(new WindowAdapter() {
                @Override
                public void windowClosing(WindowEvent e) {
                    if (component.close()) {
                        jf.dispose();
                        System.exit(0);
                    }
                }
            });
        } catch (RuntimeException e) {
            JOptionPane.showMessageDialog(jf, "Library initialization failed! Apllication will be closed", "Library Error", JOptionPane.ERROR_MESSAGE);

            jf.dispose();
            System.exit(0);
        }
    }
}
