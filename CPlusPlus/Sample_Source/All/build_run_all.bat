@ECHO OFF
REM ***
REM ***  Copyright (c) 2015, Datalogics, Inc. All rights reserved.
REM ***

REM **********************************************************************************************************************
REM *** Sample: All - Builds and runs each APDFL sample, and outputs the results.
REM ***
REM *** By default, this occurs with the debug configuration.
REM *** Pass in "release" as an argument to use the release configuration.
REM *** It is important to note that this program assumes: 
REM ***    1. A sample was built successfully <-> Its exe is located in <samplefolder>/<arch>/<stage>/<samplename>.exe
REM ***    2. Above, <samplename> is equal to <samplefolder>.
REM ***
REM *** MAINTENANCE:
REM ***   Step 3 is where you want to be if you want to add or remove samples from this script.
REM ***   Add or remove items from DL_SAMPLE_LIST, making sure to update NUM_SAMPLES,
REM ***   NUM_DL_SAMPLES accordingly. Bear in mind that each item in the list must
REM ***   be, simultaneously, the name of the sample's folder in ../, the name of the sample's .sln,
REM ***   and the name of the executable it builds...
REM ***   If necessary, you can also specify arguments for samples. Follow the examples therein.
REM ***
REM *** USAGE:
REM ***   All arguments are case-insensitive.
REM ***
REM ***   ARGUMENT      EFFECT
REM ***   -noRun        Don't run the samples, just build them.
REM ***   -release      Build Release configuration instead of Debug configuration.
REM ***   -64-bit       Build the 64-bit version instead of 32-bit version.
REM ***
REM *** Steps:
REM *** 1) Initialize.
REM *** 2) Build each sample.
REM *** 3) Decide which samples to run.
REM *** 4) Run the samples.
REM *** 4) Output the results.
REM **********************************************************************************************************************

REM ***  This agreement is between Datalogics, Inc. 101 N. Wacker Drive, Suite 1800,
REM ***  Chicago, IL 60606 ("Datalogics") and you, an end user who downloads
REM ***  source code examples for integrating to the Adobe PDF Library
REM ***  ("the Example Code"). By accepting this agreement you agree to be bound
REM ***  by the following terms of use for the Example Code.
REM *** 
REM ***  LICENSE
REM ***  -------
REM ***  Datalogics hereby grants you a royalty-free, non-exclusive license to
REM ***  download and use the Example Code for any lawful purpose. There is no charge
REM ***  for use of Example Code.
REM *** 
REM ***  OWNERSHIP
REM ***  ---------
REM ***  The Example Code and any related documentation and trademarks are and shall
REM ***  remain the sole and exclusive property of Datalogics and are protected by
REM ***  the laws of copyright in the U.S. and other countries.
REM *** 
REM ***  Datalogics is a trademark of Datalogics, Inc.
REM *** 
REM ***  TERM
REM ***  ----
REM ***  This license is effective until terminated. You may terminate it at any
REM ***  other time by destroying the Example Code.
REM *** 
REM ***  WARRANTY DISCLAIMER
REM ***  -------------------
REM ***  THE EXAMPLE CODE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER
REM ***  EXPRESSED OR IMPLIED, INCLUDING, BUT NOT LIMITED TO THE IMPLIED WARRANTIES
REM ***  OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.
REM *** 
REM ***  DATALOGICS DISCLAIM ALL OTHER WARRANTIES, CONDITIONS, UNDERTAKINGS OR
REM ***  TERMS OF ANY KIND, EXPRESS OR IMPLIED, WRITTEN OR ORAL, BY OPERATION OF
REM ***  LAW, ARISING BY STATUTE, COURSE OF DEALING, USAGE OF TRADE OR OTHERWISE,
REM ***  INCLUDING, WARRANTIES OR CONDITIONS OF MERCHANTABILITY, FITNESS FOR A
REM ***  PARTICULAR PURPOSE, SATISFACTORY QUALITY, LACK OF VIRUSES, TITLE,
REM ***  NON-INFRINGEMENT, ACCURACY OR COMPLETENESS OF RESPONSES, RESULTS, AND/OR
REM ***  LACK OF WORKMANLIKE EFFORT. THE PROVISIONS OF THIS SECTION SET FORTH
REM ***  SUBLICENSEE'S SOLEREMEDY AND DATALOGICS'S SOLE LIABILITY WITH RESPECT
REM ***  TO THE WARRANTY SET FORTH HEREIN. NO REPRESENTATION OR OTHER AFFIRMATION
REM ***  OF FACT, INCLUDING STATEMENTS REGARDING PERFORMANCE OF THE EXAMPLE CODE,
REM ***  WHICH IS NOT CONTAINED IN THIS AGREEMENT, SHALL BE BINDING ON DATALOGICS.
REM ***  NEITHER DATALOGICS WARRANT AGAINST ANY BUG, ERROR, OMISSION, DEFECT,
REM ***  DEFICIENCY, OR NONCONFORMITY IN ANY EXAMPLE CODE.
REM ***  

REM *************************************************
REM *** 1) Initialize.
REM *************************************************

REM *** Initialize environment variables, enable delayed expansion.
SETLOCAL EnableDelayedExpansion  
REM *** Filename of All project.
SET ALL_DL_SLN=All_Datalogics_32Bit.sln

REM ************* Initialize variables which track our progress ******************
REM *** The number of samples that failed to build.
SET /A "NUM_FAIL_BUILD=0"
REM *** A list of the failed builds.
SET DESC_FAIL_BUILD=
REM *** The number of samples that successfully ran.
SET /A "NUM_SUCCEED_RUN=0"
REM *** The return code with failure information
SET /A "RETURN_CODE=0"
REM *** A list of the successful runs.
SET DESC_SUCCEED_RUN=
REM *** The number of samples that failed to run.
SET /A "NUM_FAIL_RUN=0"
REM *** A list of the failed runs.
SET DESC_FAIL_RUN=

REM *** Assume default settings.

REM *** The configuration to use.
SET STAGE=Debug
REM *** Only build the samples, don't run them.
SET ONLY_BUILD=N
REM *** Build 32-Bit by default.
SET ARCH=Win32

:AcceptCommands
REM *** Iterate through arguments, changing settings when needed.
IF /i "%1"=="" (
	GOTO ArgumentsEnd
)
IF /i "%1"=="-release" (
    SET STAGE=Release
) 
IF /i "%1"=="-noRun" (
	SET ONLY_BUILD=Y
)

IF /i "%1"=="-64-bit" (
	SET ALL_DL_SLN=All_Datalogics_64Bit.sln
	SET ARCH=x64
)
SHIFT
GOTO AcceptCommands

:ArgumentsEnd

REM *** Set up the visual studio environment.
IF "%VS120COMNTOOLS%" == "" GOTO Usage
CALL "%VS120COMNTOOLS%\..\..\VC\vcvarsall.bat" x86
IF "%VSINSTALLDIR%" == "" GOTO Usage

REM *************************************************
REM *** 2) Build each sample.
REM *************************************************

ECHO #Building Datalogics samples...
devenv %ALL_DL_SLN% /rebuild "%STAGE%|%ARCH%"
ECHO.

REM *************************************************
REM *** 3) Decide which samples to run.
REM *************************************************

REM *** The total number of samples. This must be accurate!
SET /A "NUM_SAMPLES=55"

REM *** Datalogics Samples.
SET "DL_SAMPLE_LIST=("
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% Annotations\CreateAnnotations"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% Annotations\FlattenAnnotations"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% Printing\PostScriptInjection"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% DocumentOptimization\PDFOptimizer"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% DocumentOptimization\WebOptimizedPDF"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% ContentModification\AddBookmarks"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% ContentModification\AddDocumentInformation"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% ContentModification\AddLinks"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% ContentModification\AddPageNumbers"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% ContentModification\AttachMimeToPDF"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% ContentModification\FlattenTransparency"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% ContentModification\MergeAcroforms"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% ContentModification\MergeDocuments"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% ContentModification\PDFMakeOCGVisible"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% ContentModification\PDFUncompress"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% ContentModification\SplitPDF"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% ContentModification\ImportPages"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% ContentModification\AddWatermark"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% ContentModification\EmbedFonts"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% DocumentConversion\ConvertPDFtoEPS"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% DocumentConversion\ConvertPDFtoPostscript"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% DocumentConversion\XPStoPDF"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% Images\AddThumbnailsToPDF"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% Images\RenderPage"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% Images\CreateImageWithTransparency"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% Images\CreateSeparations"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% Images\CalcImageDPI"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% Images\FindImageResolutions"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% Text\InsertHeadFoot"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% Text\ExtractText"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% Text\AddText"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% Text\UnicodeText"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% Text\HelloJapan"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% Text\TextSearch"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% ContentCreation\AddArt"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% ContentCreation\AddAttachments"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% ContentCreation\AddContent"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% ContentCreation\CreateBookmarks"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% ContentCreation\CreateDocument"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% ContentCreation\CreateLayers"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% ContentCreation\CreateTransparency"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% Security\AESEncryption"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% Security\AddPassword"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% Security\AddRedaction"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% Security\EncryptDocument"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% Security\LockDocument"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% Security\SetUniquePermissions"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% Security\OpenEncrypted"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% Security\ValidateSignatures"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% ContentExtraction\CopyContent"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% ContentExtraction\ExtractAttachments"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% ContentExtraction\ExtractFonts"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% InformationExtraction\CountColorsInDoc"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% InformationExtraction\ExtractDocumentInfo"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST% FileSystem\AlternateFileSystem"
SET "DL_SAMPLE_LIST=%DL_SAMPLE_LIST%)"
REM *** The total number of DL samples. This must be accurate!
SET /A "NUM_DL_SAMPLES=55"

REM *** Di iterates over Datalogics samples. Do not change this value.
SET /A "Di=0"

REM *** Some samples require arguments. The variable name before "_args"
REM *** is the name of the sample.

REM *************************************************
REM *** 4) Run the samples.
REM *************************************************

REM ** Prepare to go through all the samples.
CD ..
REM *** The directory in which the sample folders are located.
SET SAMPLEDIR=%CD%
REM *** Necessary for running.
SET PATH=..\..\..\Binaries;%PATH%
SET CURRENT_SAMPLE=

REM ************************************************************
REM ******************** MAIN LOOP *****************************
REM ************************************************************
:RunSampleLoop_START
REM *** If we've run all the samples, end.
REM *** PL samples are always done last.)
IF %Di% GEQ %NUM_DL_SAMPLES% GOTO RunSampleLoop_END
ECHO.

REM *** Retrieve the next sample.
GOTO GetNextSample
:GotNextSample

CD %SAMPLEDIR%\%CURRENT_SAMPLE%\%ARCH%\%STAGE%

REM Isolate the actual name fromt the group name
for %%F in ("%CURRENT_SAMPLE%") do ( set "EXE_NAME=%%~nxF")

REM *** If the directory could not be found.
IF %ERRORLEVEL% NEQ 0 (GOTO CantFindExe)
REM *** If the exe file could not be found.
IF NOT EXIST %EXE_NAME%.exe (GOTO CantFindExe)

If %ONLY_BUILD% == Y GOTO RunSampleLoop_Call_End 

:RunSampleLoop_Call
	ECHO #Run %CURRENT_SAMPLE%.
	REM *** If the exe file could not be found.
	IF NOT EXIST %EXE_NAME%.exe (GOTO CantFindExe)

	REM *** Call the sample with its arguments, if any.
	REM *** (undefined variables expand to nothing.)
	CD ../../
	%ARCH%\%STAGE%\!EXE_NAME!.exe %!EXE_NAME!_args%
	
	REM *** If it failed to run.
	IF %ERRORLEVEL% NEQ 0 (GOTO FailedRun)
	REM *** Otherwise, it succeeded!
	GOTO SuccessfulRun
:RunSampleLoop_Call_End

:RunSampleLoop_NEXT_ITER
GOTO RunSampleLoop_START

REM ************************************************************
REM ************************************************************
REM ************************************************************

REM ********************************
REM ******* SUBROUTINES ************
REM ********************************
:GetNextSample
SET /A "n=0"

IF %Di% LSS %NUM_DL_SAMPLES% GOTO NextDLSample

:NextDLSample
FOR %%S IN %DL_SAMPLE_LIST% DO (
	IF !n! EQU !Di! SET CURRENT_SAMPLE=%%S
	SET /A "n+=1"
)
SET /A "Di+=1"
GOTO GotNextSample

REM ********************************
:SuccessfulRun

SET /A "NUM_SUCCEED_RUN+=1"
ECHO #Ran successfully.
SET "DESC_SUCCEED_RUN=!DESC_SUCCEED_RUN!^*!CURRENT_SAMPLE! ^& ECHO."

GOTO RunSampleLoop_NEXT_ITER
REM ********************************
:FailedRun

SET /A "NUM_FAIL_RUN+=1"
ECHO #Failed ^(%ERRORLEVEL%^)
SET "DESC_FAIL_RUN=!DESC_FAIL_RUN!^*!CURRENT_SAMPLE! !ERRORLEVEL! ^& ECHO."

GOTO RunSampleLoop_NEXT_ITER
REM ********************************
:CantFindExe

ECHO #%CURRENT_SAMPLE% failed to build.
SET /A "NUM_FAIL_BUILD+=1"
SET "DESC_FAIL_BUILD=!DESC_FAIL_BUILD!^*!CURRENT_SAMPLE! ^& ECHO."

GOTO RunSampleLoop_NEXT_ITER
REM ********************************
REM ********************************
REM ********************************
:RunSampleLoop_END



REM *************************************************
REM *** 5) Print the results.
REM *************************************************


SET /A "NUM_SAMPLES=0"
SET /A "NUM_SAMPLES+=%NUM_DL_SAMPLES%"

ECHO =====================================
IF %ONLY_BUILD% == N (
ECHO =========Build/Run complete.
) ELSE (
ECHO =========Build complete.
)
ECHO =========Total samples^: %NUM_SAMPLES%
ECHO =====================================

IF %NUM_SUCCEED_RUN% EQU %NUM_SAMPLES% (
    ECHO All samples built and ran successfuly.
    ECHO %DESC_SUCCEED_RUN%
) ELSE (
    IF %NUM_FAIL_BUILD% GTR 0 (
        ECHO Failed builds^: %NUM_FAIL_BUILD% 
        ECHO %DESC_FAIL_BUILD%
    ) ELSE (
		ECHO All samples built successfuly.
	)
    IF %NUM_FAIL_RUN% GTR 0 (
        ECHO Failed runs^: %NUM_FAIL_RUN%
        ECHO %DESC_FAIL_RUN%
    )
    IF %NUM_SUCCEED_RUN% GTR 0 (
        ECHO Successful runs^: %NUM_SUCCEED_RUN%
        ECHO %DESC_SUCCEED_RUN%
    )
)
ECHO =====================================
ECHO =====================================
GOTO End

:Usage
ECHO You must have Visual Studio 2013 installed to use this executable.
GOTO End

:MustIncludeFiles
ECHO You must choose to run the DL or the Adobe samples^!
GOTO End

:End

REM  Return NUM_FAIL_BUILDx100 + NUM_FAIL_RUN
REM  e.g. 203 means 2 failed build and 3 failed run
SET /A "RETURN_CODE=NUM_FAIL_BUILD*100+NUM_FAIL_RUN"
ECHO RETURN_CODE %RETURN_CODE%
EXIT /b %RETURN_CODE%