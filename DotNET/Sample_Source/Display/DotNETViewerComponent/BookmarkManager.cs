/*
Copyright (C) 2008-2009, Datalogics, Inc. All rights reserved.
 
The information and code in this sample is for the exclusive use
of Datalogics customers and evaluation users only.  Datalogics 
permits you to use, modify and distribute this file in accordance 
with the terms of your license agreement. Sample code is for 
demonstrative purposes only and is not intended for production use.
*/

/*
 * BookmarkManager :
 * 
 * This class is used to handle the bookmarks within a Document. It has
 * methods for creating and destroying the bookmark tree that can be
 * displayed in the User Interface.
 */

using System;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using Datalogics.PDFL;

namespace DotNETViewerComponent
{
    [ToolboxItem(false)]
    public class BookmarkManager : UserControl
    {
        #region Members
        DotNETController dleController;

        public TreeView bookmarkTree;
        
        public int numberOfBookmarks = 0;
        public Bookmark rootBookmark;

        public TreeToBookmarkMapping[] bookmarkMappingArray;
        public int bookmarkMappingIndex = 0;
        #endregion

        #region Constructors
        public BookmarkManager(DotNETController parentControl)
        {
            dleController = parentControl;

            bookmarkTree = new TreeView();
            bookmarkTree.Dock = DockStyle.Fill;
            bookmarkTree.AfterSelect += new TreeViewEventHandler(dleController.bookmarkTree_AfterSelect);

            this.Dock = DockStyle.Fill;
            this.Controls.Add(bookmarkTree);
        }
        #endregion

        #region Methods
        /**
         * CreateBookmarkTree -
         * 
         * Build the bookmark arrary and the treeview of the bookmarks
         * that is used in the UI.
         */
        public void CreateBookmarkTree(Document doc)
        {
            DestroyBookmarkTree();
            if (doc == null) return;
            rootBookmark = doc.BookmarkRoot;

            if (rootBookmark != null)
            {
                if (rootBookmark.HasChildren())
                {
                    numberOfBookmarks = GetBookmarkArraySize(rootBookmark.FirstChild);
                    bookmarkMappingArray = new TreeToBookmarkMapping[numberOfBookmarks];
                    TreeNode startingTreeNode = null;
                    Bookmark startingBookmark = rootBookmark.FirstChild;
                    // start stepping through the bookmarks
                    RecurseThroughBookmarks(startingBookmark, startingTreeNode, 0);
                }
            }
        }
        /**
         * RecurseThroughBookmarks(Bookmark, TreeNode) -
         * 
         * Step through each bookmark, add it to the array and the tree
         * checking for children and adding them as we go.
         */
        private int RecurseThroughBookmarks(Bookmark currentBookmark, TreeNode currentTreeNode, int index)
        {
            TreeNode justAddedNode = null;

            while (currentBookmark != null)
            {
                // If this bookmark has children, add it to the tree and recurse through its children
                if (currentBookmark.HasChildren())
                {
                    // if this is the first node, then make this the initial node
                    if (currentTreeNode == null)
                        index = RecurseThroughBookmarks(currentBookmark.FirstChild, justAddedNode = bookmarkTree.Nodes.Add(currentBookmark.Title), index);
                    else // else we just need to hang the node off of the existing one
                        index = RecurseThroughBookmarks(currentBookmark.FirstChild, justAddedNode = currentTreeNode.Nodes.Add(currentBookmark.Title), index);
                }
                else
                {
                    // if this is the first node, then make this the initial node
                    if (currentTreeNode == null)
                        justAddedNode = bookmarkTree.Nodes.Add(currentBookmark.Title);
                    else // else we just need to hang the node off of the existing one
                        justAddedNode = currentTreeNode.Nodes.Add(currentBookmark.Title);
                }
                
                // map the node in our tree to the bookmark object 
                bookmarkMappingArray[index].action = currentBookmark.Action;
                bookmarkMappingArray[index].treeNode = justAddedNode;
                ++index;

                // move to the next bookmark
                currentBookmark = currentBookmark.Next;
            }
            return index;
        }
        /**
         * GetBookmarkArraySize(Bookmark) - 
         * 
         * Count the number of bookmarks.
         */
        private int GetBookmarkArraySize(Bookmark currentBookmark)
        {
            int size = 0;
            while (currentBookmark != null)
            {
                ++size;
                if (currentBookmark.HasChildren()) size += GetBookmarkArraySize(currentBookmark.FirstChild);
                currentBookmark = currentBookmark.Next;
            }
            return size;
        }
        /**
         * DestroyBookmarkTree -
         * 
         * Clear the bookmark tree in DotNETController and reset our
         * counts/indexes back to zero.
         */
        public void DestroyBookmarkTree()
        {
            if (bookmarkTree != null) bookmarkTree.Nodes.Clear();
            numberOfBookmarks = 0;
        }
        #endregion
    }
}