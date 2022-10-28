/*
 * Copyright (c) 2009-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 *
 * =============================== PDF NODE ===================================
 * The PDFNode object extends the DefaultMutableTreeNode.  It extends the basic
 * node class by associating a TreeNode with a PDFObject in the document.
 *
 * As the user explores the document by clicking on items in the tree, we
 * dynamically add nodes to the tree, associating each node with a newly
 * discovered PDFObject.  In this way, the structure of the tree mirrors
 * the structure of the document--i.e. the relationship of dictionaries
 * containing other dictionaries and arrays and such--in a very direct manner.
 *
 * The alternative is to maintain a separate database of the document structure
 * that contains all the PDFObjects and keep that database in sync with the
 * TreeModel.
 *
 * Note that one side effect of this design is that we do not detect loops in
 * the document structure: if a dictionary contains a pointer to its own parent,
 * our user tree will have branches that never terminate (i.e. reach a node
 * that only holds scalar objects.)  However, since the node exploration process
 * never searches more than two nodes deep, the code never enters an
 * 'infinite loop'--node exploration is entirely under user control.
 *
 * These nodes are created using the EnumObjectsForTree class.
 *
 * Additional notes:
 *
 * - Nodes are created passing in the string value of pdfObject and is assigned
 *   to 'userObject'.  'userObject' is the text that is displayed on screen.
 *
 * - the PDFObjects are returned to the main class through getPDFObject()
 */
package com.datalogics.pdfl.samples.Display.PDFObjectExplorer;

import javax.swing.tree.DefaultMutableTreeNode;
import com.datalogics.PDFL.PDFObject;
/**
 *
 * @author Datalogics
 */
@SuppressWarnings("serial")
public class PDFNode extends DefaultMutableTreeNode  {

    //The entire pdfObject
    private PDFObject pdfObject;

    /** Constructor for data-less nodes like "(No Document)" and "Document"
     *
     * @param displayName  - set's the userObject equal to a string to display on screen
     */
    public PDFNode(String displayName){
        this.userObject = displayName;
    }

    /** Constructor for nodes that contain data.
     *
     * @param displayName  - set's the userObject equal to a string to display on screen
     * @param pdfObject    - the node's actual data
     */
    public PDFNode(String displayName, PDFObject pdfObject){
        this.userObject = displayName;
        this.pdfObject = pdfObject;
    }

    /** Sets the PDFNode's pdfObjectValue.  This is used when the node actually
     *  contains a pdfObjectValue
     *
     * @param pdfObjectValue  - the value of the node
     */
    public void setPDFObject(PDFObject pdfObjectValue){
        this.pdfObject = pdfObjectValue;
    }

    /** Returns the PDFObject as a PDFObject
     *
     * @return
     */

    /** Returns the pdfObjectValue as a PDFObject
     *
     * @return
     */
    public PDFObject getPDFObject() {
        return pdfObject;
    }

    /** Appends the node's ID and Generation onto the display name
     *
     * @return  - a string containing the display name plus ID and Generation
     */
    public String appendIdAndGeneration(){
        return (this.getUserObject().toString() + " [ID: "
                + this.pdfObject.getID() + ", Gen: "
                + this.pdfObject.getGeneration() + "]");
    }
}
