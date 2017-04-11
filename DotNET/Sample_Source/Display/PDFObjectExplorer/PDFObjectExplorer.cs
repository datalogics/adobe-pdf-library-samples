using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Datalogics.PDFL;

/*
 * This sample demonstrates using the PDF Object Explorer, a viewing tool that allows you to look at
 * information about objects embedded in a PDF file. These objects include arrays, dictionaries,
 * streams of characters, and integers.
 *
 * For more detail see the description of the PDFObjectExplorer sample on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-sample-programs/listing-information-about-values-and-objects-in-pdf-files#pdfobjectexplorer
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace PDF_Object_Explorer
{
    class PDFObjectExplorer
    {
        private Document PDFDoc;
        private TreeView mainTree;
        private DataGridView dataGridView;

        int columnIndex;
        DataGridViewCell selectedCell;
        object oldValueInDataGridCell;
        TreeNode selectedNodeBeforeEdit;
        bool docWasModified;

        public PDFObjectExplorer(Document PDFDoc, TreeView mainTree, DataGridView dataGridView)
        {
            this.PDFDoc = PDFDoc;
            this.mainTree = mainTree;
            this.dataGridView = dataGridView;
            docWasModified = false;
        }

        public void beginEdit(int rowIndex, int colomnIndex)
        {
            selectedCell = dataGridView.Rows[rowIndex].Cells[colomnIndex];
            oldValueInDataGridCell = dataGridView.Rows[rowIndex].Cells[colomnIndex].Value;
            selectedNodeBeforeEdit = mainTree.SelectedNode;
            this.columnIndex = colomnIndex;
        }

        public void endEdit()
        {
            if (oldValueInDataGridCell.Equals(selectedCell.Value))
                    return;

                PDFObject selectedPDFObject = (selectedNodeBeforeEdit.Tag as PDFObject);

                if (selectedPDFObject is PDFDict)
                {
                    editSelectedDict(selectedPDFObject as PDFDict);
                }
                else if(selectedPDFObject is PDFArray)
                {
                    editSelectedArray(selectedPDFObject as PDFArray);
                }
                else if (selectedPDFObject is PDFStream)
                {
                    if (oldValueInDataGridCell.ToString().Equals("Length"))
                    {
                        throw new InvalidOperationException("Key 'Length' of the PDFStream can't be modified");
                    }
                    else
                    {
                        editSelectedDict((selectedPDFObject as PDFStream).Dict);
                    }
                }
                else
                {
                    //special case for metadata
                    if (selectedNodeBeforeEdit.FullPath.Contains("Document\\InfoDict"))
                    {
                        PDFDoc.SetInfo(selectedNodeBeforeEdit.Text, selectedCell.Value.ToString());
                    }
                    editSelectedScalar(selectedCell.Value);
                }
        }

        private void editSelectedArray(PDFArray selectedArray)
        {
            string indexString = selectedCell.OwningRow.Cells[0].Value.ToString();
            editSelectedArrayValue(selectedArray, int.Parse(indexString.Substring(1, indexString.Length - 2)), selectedCell.Value);
            selectedNodeBeforeEdit.Tag = selectedArray;
            docWasModified = true;
        }

        private void editSelectedArrayValue(PDFArray selectedArray, int index, object newValue)
        {
            PDFObject newPDFObject = createNewValueForSelectedScalar(selectedArray.Get(index), newValue.ToString());
            updateArrayValue(selectedArray, index, newPDFObject);
            //refresh the tree
            mainWindow.addArrayItemToTree(selectedArray, index, selectedNodeBeforeEdit);
            mainWindow.expandNode(selectedNodeBeforeEdit);
        }

        private void editSelectedDict(PDFDict selectedDict)
        {
            
            if (selectedNodeBeforeEdit.FullPath.Contains("Document\\InfoDict"))
            {
                PDFDoc.SetInfo(selectedCell.OwningRow.Cells[0].Value.ToString(), selectedCell.Value.ToString());
            }
            if (columnIndex == 0)
            {
                //special case for metadata
                if (selectedNodeBeforeEdit.FullPath.Contains("Document\\InfoDict"))
                {
                    throw new InvalidOperationException("Keys of InfoDict can't be modified");
                }
                editSelectedDictKey(selectedDict, selectedCell.Value);
            }
            else if (columnIndex == 2)
            {
                //special case for metadata
                if (selectedNodeBeforeEdit.FullPath.Contains("Document\\InfoDict"))
                {
                    PDFDoc.SetInfo(selectedCell.OwningRow.Cells[0].Value.ToString(), selectedCell.Value.ToString());
                }
                editSelectedDictValue(selectedDict, selectedCell.OwningRow.Cells[0].Value.ToString(), selectedCell.Value);
            }
            selectedNodeBeforeEdit.Tag = selectedDict;
            docWasModified = true;
        }

        private void editSelectedDictValue(PDFDict selectedDict, string key, object newValue)
        {
            PDFObject newPDFObject = createNewValueForSelectedScalar(selectedDict.Get(key), newValue.ToString());
            updateDictionaryValue(selectedDict, key, newPDFObject);
            //refresh the tree 
            selectedDict.EnumPDFObjects(new EnumObjectsForTree(selectedNodeBeforeEdit));
            mainWindow.expandNode(selectedNodeBeforeEdit);
        }

        public void restoreValueBeforeEdit()
        {
            if (oldValueInDataGridCell != null) selectedCell.Value = oldValueInDataGridCell;
        }

        public bool WasModified
        {
            get { return docWasModified; }
        }


        public TreeNode SelectedNode
        {
            get { return selectedNodeBeforeEdit; }
        }

        private void editSelectedDictKey(PDFDict selectedDict, object newValue)
        {
            updateDictionaryKey(selectedDict, newValue.ToString(), oldValueInDataGridCell.ToString());
            //refresh the tree 
            selectedDict.EnumPDFObjects(new EnumObjectsForTree(selectedNodeBeforeEdit));
            mainWindow.expandNode(selectedNodeBeforeEdit);
        }

        private void updateDictionaryKey(PDFDict pdfDict, string newKey, string oldKey)
        {
            PDFObject valueInDict = pdfDict.Get(oldKey);
            pdfDict.Remove(oldKey);
            pdfDict.Put(newKey, valueInDict);
        }

        private void updateDictionaryValue(PDFDict pdfDict, string key, PDFObject newValue)
        {
            pdfDict.Remove(key);
            pdfDict.Put(key, newValue);
        }

        private void editSelectedScalar(object newValue)
        {
            PDFObject newPDFObject = createNewValueForSelectedScalar(selectedNodeBeforeEdit.Tag as PDFObject, newValue.ToString());
            modifyParentOfSelectedScalar(newPDFObject);
            selectedNodeBeforeEdit.Tag = newPDFObject;
            docWasModified = true;
        }

        private void modifyParentOfSelectedScalar(PDFObject newPDFObject)
        {
            PDFObject parentPDFObject = (selectedNodeBeforeEdit.Parent.Tag as PDFObject);
            if (parentPDFObject is PDFDict)
            {
                PDFDict parentDict = (parentPDFObject as PDFDict);
                updateDictionaryValue(parentDict, selectedNodeBeforeEdit.Text, newPDFObject);
            }
            else if (parentPDFObject is PDFArray)
            {
                PDFArray parentArray = (parentPDFObject as PDFArray);
                updateArrayValue(parentArray, selectedNodeBeforeEdit.Index, newPDFObject);
                //refresh the tree
                selectedNodeBeforeEdit = mainWindow.addArrayItemToTree(parentArray, selectedNodeBeforeEdit.Index, selectedNodeBeforeEdit.Parent);
            }
            else if (parentPDFObject is PDFStream)
            {
                string key = selectedNodeBeforeEdit.Text;
                PDFDict parentDict = (parentPDFObject as PDFStream).Dict;
                updateDictionaryValue(parentDict, key, newPDFObject);
            }
        }

        private void updateArrayValue(PDFArray array, int index, PDFObject newVal)
        {
            array.RemoveAt(index);
            array.Insert(index, newVal);
        }

        private PDFObject createNewValueForSelectedScalar(PDFObject selectedPDFObject, string newValue)
        {
            PDFObject newPDFObject = null;
            if (selectedPDFObject is PDFString)
            {
                PDFString selectedPDFString = selectedPDFObject as PDFString;
                newPDFObject = new PDFString(newValue, PDFDoc, selectedPDFString.Indirect, selectedPDFString.StoredAsHex);
            }
            else if (selectedPDFObject is PDFName)
            {
                newPDFObject = new PDFName(newValue, PDFDoc, selectedPDFObject.Indirect);
            }
            else if (selectedPDFObject is PDFReal)
            {
                double newVal;
                if (double.TryParse(newValue, out newVal))
                    newPDFObject = new PDFReal(newVal, PDFDoc, selectedPDFObject.Indirect);
                else
                {
                    throw new FormatException("The value is not of type double.");
                }
            }
            else if (selectedPDFObject is PDFInteger)
            {
                int newVal;
                if (int.TryParse(newValue, out newVal))
                    newPDFObject = new PDFInteger(newVal, PDFDoc, selectedPDFObject.Indirect);
                else
                {
                    throw new FormatException("The value is not of type integer.");
                }
            }
            else
            {
                throw new InvalidOperationException("Non scalar types can't be altered");
            }
            return newPDFObject;
        }
    }
}
