/*
 * Copyright (c) 2009-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 *
 * ===================== PDF Object Tree Icon Renderer ========================
 * This is a very simple class that allows us to add customized icons to the
 * tree.  These icons are type specific and add polish to the code.
 */

package com.datalogics.pdfl.samples.Display.PDFObjectExplorer;

import java.awt.Component;
import javax.swing.tree.DefaultTreeCellRenderer;
import javax.swing.ImageIcon;
import javax.swing.JTree;

// Import classes from DLE
import com.datalogics.PDFL.PDFObject;
import com.datalogics.PDFL.PDFArray;
import com.datalogics.PDFL.PDFBoolean;
import com.datalogics.PDFL.PDFDict;
import com.datalogics.PDFL.PDFInteger;
import com.datalogics.PDFL.PDFName;
import com.datalogics.PDFL.PDFReal;
import com.datalogics.PDFL.PDFStream;
import com.datalogics.PDFL.PDFString;
/**
 *
 * @author Datalogics
 */
@SuppressWarnings("serial")
public class PDFObjectTreeIconRenderer extends DefaultTreeCellRenderer{

    ImageIcon integerIcon;
    ImageIcon booleanIcon;
    ImageIcon realIcon;
    ImageIcon nameIcon;
    ImageIcon stringIcon;
    ImageIcon arrayIcon;
    ImageIcon dictIcon;
    ImageIcon streamIcon;
    ImageIcon documentIcon;

    /** Constructor:  initializes the tree icons
     *
     */
    public PDFObjectTreeIconRenderer(){
        super();
        integerIcon  = new ImageIcon(getClass().getResource("icons/Integer1616.png"));
        booleanIcon  = new ImageIcon(getClass().getResource("icons/Boolean1616.png"));
        realIcon     = new ImageIcon(getClass().getResource("icons/Real1616.png"));
        nameIcon     = new ImageIcon(getClass().getResource("icons/Name1616.png"));
        stringIcon   = new ImageIcon(getClass().getResource("icons/String1616.png"));
        arrayIcon    = new ImageIcon(getClass().getResource("icons/Array1616.png"));
        dictIcon     = new ImageIcon(getClass().getResource("icons/Dictionary1616.png"));
        streamIcon   = new ImageIcon(getClass().getResource("icons/Stream1616.png"));
        documentIcon = new ImageIcon(getClass().getResource("icons/Document1616.png"));
    }

    /** Overrides getTreeCellRendererComponent and determines which icon to
     *  display based on the object type
     *
     * @param tree
     * @param value  - PDFNode which holds pdfObjectValue
     * @param sel
     * @param expanded
     * @param leaf
     * @param row
     * @param hasFocus
     * @return
     */
    @Override
    public Component getTreeCellRendererComponent(JTree tree, Object value,
            boolean sel, boolean expanded, boolean leaf, int row, boolean hasFocus){

        super.getTreeCellRendererComponent(tree, value, sel, expanded, leaf, row, hasFocus);

        //"value" is a PDFNode.  Typecast value and retrieve the node's objectValue
        PDFObject pdfObjectValue = ((PDFNode)value).getPDFObject();

        //Determine the objectValue's type and set Icon accordingly
        if(pdfObjectValue == null){
            setIcon(documentIcon);
        }
        else if(pdfObjectValue instanceof PDFInteger){
            setIcon(integerIcon);
        }
        else if(pdfObjectValue instanceof PDFBoolean){
            setIcon(booleanIcon);
        }
        else if(pdfObjectValue instanceof PDFReal){
            setIcon(realIcon);
        }
        else if(pdfObjectValue instanceof PDFName){
            setIcon(nameIcon);
        }
        else if(pdfObjectValue instanceof PDFString){
            setIcon(stringIcon);
        }
        else if(pdfObjectValue instanceof PDFArray){
            setIcon(arrayIcon);
        }
        else if(pdfObjectValue instanceof PDFDict){
            setIcon(dictIcon);
        }
        else if(pdfObjectValue instanceof PDFStream){
            setIcon(streamIcon);
        }
        return this;
    }
}
