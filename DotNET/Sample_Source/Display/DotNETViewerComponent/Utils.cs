using System;
using System.Collections.Generic;
using System.Text;
using Datalogics.PDFL;

namespace DotNETViewerComponent
{
    public static class Utils
    {
        public static Datalogics.PDFL.Point Transform(System.Drawing.Point point, Datalogics.PDFL.Matrix matrix)
        {
            return Transform(new Datalogics.PDFL.Point(point.X + 0.5, point.Y - 0.5), matrix);
        }

        public static Datalogics.PDFL.Point Transform(Datalogics.PDFL.Point point, Datalogics.PDFL.Matrix matrix)
        {
            Datalogics.PDFL.Point result = new Datalogics.PDFL.Point();

            double x = point.H;
            double y = point.V;

            result.H = x * matrix.A + y * matrix.B + matrix.H;
            result.V = x * matrix.C + y * matrix.D + matrix.V;

            return result;
        }

        public static Datalogics.PDFL.Rect Transform(Datalogics.PDFL.Rect rect, Datalogics.PDFL.Matrix matrix)
        {
            Datalogics.PDFL.Point lowerLeft = new Datalogics.PDFL.Point(rect.LLx, rect.LLy);
            Datalogics.PDFL.Point upperRight = new Datalogics.PDFL.Point(rect.URx, rect.URy);
            lowerLeft = Transform(lowerLeft, matrix);
            upperRight = Transform(upperRight, matrix);

            Datalogics.PDFL.Rect result = new Datalogics.PDFL.Rect(lowerLeft.H, lowerLeft.V, upperRight.H, upperRight.V);

            return result;
        }

        public static System.Drawing.Color Transform(Datalogics.PDFL.Color color)
        {
            if (color == null) return new System.Drawing.Color();

            if (color.Space == ColorSpace.DeviceGray)
            {
                double grey = color.Value[0];
                return System.Drawing.Color.FromArgb(255, (int)(255.0 * grey), (int)(255.0 * grey), (int)(255.0 * grey));
            }
            else
            {
                IList<double> rgba = color.Value;
                return System.Drawing.Color.FromArgb((int)(rgba[0] * 255), (int)(rgba[1] * 255), (int)(rgba[2] * 255));
            }
        }

        public static Datalogics.PDFL.Color Transform(System.Drawing.Color color)
        {
            return new Datalogics.PDFL.Color(color.R / 255.0, color.G / 255.0, color.B / 255.0);
        }
        // creates axis-aligned bounding rect for given quad
        public static Datalogics.PDFL.Rect Rect(Quad quad)
        {
            double minX = quad.BottomLeft.H;
            double maxX = minX;
            double minY = quad.BottomLeft.V;
            double maxY = minY;

            minX = Math.Min(minX, quad.BottomRight.H);
            minX = Math.Min(minX, quad.TopLeft.H);
            minX = Math.Min(minX, quad.TopRight.H);

            maxX = Math.Max(maxX, quad.BottomRight.H);
            maxX = Math.Max(maxX, quad.TopLeft.H);
            maxX = Math.Max(maxX, quad.TopRight.H);

            minY = Math.Min(minY, quad.BottomRight.V);
            minY = Math.Min(minY, quad.TopLeft.V);
            minY = Math.Min(minY, quad.TopRight.V);

            maxY = Math.Max(maxY, quad.BottomRight.V);
            maxY = Math.Max(maxY, quad.TopLeft.V);
            maxY = Math.Max(maxY, quad.TopRight.V);

            return new Datalogics.PDFL.Rect(minX, minY, maxX, maxY);
        }
        // creates quad by given rect
        public static Datalogics.PDFL.Quad Quad(Datalogics.PDFL.Rect rect)
        {
            return new Quad(
                new Datalogics.PDFL.Point(rect.LLx, rect.URy),
                new Datalogics.PDFL.Point(rect.URx, rect.URy),
                new Datalogics.PDFL.Point(rect.LLx, rect.LLy),
                new Datalogics.PDFL.Point(rect.URx, rect.LLy));
        }
        // clones point
        public static Datalogics.PDFL.Point Clone(Datalogics.PDFL.Point p)
        {
            return new Datalogics.PDFL.Point(p.H, p.V);
        }
        // clones quad
        public static Quad Clone(Quad q)
        {
            return new Quad(Clone(q.TopLeft), Clone(q.TopRight), Clone(q.BottomLeft), Clone(q.BottomRight));
        }
        // clones list of points
        public static IList<Datalogics.PDFL.Point> Clone(IList<Datalogics.PDFL.Point> list)
        {
            if (list == null) return null;
            IList<Datalogics.PDFL.Point> newList = new List<Datalogics.PDFL.Point>();
            foreach (Datalogics.PDFL.Point point in list)
            {
                newList.Add(Clone(point));
            }
            return newList;
        }
        // clones list of quads
        public static IList<Quad> Clone(IList<Quad> list)
        {
            if (list == null) return null;
            IList<Quad> newList = new List<Quad>();
            foreach (Quad quad in list)
            {
                newList.Add(Clone(quad));
            }
            return newList;
        }
        // clones list of list of points (used for ink annotation's scribbles)
        public static IList<IList<Datalogics.PDFL.Point>> Clone(IList<IList<Datalogics.PDFL.Point>> list)
        {
            if (list == null) return null;
            IList<IList<Datalogics.PDFL.Point>> newList = new List<IList<Datalogics.PDFL.Point>>();
            foreach (IList<Datalogics.PDFL.Point> subList in list)
            {
                newList.Add(Clone(subList));
            }
            return newList;
        }
    }
}
