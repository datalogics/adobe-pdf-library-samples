using System;
using System.Threading;
using Datalogics.PDFL;

/*
 * A sample which demonstrates the use of the DLE API to print a
 * PDF file to a windows printer output device.
 * 
 * Copyright 2015, Datalogics, Inc. All rights reserved.
 *
 * The information and code in this sample is for the exclusive use of Datalogics
 * customers and evaluation users only.  Datalogics permits you to use, modify and
 * distribute this file in accordance with the terms of your license agreement.
 * Sample code is for demonstrative purposes only and is not intended for production use.
 *
 */

namespace PrintPDFGUI
{
    #region ProgressUpdateState
    public enum ProgressUpdateState
    {   // Which portion of the (greater progress) window to update...
        Unknown = 0,
        Page,
        Stage,
    }
    #endregion

    #region ProgressUpdateStateBlock
    public class ProgressUpdateStateBlock
    {   // Local tracking state that holds the current progress information
        public ProgressUpdateState what = ProgressUpdateState.Unknown;  // the stage (enum)
        public int pageSum = 0;                                         // total page count
        public int pageCurrent = 0;                                     // current page number
        public string pageInfo = null;                                  // page information (e.g., "Page X of Y") (used for a label, etc.)
        public float stagePercent = 0;                                  // percent complete of the current stage
        public string stageInfo = null;                                 // stage information, if any (used for a label, etc.)

        public ProgressUpdateStateBlock()
        {
            // Nothing more
        }

        public ProgressUpdateStateBlock(ProgressUpdateStateBlock rhs)
        {   // Copy constructor
            this.what = rhs.what;
            this.pageSum = rhs.pageSum;
            this.pageCurrent = rhs.pageCurrent;
            this.pageInfo = rhs.pageInfo;
            this.stagePercent = rhs.stagePercent;
            this.stageInfo = rhs.stageInfo;
        }
    }
    #endregion

    public class SamplePrintProgress : PrintProgressProc
    {
        /// <summary>
        /// The method implements a callback (it MUST be named "Call" and exhibit the method signature described)
        /// It will be driven by DLE and provide data that can be used to update a progress bar, etc.
        /// </summary>
        /// <param name="page">The current page number. It is *always* 0-based. One *will* encounter a value of -1. That means "not applicable".</param>
        /// <param name="totalPages">The total number of pages printing. One *may* encounter a value of -1. That means "not applicable".</param>
        /// <param name="stagePercent">A percentage complete (of the stage). Values will always be in the range of 0.0 (0%) to 1.0 (100%)</param>
        /// <param name="info">A string that will present optional information that may be written to user interface</param>
        /// <param name="stage">An enumeration that indicates the current printing stage</param>
        public override void Call(int page, int totalPages, float stagePercent, string info, PrintProgressStage stage)
        {
            //
            // Stage Information (info) is a *English language only* stage information string (e.g., font name)
            // suitable for use as (pre-localized) source material for a text label related to the current stage.
            // Most stages do not present any stage information (it's never null but *often* empty). For those
            // conditions one must create their own string value(s) to represent the stage name.
            // Stage (stage) is an enumeration of the stage (type). Switch on this value (always) to
            // vary the user presentation of any stage progress bar/info.
            //

            ReportProgress(page + 1 /* 0 to 1-based */, totalPages, stagePercent, info, stage);
            //Console.WriteLine(String.Format("Call (page/stage/stagePercent/info): {0} {1} {2} {3}", page + 1 /* 0 to 1-based */, stage, stagePercent, info));
        }

        #region NumPages
        private int mNumPages;
        public int NumPages
        {   // Storage for a total page count
            set
            {
                this.mNumPages = value;
            }
            get
            {
                return this.mNumPages;
            }
        }
        #endregion

        #region ProgressUI
        PrintProgressForm mProgressUI;
        public PrintProgressForm ProgressUI
        {   // A (reverse) reference to the owning UI (so this code can send messages to controls, there)
            set
            {
                mProgressUI = value;
            }
            private get
            {
                return mProgressUI;
            }
        }
        #endregion

        #region PrintingStarted
        private bool mPrintingStarted = false;
        public bool PrintingStarted
        {   // A simple boolean that tells this thread that the human actually wants to print (execution made it beyond the print dialog)
            set
            {
                this.mPrintingStarted = value;
            }
            get
            {
                return this.mPrintingStarted;
            }
        }
        #endregion

        public void ReportProgressDone()
        {   // Tell the Print Progress form we're done...
            ProgressUI.ProgressDone();
        }

        private void ReportProgress(int page, int totalPages, float stagePercent, string info, PrintProgressStage stage)
        {   // The primary progress update handler...
            switch (stage)
            {
                //
                // If all one wants is a simple, per page progress bar ALL THAT NEEDS BE DONE
                // is to switch on the PrintPage enumeration and present page count information.
                // When doing so, do not forget to change the page from 0-based to 1-based notation.
                // A value of -1 for either value means "not applicable". If one encounters -1,
                // simply ignore the update process for page/NumPages during that encounter.
                //

                //
                // APDFL cannot know how long things will take. So, any desire for an accurate
                // or trivial 0-100% progress bar is a bit of a misnomer. The closest one can
                // come to that sort of thing is to show page-level tracking (e.g., Page X of Y).
                // NONE of the percentage (or page progression) values can ever be equated
                // to any notion of time, remaining or otherwise.
                //
                // Performing simple math like:
                //
                // ((page / totalPages) * 100) * <median page time consumption thus far>
                //
                // ...will NOT accurately predict remaining time.
                //
                // Attempting to provide an estimate of time remaining (when printing or rendering)
                // a PDF is a futile process. Unlike network transmission times for a known quantity,
                // APDFL *cannot know* (and cannot be altered to know) how long the next page will
                // take to process.
                //

                //
                // TIP: When working on a solution that drives commercial digital print devices or
                // a similar high throughput 24/7/365 "keep the planes in the air, not on the ground"
                // environment...
                // 
                // Flattening transparent artwork within the (greater) context of printing can be
                // a bad idea.
                //
                // One should investigate structuring the solution so that artwork to be printed
                // is pre-flattened (see FlattenPDF sample) and saved to a completely non-transparent
                // PDF *first* (separately). Then, simply print the pre-flattened (temp) file for
                // best performance. Separating these activities can greatly improve overall solution
                // performance and (printing) predictability.
                //

                case PrintProgressStage.PrintPage:
                {   // Happens twice...
                    // Once just before a page begins. Then again, just after the page ends.
                    // This is the case where one should update Page progress...
                    ProgressUpdateStateBlock state = new ProgressUpdateStateBlock();
                    if (state.stagePercent < 1.0)
                    {   // Start page...
                        state.what = ProgressUpdateState.Page;
                        state.pageCurrent = ((page > 1) ? page : 1);
                        state.pageSum = ((NumPages > 1) ? NumPages : 1);
                        state.pageInfo = String.Format("Page {0} of {1}", state.pageCurrent, state.pageSum);
                        ProgressUI.ProgressChanged(state);

                        Console.WriteLine(String.Format("Printing {0}.", state.pageInfo));
                    }
                    else
                    {   // End page...
                        // Reset the stage info at page end
                        state.what = ProgressUpdateState.Stage;
                        state.stageInfo = @"";
                        state.stagePercent = 0;
                        ProgressUI.ProgressChanged(state);
                    }
                    break;
                }

                //
                // ALL of the remaining enumeration values are optional.
                // If not interested is presenting stage information to a human,
                // the remaining case block(s) can be omitted entirely.
                //

                case PrintProgressStage.PrintProg_StreamingDocumentResource:
                case PrintProgressStage.PrintProg_StreamingDocumentProcset:
                {   // Downloading a (document level) PostScript dictionary (%%BeginProlog)...
                    ProgressUpdateStateBlock state = new ProgressUpdateStateBlock();
                    state.what = ProgressUpdateState.Stage;
                    state.stageInfo = "Preparing Document";
                    state.stagePercent = stagePercent;
                    ProgressUI.ProgressChanged(state);

                    //
                    // Sleeping here helps with human comprehension
                    // of UI updates but slows printing throughput.
                    //

                    Thread.Sleep(10);
                    break;
                }
                case PrintProgressStage.PrintProg_StreamingDocumentFont:
                case PrintProgressStage.PrintProg_StreamingPageFont:
                {   // Downloading a font (info will contain the font name)...
                    ProgressUpdateStateBlock state = new ProgressUpdateStateBlock();
                    state.what = ProgressUpdateState.Stage;
                    if (info.Length > 0)
                    {
                        state.stageInfo = String.Format("Downloading Font: {0}", info);
                    }
                    else
                    {
                        state.stageInfo = @"Downloading Font";
                    }
                    state.stagePercent = stagePercent;
                    ProgressUI.ProgressChanged(state);

                    //
                    // Sleeping here helps with human comprehension
                    // of UI updates but slows printing throughput.
                    //

                    Thread.Sleep(10);
                    break;
                }
                case PrintProgressStage.PrintProg_StreamingPageContent:
                {   // Sending miscellaneous, page content...
                    ProgressUpdateStateBlock state = new ProgressUpdateStateBlock();
                    state.what = ProgressUpdateState.Stage;
                    state.stageInfo = @"Streaming Page Content";
                    state.stagePercent = stagePercent;
                    ProgressUI.ProgressChanged(state);

                    //
                    // Sleeping here helps with human comprehension
                    // of UI updates but slows printing throughput.
                    //

                    Thread.Sleep(10);
                    break;
                }
                case PrintProgressStage.PrintProg_StreamingPageResource:
                {   // Downloading a (page level) PostScript dictionary...
                    ProgressUpdateStateBlock state = new ProgressUpdateStateBlock();
                    state.what = ProgressUpdateState.Stage;
                    state.stageInfo = @"Streaming Page Resource";
                    state.stagePercent = stagePercent;
                    ProgressUI.ProgressChanged(state);

                    //
                    // Sleeping here helps with human comprehension
                    // of UI updates but slows printing throughput.
                    //

                    Thread.Sleep(10);
                    break;
                }
                case PrintProgressStage.PrintProg_StreamingPageGradient:
                {   // Downloading a gradient...
                    ProgressUpdateStateBlock state = new ProgressUpdateStateBlock();
                    state.what = ProgressUpdateState.Stage;
                    state.stageInfo = @"Streaming Page Gradient";
                    state.stagePercent = stagePercent;
                    ProgressUI.ProgressChanged(state);

                    //
                    // Sleeping here helps with human comprehension
                    // of UI updates but slows printing throughput.
                    //

                    Thread.Sleep(10);
                    break;
                }
                case PrintProgressStage.PrintProg_StreamingPageImage:
                case PrintProgressStage.PrintProg_StreamingPageImageProgressPercent:
                {   // Downloading raster image data...
                    ProgressUpdateStateBlock state = new ProgressUpdateStateBlock();
                    state.what = ProgressUpdateState.Stage;
                    state.stageInfo = @"Streaming Page Image";
                    state.stagePercent = stagePercent;
                    ProgressUI.ProgressChanged(state);

                    //
                    // Sleeping here helps with human comprehension
                    // of UI updates but slows printing throughput.
                    //

                    Thread.Sleep(10);
                    break;
                }
                case PrintProgressStage.PrintProg_StreamingPageImageOPI:
                {   // Replacing an OPI proxy (only occurs when client provides additional OPI replacement callbacks)...
                    ProgressUpdateStateBlock state = new ProgressUpdateStateBlock();
                    state.what = ProgressUpdateState.Stage;
                    state.stageInfo = @"Replacing Open Prepress Interface (OPI) Proxy Image";
                    state.stagePercent = stagePercent;
                    ProgressUI.ProgressChanged(state);

                    //
                    // Sleeping here helps with human comprehension
                    // of UI updates but slows printing throughput.
                    //

                    Thread.Sleep(10);
                    break;
                }
                case PrintProgressStage.PrintProg_StreamingPageSeparation:
                {   // Sending a color separation (plate)...
                    ProgressUpdateStateBlock state = new ProgressUpdateStateBlock();
                    state.what = ProgressUpdateState.Stage;
                    state.stageInfo = @"Streaming Page Separation";
                    state.stagePercent = stagePercent;
                    ProgressUI.ProgressChanged(state);

                    //
                    // Sleeping here helps with human comprehension
                    // of UI updates but slows printing throughput.
                    //

                    Thread.Sleep(10);
                    break;
                }
                case PrintProgressStage.PrintProg_StreamingPageCSA:
                case PrintProgressStage.PrintProg_StreamingPageCRD:
                {   // (Converting a ICC profile to and) Downloading (PostScript) ColorSpace Array or Color Rendering Dictionary...
                    ProgressUpdateStateBlock state = new ProgressUpdateStateBlock();
                    state.what = ProgressUpdateState.Stage;
                    state.stageInfo = @"Streaming Page Content (CSA/CRD)";
                    state.stagePercent = stagePercent;
                    ProgressUI.ProgressChanged(state);

                    //
                    // Sleeping here helps with human comprehension
                    // of UI updates but slows printing throughput.
                    //

                    Thread.Sleep(10);
                    break;
                }
                case PrintProgressStage.PrintFlat_EnterInFlattener: // Start of flattener (0%)
                case PrintProgressStage.PrintFlat_IdentifyingComplexityRegion:
                case PrintProgressStage.PrintFlat_ComputingComplexityRegionClippath:
                case PrintProgressStage.PrintFlat_EnterInPlanarMap:
                case PrintProgressStage.PrintFlat_FlattenAtomicRegions:
                {   // Flattener preparation activities...
                    ProgressUpdateStateBlock state = new ProgressUpdateStateBlock();
                    state.what = ProgressUpdateState.Stage;
                    state.stageInfo = "Flattening...";
                    state.stagePercent = stagePercent;
                    ProgressUI.ProgressChanged(state);

                    //
                    // Sleeping here helps with human comprehension
                    // of UI updates but slows printing throughput.
                    //

                    Thread.Sleep(10);
                    break;
                }
                case PrintProgressStage.PrintFlat_FindObjectsInvolvedInTransparency:
                {   // (Flattener) Searching for Transparency...
                    ProgressUpdateStateBlock state = new ProgressUpdateStateBlock();
                    state.what = ProgressUpdateState.Stage;
                    state.stageInfo = "Searching for Transparent Art...";
                    state.stagePercent = stagePercent;
                    ProgressUI.ProgressChanged(state);

                    //
                    // Sleeping here helps with human comprehension
                    // of UI updates but slows printing throughput.
                    //

                    Thread.Sleep(10);
                    break;
                }
                case PrintProgressStage.PrintFlat_TextHeuristics:
                {   // (Flattener) Calculating Text Interactions...
                    ProgressUpdateStateBlock state = new ProgressUpdateStateBlock();
                    state.what = ProgressUpdateState.Stage;
                    state.stageInfo = "Searching for Transparent Text...";
                    state.stagePercent = stagePercent;
                    ProgressUI.ProgressChanged(state);

                    //
                    // Sleeping here helps with human comprehension
                    // of UI updates but slows printing throughput.
                    //

                    Thread.Sleep(10);
                    break;
                }
                case PrintProgressStage.PrintFlat_RasterizingComplexityRegion:
                {   // (Flattener) Rasterizing...
                    ProgressUpdateStateBlock state = new ProgressUpdateStateBlock();
                    state.what = ProgressUpdateState.Stage;
                    state.stageInfo = "Rasterizing Transparent Art...";
                    state.stagePercent = stagePercent;
                    ProgressUI.ProgressChanged(state);

                    //
                    // Sleeping here helps with human comprehension
                    // of UI updates but slows printing throughput.
                    //

                    Thread.Sleep(10);
                    break;
                }
                case PrintProgressStage.PrintProg_OnHostTrapBeginPage:
                case PrintProgressStage.PrintProg_BeginStreamingTraps:
                {   // On-Host (not in-RIP) trapping activities...
                    ProgressUpdateStateBlock state = new ProgressUpdateStateBlock();
                    state.what = ProgressUpdateState.Stage;
                    state.stageInfo = "On-Host Trapping";
                    state.stagePercent = stagePercent;
                    ProgressUI.ProgressChanged(state);

                    //
                    // Sleeping here helps with human comprehension
                    // of UI updates but slows printing throughput.
                    //

                    Thread.Sleep(10);
                    break;
                }
                case PrintProgressStage.PrintProg_StreamingPageEpilogue:
                {   // End of page activities (i.e., %%PageTrailer)...
                    ProgressUpdateStateBlock state = new ProgressUpdateStateBlock();
                    state.what = ProgressUpdateState.Stage;
                    state.stageInfo = "Finalizing Page";
                    for (float i = 0.0F; i <= 1.0F; i += 0.25F)
                    {   // Emulate stage progression (when we only get a single call)
                        state.stagePercent = i;
                        ProgressUI.ProgressChanged(state);

                        //
                        // Sleeping here helps with human comprehension
                        // of UI updates but slows printing throughput.
                        //

                        Thread.Sleep(2);
                    }

                    //
                    // Sleeping here helps with human comprehension
                    // of UI updates but slows printing throughput.
                    //

                    Thread.Sleep(10);
                    break;
                }
                case PrintProgressStage.PrintProg_StreamingDocumentEpilogue:
                {   // End of file activities (i.e., %%Trailer)...
                    ProgressUpdateStateBlock state = new ProgressUpdateStateBlock();
                    state.what = ProgressUpdateState.Stage;
                    state.pageInfo = "Finalizing Document";
                    for (float i = 0.0F; i <= 1.0F; i += 0.25F)
                    {   // Eemulate stage progression (when we only get a single call)
                        state.stagePercent = i;
                        ProgressUI.ProgressChanged(state);

                        //
                        // Sleeping here helps with human comprehension
                        // of UI updates but slows printing throughput.
                        //

                        Thread.Sleep(2);
                    }


                    //
                    // Sleeping here helps with human comprehension
                    // of UI updates but slows printing throughput.
                    //

                    Thread.Sleep(10);

                    // Conclude the progress UI...
                    state.what = ProgressUpdateState.Page;
                    state.stageInfo = "Printing Complete";
                    state.pageCurrent = 0;
                    state.pageSum = 0;
                    break;
                }
            }
        }
    }
}
