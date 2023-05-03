using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using Datalogics.PDFL;

/*
Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved. 

The information and code in this sample is for the exclusive use of Datalogics
customers and evaluation users only.  Datalogics permits you to use, modify and
distribute this file in accordance with the terms of your license agreement.
Sample code is for demonstrative purposes only and is not intended for production use.
 */

namespace DisplayPDF
{
    public class MyPictureBox : PictureBox
    {
        public List<Quad> HighLightQuads;
        Graphics graphic;
        public Datalogics.PDFL.Matrix matrix;

        public MyPictureBox()
        {
            HighLightQuads = new List<Quad>(0);
        }

        public void ClearList()
        {
            HighLightQuads.Clear();
        }

        public void highlightwords()
        {
            Pen highlightpen = new Pen(System.Drawing.Color.Blue, 1.5F);
            // For each word in the list, highlight the rectangle of the Word's Quads
            foreach (Quad hq in HighLightQuads)
            {
                Quad q = hq.Transform(matrix);
                GraphicsPath highlightpath = new GraphicsPath();
                PointF[] DrawingPoints = new PointF[5];
                DrawingPoints.Initialize();
                DrawingPoints[0] = new System.Drawing.Point((int)q.TopLeft.H, (int)q.TopLeft.V);
                DrawingPoints[1] = new System.Drawing.Point((int)q.TopRight.H, (int)q.TopRight.V);
                DrawingPoints[2] = new System.Drawing.Point((int)q.BottomRight.H, (int)q.BottomRight.V);
                DrawingPoints[3] = new System.Drawing.Point((int)q.BottomLeft.H, (int)q.BottomLeft.V);
                DrawingPoints[4] = new System.Drawing.Point((int)q.TopLeft.H, (int)q.TopLeft.V);

                highlightpath.AddLines(DrawingPoints);
                graphic.DrawPath(highlightpen, highlightpath);
            }
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            graphic = pe.Graphics;
            /*
             * If our word to be searched on the current page is not null,
             * then highlight the word in the PDF document.
             */
            if (HighLightQuads.Count != 0)
            {
                highlightwords();
            }
        }
    }
}
