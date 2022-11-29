/*
Copyright (C) 2008-2009, Datalogics, Inc. All rights reserved.
 
The information and code in this sample is for the exclusive use
of Datalogics customers and evaluation users only.  Datalogics 
permits you to use, modify and distribute this file in accordance 
with the terms of your license agreement. Sample code is for 
demonstrative purposes only and is not intended for production use.
*/

/*
 * Enums.cs
 * 
 * Contains a the declarations of all enumerations used in DotNETViewerComponent
 */

using System;
using System.Text;
using System.Collections.Generic;

namespace DotNETViewerComponent
{
    // Possible Display Modes
    public enum PageViewMode
    {
        continuousPage,
        singlePage,
        notApplicable
    };

    // Possible modes for the application
    public enum ApplicationFunctionMode
    {
        ZoomMode,
        MarqueeZoomMode,
        ScrollMode,
        TargetMode,
        AnnotationEditMode,
        AnnotationShapeCreateMode,
    };

    // Highlight states
    public enum ApplicationHighlight
    {
        Highlight,
        NoHighlight
    };

    // Zoom fit modes
    public enum FitModes
    {
        FitNone,
        FitWidth,
        FitPage
    };
}
