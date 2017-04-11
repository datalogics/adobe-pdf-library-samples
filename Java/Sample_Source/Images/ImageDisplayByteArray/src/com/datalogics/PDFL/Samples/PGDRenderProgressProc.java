package com.datalogics.PDFL.Samples;

/*
Copyright (c) 2007-2016, Datalogics, Inc. All rights reserved. 

The information and code in this sample is for the exclusive use of Datalogics
customers and evaluation users only.  Datalogics permits you to use, modify and
distribute this file in accordance with the terms of your license agreement.
Sample code is for demonstrative purposes only and is not intended for production use.
 */

import com.datalogics.PDFL.RenderProgressProc;
import com.datalogics.PDFL.RenderProgressStage;

public class PGDRenderProgressProc extends RenderProgressProc {
    public void Call(float stagePercent, java.lang.String info, RenderProgressStage stage)
    {
        
        String stageName = currentStageName(stage);
        
    	System.out.println("Current Stage: " + stageName + String.format(" : %3.0f%%",stagePercent*100));
    }
    
    public String currentStageName(RenderProgressStage stage)
    {
        
        String stageName;
        
        if( stage == RenderProgressStage.RENDER_FLAT_COMPUTING_COMPLEXITY_REGION_CLIPPATH )
            stageName = "RENDER_FLAT_COMPUTING_COMPLEXITY_REGION_CLIPPATH";
        else if( stage == RenderProgressStage.RENDER_FLAT_ENTER_IN_FLATTENER )
            stageName = "RENDER_FLAT_ENTER_IN_FLATTENER";
        else if( stage == RenderProgressStage.RENDER_FLAT_ENTER_IN_PLANAR_MAP )
            stageName = "RENDER_FLAT_ENTER_IN_PLANAR_MAP";
        else if( stage == RenderProgressStage.RENDER_FLAT_FIND_OBJECTS_INVOLVED_IN_TRANSPARENCY )
            stageName = "RENDER_FLAT_FIND_OBJECTS_INVOLVED_IN_TRANSPARENCY";
        else if( stage == RenderProgressStage.RENDER_FLAT_FLATTEN_ATOMIC_REGIONS )
            stageName = "RENDER_FLAT_FLATTEN_ATOMIC_REGIONS";
        else if( stage == RenderProgressStage.RENDER_FLAT_IDENTIFYING_COMPLEXITY_REGION )
            stageName = "RENDER_FLAT_IDENTIFYING_COMPLEXITY_REGION";
        else if( stage == RenderProgressStage.RENDER_FLAT_RASTERIZING_COMPLEXITY_REGION )
            stageName = "RENDER_FLAT_RASTERIZING_COMPLEXITY_REGION";
        else if( stage == RenderProgressStage.RENDER_PROG_STAGE_1 )
            stageName = "RENDER_PROG_STAGE_1";
        else if( stage == RenderProgressStage.RENDER_PROG_STAGE_2 )
            stageName = "RENDER_PROG_STAGE_2";
        else if( stage == RenderProgressStage.RENDER_PROG_STAGE_3 )
            stageName = "RENDER_PROG_STAGE_3";
        else if( stage == RenderProgressStage.RENDER_PROG_STAGE_4 )
            stageName = "RENDER_PROG_STAGE_4";
        else if( stage == RenderProgressStage.RENDER_PROG_STAGE_5 )
            stageName = "RENDER_PROG_STAGE_5";
        else if( stage == RenderProgressStage.RENDER_PROG_STAGE_6 )
            stageName = "RENDER_PROG_STAGE_6";
        else if( stage == RenderProgressStage.RENDER_PROG_STAGE_7 )
            stageName = "RENDER_PROG_STAGE_7";
        else if( stage == RenderProgressStage.RENDER_PROG_STAGE_8 )
            stageName = "RENDER_PROG_STAGE_8";
        else if( stage == RenderProgressStage.RENDER_PROG_STAGE_9 )
            stageName = "RENDER_PROG_STAGE_9";
        else 
            stageName = "RENDER_PROG_UNKNOWN";

        return stageName;
    }
};
