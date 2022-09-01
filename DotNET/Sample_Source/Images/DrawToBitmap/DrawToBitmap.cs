using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using Datalogics.PDFL;

/*
 *
 * This program sample converts a PDF file to a series of bitmap image files.
 * 
 * For more detail see the description of the DrawtoBitmap sample program on our Developer's site,
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-sample-programs/converting-pdf-pages-to-images/#drawtobitmap
 * 
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace DrawToBitmap
{
    /// <summary>
    /// The class implements the SampleCancelProc type
    /// </summary>
    class SampleCancelProc : CancelProc
    {
        /// <summary>
        /// The method implements a callback (it MUST be named "Call", return a bool and take no arguments)
        /// It will be driven by DLE and if the method returns true rendering will attempt to cancel.
        /// </summary>
        /// <returns>A boolean that tells DLE to continue (false) or cancel (true)</returns>
        public override bool Call()
        {
            mSomeBoolean = ((mSomeBoolean) ? false : true);
            Console.WriteLine("SampleCancelProc Call.");

            return false;
        }

        static private bool mSomeBoolean;
    }

    /// <summary>
    /// The class implements the SampleRenderProgressProc type
    /// </summary>
    class SampleRenderProgressProc : RenderProgressProc
    {
        /// <summary>
        /// The method implements a callback (it MUST be named "Call" and exhibit the method signature described)
        /// It will be driven by DLE and provide data that can be used to update a progress bar, etc.
        /// </summary>
        /// <param name="stagePercent">A percentage complete (of the stage!). Values will always be in the range of 0.0 (0%) to 1.0 (100%)</param>
        /// <param name="info">A string that will present optional information that may be written to user interface</param>
        /// <param name="stage">An enumeration that indicates the current rendering stage</param>
        public override void Call(float stagePercent, string info, RenderProgressStage stage)
        {
            mSomeBoolean = ((mSomeBoolean) ? false : true);
            Console.WriteLine(String.Format("SampleRenderProgressProc Call (stage/stagePercent/info): {0} {1} {2}", stage, stagePercent, info));
        }

        static private bool mSomeBoolean;
    }

    class ScaledDrawToBitmap
    {
        const double resolution = 96.0;

        /// <summary>
        /// The method constructs a DrawParams instance.
        /// </summary>
        /// <param name="matrix">the matrix for DrawParams</param>
        /// <param name="width">the width for Update/Dest rectangle</param>
        /// <param name="height">the height for Update/Dest rectangle</param>
        /// <param name="blackPointCompensation">the flag which allows to turn on black point compensation</param>
        /// <returns>DrawParams instance</returns>
        private static DrawParams ConstructDrawParams(Matrix matrix, Rect updateRect, bool blackPointCompensation)
        {
            DrawParams parms = new DrawParams();

            parms.Matrix = matrix;
            parms.UpdateRect = updateRect;
            parms.DestRect = parms.UpdateRect.Transform(matrix);

            parms.Flags = DrawFlags.DoLazyErase | DrawFlags.UseAnnotFaces;

            parms.EnableBlackPointCompensation = blackPointCompensation;

            parms.CancelProc = new SampleCancelProc();
            parms.ProgressProc = new SampleRenderProgressProc();

            return parms;
        }

        /// <summary>
        /// The method saves a bitmap to various files.
        /// </summary>
        /// <param name="bitmap">the bitmap to save</param>
        /// <param name="nameSuffix">the suffix for saved file.</param>
        private static void SaveBitmap(Bitmap bitmap, string nameSuffix)
        {
            bitmap.Save(String.Format("DrawTo{0}.bmp", nameSuffix), ImageFormat.Bmp);
            bitmap.Save(String.Format("DrawTo{0}.jpg", nameSuffix), ImageFormat.Jpeg);
            bitmap.Save(String.Format("DrawTo{0}.gif", nameSuffix), ImageFormat.Gif);
            bitmap.Save(String.Format("DrawTo{0}.png", nameSuffix), ImageFormat.Png);
        }

        /// <summary>
        /// The method renders and saves the specified layer to the System.Drawing.Bitmap.
        /// </summary>
        /// <param name="pg">the page that should be rendered into the bitmap</param>
        /// <param name="parms">the DrawParams object</param>
        /// <param name="layerName">the layer name. It uses for saving process.</param>
        private static void DrawLayerToBitmap(Page pg, DrawParams parms, string layerName)
        {
            int width = (int)Math.Ceiling(parms.DestRect.Width);
            int height = (int)Math.Ceiling(parms.DestRect.Height);

            using (Bitmap bitmap = new Bitmap(width, height))
            {
                pg.DrawContents(bitmap, parms);
                bitmap.Save(String.Format("DrawLayer{0}ToBitmap.png", layerName), ImageFormat.Png);
            }
        }

        /// <summary>
        /// The method renders and saves the specified layer to a System.Drawing.Graphics.
        /// </summary>
        /// <param name="pg">the page wich should be rendered into the bitmap</param>
        /// <param name="parms">the DrawParams object</param>
        /// <param name="layerName">the layer name. It uses for saving process.</param>
        private static void DrawLayerToGraphics(Page pg, DrawParams parms, string layerName)
        {   // Draw as if to a DeviceContext (DC)
            int width = (int)Math.Ceiling(parms.DestRect.Width);
            int height = (int)Math.Ceiling(parms.DestRect.Height);

            using (Bitmap bitmap = new Bitmap(width, height))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                pg.DrawContents(graphics, parms);
                bitmap.Save(String.Format("DrawLayer{0}ToGraphics.png", layerName), ImageFormat.Png);
            }
        }

        /// <summary>
        /// The method renders the specified page's layers to a System.Drawing.Bitmap.
        /// </summary>
        /// <param name="doc">Document whose layers should be rendered</param>
        /// <param name="pg">page to render</param>
        /// <param name="parms">the DrawParams object</param>
        static void DrawLayersToBitmap(Document doc, Page pg, DrawParams parms)
        {
            IList<OptionalContentGroup> ocgs = doc.OptionalContentGroups;
            if (ocgs.Count > 0)
            {
                List<bool> states = new List<bool>(new bool[ocgs.Count]);

                for (int i = 0; i < ocgs.Count; ++i)
                {
                    states[i] = true;

                    using (OptionalContentContext occ = new OptionalContentContext(doc))
                    {   // Render and save current layer
                        occ.SetOCGStates(ocgs, states);
                        parms.OptionalContentContext = occ;

                        DrawLayerToBitmap(pg, parms, ocgs[i].Name);

                        // Return DrawParams to its initial value
                        parms.OptionalContentContext = null;
                    }

                    states[i] = false;
                }
            }
        }

        /// <summary>
        /// The method renders the specified page's layers to a System.Drawing.Bitmap.
        /// </summary>
        /// <param name="doc">Document whose layers should be rendered</param>
        /// <param name="pg">page to render</param>
        /// <param name="parms">the DrawParams object</param>
        static void DrawLayersToGraphics(Document doc, Page pg, DrawParams parms)
        {
            IList<OptionalContentGroup> ocgs = doc.OptionalContentGroups;
            if (ocgs.Count > 0)
            {
                List<bool> states = new List<bool>(new bool[ocgs.Count]);
                for (int i = 0; i < ocgs.Count; ++i)
                {
                    states[i] = true;

                    using (OptionalContentContext occ = new OptionalContentContext(doc))
                    {   // Render and save current layer
                        occ.SetOCGStates(ocgs, states);
                        parms.OptionalContentContext = occ;

                        DrawLayerToGraphics(pg, parms, ocgs[i].Name);

                        // return DrawParams to its initial value
                        parms.OptionalContentContext = null;
                    }

                    states[i] = false;
                }
            }
        }

        /// <summary>
        /// The method renders the specified page to a System.Drawing.Bitmap using DrawParams.
        /// </summary>
        /// <param name="pg">page to render</param>
        /// <param name="parms">the DrawParams object</param>
        static void DrawToBitmapWithDrawParams(Page pg, DrawParams parms)
        {
            Rect boundBox = parms.DestRect;

            int width = (int)Math.Ceiling(boundBox.Width);
            int height = (int)Math.Ceiling(boundBox.Height);

            using (Bitmap bitmap = new Bitmap(width, height))
            {
                pg.DrawContents(bitmap, parms);
                SaveBitmap(bitmap, "Bitmap");
            }
        }

        /// <summary>
        /// The method renders the specified page to a System.Drawing.Bitmap.
        /// </summary>
        /// <param name="pg">page to render</param>
        /// <param name="matrix">matrix</param>
        /// <param name="width">width of the destination Bitmap</param>
        /// <param name="height">height of the destination Bitmap</param>
        static void DrawToBitmapWithMatrix(Page pg, Matrix matrix, double width, double height)
        {
            int w = (int)Math.Ceiling(width);
            int h = (int)Math.Ceiling(height);

            using (Bitmap bitmap = new Bitmap(w, h))
            {
                Rect updateRect = new Rect(0, 0, width, height);
                pg.DrawContents(bitmap, matrix, updateRect);
                SaveBitmap(bitmap, "Bitmap");
            }
        }

        /// <summary>
        /// The method renders the specified page to a System.Drawing.Graphics using DrawParams.
        /// </summary>
        /// <param name="pg">page to render</param>
        /// <param name="parms">the DrawParams object</param>
        static void DrawToGraphicsWithDrawParams(Page pg, DrawParams parms)
        {
            Rect boundBox = parms.DestRect;

            int width = (int)Math.Ceiling(boundBox.Width);
            int height = (int)Math.Ceiling(boundBox.Height);

            //
            // Draw as if to a DeviceContext (DC)...
            //

            using (Bitmap bitmap = new Bitmap(width, height))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                pg.DrawContents(graphics, parms);
                SaveBitmap(bitmap, "Graphics");
            }
        }

        /// <summary>
        /// The method renders the specified page to a System.Drawing.Graphics.
        /// </summary>
        /// <param name="pg">page to render</param>
        /// <param name="matrix">matrix</param>
        /// <param name="width">width of the destination Bitmap</param>
        /// <param name="height">height of the destination Bitmap</param>
        static void DrawToGraphicsWithMatrix(Page pg, Matrix matrix, double width, double height)
        {
            int w = (int)Math.Ceiling(width);
            int h = (int)Math.Ceiling(height);

            //
            // Draw as if to a DeviceContext (DC)...
            //

            using (Bitmap bitmap = new Bitmap(w, h))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                Rect updateRect = new Rect(0, 0, width, height);
                pg.DrawContents(graphics, matrix, updateRect);
                SaveBitmap(bitmap, "Graphics");
            }
        }

        /// <summary>
        /// The method renders the specified page to a raw byte buffer.
        /// </summary>
        /// <param name="pg">page to render</param>
        /// <param name="width">width of the destination rect </param>
        /// <param name="height">height of the destination rect </param>
        /// <param name="matrix">the matrix</param>
        static void DrawToByteArray(Page pg, double width, double height, Matrix matrix)
        {
            Byte[] rawBytes = null;

            Rect roundedDestRect;
            using (DrawParams parms = new DrawParams())
            {
                parms.ColorSpace = ColorSpace.DeviceRGB;
                parms.UpdateRect = pg.MediaBox;
                parms.Matrix = matrix;
                parms.Flags = DrawFlags.DoLazyErase | DrawFlags.UseAnnotFaces | DrawFlags.SwapComponents;
                parms.SmoothFlags = SmoothFlags.Image | SmoothFlags.Text;
                var destRect = parms.UpdateRect.Transform(matrix);

                // Round the corners of the destRect so that it's a whole number of pixels.
                // This removes any ambiguity about how DrawContents treats a rectangle that has
                // a width or height that contains a fraction. It ensures that our assumptions about
                // the data in the returned Byte array are the same as those made by DrawContents.
                roundedDestRect = new Rect(
                    Math.Round(destRect.LLx),
                    Math.Round(destRect.LLy),
                    Math.Round(destRect.URx),
                    Math.Round(destRect.URy));

                parms.DestRect = roundedDestRect;

                parms.CancelProc = new SampleCancelProc();
                parms.ProgressProc = new SampleRenderProgressProc();

                rawBytes = (Byte[])pg.DrawContents(parms);
            }

            if (rawBytes == null)
            {   // didn't draw
                return;
            }

            // Make a Bitmap. Get the dimensions from the same rectangle specified
            // as the DestRect in the DrawParams.
            int w = (int) roundedDestRect.Width;
            int h = (int) roundedDestRect.Height;
            int stride = (w * 3 /* components */ + 3 /* padding */) & ~3;

            using (Bitmap bitmap = new Bitmap(w, h, PixelFormat.Format24bppRgb))
            {
                Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                var bitmapData =
                    bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly,
                                    bitmap.PixelFormat);
                Debug.Assert(stride == bitmapData.Stride);
                Marshal.Copy(rawBytes, 0, bitmapData.Scan0, rawBytes.Length);
                bitmap.UnlockBits(bitmapData);
                bitmap.Save("DrawToByteArray.png", ImageFormat.Png);
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("DrawToBitmap Sample");

            try
            {
                using (Library lib = new Library())
                {
                    Console.WriteLine("Initialized the library.");

                    String sInput = Library.ResourceDirectory + "Sample_Input/ducky.pdf";

                    if (args.Length > 0)
                        sInput = args[0];

                    Console.WriteLine("Input file: " + sInput);

                    using (Document doc = new Document(sInput))
                    {
                        using (Page pg = doc.GetPage(0))
                        {
                            //
                            // Must tumble the page to get from PDF with origin at lower left
                            // to a Bitmap with the origin at upper left.
                            //

                            double scaleFactor = resolution / 72.0;
                            double width = (pg.MediaBox.Width * scaleFactor);
                            double height = (pg.MediaBox.Height * scaleFactor);

                            //When the MediaBox's origin isn't at the lower-left of the page we can't use the 'Height' member and
                            //instead used the 'Top'.
                            double ty = pg.MediaBox.Height;
                            if (pg.MediaBox.Bottom != 0)
                            {
                                ty = pg.MediaBox.Top;
                            }

                            Matrix matrix = new Matrix().Scale(scaleFactor, -scaleFactor).Translate(0, -ty);

                            bool enableBlackPointCompensation = true;

                            using (DrawParams parms = ConstructDrawParams(matrix, pg.MediaBox, enableBlackPointCompensation))
                            {
#if !MONO
                                // Draw to Graphics
                                Console.WriteLine(String.Format("DrawToGraphicsWithMatrix: {0} {1} {2}", matrix.ToString(), width, height));
                                DrawToGraphicsWithMatrix(pg, matrix, width, height);    // Will NOT drive SampleRenderProgress(Cancel)Proc

                                // Draw to Graphics using DrawParams with turned on black point compensation
                                Console.WriteLine(String.Format("DrawToGraphicsWithDrawParams: {0} {1} {2}", parms.Matrix.ToString(), parms.UpdateRect.Width, parms.UpdateRect.Height));
                                DrawToGraphicsWithDrawParams(pg, parms);    // Will drive SampleRenderProgress(Cancel)Proc

                                // Demonstrate drawing to Graphics with params and OCGs
                                // Demonstrate drawing layers
                                Console.WriteLine(String.Format("DrawLayersToGraphics: {0} {1} {2}", parms.Matrix.ToString(), parms.UpdateRect.Width, parms.UpdateRect.Height));
                                DrawLayersToGraphics(doc, pg, parms);   // Will NOT drive SampleRenderProgress(Cancel)Proc
#endif
                                // Demonstrate drawing to Bitmaps with params and OCGs
                                // Demonstrate drawing layers
                                Console.WriteLine(String.Format("DrawLayersToBitmap: {0} {1} {2}", parms.Matrix.ToString(), parms.UpdateRect.Width, parms.UpdateRect.Height));
                                DrawLayersToBitmap(doc, pg, parms); // Will NOT drive SampleRenderProgressProc

                                // Make a Bitmap
                                Console.WriteLine(String.Format("DrawToBitmapWithMatrix: {0} {1} {2}", matrix.ToString(), width, height));
                                DrawToBitmapWithMatrix(pg, matrix, width, height);  // Will NOT drive SampleRenderProgress(Cancel)Proc

                                // Make a Bitmap using DrawParams with black point compensation turned on
                                Console.WriteLine(String.Format("DrawToBitmapWithDrawParams: {0} {1} {2}", parms.Matrix.ToString(), parms.UpdateRect.Width, parms.UpdateRect.Height));
                                DrawToBitmapWithDrawParams(pg, parms);  // Will drive SampleRenderProgress(Cancel)Proc

                                // Demonstrate drawing to a byte array
                                Console.WriteLine(String.Format("DrawToByteArray: {0} {1} {2}", matrix.ToString(), width, height));
                                DrawToByteArray(pg, width, height, matrix); // Will drive SampleRenderProgress(Cancel)Proc
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"An exception occurred. Here is the related information:");
                Console.Write(ex.ToString());
            }
        }
    }
}
