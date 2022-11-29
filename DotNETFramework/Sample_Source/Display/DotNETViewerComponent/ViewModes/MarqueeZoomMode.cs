using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace DotNETViewerComponent
{
    /**
     * In this mode user selects a rectangle, and then zoom applied to fit this rectangle to view
     */
    public class MarqueeZoomMode : ViewMode
    {
        #region Constructors
        public MarqueeZoomMode(DocumentView view)
            : base(view)
        {
        }
        #endregion
        #region Methods
        public override void MouseDown(MouseEventArgs e, System.Drawing.Point location)
        {
            if (e.Button != MouseButtons.Left) return;
            startPoint = location;
            endPoint = location;
            docView.MarqueeZoomRect = new Rectangle(
                Math.Min(startPoint.X, endPoint.X), Math.Min(startPoint.Y, endPoint.Y),
                Math.Abs(endPoint.X - startPoint.X), Math.Abs(endPoint.Y - startPoint.Y));
        }
        public override void MouseMove(MouseEventArgs e, System.Drawing.Point location)
        {
            if (e.Button != MouseButtons.Left) return;
            endPoint = location;
            docView.MarqueeZoomRect = new Rectangle(
                Math.Min(startPoint.X, endPoint.X), Math.Min(startPoint.Y, endPoint.Y),
                Math.Abs(endPoint.X - startPoint.X), Math.Abs(endPoint.Y - startPoint.Y));
        }
        public override void MouseUp(MouseEventArgs e, System.Drawing.Point location)
        {
            if (e.Button != MouseButtons.Left) return;
            endPoint = location;
            Rectangle marqueeZoomRect = new Rectangle(
                Math.Min(startPoint.X, endPoint.X), Math.Min(startPoint.Y, endPoint.Y),
                Math.Abs(endPoint.X - startPoint.X), Math.Abs(endPoint.Y - startPoint.Y));
            docView.MarqueeZoomRect = new Rectangle();
            double k = 0;
            if (marqueeZoomRect.Width > 0)
            {
                k = (double)docView.Size.Width / marqueeZoomRect.Width;
            }
            if (marqueeZoomRect.Height > 0)
            {
                if (k == 0) k = (double)docView.Size.Height / marqueeZoomRect.Height;
                else k = Math.Min(k, (double)docView.Size.Height / marqueeZoomRect.Height);
            }
            if (k != 0)
            {
                System.Drawing.Point scroll = marqueeZoomRect.Location;
                scroll.X = (int)(scroll.X * k);
                scroll.Y = (int)(scroll.Y * k);

                docView.RequestZoom(new ZoomEventArgs(Math.Max(Math.Min(docView.Zoom * k, GlobalConsts.MAX_PERCENT_ZOOM_LEVEL), GlobalConsts.MIN_PERCENT_ZOOM_LEVEL), FitModes.FitNone));
                docView.Scroll = scroll;
            }
            docView.FunctionMode = ApplicationFunctionMode.AnnotationEditMode;
        }
        #endregion
        #region Members
        private System.Drawing.Point startPoint;
        private System.Drawing.Point endPoint;
        #endregion
    }
}
