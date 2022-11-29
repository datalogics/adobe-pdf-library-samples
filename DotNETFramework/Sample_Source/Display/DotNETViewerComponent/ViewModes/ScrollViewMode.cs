using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace DotNETViewerComponent
{
    /**
     * This simple view mode just scrolls the document on mouse dragging.
     */
    public class ScrollViewMode : ViewMode
    {
        #region Contructors
        public ScrollViewMode(DocumentView view)
            : base(view)
        {
        }
        #endregion
        #region Methods
        public override void MouseDown(MouseEventArgs e, System.Drawing.Point location)
        {
            captured = true;
            capturedPoint = location;
        }
        public override void MouseMove(MouseEventArgs e, System.Drawing.Point location)
        {
            if (captured)
            {
                System.Drawing.Point delta = location;
                delta.X -= capturedPoint.X;
                delta.Y -= capturedPoint.Y;
                System.Drawing.Point scroll = docView.Scroll;
                scroll.X -= delta.X;
                scroll.Y -= delta.Y;
                docView.Scroll = scroll;
            }
        }
        public override void MouseUp(MouseEventArgs e, System.Drawing.Point location)
        {
            captured = false;
        }
        #endregion
        #region Members
        private bool captured = false;
        private System.Drawing.Point capturedPoint;
        #endregion
    }
}
