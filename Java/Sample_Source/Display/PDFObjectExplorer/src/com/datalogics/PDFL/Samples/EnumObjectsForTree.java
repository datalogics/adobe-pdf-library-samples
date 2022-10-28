/*
 * Copyright (c) 2009-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 *
 * ========================== Enum Objects For Tree ===========================
 * This file contains two classes.  Both operate the same way but handle two
 * different functions.  The first class, EnumObjectsForTree enumerates the
 * node's children and returns the values to the caller and adds the returned
 * values as new child nodes.
 *
 * The second class enumerates the user selected object and places the returned
 * data into the JTable to the right of the tree.
 *
 * Both classes enumerate the nodes value and populate the Tree or the Table
 * depending on which procedure is called.
 */

package com.datalogics.pdfl.samples.Display.PDFObjectExplorer;

import javax.swing.table.DefaultTableModel;

import com.datalogics.PDFL.PDFObjectEnumProc;
import com.datalogics.PDFL.PDFObject;
import com.datalogics.PDFL.PDFName;

/**
 *
 * @author Datalogics
 */
// Callback used to enumerate PDFDict entries for the tree view
public class EnumObjectsForTree extends PDFObjectEnumProc {
    PDFNode rootNode;
    public EnumObjectsForTree(PDFNode n)
    {
        rootNode = n;
    }
    // For each key/value pair, we create a new node with the name for a key,
    // and the value as the tag property
    public boolean Call(PDFObject pdfObject, PDFObject pdfObjectValue)
    {
         // The key value SHOULD be a PDFName
        if (pdfObject instanceof PDFName)
        {
            //Create a node with the object's name and the actual object
            PDFNode currentNode = new PDFNode(((PDFName)pdfObject).getValue(), pdfObject);
            //Set the object's value, then add the node to the root
            currentNode.setPDFObject(pdfObjectValue);
            rootNode.add(currentNode);

            // If it's an indirect object, append the ID and Generation numbers
            // NOTE: getting ID and Generation props on a DIRECT object casues an exception
            if (((PDFObject)currentNode.getPDFObject()).getIndirect())
                currentNode.setUserObject(currentNode.appendIdAndGeneration());
           }
        return true;
    }
}

// Callback used to enumerate PDFDict entries for the list view
class EnumObjectsForList extends PDFObjectEnumProc {
    DefaultTableModel tableModel;
    javax.swing.JTable dataTable;
    public EnumObjectsForList(DefaultTableModel model, javax.swing.JTable table)
    {
        tableModel = model;
        dataTable = table;
    }

    // For each key/value pair, we create a new item with the name for a key,
    // plus type and value entries
    @Override
    public boolean Call(PDFObject pdfObject, PDFObject pdfObjectValue)
    {
        // The key value SHOULD be a PDFName
        if (pdfObject instanceof PDFName)
        {
            // Set model equal to the DefaultTableModel passed in
            DefaultTableModel model = tableModel;
            // Get the returnValue information of the pdfObjectValue
            Object[] returnValue = PDFObjectExplorer.GetObjectTypeAndValue(pdfObjectValue);
            java.util.Vector<Object> obj = new java.util.Vector<Object>();

            //Add the value type, the object's name, and the object's value

            obj.addElement(((PDFName)pdfObject).getValue());
            obj.addElement(returnValue[2]);
            obj.addElement(returnValue[1]);

            // Add the object to the model, then update the dataTable
            model.addRow(obj);
            dataTable.setModel(model);
        }
        return true;
    }
}
