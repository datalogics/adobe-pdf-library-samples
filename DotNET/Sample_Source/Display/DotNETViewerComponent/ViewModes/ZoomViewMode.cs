using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace DotNETViewerComponent
{
    /**
     * ZoomEventArgs - class describing document zoom, including increasing/decreasing/fit zooms.
     */
    public class ZoomEventArgs : EventArgs
    {
        public enum ZoomMode { INCREASE, DECREASE, CUSTOM };

        public ZoomEventArgs(bool decInc)
        {
            zoomMode = decInc ? ZoomMode.DECREASE : ZoomMode.INCREASE;
        }
        public ZoomEventArgs(double zoom, FitModes fit)
        {
            zoomMode = ZoomMode.CUSTOM;
            this.zoom = zoom;
            this.fit = fit;
        }

        public FitModes Fit { get { return fit; } }
        public double Zoom { get { return zoom; } }
        public ZoomMode Mode { get { return zoomMode; } }

        private FitModes fit;
        private double zoom;
        private ZoomMode zoomMode;
    }
    /**
     * ZoomViewMode - allows to zoom document using mouse clicks.
     */
    public class ZoomViewMode : ViewMode
    {
        #region Contructors
        public ZoomViewMode(DocumentView view)
            : base(view)
        {
        }
        #endregion
        #region Methods
        public override void MouseDown(MouseEventArgs e, System.Drawing.Point location)
        {
            ZoomEventArgs zoomEvent = new ZoomEventArgs(Control.ModifierKeys == Keys.Shift);
            docView.RequestZoom(zoomEvent);
        }
        #endregion
    }
}
