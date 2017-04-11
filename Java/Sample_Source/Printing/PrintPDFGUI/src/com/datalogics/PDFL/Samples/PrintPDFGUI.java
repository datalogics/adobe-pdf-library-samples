package com.datalogics.PDFL.Samples;

import javax.swing.*;
import java.awt.*;
import java.awt.event.*;
import com.datalogics.PDFL.*;

/*
 * This sample demonstrates printing a PDF file. It is similar to PrintPDF, but this
 * program provides a user interface.
 * 
 * For more detail see the description of the PrintPDFGUI sample program on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/printing-pdf-files-and-generating-postscript-ps-files-from-pdf
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

@SuppressWarnings("serial")
public class PrintPDFGUI extends JFrame 
{
    JTextField PDFfilenametext;
    public PrintPDFGUI() 
    {
        super("Print PDF");

        java.awt.Container contentPane = getContentPane();

        JLabel mylabel = new JLabel("Print a PDF file");
        contentPane.setLayout(new FlowLayout());
        contentPane.add(mylabel);

        JButton PrintPDFbutton = new JButton("Click to print PDF file");
        
        PDFfilenametext = new JTextField(64);
        contentPane.add(PDFfilenametext);
        contentPane.add(PrintPDFbutton);
                
        PrintPDFbutton.addActionListener( new ActionListener() {
            public void actionPerformed(ActionEvent event) {
                PrintPDFAction();
                PrintPDFClose();

            }
        });
    }
    
    public void PrintPDFClose() {        
        this.dispose();
    }
 
    public void PrintPDFAction() {

         Library lib = new Library();
         String filename = PDFfilenametext.getText();
         try
         {
             Document doc = new Document(filename);

             // Get some parameters 
             PrintUserParams userParams = new PrintUserParams();
             
             // Not available on some platforms yet.
             // userParams.setShrinkToFit(true);

             // Print to a file
             doc.printToFile(userParams, "PrintPDF_out.ps");

             // Print to a printer
             // For a list of the current print drivers available under WinNT, look at:
             // HKEY_CURRENT_USER\Software\Microsoft\WindowsNT\CurrentVersion\Devices
             // but some special virtual printers modify their ports, so pose a print dialog

             this.setVisible(false);
             if ( userParams.posePrintDialog(doc))

             // Or use the default printer
             // userParams.useDefaultPrinter(doc);

             {
                 // Added override of dialog box paper height and width
                 // because UseMediaBox seems to yield a printed product that more closely
                 // resembles what comes out of Acrobat.  This eliminates a number of
                 // print problems, including PCL blank page problems.
                 userParams.setPaperHeight(PrintUserParams.USE_MEDIA_BOX);
                 userParams.setPaperWidth(PrintUserParams.USE_MEDIA_BOX);
            	 doc.print(userParams);
             }
         }
         catch (Exception err)
         {
        	 JOptionPane.showMessageDialog((Component)null,
        			 "An exception has occured: " + err.getMessage(),
        			 "An exception has occured",
        			 JOptionPane.ERROR_MESSAGE);
         } finally {
        	 // this properly terminates the library, and is required
        	 lib.delete();
         }
    }

    public static void main(String args[]) 
    {
        final JFrame f = new PrintPDFGUI();

        f.setBounds(100, 100, 750, 300);
        f.setVisible(true);
        f.setBackground(java.awt.Color.white);
        f.setDefaultCloseOperation(DISPOSE_ON_CLOSE);

        f.addWindowListener(new WindowAdapter() {
            public void windowClosed(WindowEvent e) {
                System.exit(0);
            }
        });
    }
}
