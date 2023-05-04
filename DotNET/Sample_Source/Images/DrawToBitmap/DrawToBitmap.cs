using System;
using System.Collections.Generic;
using SkiaSharp;
using Datalogics.PDFL;
using System.IO;

/*
 *
 * This program sample converts a PDF file to a series of image files.
 * 
 * For more detail see the description of the DrawtoBitmap sample program on our Developer’s site,
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/converting-pdf-pages-to-images/#drawtobitmap
 * 
 * 
 * Copyright (c) 2007-2023, Datalogics, Inc. All rights reserved.
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
        /// It will be driven by PDFL and if the method returns true rendering will attempt to cancel.
        /// </summary>
        /// <returns>A boolean that tells PDFL to continue (false) or cancel (true)</returns>
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
        /// It will be driven by PDFL and provide data that can be used to update a progress bar, etc.
        /// </summary>
        /// <param name="stagePercent">A percentage complete (of the stage!). Values will always be in the range of 0.0 (0%) to 1.0 (100%)</param>
        /// <param name="info">A string that will present optional information that may be written to user interface</param>
        /// <param name="stage">An enumeration that indicates the current rendering stage</param>
        public override void Call(float stagePercent, string info, RenderProgressStage stage)
        {
            mSomeBoolean = ((mSomeBoolean) ? false : true);
            Console.WriteLine("SampleRenderProgressProc Call (stage/stagePercent/info): {0} {1} {2}", stage,
                stagePercent, info);
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
        /// <param name="updateRect">The portation of the page to draw</param>
        /// <param name="blackPointCompensation">the flag which allows to turn on black point compensation</param>
        /// <returns>DrawParams instance</returns>
        private static DrawParams ConstructDrawParams(Matrix matrix, Rect updateRect, bool blackPointCompensation)
        {
            DrawParams parms = new DrawParams();

            parms.Matrix = matrix;
            parms.UpdateRect = updateRect;
            parms.ColorSpace = ColorSpace.DeviceRGBA;
            parms.DestRect = parms.UpdateRect.Transform(matrix);

            parms.Flags = DrawFlags.DoLazyErase | DrawFlags.UseAnnotFaces;

            parms.EnableBlackPointCompensation = blackPointCompensation;

            return parms;
        }

        /// <summary>
        /// The method saves a bitmap to various files.
        /// </summary>
        /// <param name="bitmap">the bitmap to save</param>
        /// <param name="nameSuffix">the suffix for saved file.</param>
        private static void SaveBitmap(SKBitmap sKBitmap, string nameSuffix)
        {
            using (FileStream f = File.OpenWrite(String.Format("DrawTo{0}.jpg", nameSuffix)))
                sKBitmap.Encode(SKEncodedImageFormat.Jpeg, 100).SaveTo(f);
            using (FileStream f = File.OpenWrite(String.Format("DrawTo{0}.png", nameSuffix)))
                sKBitmap.Encode(SKEncodedImageFormat.Png, 100).SaveTo(f);
        }

        /// <summary>
        /// The method renders and saves the specified layer to the SkiaSharp.SKBitmap.
        /// </summary>
        /// <param name="pg">the page that should be rendered into the SKBitmap</param>
        /// <param name="parms">the DrawParams object</param>
        /// <param name="layerName">the layer name. It uses for saving process.</param>
        private static void DrawLayerToBitmap(Page pg, DrawParams parms, string layerName)
        {
            int width = (int)Math.Ceiling(parms.DestRect.Width);
            int height = (int)Math.Ceiling(parms.DestRect.Height);

            using (SKBitmap sKBitmap = new SKBitmap(width, height))
            {
                pg.DrawContentsToSKBitmap(sKBitmap, parms);
                using (FileStream f = File.OpenWrite(String.Format("DrawLayer{0}ToBitmap.png", layerName)))
                    sKBitmap.Encode(SKEncodedImageFormat.Png, 100).SaveTo(f);
            }
        }

        /// <summary>
        /// The method renders the specified page's layers to a SkiaSharp.SKBitmap.
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
                    {
                        // Render and save current layer
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
        /// The method renders the specified page to a SkiaSharp.SKBitmap using DrawParams.
        /// </summary>
        /// <param name="pg">page to render</param>
        /// <param name="parms">the DrawParams object</param>
        static void DrawToBitmapWithDrawParams(Page pg, DrawParams parms)
        {
            Rect boundBox = parms.DestRect;

            int width = (int)Math.Ceiling(boundBox.Width);
            int height = (int)Math.Ceiling(boundBox.Height);

            using (SKBitmap sKBitmap = new SKBitmap(width, height))
            {
                parms.CancelProc = new SampleCancelProc();
                parms.ProgressProc = new SampleRenderProgressProc();
                pg.DrawContentsToSKBitmap(sKBitmap, parms);
                SaveBitmap(sKBitmap, "Bitmap");

            }
        }

        static void Main(string[] args)
        {

            Console.WriteLine("DrawToBitmap Sample");

            try
            {

                // ReSharper disable once UnusedVariable
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

                            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                            using (DrawParams parms =
                                ConstructDrawParams(matrix, pg.MediaBox, enableBlackPointCompensation))
                            {
                                // Demonstrate drawing to Bitmaps with params and OCGs
                                // Demonstrate drawing layers
                                Console.WriteLine("DrawLayersToBitmap: {0} {1} {2}", parms.Matrix,
                                    parms.UpdateRect.Width, parms.UpdateRect.Height);
                                DrawLayersToBitmap(doc, pg, parms); // Will NOT drive SampleRenderProgressProc

                                // Make a Bitmap using DrawParams with black point compensation turned on
                                Console.WriteLine("DrawToBitmapWithDrawParams: {0} {1} {2}", parms.Matrix,
                                    parms.UpdateRect.Width, parms.UpdateRect.Height);
                                DrawToBitmapWithDrawParams(pg, parms); // Will drive SampleRenderProgress(Cancel)Proc
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
