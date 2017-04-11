using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace DotNETViewerComponent
{
    /**
     * View mode handles user input: mouse and keyboard messages.
     * There is base class for different modes:
     * scroll mode, annotation edit mode, annotation creation modes.
     */
    public class ViewMode
    {
        #region Contructors
        public ViewMode(DocumentView view)
        {
            docView = view;
        }
        #endregion
        #region Methods
        public virtual void MouseDown(MouseEventArgs e, System.Drawing.Point location)
        {
        }
        public virtual void MouseMove(MouseEventArgs e, System.Drawing.Point location)
        {
        }
        public virtual void MouseUp(MouseEventArgs e, System.Drawing.Point location)
        {
        }
        public virtual void MouseDoubleClick(MouseEventArgs e, System.Drawing.Point location)
        {
        }
        public virtual void MouseWheel(MouseEventArgs e, System.Drawing.Point location)
        {
        }
        public virtual void KeyDown(KeyEventArgs e)
        {
        }
        #endregion
        #region Members
        protected DocumentView docView;
        #endregion
    }
}
