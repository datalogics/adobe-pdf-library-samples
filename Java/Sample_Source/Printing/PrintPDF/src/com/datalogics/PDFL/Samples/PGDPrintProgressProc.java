package com.datalogics.pdfl.samples.Printing.PrintPDF;
/*
 * 
 * A sample which demonstrates the use of the DLE API to obtain information about
 * print job progress.
 * 
 * Copyright (c) 2007-2016, Datalogics, Inc. All rights reserved.
 *
 * The information and code in this sample is for the exclusive use of Datalogics
 * customers and evaluation users only.	 Datalogics permits you to use, modify and
 * distribute this file in accordance with the terms of your license agreement.
 * Sample code is for demonstrative purposes only and is not intended for production use.
 *
 */

import java.lang.String;
import com.datalogics.PDFL.PrintProgressProc;
import com.datalogics.PDFL.PrintProgressStage;

public class PGDPrintProgressProc extends PrintProgressProc {
	
	public void Call(int page, int totalPages, float stagePercent, java.lang.String info, PrintProgressStage stage)
	{
		String stageName = stageName(stage);
		System.out.println(String.format("Total:%3.0f%% Stage Name: %10s", stagePercent * 100 ,stageName));
	}
	
	public String stageName(PrintProgressStage stage)
	{
		String stageName;
		
        if( stage == PrintProgressStage.PRINT_PROG_COMPILING_DOCUMENT_RESOURCES )
            stageName = "PRINT_PROG_COMPILING_DOCUMENT_RESOURCES";
        else if( stage == PrintProgressStage.PRINT_PROG_COMPILING_PAGE_RESOURCES )
            stageName = "PRINT_PROG_COMPILING_PAGE_RESOURCES";
        else if( stage == PrintProgressStage.PRINT_PROG_STREAMING_DOCUMENT_FONT )
            stageName = "PRINT_PROG_STREAMING_DOCUMENT_FONT";
        else if( stage == PrintProgressStage.PRINT_PROG_STREAMING_DOCUMENT_RESOURCE )
            stageName = "PRINT_PROG_STREAMING_DOCUMENT_RESOURCE";
        else if( stage == PrintProgressStage.PRINT_PROG_STREAMING_PAGE_FONT )
            stageName = "PRINT_PROG_STREAMING_PAGE_FONT";
        else if( stage == PrintProgressStage.PRINT_PROG_STREAMING_PAGE_RESOURCE )
            stageName = "PRINT_PROG_STREAMING_PAGE_RESOURCE";
        else if( stage == PrintProgressStage.PRINT_PROG_STREAMING_PAGE_CONTENT )
            stageName = "PRINT_PROG_STREAMING_PAGE_CONTENT";
        else if( stage == PrintProgressStage.PRINT_PROG_STREAMING_PAGE_EPILOGUE )
            stageName = "PRINT_PROG_STREAMING_PAGE_EPILOGUE";
        else if( stage == PrintProgressStage.PRINT_PROG_STREAMING_DOCUMENT_EPILOGUE )
            stageName = "PRINT_PROG_STREAMING_DOCUMENT_EPILOGUE";
        else if( stage == PrintProgressStage.PRINT_PROG_STREAMING_DOCUMENT_PROCSET )
            stageName = "PRINT_PROG_STREAMING_DOCUMENT_PROCSET";
        else if( stage == PrintProgressStage.PRINT_PROG_STREAMING_PAGE_SEPARATION )
            stageName = "PRINT_PROG_STREAMING_PAGE_SEPARATION";
        else if( stage == PrintProgressStage.PRINT_PROG_STREAMING_PAGE_IMAGE )
            stageName = "PRINT_PROG_STREAMING_PAGE_IMAGE";
        else if( stage == PrintProgressStage.PRINT_PROG_STREAMING_PAGE_IMAGE_OPI)
            stageName = "PRINT_PROG_STREAMING_PAGE_IMAGE_OPI";
        else if( stage == PrintProgressStage.PRINT_PROG_STREAMING_PAGE_CSA )
            stageName = "PRINT_PROG_STREAMING_PAGE_CSA";
        else if( stage == PrintProgressStage.PRINT_PROG_STREAMING_PAGE_CRD )
            stageName = "PRINT_PROG_STREAMING_PAGE_CRD";
        else if( stage == PrintProgressStage.PRINT_PROG_STREAMING_PAGE_GRADIENT	)
            stageName = "PRINT_PROG_STREAMING_PAGE_GRADIENT";
        else if( stage == PrintProgressStage.PRINT_PROG_ON_HOST_TRAP_BEGIN_PAGE	 )
            stageName = "PRINT_PROG_ON_HOST_TRAP_BEGIN_PAGE";
        else if( stage == PrintProgressStage.PRINT_PROG_SET_ON_HOST_TRAP_PROGRESS_PERCENT  )
            stageName = "PRINT_PROG_SET_ON_HOST_TRAP_PROGRESS_PERCENT";
        else if( stage == PrintProgressStage.PRINT_PROG_STREAMING_PAGE_IMAGE_PROGRESS_PERCENT  )
            stageName = "PRINT_PROG_STREAMING_PAGE_IMAGE_PROGRESS_PERCENT";
        else if( stage == PrintProgressStage.PRINT_PROG_BEGIN_STREAMING_TRAPS  )
            stageName = "PRINT_PROG_BEGIN_STREAMING_TRAPS";
        else if( stage == PrintProgressStage.PRINT_PROG_SET_STREAMING_TRAP_PERCENT	)
            stageName = "PRINT_PROG_SET_STREAMING_TRAP_PERCENT";
        else if( stage == PrintProgressStage.PRINT_FLAT_ENTER_IN_FLATTENER	)
            stageName = "PRINT_FLAT_ENTER_IN_FLATTENER";
        else if( stage == PrintProgressStage.PRINT_FLAT_FIND_OBJECTS_INVOLVED_IN_TRANSPARENCY  )
            stageName = "PRINT_FLAT_FIND_OBJECTS_INVOLVED_IN_TRANSPARENCY";
        else if( stage == PrintProgressStage.PRINT_FLAT_TEXT_HEURISTICS	 )
            stageName = "PRINT_FLAT_TEXT_HEURISTICS";
        else if( stage == PrintProgressStage.PRINT_FLAT_IDENTIFYING_COMPLEXITY_REGION  )
            stageName = "PRINT_FLAT_IDENTIFYING_COMPLEXITY_REGION";
        else if( stage == PrintProgressStage.PRINT_FLAT_COMPUTING_COMPLEXITY_REGION_CLIPPATH  )
            stageName = "PRINT_FLAT_COMPUTING_COMPLEXITY_REGION_CLIPPATH";
        else if( stage == PrintProgressStage.PRINT_FLAT_ENTER_IN_PLANAR_MAP	 )
            stageName = "PRINT_FLAT_ENTER_IN_PLANAR_MAP";
        else if( stage == PrintProgressStage.PRINT_FLAT_FLATTEN_ATOMIC_REGIONS	)
            stageName = "PRINT_FLAT_FLATTEN_ATOMIC_REGIONS";
        else if( stage == PrintProgressStage.PRINT_FLAT_RASTERIZING_COMPLEXITY_REGION  )
            stageName = "PRINT_FLAT_RASTERIZING_COMPLEXITY_REGION";
        else if( stage == PrintProgressStage.PRINT_PAGE )
            stageName = "PRINT_PAGE";
        else
            stageName = "PRINT_PROG_UNKNOWN";

	
		return stageName;
	}
}
