using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Collections.Generic;

using System.Windows.Forms;
using WinForm = System.Windows.Forms.Form;
using Datalogics.PDFL;
using System.Text;

/*
 * Copyright (c) 2007-2013, Datalogics, Inc. All rights reserved.
 *
 * The information and code in this sample is for the exclusive use of Datalogics
 * customers and evaluation users only.  Datalogics permits you to use, modify and
 * distribute this file in accordance with the terms of your license agreement.
 * Sample code is for demonstrative purposes only and is not intended for production use.
 *
 * ============================ PDF OBJECT EXPLORER ===================================
 * Demonstrates a GUI-based browser for PDF objects, letting the user examine a PDF at
 * the level of dictionaries, streams, and simple datatypes like strings and integers.
 * This sample serves two purposes:
 *
 * 1) It demonstrates how to use the PDFObject API in DLE, including features like
 *    the PDFObjectEnumProc callback
 *
 * 2) It can also be built and run 'out of the box' as a tool for inspecting PDF
 *    files
 */
 
 /*
 *	Icon for save button was taken from
 *  http://www.iconarchive.com/show/toolbar-icons-by-ruby-software/save-icon.html
 *	
 */

/*
 * ======================== DETAILS OF OPERATION ===============================
 * The PDF Object Explorer effectively builds and maintains a 'database' of
 * low level objects in the PDF, and lets the user examine those objects.
 * The trick is that it uses the System.Windows.Forms.TreeView control to
 * do it: TreeNodes have a 'Tag' property that can be any subclass of Object,
 * so we simply stick any PDFObjects from the document into this property.
 *
 * As the user explores the document by clicking on items in the tree, we
 * dynamically add nodes to the tree, associating each node with a newly
 * discovered PDFObject.  In this way, the structure of the tree mirrors
 * the structure of the document--i.e. the relationship of dictionaries
 * containing other dictionaries and arrays and such--in a very direct manner.
 *
 * The alternative is to maintain a separate database of the document structure
 * that contains all the PDFObjects and keep that database in sync with the
 * Tree.
 *
 * Note that one side effect of this design is that we do not detect loops in
 * the document structure: if a dictionary contains a pointer to its own parent,
 * our user tree will have branches that never terminate (i.e. reach a node
 * that only holds scalar objects.)  However, since the node exploration process
 * never searches more than two nodes deep, the code never enters an
 * 'infinite loop'--node exploration is entirely under user control.
 *
 */

namespace PDF_Object_Explorer
{
    enum DisplayMode {String, Hex};

    public partial class mainWindow : WinForm
    {
        // Class variables
        Document PDFDoc = null;         // The current document
        String currFilePath = null;     // The path & name of the current document
        String currPassword = null;     // The password used to open the current document

        //object oldValueInDataGridCell = null;
        //TreeNode selectedNodeBeforeEdit = null;
        bool docWasModified = false;
        PDFObjectExplorer editor = null;

        public mainWindow()
        {
            InitializeComponent();

            // Initialize the ImageList for the tree
            ImageList treeImageList = new ImageList();
            treeImageList.Images.Add("DefaultIcon", PDF_Object_Explorer.Properties.Resources.NoPDF);
            treeImageList.Images.Add("DocumentIcon", PDF_Object_Explorer.Properties.Resources.Document16162);
            treeImageList.Images.Add("DictIcon", PDF_Object_Explorer.Properties.Resources.Dictionary1616);
            treeImageList.Images.Add("ArrayIcon", PDF_Object_Explorer.Properties.Resources.Array1616);
            treeImageList.Images.Add("StreamIcon", PDF_Object_Explorer.Properties.Resources.Stream1616);
            treeImageList.Images.Add("BooleanIcon", PDF_Object_Explorer.Properties.Resources.Boolean16162);
            treeImageList.Images.Add("IntegerIcon", PDF_Object_Explorer.Properties.Resources.Integer16162);
            treeImageList.Images.Add("RealIcon", PDF_Object_Explorer.Properties.Resources.Real16162);
            treeImageList.Images.Add("NameIcon", PDF_Object_Explorer.Properties.Resources.Name1616);
            treeImageList.Images.Add("StringIcon", PDF_Object_Explorer.Properties.Resources.String1616);

            mainTree.ImageList = treeImageList;
        }

        private void mainWindow_Load(object sender, EventArgs e)
        {
            // Load a 'no document' entry to the tree at startup
            mainTree.Nodes.Add("(no document)");
        }

        // Open a PDF to examine
        private void OpenPDFButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "PDF File (*.pdf)|*.pdf";
            dialog.Title = "Select a PDF file";
            DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                String FileName = dialog.FileName;
                try
                {
                    // Dispense with the previous PDF (if any)
                    if (PDFDoc != null)
                    {
                        if (docWasModified) runSaveDialog();
                        PDFDoc.Dispose();
                    }
                    mainWindow.ActiveForm.Text = FileName;

                    // First attempt to open the document with no password
                    try
                    {
                        PDFDoc = new Document(FileName);
                        currPassword = null;    // Null out the password if we didn't need it
                    }
                    catch (Exception fileOpenException)
                    {
                        string exceptionMessage = fileOpenException.Message;

                        // Trap password-required exception
                        if (exceptionMessage.Contains("1073938471"))
                        {
                            // Prompt user for a password
                            string password = "";

                            frmPassword passwordForm = new frmPassword();
                            DialogResult passwordFormResult = passwordForm.ShowDialog();

                            try
                            {
                                if (passwordFormResult == DialogResult.OK)
                                {
                                    password = passwordForm.password;

                                    PDFDoc = new Document(FileName, password, PermissionRequestOperation.Open, false);
                                    currPassword = password;    // Save the password
                                }
                                // If not OK, user cancelled.  Abandon the open, no document available.
                                else
                                {
                                    mainTree.Nodes.Clear();
                                    mainTree.Nodes.Add("(no document)");
                                    return;
                                }
                            }
                            catch (Exception fileOpenWithPasswordException)
                            {
                                // Just throw to the outer handler
                                throw fileOpenWithPasswordException;
                            }
                        }
                    }

                    // Now repopulate the tree
                    GrabDocRootAndInfo();

                    // Re-initialize the data view
                    InitDataView();

                    editor = new PDFObjectExplorer(PDFDoc, mainTree, dataGridView);

                    // Save the file path
                    currFilePath = FileName;
                }
                catch (Exception exc)
                {
                    MessageBox.Show("The File " + FileName + " cannot be opened. \n\n" + exc.ToString());
                }
            }
        }

        private void runSaveDialog()
        {
            string messageBoxText = "You have some unsaved changes. Do you want to save?";
            string caption = "Saving";
            MessageBoxButtons button = MessageBoxButtons.YesNo;
            DialogResult result = MessageBox.Show(messageBoxText, caption, button);
            if (result == DialogResult.Yes)
            {
                savePDFDoc();
            }
        }

        // Close and reopen the document from the file - useful if you've made changes to the PDF
        private void RefreshButton_Click(object sender, EventArgs e)
        {
            // We only need to refesh if a doc is open
            if (PDFDoc != null)
            {
                if (docWasModified) runSaveDialog();
                // Clear the existing views
                mainTree.Nodes.Clear();
                clearDataGrid();

                PDFDoc.Dispose();

                // If we opened with a password, use it again
                if (currPassword != null)
                    PDFDoc = new Document(currFilePath, currPassword, PermissionRequestOperation.Open, false);
                else
                    PDFDoc = new Document(currFilePath);

                // Now repopulate the tree
                GrabDocRootAndInfo();

                // Re-initialize the data view
                InitDataView();

                editor = new PDFObjectExplorer(PDFDoc, mainTree, dataGridView);

            }
        }

        private void clearDataGrid()
        {
            dataGridView.Rows.Clear();
            dataGridView.ColumnCount = 0;
        }

        // Reset the TreeView and add the InfoDict and Root

        // COMPARISON WITH THE JAVA PDF OBJECT EXPLORER:
        // In .NET, we simply grab the InfoDict and Root dictionaries
        // (which must be present in any well formed PDF) and add nodes
        // for them under the 'Document' node at the root of the tree.
        // 'InfoDict' and 'Root' have no child nodes at this moment, but
        // they will be populated (via mainTree_BeforeExpand()) when the
        // user expands the 'Document' node.  This is possible because
        // the root of the .NET tree control starts out in its
        // collapsed state.
        //
        // In JAVA we would have to populate the 'InfoDict' and 'Root' nodes
        // right here in GrabDocRootAndInfo() because the top-level node
        // is not collapsible; they would never get children if they
        // weren't immediately populated.

        private void GrabDocRootAndInfo()
        {
            // Release the existing tree
            mainTree.Nodes.Clear();
            TreeNode docNode = mainTree.Nodes.Add("Document", "Document", "DocumentIcon");
            docNode.SelectedImageKey = docNode.ImageKey;

            // Create nodes for the InfoDict and Root
            TreeNode infoDict = docNode.Nodes.Add("InfoDict", "InfoDict", "DictIcon");
            infoDict.SelectedImageKey = infoDict.ImageKey;
            infoDict.Tag = PDFDoc.InfoDict;


            TreeNode root = docNode.Nodes.Add("Root", "Root", "DictIcon");
            root.SelectedImageKey = root.ImageKey;
            root.Tag = PDFDoc.Root;
        }

        // Initialize the data list view
        private void InitDataView()
        {
            // Create column headers
            SetHeadersForScalar();
        }

        // Set up data list view for scalar item
        private void SetHeadersForScalar()
        {
            clearDataGrid();
            dataGridView.ColumnCount = 2;
            dataGridView.Columns[0].Name = "Type";
            dataGridView.Columns[1].Name = "Value";
            dataGridView.Columns[0].ReadOnly = true;
            dataGridView.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        // Set up data list view for dictionary
        private void SetHeadersForDict()
        {
            clearDataGrid();
            dataGridView.ColumnCount = 3;
            dataGridView.Columns[0].Name = "Key";
            dataGridView.Columns[1].Name = "Type";
            dataGridView.Columns[2].Name = "Value";
            dataGridView.Columns[1].ReadOnly = true;
            //dataGridView.Columns[2].ReadOnly = true;
            dataGridView.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        // Set up data list view for array
        private void SetHeadersForArray()
        {
            clearDataGrid();
            dataGridView.ColumnCount = 3;
            dataGridView.Columns[0].Name = "Index";
            dataGridView.Columns[1].Name = "Type";
            dataGridView.Columns[2].Name = "Value";
            dataGridView.Columns[0].ReadOnly = true;
            dataGridView.Columns[1].ReadOnly = true;
            //dataGridView.Columns[2].ReadOnly = true;
            dataGridView.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        // When an object is chosen in the tree, update the list view.
        private void mainTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Action == TreeViewAction.ByMouse || e.Action == TreeViewAction.ByKeyboard)
            {
                // Clear the existing data view
                clearDataGrid();

                // Clear the stream views
                streamViewCooked.Clear();
                streamViewRaw.Clear();

                // Special handling: If the user clicks the document icon, we should
                // do nothing.  Anything else in the tree is a PDFObject, so grab it
                // and put its contents in the list view
                if (e.Node.Text == "Document" || e.Node.Text == "(no document)")
                    return;
                // Must be a PDFObject; extract it from the tree node
                PDFObject obj = (e.Node.Tag as PDFObject);

                // Select what to do based on the object subtype
                if (obj is PDFInteger)
                {
                    SetHeadersForScalar();
                    string[] row1 = new string[] { "Int", (obj as PDFInteger).Value.ToString() };
                    dataGridView.Rows.Add(row1);
                }
                else if (obj is PDFBoolean)
                {
                    SetHeadersForScalar();
                    string[] row1 = new string[] { "Bool", (obj as PDFBoolean).Value.ToString() };
                    dataGridView.Rows.Add(row1);
                }
                else if (obj is PDFReal)
                {
                    SetHeadersForScalar();
                    string[] row1 = new string[] { "Real", (obj as PDFReal).Value.ToString() };
                    dataGridView.Rows.Add(row1);
                }
                else if (obj is PDFName)
                {
                    SetHeadersForScalar();
                    string[] row1 = new string[] { "Name", (obj as PDFName).Value };
                    dataGridView.Rows.Add(row1);
                }
                else if (obj is PDFString)
                {
                    SetHeadersForScalar();
                    string[] row1 = new string[] { "String", (obj as PDFString).Value };
                    dataGridView.Rows.Add(row1);
                }
                else if (obj is PDFArray)
                {
                    PDFArray array = (obj as PDFArray);
                    SetHeadersForArray();

                    for (int i = 0; i < array.Length; i++)
                    {
                        PDFObject subobj = array.Get(i);
                        String type;
                        String value;

                        // Interrogate the object for its type and value
                        GetObjectTypeAndValue(subobj, out type, out value);
                        string[] row = new string[] { "[" + i.ToString() + "]", type, value };
                        dataGridView.Rows.Add(row);
                    }
                }
                else if (obj is PDFDict)
                {
                    SetHeadersForDict();
                    (obj as PDFDict).EnumPDFObjects(new EnumObjectsForDataGrid(dataGridView));
                }
                else if (obj is PDFStream)
                {
                    SetHeadersForDict();
                    PDFStream streamobj = (obj as PDFStream);

                    // Put the dictionary into the list view
                    streamobj.Dict.EnumPDFObjects(new EnumObjectsForDataGrid(dataGridView));

                    System.IO.BinaryReader binaryReader = new System.IO.BinaryReader(streamobj.UnfilteredStream);
                    byte[] buffer = binaryReader.ReadBytes(streamobj.Length);
                    streamViewRaw.Tag = new StreamDataDisplayModePair(buffer, DisplayMode.String);
                    streamViewRaw.Text = convertToRawStringDisplay(buffer);

                    binaryReader = new System.IO.BinaryReader(streamobj.FilteredStream);
                    buffer = readAllBytesFromBinaryReader(binaryReader, 256);
                    streamViewCooked.Tag = new StreamDataDisplayModePair(buffer, DisplayMode.String);
                    streamViewCooked.Text = convertToCookedStringDisplay(buffer);
                }
                else if (obj == null)
                    return; // Do nothing for null objects -- they can legally appear in PDFArrays
                else
                    MessageBox.Show("This data type not recognized");
            }
        }

        private string convertToCookedStringDisplay(byte[] buffer)
        {
            PDFString str = new PDFString(buffer, PDFDoc, false, false);
            List<char> exceptions = new List<char>();
            exceptions.Add('\n');
            exceptions.Add(' ');
            return replaceNonPrintingASCII(str.Value, '.', exceptions).Replace("\n", "\r\n");
        }

        private string convertToRawStringDisplay(byte[] buffer)
        {
            PDFString str = new PDFString(buffer, PDFDoc, false, false);
            List<char> exceptions = new List<char>();
            return replaceNonPrintingASCII(str.Value, '.', exceptions);
        }

        private byte[] readAllBytesFromBinaryReader(BinaryReader binaryReader, int tempBufferSize)
        {
            byte[] buffer = new byte[0];
            byte[] smallBuffer;
            do
            {
                smallBuffer = binaryReader.ReadBytes(tempBufferSize);
                byte[] largerBuffer = new byte[buffer.Length + smallBuffer.Length];
                for (int i = 0; i < buffer.Length; i++)
                {
                    largerBuffer[i] = buffer[i];
                }
                for (int i = buffer.Length; i < buffer.Length + smallBuffer.Length; i++)
                {
                    largerBuffer[i] = smallBuffer[i - buffer.Length];
                }
                buffer = largerBuffer;
            }
            while (smallBuffer.Length == tempBufferSize);
            return buffer;
        }

        private string replaceNonPrintingASCII(string source, char replacement, List<char> exceptions)
        {
            char[] sourceChars = source.ToCharArray();
            for(int i = 0; i < sourceChars.Length; i++)
            {
                int sourceCharCode = (int)sourceChars[i];
                if (0 <= sourceCharCode && sourceCharCode < 38 && !exceptions.Contains(sourceChars[i]))
                {
                    sourceChars[i] = replacement;
                }
            }
            return new string(sourceChars);
        }

        // Whenever a user expands a tree node, populate its subnodes
        private void mainTree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            // We do a limited depth recursive search when a node gets expanded:
            // we iterate through each PDFDict or PDFArray in the target node (e.Node)
            // and add sub-nodes for its contents. Only do this if the node has not
            // yet been populated (chNode.Nodes.Count == 0)
            expandNode(e.Node);
        }

        public static void expandNode(TreeNode node)
        {
            foreach (TreeNode chNode in node.Nodes)
            {
                chNode.Nodes.Clear();
                // If a dict, add nodes for each entry
                if (chNode.Tag is PDFDict)
                {
                    (chNode.Tag as PDFDict).EnumPDFObjects(new EnumObjectsForTree(chNode));
                }
                // If an array, add nodes for each element
                else if (chNode.Tag is PDFArray)
                {
                    enumPDFArrayForTree(chNode);
                }
                // If a stream, add nodes for each entry in its dictionary
                else if (chNode.Tag is PDFStream)
                {
                    (chNode.Tag as PDFStream).Dict.EnumPDFObjects(new EnumObjectsForTree(chNode));
                }
            }
        }

        public static void enumPDFArrayForTree(TreeNode chNode)
        {
            chNode.Nodes.Clear();
            PDFArray array = (chNode.Tag as PDFArray);
            try
            {
                for (int i = 0; i < array.Length; i++)
                {
                    addArrayItemToTree(array, i, chNode);
                }
            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show("This PDF document has invalid array.");
            }
        }

        public static TreeNode addArrayItemToTree(PDFArray array, int i, TreeNode parentNode)
        {
            string type, value;
            TreeNode curr = null;
            if (GetObjectTypeAndValue(array.Get(i), out type, out value))
            {
                if (parentNode.Nodes.Count > i)
                    parentNode.Nodes.RemoveAt(i);
                curr = parentNode.Nodes.Insert(i, value);
                curr.Tag = array.Get(i);
                curr.ImageKey = GetObjectIconName((PDFObject)curr.Tag);
                curr.SelectedImageKey = curr.ImageKey;

                // If it's an indirect object, append the ID and Generation numbers
                if ((curr.Tag != null) && ((curr.Tag as PDFObject).Indirect))
                    curr.Text += (" [ID: " + (curr.Tag as PDFObject).ID
                        + ", Gen: " + (curr.Tag as PDFObject).Generation + "]");
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
            return curr;
        }


        // These methods are public static so the Enum classes can use them
        // Return strings for the data type and contents of the object
        public static bool GetObjectTypeAndValue(PDFObject obj, out String type, out String value)
        {
            bool retval = true;

            // Classify based on object type
            if (obj is PDFBoolean)
            {
                type = "Bool";
                value = (obj as PDFBoolean).Value.ToString();
            }
            else if (obj is PDFInteger)
            {
                type = "Int";
                value = (obj as PDFInteger).Value.ToString();
            }
            else if (obj is PDFReal)
            {
                type = "Real";
                value = (obj as PDFReal).Value.ToString();
            }
            else if (obj is PDFName)
            {
                type = "Name";
                value = (obj as PDFName).Value;
            }
            else if (obj is PDFString)
            {
                type = "String";
                value = (obj as PDFString).Value;
            }
            else if (obj is PDFArray)
            {
                type = "Array";
                value = "[...]";
            }
            else if (obj is PDFDict)
            {
                type = "Dict";
                value = "<<...>>";
            }
            else if (obj is PDFStream)
            {
                type = "Stream";
                value = "<<...>>";
            }
            else if (obj == null)
            {
                // Arrays can have "null" objects, we respect that here
                type = "(null)";
                value = "(null)";
            }
            else
            {
                type = "unknown";
                value = "???";
                retval = false;
            }

            return retval;
        }

        // Return a string handle for the icon for this datatype
        public static String GetObjectIconName(PDFObject obj)
        {
            String iconName;

            // Classify based on object type
            if (obj is PDFBoolean)
            {
                iconName = "BooleanIcon";
            }
            else if (obj is PDFInteger)
            {
                iconName = "IntegerIcon";
            }
            else if (obj is PDFReal)
            {
                iconName = "RealIcon";
            }
            else if (obj is PDFName)
            {
                iconName = "NameIcon";
            }
            else if (obj is PDFString)
            {
                iconName = "StringIcon";
            }
            else if (obj is PDFArray)
            {
                iconName = "ArrayIcon";
            }
            else if (obj is PDFDict)
            {
                iconName = "DictIcon";
            }
            else if (obj is PDFStream)
            {
                iconName = "StreamIcon";
            }
            else
            {
                iconName = null;
            }

            return iconName;
        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dataGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (mainTree.SelectedNode != null)
            {
                if (mainTree.SelectedNode.FullPath.Contains("Document\\Root\\Metadata"))
                {
                    MessageBox.Show("Metadata can't be modified");
                    return;
                }
                editor.beginEdit(e.RowIndex, e.ColumnIndex);
            }
            else
            {
                MessageBox.Show("Please select node in the tree to your left.");
            }
        }

        private void dataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                editor.endEdit();
                docWasModified = editor.WasModified;
                //if(docWasModified)
                //{
                //    //redraw the tree
                //    expandNode(editor.SelectedNode);
                //}
            }
            catch(NullReferenceException)
            {
                editor.restoreValueBeforeEdit();
            }
            catch(InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message);
                editor.restoreValueBeforeEdit();
            }
            catch (FormatException ex)
            {
                MessageBox.Show(ex.Message);
                editor.restoreValueBeforeEdit();
            }
        }


        private void mainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (docWasModified) runSaveDialog();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            savePDFDoc();
        }

        private void savePDFDoc()
        {
            if (PDFDoc != null && docWasModified)
            {
                PDFDoc.Save(SaveFlags.Full);
                docWasModified = false;
            }
        }

        private void viewInHexToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;

            ContextMenuStrip menu = menuItem.Owner as ContextMenuStrip;
            if(menu != null)
            {
                TextBox selectedStreamView = menu.SourceControl as TextBox;
                if (selectedStreamView.Tag != null)
                {
                    
                    StreamDataDisplayModePair dataDisplayModePair = (StreamDataDisplayModePair)selectedStreamView.Tag;
                    if (dataDisplayModePair.Mode == DisplayMode.String)
                    {
                        selectedStreamView.Text = convertToHexDisplay(dataDisplayModePair.Data, 16);
                        dataDisplayModePair.Mode = DisplayMode.Hex;
                        //selectedStreamView.Tag = new StreamDataDisplayModePair(dataDisplayModePair.Key, DisplayMode.Hex);
                    }
                    else
                    {
                        if (selectedStreamView.Equals(streamViewRaw))
                            selectedStreamView.Text = convertToRawStringDisplay(dataDisplayModePair.Data);
                        else
                            selectedStreamView.Text = convertToCookedStringDisplay(dataDisplayModePair.Data);
                        dataDisplayModePair.Mode = DisplayMode.String;
                        //selectedStreamView.Tag = new KeyValuePair<byte[], DisplayMode>(dataDisplayModePair.Key, DisplayMode.String);
                    }
                }
            }
        }

        private string convertToHexDisplay(byte[] data, int charsPerLine)
        {
            StringBuilder sb = new StringBuilder();
            int count = 0;
            foreach (byte b in data)
            {
                if (++count == charsPerLine)
                {
                    sb.Append('\r');
                    sb.Append('\n');
                    count = 0;
                }
                sb.Append(b.ToString("X2"));
                sb.Append(' ');
            }
            return sb.ToString();
        }
    }

    // Callback used to enumerate PDFDict entries for the tree view
    class EnumObjectsForTree : PDFObjectEnumProc
    {
        TreeNode rootNode;
        public EnumObjectsForTree(TreeNode n)
        {
            rootNode = n;
            rootNode.Nodes.Clear();
        }

        // For each key/value pair, we create a new node with the name for a key,
        // and the value as the tag property
        public override bool Call(PDFObject pdfObject, PDFObject pdfObjectValue)
        {
            // The key value SHOULD be a PDFName
            if (pdfObject is PDFName)
            {
                TreeNode curr = rootNode.Nodes.Add((pdfObject as PDFName).Value);
                curr.Tag = pdfObjectValue;
                curr.ImageKey = mainWindow.GetObjectIconName((PDFObject)curr.Tag);
                curr.SelectedImageKey = curr.ImageKey;

                // If it's an indirect object, append the ID and Generation numbers
                // NOTE: getting ID and Generation props on a DIRECT object casues an exception
                if (pdfObjectValue.Indirect)
                    curr.Text += (" [ID: " + pdfObjectValue.ID + ", Gen: " + pdfObjectValue.Generation + "]");
            }
            return true;
        }
    }

    public class EnumObjectsForDataGrid : PDFObjectEnumProc
    {
        DataGridView dataGrid;
        public EnumObjectsForDataGrid(DataGridView dataGrid)
        {
            this.dataGrid = dataGrid;
        }

        // For each key/value pair, we create a new item with the name for a key,
        // plus type and value entries
        public override bool Call(PDFObject pdfObject, PDFObject pdfObjectValue)
        {
            String type;
            String value;

            // The key value SHOULD be a PDFName
            if (pdfObject is PDFName)
            {
                mainWindow.GetObjectTypeAndValue(pdfObjectValue, out type, out value);

                string[] row = new string[] { (pdfObject as PDFName).Value, type, value };
                dataGrid.Rows.Add(row);
            }
            return true;
        }
    }

    class StreamDataDisplayModePair
    {
        private byte[] buffer;
        private DisplayMode mode;

        public StreamDataDisplayModePair(byte[] buffer, DisplayMode displayMode)
        {
            this.buffer = buffer;
            this.mode = displayMode;
        }

        public byte[] Data
        {
            get { return buffer; }
        }

        public DisplayMode Mode
        {
            set { mode = value; }
            get { return mode; }
        }
    }
}
