/*--------------------------------------------------------------------------------
** Copyright (c) 2014, Datalogics, Inc. All rights reserved.
*/
//
// For complete copyright information, refer to:
// http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
//
// PDFViewer is an application to use to open and view PDF documents in Windows environments,
// intended to serve as a model that you can use to build your own viewing tool. As such
// PDFViewer is a simple utility, though it will quickly open and display PDF documents
// with standard page and file sizes.
//
// The user interface provided with PDFViewer allows you to set the page orientation and
// scaling, and to select the page in the document to display. You can also page through
// the document using arrows or the Home key to return to the first page or the End key
// to go to the last page, and you can use the mouse to position pages or change the page
// scaling or rotation.
//
// For more detail see the description of the PDFViewer sample program on our Developer’s site,
// http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/c1samples#pdfviewer
//

#include "PDFViewer.h" /* Definitions for this application. 
                            ** This includes the APDFL Library interfaces,
                            ** since many are required for the defintions of objects
                            */

/* Make these controls globally available
**
** These are the controls to select Page, Orientation, and Scale
**
** They are created in the Main Frame, but are accessed in the view
*/
static CComboBox SelectRotationList; /* Pull Down List of Rotations */
static CComboBox SelectScaleList;    /* Pull Down List of Scales */
static CComboBox SelectPageList;     /* Pull Down List of Pages */

/* This object is the one and only instance of the PDFViewer */
CPDFViewerApp theApp;

/* Message map connecting messages to methods
** for the PDFViewer itself
*/
BEGIN_MESSAGE_MAP(CPDFViewerApp, CWinApp)
ON_COMMAND(ID_APP_ABOUT, OnAppAbout)
ON_COMMAND(ID_FILE_OPEN, CWinApp::OnFileOpen)
END_MESSAGE_MAP()

CPDFViewerApp::CPDFViewerApp() {
    APDFLInitialized = false; /* APDFL is not initialized when the viewer is created */
}

/* This is the initialization for the Viewer Instance.
** This creates the Windows structure here, and open the Adobe PDF Library
*/
BOOL CPDFViewerApp::InitInstance() {
    /* This registry directory is used to save the Recent Files
    ** list, and settings, from one run to another. It will be saved in
    ** HKEY_CURRENT_USERS/Software/ (The key below) /DLViewer/...
    */
    SetRegistryKey(_T("Datalogics"));

    /* Load standard INI file options (including MRU)
    **   (Set the "recent" list size to 10)
    */
    LoadStdProfileSettings(10);

    /* Create the template needed to build document windows.
     */
    CMultiDocTemplate *pDocTemplate;
    pDocTemplate = new CMultiDocTemplate(IDI_ICON1, RUNTIME_CLASS(CPDFViewerDoc),
                                         RUNTIME_CLASS(CDocumentFrame), RUNTIME_CLASS(CPDFViewerView));
    AddDocTemplate(pDocTemplate);

    /* Create and show the DLViewer frame */
    CMainFrame *pMainFrame = new CMainFrame;
    if (!pMainFrame->LoadFrame(IDI_ICON1))
        return false;
    m_pMainWnd = pMainFrame;
    pMainFrame->ShowWindow(m_nCmdShow);
    pMainFrame->UpdateWindow();

    /*  Initialize the PDF library
    **  Point resources to appropriate directories
    **
    ** NOTE: The resources set here assumes that this application
    ** runs using the samples directory as the current working
    ** directory. Hence all paths are relative from the samples
    ** directory to the resources installed with APDFL, and with
    ** the samples.
    **
    */

    /* Where to look for fonts */
    ASUTF16Val *Paths[3];
    Paths[0] = (ASUTF16Val *)L"..\\..\\..\\..\\Resources\\CMap";
    Paths[1] = (ASUTF16Val *)L"..\\..\\..\\..\\Resources\\Font";

    /* Where to look for Color Profiles */
    ASUTF16Val *Colors[3];
    Colors[0] = (ASUTF16Val *)L"..\\..\\..\\..\\Resources\\Color";

    /* Where Windows keeps color profiles */
    Colors[1] = (ASUTF16Val *)malloc(256 * sizeof(ASUTF16Val));
    GetSystemDirectory((LPWSTR)Colors[1], 256);
    wcscat_s((wchar_t *)Colors[1], 256, L"\\spool\\drivers\\color");

    /* Where to look for Plugins  */
    ASUTF16Val *Plugins[3];
    Plugins[0] = (ASUTF16Val *)L"..\\..\\..\\Binaries";

    /* Construct the APDFL Initialization record.
    **  NOTE: PDFViewer does not use a local memory manager
    */
    PDFLDataRec pdflData;
    memset(&pdflData, 0, sizeof(PDFLDataRec));
    pdflData.size = sizeof(PDFLDataRec);
    pdflData.dirList = Paths;
    pdflData.listLen = 2;
    pdflData.cMapDirectory = (ASUTF16Val *)L"..\\..\\..\\..\\Resources\\CMap";
    pdflData.unicodeDirectory = (ASUTF16Val *)L"..\\..\\..\\..\\Resources\\Unicode";
    pdflData.pluginDirList = Plugins;
    pdflData.pluginDirListLen = 1;
    pdflData.colorProfileDirList = Colors;
    pdflData.colorProfileDirListLen = 2;

    /* Initialize the Library
     */
    if (PDFLInitHFT(&pdflData) != 0)
        return false;

    APDFLInitialized = TRUE;

    /* Set up APDFL parameters to keep throughout the application's lifetime
    ** Typically these options govern how the document appears.
    */

    return TRUE;
}

/* Terminate APDFL, and close the
** primary window
*/
int CPDFViewerApp::ExitInstance() {
    if (APDFLInitialized)
        PDFLTermHFT();
    return CWinApp::ExitInstance();
}

/* Definition of the "About" box
 */
class CAboutDlg : public CDialog {
  public:
    CAboutDlg();
    enum { IDD = IDD_ABOUTBOX };

  protected:
    virtual void DoDataExchange(CDataExchange *pDX); // DDX/DDV support
  protected:
    DECLARE_MESSAGE_MAP()
};

CAboutDlg::CAboutDlg() : CDialog(CAboutDlg::IDD) {}

void CAboutDlg::DoDataExchange(CDataExchange *pDX) { CDialog::DoDataExchange(pDX); }

BEGIN_MESSAGE_MAP(CAboutDlg, CDialog)
END_MESSAGE_MAP()

/* Display the "About" Box
 */
void CPDFViewerApp::OnAppAbout() {
    CAboutDlg aboutDlg;
    aboutDlg.DoModal();
}

/* This is the definition of the primary window displayed for PDFViewer.
** It contains a tool bar and a window to display individual documents.
*/
IMPLEMENT_DYNAMIC(CMainFrame, CMDIFrameWnd)

BEGIN_MESSAGE_MAP(CMainFrame, CMDIFrameWnd)
ON_WM_CREATE()
END_MESSAGE_MAP()

/* Create the main window Object */
CMainFrame::CMainFrame() {}

/* Destroy the main window Object*/
CMainFrame::~CMainFrame() {}

/* Display the main window */
int CMainFrame::OnCreate(LPCREATESTRUCT lpCreateStruct) {
    /* Open the main window */
    if (CMDIFrameWnd::OnCreate(lpCreateStruct) == -1)
        return -1;

    /* Create and Display the toolbar */
    if (!Toolbar.CreateEx(this, TBSTYLE_FLAT,
                          WS_CHILD | WS_VISIBLE | CBRS_TOP | CBRS_GRIPPER | CBRS_TOOLTIPS | CBRS_FLYBY | CBRS_SIZE_DYNAMIC) ||
        !Toolbar.LoadToolBar(IDI_ICON1)) {
        TRACE0("Failed to create toolbar\n");
        return -1; // fail to create
    }

    /* Create and display a pull down to select rotation */
    int nIndex = Toolbar.GetToolBarCtrl().CommandToIndex(ID_PAGE_ROTATE);
    Toolbar.SetButtonInfo(nIndex, ID_PAGE_ROTATE, TBBS_SEPARATOR, 205);

    /* Position it on the toolbar, with its right edge aligned to the
    ** "button" for rotation
    ** Increase its width and depth to allow for the orientation names
    */
    CRect rect;
    Toolbar.GetToolBarCtrl().GetItemRect(nIndex, &rect);
    rect.top = 1;
    rect.bottom = rect.top + 250;
    rect.right = rect.left + 140;
    if (!SelectRotationList.Create(CBS_DROPDOWNLIST | WS_VISIBLE | WS_TABSTOP | WS_VSCROLL, rect,
                                   &Toolbar, ID_PAGE_ROTATE)) {
        TRACE0("Failed to create Select Rotation\n");
        return false;
    }

    /* Add the names of the standard orientations */
    SelectRotationList.AddString(L"Portrait");
    SelectRotationList.AddString(L"Landscape");
    SelectRotationList.AddString(L"Inverted Portrait");
    SelectRotationList.AddString(L"Inverted Landscape");
    SelectRotationList.SetCurSel(0);

    /* Create and display a pull down to select scaling */
    nIndex = Toolbar.GetToolBarCtrl().CommandToIndex(ID_PAGE_SCALE);
    Toolbar.SetButtonInfo(nIndex, ID_PAGE_SCALE, TBBS_SEPARATOR, 205);

    /* Position it just to the right of the orientation box */
    rect.left = rect.right + 5;
    rect.right = rect.left + 70;

    if (!SelectScaleList.Create(CBS_DROPDOWN | WS_VISIBLE | WS_TABSTOP | WS_VSCROLL, rect, &Toolbar, ID_PAGE_SCALE)) {
        TRACE0("Failed to create Select Scale\n");
        return false;
    }

    /* Provide a list of standard scalings.
    ** NOTE: The edit box may be used to specify a non-standard
    ** scale factor.
    */
    SelectScaleList.AddString(L"0.25");
    SelectScaleList.AddString(L"0.5");
    SelectScaleList.AddString(L"0.75");
#define SelectPage100Percent 3
    SelectScaleList.AddString(L"1");
    SelectScaleList.AddString(L"1.25");
    SelectScaleList.AddString(L"1.5");
    SelectScaleList.AddString(L"2");
    SelectScaleList.AddString(L"4");
    SelectScaleList.AddString(L"8");
    SelectScaleList.AddString(L"16");
    SelectScaleList.AddString(L"32");
    SelectScaleList.AddString(L"64");
    SelectScaleList.SetCurSel(SelectPage100Percent);

    /* Create and display a pull down to select page */
    nIndex = Toolbar.GetToolBarCtrl().CommandToIndex(ID_PAGE_SELECT);
    Toolbar.SetButtonInfo(nIndex, ID_PAGE_SELECT, TBBS_SEPARATOR, 205);

    /* Positioned just to the right of the scale bar
     */
    rect.left = rect.right + 5;
    rect.right = rect.left + 70;

    if (!SelectPageList.Create(CBS_DROPDOWN | WS_VISIBLE | WS_TABSTOP | WS_VSCROLL, rect, &Toolbar, ID_PAGE_SELECT)) {
        TRACE0("Failed to create Select Page\n");
        return false;
    }

    /* The user may enter a page number in the edit box.
    ** The list of pages will be updated when the document is opened.
    */
    SelectPageList.AddString(L"1");
    SelectPageList.SetCurSel(0);

    lpCreateStruct->style |= WS_VSCROLL | WS_HSCROLL;
    return 0;
}

/* Set values for the creation of the main window */
BOOL CMainFrame::PreCreateWindow(CREATESTRUCT &cs) {
    if (!CMDIFrameWnd::PreCreateWindow(cs))
        return false;

    cs.style |= WS_VSCROLL | WS_HSCROLL;

    return TRUE;
}

/* This is the definition of the window to contain a
** single PDF document
*/
IMPLEMENT_DYNCREATE(CDocumentFrame, CMDIChildWnd)

BEGIN_MESSAGE_MAP(CDocumentFrame, CMDIChildWnd)
END_MESSAGE_MAP()

/* Create the window object */
CDocumentFrame::CDocumentFrame() {}

/* Destroy the window object */
CDocumentFrame::~CDocumentFrame() {}

/* Definition of a PDF document, displayed in a window */
IMPLEMENT_DYNCREATE(CPDFViewerDoc, CDocument)

BEGIN_MESSAGE_MAP(CPDFViewerDoc, CDocument)
ON_COMMAND(ID_FILE_CLOSE, OnFileClose)
ON_COMMAND(ID_FILE_SEND_MAIL, OnFileSendMail)
ON_UPDATE_COMMAND_UI(ID_FILE_SEND_MAIL, OnUpdateFileSendMail)
END_MESSAGE_MAP()

/* Create the document display */
CPDFViewerDoc::CPDFViewerDoc() {
    Doc = NULL;
    Page = NULL;
    NumberPages = 0;
}

/* Destroy the document display */
CPDFViewerDoc::~CPDFViewerDoc() {}

/* Open a given document for display in this window */
BOOL CPDFViewerDoc::OnOpenDocument(LPCTSTR lpszPathName) {
    ASPathName Path;

    DURING

        /* Save the document name internally */
        wcscpy_s(DocName, sizeof(DocName) / sizeof(wchar_t), lpszPathName);

        /* Create an APDFL ASPath from the name.
        ** The name is specified in host ordered UTF16
        */
        ASText pathNameASText = ASTextFromUnicode((ASUTF16Val *)lpszPathName, kUTF16HostEndian);
        Path = ASFileSysCreatePathFromDIPathText(NULL, pathNameASText, NULL);
        ASTextDestroy(pathNameASText);

        /* Open the document */
        Doc = PDDocOpen(Path, 0, 0, true);

        /* Acquire and store the number of pages in the document */
        NumberPages = PDDocGetNumPages(Doc);

    HANDLER
        wchar_t Buffer[1024];
        swprintf_s(Buffer, sizeof(Buffer) / sizeof(wchar_t), L"Unable to open '%s'.", lpszPathName);
        AfxGetMainWnd()->MessageBox(Buffer);
        Doc = NULL;
        NumberPages = 0;
    END_HANDLER

    ASFileSysReleasePath(0, Path);
    if (Doc) {
        /* Save the current file name in the MRU list */
        CWinApp *app = AfxGetApp();
        wchar_t path[_MAX_PATH];
        if (GetCurrentDirectory(sizeof(path) / sizeof(wchar_t), path) < sizeof(path))
            app->WriteProfileString(L"Settings", L"Dir", path);
    }

    return Doc != NULL;
}

void CPDFViewerDoc::OnFileClose() {
    /* If there is no current document, there is nothing to do */
    if (!Doc)
        return;

    /* Release the current page, if there is one */
    if (Page)
        PDPageRelease(Page);

    /* Release the current document */
    PDDocClose(Doc);

    /* Set pointers to NULL, to avoid freeing twice */
    Doc = NULL;
    Page = NULL;

    /* Tell the document object to close
    ** This will close and free the view, stop the render thread,
    ** and free all resources
    */
    OnCloseDocument();
}

/* This is the PDFViewerView, the actual display within the Document Window
 */

IMPLEMENT_DYNCREATE(CPDFViewerView, CScrollView)

BEGIN_MESSAGE_MAP(CPDFViewerView, CScrollView)

/* "Hot Key" controls */
ON_COMMAND(ID_NEXT_PAGE, OnNextPage)
ON_COMMAND(ID_PREV_PAGE, OnPrevPage)
ON_COMMAND(ID_FIRST_PAGE, OnFirstPage)
ON_COMMAND(ID_LAST_PAGE, OnLastPage)

/* Mouse button controls */
ON_WM_MOUSEMOVE()
ON_WM_LBUTTONDOWN()
ON_WM_LBUTTONUP()
ON_WM_RBUTTONDOWN()
ON_WM_LBUTTONDBLCLK()

/* Command line buttons */
ON_CBN_SELCHANGE(ID_PAGE_ROTATE, CPDFViewerView::OnRotationSelectChange)
ON_CBN_SELCHANGE(ID_PAGE_SCALE, CPDFViewerView::OnScaleSelectChange)
ON_CBN_SELCHANGE(ID_PAGE_SELECT, CPDFViewerView::OnPageSelectChange)
ON_CBN_SELCHANGE(ID_PAGE_SELECT, CPDFViewerView::OnPageSelectChange)

END_MESSAGE_MAP()

/* Create a View */
CPDFViewerView::CPDFViewerView() {
    /* Note that there is no offscreen DC in existance
     */
    OffScreenDC = NULL;
    OldScreenDC = NULL;

    /* Create mutexes to synch drawing to a DC
    ** in a separate thread.
    **
    ** A mutex is a program object that allows more than one program thread
    ** to share a resource, such as a file access, but not at the same time.
    */
    RenderThreadReady = CreateMutex(NULL, false, NULL);
    DrawNextBitmap = CreateMutex(NULL, false, NULL);
    NextBitmapComplete = CreateMutex(NULL, false, NULL);

    /* Take ownership of "DrawNextBitmap"
    **
    ** This will stop the render thread from rendering a page
    ** until it is released.
    */
    WaitForSingleObject(DrawNextBitmap, INFINITE);

    /*
    ** The program cannot start the thread to render pages here, as
    ** it does not yet have a connection to the PDFViewerDocument.
    ** So the program will do that in OnInitialUpdate.
    **
    ** In fact, much of the initialization has to wait for
    ** that connection, and is done in Initial Update
    */

    /* Initially, there is no old map to scale */
    OldMapValid = false;

    /* initially, we are not tracking the mouse. */
    TrackMouse = false;

    /* Note that there is no pending rotation */
    PendingRotate = 0;

    /* Set up no antialiasing */
    AATextDDR = true;
    AATextPreview = false;
    AAText = true;
    AAArt = true;
    AAImage = true;
    AABiCubic = false;
    AAThinLine = true;

    CurrentPage = 0;
}

/* Destroy a View */
CPDFViewerView::~CPDFViewerView() {
    /* If an Off Screen DC was created, free it
     */
    if (OffScreenDC)
        DeleteDC(OffScreenDC);

    /* If an Old Offscreen DC was created,
    ** free it as well
    */
    if (OldScreenDC)
        DeleteDC(OldScreenDC);

    /* Tell the render threads to shut down,
    ** and release it to run
    */
    ThreadData.Done = true;
    ReleaseMutex(DrawNextBitmap);

    /* Free the mutexes used to control the
    ** render thread
    */
    CloseHandle(RenderThreadReady);
    CloseHandle(DrawNextBitmap);
    CloseHandle(NextBitmapComplete);
}

/* This draw method will use a single thread, started at View creation, to
** draw to the off screen DC.
**
** The PDFViewer keeps a block of information, shared with that thread, that will allow a user to
** specify page and matrix. A pair of mutexes will be used to start and note the end of
** rendering. Note that the program also needs to notify the main thread when it has completed the
** APDFL initialization, and open document. Otherwise the Viewer might get a call to render before it is ready.
*/

DWORD WINAPI RenderToDCViaThreadRepeatedly(void *clientData) {
    ThreadCommunication *threadData = (ThreadCommunication *)clientData;

    /* Initialize the library in a second thread
    **
    ** CMaps will not be located correctly unless they are included in the
    ** resource directory list of the second init
    */

    /* Where to look for fonts */
    ASUTF16Val *Paths[3];
    Paths[0] = (ASUTF16Val *)L"..\\..\\..\\..\\Resources\\CMap";
    Paths[1] = (ASUTF16Val *)L"..\\..\\..\\..\\Resources\\Font";

    /* Where to look for Color Profiles */
    ASUTF16Val *Colors[3];
    Colors[0] = (ASUTF16Val *)L"..\\..\\..\\..\\Resources\\Color";

    /* Where Windows keeps color profiles */
    Colors[1] = (ASUTF16Val *)malloc(256 * sizeof(ASUTF16Val));
    GetSystemDirectory((LPWSTR)Colors[1], 256);
    wcscat_s((wchar_t *)Colors[1], 256, L"\\spool\\drivers\\color");

    /* Where to look for Plugins  */
    ASUTF16Val *Plugins[3];
    Plugins[0] = (ASUTF16Val *)L"..\\..\\..\\Binaries";

    /* Construct the APDFL Initialization record.
    **  NOTE: PDFViewer does not use a local memory manager
    */
    PDFLDataRec pdflData;
    memset(&pdflData, 0, sizeof(PDFLDataRec));
    pdflData.size = sizeof(PDFLDataRec);
    pdflData.dirList = Paths;
    pdflData.listLen = 2;
    pdflData.cMapDirectory = (ASUTF16Val *)L"..\\..\\..\\..\\Resources\\CMap";
    pdflData.unicodeDirectory = (ASUTF16Val *)L"..\\..\\..\\..\\Resources\\Unicode";
    pdflData.pluginDirList = Plugins;
    pdflData.pluginDirListLen = 1;
    pdflData.colorProfileDirList = Colors;
    pdflData.colorProfileDirListLen = 2;
    if (PDFLInitHFT(&pdflData) != 0)
        return 0;

    /* Open the document whose page is to be rendered */
    ASText pathNameASText = ASTextFromUnicode((ASUTF16Val *)threadData->DocName, kUTF16HostEndian);
    ASPathName Path = ASFileSysCreatePathFromDIPathText(NULL, pathNameASText, NULL);
    ASTextDestroy(pathNameASText);
    PDDoc doc = PDDocOpen(Path, 0, 0, true);

    ASUns32 PageNumber = 0;
    PDPage page = NULL;

    /* The main thread must sleep until this mutex becomes
    ** unavailable.
    */
    WaitForSingleObject(threadData->ReadyMutex, INFINITE);

    while (1) {
        /* Wait for the main thread to say to render a page
         */
        WaitForSingleObject(threadData->StartMutex, INFINITE);
        ReleaseMutex(threadData->StartMutex);

        /* Main thread, before starting a new page rendering,
        ** should wait for this mutex to clear. It will be owned
        ** by the render thread until the rendering is complete.
        **
        ** The main thread may wait on this mutex, to assure a
        ** rendering is avaialable, or use the fact that it is not
        ** raised to use a previously built rendering via "StretchBlt".
        */
        WaitForSingleObject(threadData->DoneMutex, INFINITE);

        /* Stop the thread when told to
         */
        if (threadData->Done)
            break;

        /* If not already in place,
        ** Acquire the page to render
        */
        if ((!page) || (PageNumber != threadData->Page)) {
            if (page)
                PDPageRelease(page);
            page = PDDocAcquirePage(doc, threadData->Page);
        }
        PageNumber = threadData->Page;

        /* Render the page to the specified DC */
        ASRealMatrix pageMatrixR;
        ASDoubleToRealMatrix(&pageMatrixR, threadData->Matrix);

        /* Turn on Line, Image, and Text smoothing
         */
        ASUns32 AAPref = 0;
        if (threadData->AAText)
            AAPref |= kPDPrefAASmoothText;
        if (threadData->AAArt)
            AAPref |= PDPrefAASmoothLineArt;
        if (threadData->AAImage)
            AAPref |= 0x00000010;
        if (threadData->AABiCubic)
            AAPref |= 0x01000000;
        if (threadData->AATextDDR)
            AAPref |= kPDPageDrawSmoothAATextDDR;
        if (threadData->AATextPreview)
            AAPref |= kPDPageDrawSmoothAATextPreview;
        ASUns32 oldAA = PDPrefGetAntialiasLevel();
        PDPrefSetAntialiasLevel(AAPref);

        ASUns32 oldThinLine = PDPrefGetEnableThinLineHeuristics();
        PDPrefSetEnableThinLineHeuristics(threadData->AAThinLine);

        PDPageDrawWParamsRec drawParams;
        memset((char *)&drawParams, 0, sizeof(PDPageDrawWParamsRec));
        drawParams.size = sizeof(PDPageDrawWParamsRec);
        drawParams.asRealMatrix = (ASRealMatrix *)&pageMatrixR;
        drawParams.displayContext = threadData->TargetDC;
        drawParams.flags = kPDPageDoLazyErase | kPDPageThreadIPParse;
        drawParams.flags = kPDPageDoLazyErase;
        DURING /* Enclose the call to render in a DURING/HANDLER clause
               ** so that parse errors can be ignored.
               ** If a parse error does occur, the screen will be drawn up to that
               ** point in content
               */
            PDPageDrawContentsToWindowWithParams(page, &drawParams);
        HANDLER /* The empty handler will ignore any exceptions raised
                     */
        END_HANDLER

        PDPrefSetAntialiasLevel(oldAA);
        PDPrefSetEnableThinLineHeuristics(oldThinLine);

        /* Tell the main thread the DC is ready
         */
        ReleaseMutex(threadData->DoneMutex);

        if (!threadData->DrawToScreen) {
            /* Wait a few ticks. Then force a windows redraw
            **
            ** This will cause the new rendering to be displayed,
            ** even if the user has not touched the controls
            */
            Sleep(10);
            threadData->Viewer->Invalidate();
        }
    }

    /* Release Resources */
    if (page)
        PDPageRelease(page);
    PDDocClose(doc);

    /* Shut down the library within this thread.
     */
    PDFLTermHFT();

    return 1;
}

/*
** This is the routine that controls the actual window display. It is called whenever we
** change a value associated with displaying the document (Scale, Rotation, Page, Scroll
** position), or when the window is resized, moved, or uncovered.
**
** When we wish to start a new page, we should delete and clear the "OffScreenDC" Pointer.
** This will cause us to create a new rendering DC, Create a rendering for it, and pause
** until the render is complete before drawing from the rendering, to the screen, via BitBlt.
**
** When the user scrolls, we will display the existing image, using either BitBlt, if the
** rendering is complete, or StretchBlt if it is not. We will start a new rendering ONLY when
** the page scale changes. If the page is rotated, we will rotate the existing image, and
** continue to display that. If neither changes, we will simply display a different portion
** of the existing image using BitBlt.
**
*/
void CPDFViewerView::OnDraw(CDC *pDC) {
    /* Return focus to this window (It may have been moved to control)
     */
    SetFocus();

    /* What portion of the document can be viewed */
    RECT viewSize;
    GetClientRect(&viewSize);
    ASInt32 hSize = viewSize.right;
    ASInt32 vSize = viewSize.bottom;

    /* What portion of the document is selected */
    ScrollPosition = GetScrollPosition();

    /* In OffScreenDC, we have the last drawn rendering of the page, OR,
    ** if the NextBitmapComplete mutex is raised, we are drawing the next
    ** rendering.
    **
    ** In OldScreenDC, we may have the last valid rendering we displayed.
    ** it will be a rendering in the same rotation as current, but will be scaled
    ** differently. While NextBitmapComplete is raised, we will use this rendering
    ** to draw to the screen, using StretchBlt to "scale" it up or down as needed
    ** to represent the desired scaling. We will continue to do this until the new
    ** rendering is available.
    **
    ** In the case of rotation, the image we render may be in either OffScreenDC,
    ** or OldScreenDC. This depends on wheter a newly scaled image is being prepared.
    ** If one is, we will rotate the image in OldScreenDC, and continue to use it, If
    ** one is not, we will simply rotate the image OffScreenDC. We accomplish rotation
    ** by simply rotating the image data. We will not render again for rotation. But the
    ** next rendering due to a scale change will be rendered at the current rotation.
    */

    /* If we already have a rendering of the page, consider reusing it
     */
    if (OffScreenDC) {
        /* If the rendering we have is in a different scale or rotation, then
        ** we cannot just blit the current rendering
        */
        if (memcmp((char *)&LastPageToScreenMatrix, (char *)&PageToScreenMatrix, sizeof(ASDouble) * 4)) {

            /* If the rotation changed, then we can manually rotate the
            ** previous bitmap, and continue using it
            */
            if (Rotation != OldRotation) {
                /* If we are currently waiting for a newly scaled rendering, we
                ** cannot "rotate" the OffScreenDC (since it is being updated),
                ** so we will use the image in OldScreenDc (Which we would use for
                ** scrolling, in any event), and continue to use it.
                */
                HDC workingDC = OffScreenDC;
                ASUns32 workingWide = WindowWide;
                ASUns32 workingDeep = WindowDeep;

                /* Check to see if there is a rendering in progress
                 */
                if (WaitForSingleObject(NextBitmapComplete, 0) == WAIT_TIMEOUT) {
                    /* If there is, and the old map is still valid,
                    ** rotate the old map
                    */
                    if (OldMapValid) {
                        workingDC = OldScreenDC;
                        workingWide = OldWindowWide;
                        workingDeep = OldWindowDeep;
                        PendingRotate = Rotation - OldRotation;
                    } else {
                        /* If the old map was not valid, then rotate the new
                        ** map, after waiting for it to complete rendering
                        */
                        WaitForSingleObject(NextBitmapComplete, INFINITE);
                        ReleaseMutex(NextBitmapComplete);
                    }
                } else
                    /* If there was no rendering in progress, free the mutex */
                    ReleaseMutex(NextBitmapComplete);

                /* Get the bitmap out of the DC we intend to rotate */
                HBITMAP Bitmap = (HBITMAP)GetCurrentObject(workingDC, OBJ_BITMAP);

                /* Rotate the image. The rotated image will be put into the
                ** desired DC. It may or may not be in the same Bitmap.
                */
                RotateDC(workingDC, Bitmap, Rotation - OldRotation, workingWide, workingDeep);

                /* Get the bitmap after it is rotated
                ** and get the sizes
                */
                Bitmap = (HBITMAP)GetCurrentObject(workingDC, OBJ_BITMAP);
                BITMAP bitmapHeader;
                memset((char *)&bitmapHeader, 0, sizeof(BITMAP));
                GetObject(Bitmap, sizeof(BITMAP), &bitmapHeader);
                OldWindowWide = bitmapHeader.bmWidth;
                OldWindowDeep = -bitmapHeader.bmHeight;

                /* Calculate the scale factor needed for this image,
                ** if it is both scaled and rotated (this will only happen
                ** if there was a valid old image, and we are currently
                ** rendering to a new scale factor)
                */
                if (((ASUns32)(Rotation - OldRotation) % 180) == 0)
                    OldScale = ((OldWindowWide * 1.0) / (WindowDeep * 1.0));
                else
                    OldScale = ((OldWindowWide * 1.0) / (WindowWide * 1.0));
            } else {
                /* If the scale changed, then we do need to start a new rendering.
                 */

                /*
                ** We must wait for any pending rendering to complete before we
                **   start a new scaled rendering
                */
                WaitForSingleObject(NextBitmapComplete, INFINITE);
                ReleaseMutex(NextBitmapComplete);

                /* Create a new bitmap to hold the new rendering
                 */
                HBITMAP bitmap = CreateCompatibleBitmap(GetDC()->m_hDC, WindowWide, WindowDeep);

                /* Select the new bitmap into the offscreen DC
                 */
                HBITMAP oldBitmap = (HBITMAP)SelectObject(OffScreenDC, bitmap);

                /* Put the old bitmap into a second DC, to use for rendering with StretchBlt or
                ** rotate DC
                */
                HBITMAP OlderBitmap = (HBITMAP)SelectObject(OldScreenDC, oldBitmap);

                /* Delete the oldest bitmap
                 */
                DeleteObject((HGDIOBJ)OlderBitmap);

                /* Start the render thread, and stop after one rendering
                 */
                ReleaseMutex(DrawNextBitmap);
                Sleep(100); /* Wait long enough for the render thread to grab the Done mutex */
                WaitForSingleObject(DrawNextBitmap, INFINITE);

                /* Note that there is a valid image in the old
                ** map
                */
                OldMapValid = true;

                /* From the bitmap in the old screen DC,
                ** calculate the scale needed to stretch the
                ** old map to the new map size
                */
                BITMAP bitmapHeader;
                memset((char *)&bitmapHeader, 0, sizeof(BITMAP));
                GetObject(oldBitmap, sizeof(BITMAP), &bitmapHeader);
                OldWindowWide = bitmapHeader.bmWidth;
                OldWindowDeep = bitmapHeader.bmHeight;
                OldScale = ((OldWindowWide * 1.0) / (WindowWide * 1.0));
            }

            /* Save the attributes of the last offscreen image */
            memmove((char *)&LastPageToScreenMatrix, (char *)&PageToScreenMatrix, sizeof(ASDoubleMatrix));
            OldRotation = Rotation;

            /* Fall through to blit the old, or the offscreen DC to the window */
        }
    } else {
        /* If this is the first time that we are displaying a given page,
        ** create an off-screen rendering of the entire page, in the given
        ** scale and rotation
        */
        OffScreenDC = CreateCompatibleDC(GetDC()->m_hDC);
        OldScreenDC = CreateCompatibleDC(GetDC()->m_hDC);
        HBITMAP bitmap = CreateCompatibleBitmap(GetDC()->m_hDC, WindowWide, WindowDeep);
        HBITMAP oldBitmap = (HBITMAP)SelectObject(OffScreenDC, bitmap);
        DeleteObject((HGDIOBJ)oldBitmap);

        /* Set page to render */
        ThreadData.Page = CurrentPage;

        /* Start a draw thread to draw directly to the window
        ** Turn off all AA for this drawing.
        */
        PageToWindowMatrix = PageToScreenMatrix;
        if ((ASUns32)hSize > WindowWide)
            PageToWindowMatrix.h += (hSize - WindowWide) / 2;
        if ((ASUns32)vSize > WindowDeep)
            PageToWindowMatrix.v += (vSize - WindowDeep) / 2;
        ThreadData.TargetDC = GetDC()->m_hDC;
        ThreadData.AAText = ThreadData.AATextPreview = ThreadData.AATextDDR = false;
        ThreadData.AAArt = ThreadData.AAThinLine = false;
        ThreadData.AAImage = ThreadData.AABiCubic = false;
        ThreadData.DrawToScreen = true;
        ThreadData.Matrix = &PageToWindowMatrix;

        /* Start the drawing thread */
        ReleaseMutex(DrawNextBitmap);
        Sleep(100); /* Wait long enough for the render thread to grab the Done mutex */
        WaitForSingleObject(DrawNextBitmap, INFINITE);

        /* Wait for the draw towindow to complete */
        WaitForSingleObject(NextBitmapComplete, INFINITE);
        ReleaseMutex(NextBitmapComplete);

        /* Start a Thread to draw this page to the offscreen DC
         */
        ThreadData.TargetDC = OffScreenDC;
        ThreadData.AAText = AAText;
        ThreadData.AATextPreview = AATextPreview;
        ThreadData.AATextDDR = AATextDDR;
        ThreadData.AAArt = AAArt;
        ThreadData.AAThinLine = AAThinLine;
        ThreadData.AAImage = AAImage;
        ThreadData.AABiCubic = AABiCubic;
        ThreadData.DrawToScreen = false;
        ThreadData.Matrix = &PageToScreenMatrix;

        /* Start the render thread, and stop after one rendering */
        ReleaseMutex(DrawNextBitmap);
        Sleep(100); /* Wait long enough for the render thread to grab the Done mutex */
        WaitForSingleObject(DrawNextBitmap, INFINITE);

        /* Save the attributes of the last offscreen image */
        memmove((char *)&LastPageToScreenMatrix, (char *)&PageToScreenMatrix, sizeof(ASDoubleMatrix));
        OldRotation = Rotation;

        /* Note that the old map is no longer valid
         */
        OldMapValid = false;

        /* Fall through to blit the old, or the offscreen DC to the window */
    }

    /* When the page is smaller than the screen, center the page in the screen
     */
    ASUns32 leftIndent = 0;
    ASUns32 topIndent = 0;
    if ((ASUns32)hSize > WindowWide) {
        leftIndent = (hSize - WindowWide) / 2;
        ScrollPosition.x = 0;
    }
    if ((ASUns32)vSize > WindowDeep) {
        topIndent = (vSize - WindowDeep) / 2;
        ScrollPosition.y = 0;
    }

    /* Fill the background of the area with gray
    ** This will make the fact that the page is smaller than the screen
    ** appearent, when it is true
    **
    ** If we draw the entire screen to gray before we place the image over it,
    ** we get a "flickering" image when the view size is large, and we drag
    ** the image around. So just draw the (up to) four gray bars as they are needed.
    **
    ** Note also: If we do not do this, whether to gray or white, then if the page is
    ** smaller than the view, we will not repaint areas of the view. This can cause
    ** problems with the appearance of a page.
    */
    if (leftIndent) {
        RECT fillArea;
        fillArea.left = 0;
        fillArea.top = 0;
        fillArea.right = leftIndent;
        fillArea.bottom = vSize;
        FillRect(GetDC()->m_hDC, &fillArea, (HBRUSH)GetStockObject(DKGRAY_BRUSH));
        fillArea.left = hSize - leftIndent;
        fillArea.top = 0;
        fillArea.right = hSize;
        fillArea.bottom = vSize;
        FillRect(GetDC()->m_hDC, &fillArea, (HBRUSH)GetStockObject(DKGRAY_BRUSH));
    }
    if (topIndent) {
        RECT fillArea;
        fillArea.left = 0;
        fillArea.top = 0;
        fillArea.right = hSize;
        fillArea.bottom = topIndent;
        FillRect(GetDC()->m_hDC, &fillArea, (HBRUSH)GetStockObject(DKGRAY_BRUSH));
        fillArea.left = 0;
        fillArea.top = vSize - topIndent;
        fillArea.right = hSize;
        fillArea.bottom = vSize;
        FillRect(GetDC()->m_hDC, &fillArea, (HBRUSH)GetStockObject(DKGRAY_BRUSH));
    }

    /* If there is a draw in progress, then either redraw the
    ** current map using StretchBlt (if it is a scaled version of the
    ** desired page), or wait for it to complete
    */
    if (WaitForSingleObject(NextBitmapComplete, 0) == WAIT_TIMEOUT) {
        /* We are waiting for a render to complete */
        if (OldMapValid) {
            /* Draw the old rendering, "stretched" to fit */
            StretchBlt(GetDC()->m_hDC, leftIndent, topIndent, hSize, vSize, OldScreenDC,
                       (ASUns32)(ScrollPosition.x * OldScale), (ASUns32)(ScrollPosition.y * OldScale),
                       (int)(hSize * OldScale), (int)(vSize * OldScale), SRCCOPY);
            return;
        } else
            /* Wait for the new rendering to complete */
            //            WaitForSingleObject (NextBitmapComplete, INFINITE);
            /* The draw to bitmap will call OnDraw when it completes. So we
            ** do not need to wait for it.
            */
            return;
    }
    /* Allow another rendering to begin
    ** Not start one, but don't stop one either
    */
    ReleaseMutex(NextBitmapComplete);

    /* If we handled a rotation while this image was rendering,
    ** Then we need to rotate the current image
    */
    if (PendingRotate != 0) {
        HBITMAP Bitmap = (HBITMAP)GetCurrentObject(OffScreenDC, OBJ_BITMAP);
        RotateDC(OffScreenDC, Bitmap, PendingRotate, WindowWide, WindowDeep);
        PendingRotate = 0;
    }

    /* Copy the visible portion of the page to the screen
     */
    BitBlt(GetDC()->m_hDC, leftIndent, topIndent, hSize, vSize, OffScreenDC, ScrollPosition.x,
           ScrollPosition.y, SRCCOPY);
}

/*
** This method is called whenever the window is moved, resized, or uncovered.
*/
void CPDFViewerView::OnUpdate(CView *pSender, LPARAM lHint, CObject *pHint) {
    /* Get the screen resolution
    ** In case we moved from one display to another
    */
    ASUns32 hRes = GetDC()->GetDeviceCaps(LOGPIXELSX);
    ScreenResolution = (hRes * 1.0) / 72.0;

    /* Recalculate the page metrics */
    PageToScreen();
}

/* This is invoked once, AFTER the view is created, and properly "connected" to
** The document. It will cause the initial drawing of page one of the document
** on the screen.
**
** We are actaully initializing most of the values for the view in this
** method
*/
void CPDFViewerView::OnInitialUpdate() {
    /* Get a reference to the document this view is displaying */
    pDoc = GetDocument();

    /* Get the screen resolution */
    ASUns32 hRes = GetDC()->GetDeviceCaps(LOGPIXELSX);
    ScreenResolution = (hRes * 1.0) / 72.0;

    /* Initialize matrixes to erect, unscaled page */
    Scale = 1.0;
    Rotation = 0.0;

    /* Initialize Selection boxes to erect, unscaled page
     */
    SelectRotationList.SetCurSel(0);
    SelectScaleList.SetCurSel(SelectPage100Percent);

    /* Set the page selection list to match the pages in the document */
    while (pDoc->NumberPages < (ASUns32)SelectPageList.GetCount())
        SelectPageList.DeleteString(SelectPageList.GetCount() - 1);
    while (pDoc->NumberPages > (ASUns32)SelectPageList.GetCount()) {
        wchar_t tag[20];
        swprintf(tag, 20, L"%01d", SelectPageList.GetCount() + 1);
        SelectPageList.AddString(tag);
    }

    /* Start a Thread to draw pages
     */
    ThreadData.DocName = pDoc->DocName;
    ThreadData.Page = CurrentPage;
    ThreadData.Matrix = &PageToScreenMatrix;
    ThreadData.Done = false;
    ThreadData.TargetDC = NULL;
    ThreadData.StartMutex = DrawNextBitmap;
    ThreadData.DoneMutex = NextBitmapComplete;
    ThreadData.ReadyMutex = RenderThreadReady;
    ThreadData.Viewer = this;
    ThreadData.DrawToScreen = false;
    ThreadData.AAText = ThreadData.AAArt = ThreadData.AAImage = false;
    ThreadData.AATextDDR = ThreadData.AATextPreview = false;
    ThreadData.AABiCubic = false;
    ThreadData.AAThinLine = true;

    /* Start a thread to render pages using the above information */
    DWORD threadID;
    HANDLE threadHandle = ::CreateThread(NULL, 0, RenderToDCViaThreadRepeatedly, &ThreadData, 0, &threadID);

    /* Wait until the render to window thread is initialized */
    while (WaitForSingleObject(RenderThreadReady, 1) == WAIT_OBJECT_0) {
        ReleaseMutex(RenderThreadReady);
        Sleep(100);
    }

    /* Make sure windows are properly sized
     */
    GetParentFrame()->RecalcLayout();

    /* Set up page 0, and display it
     */
    SetPage(0);

    /* Pass the initialization to the parent
    **
    ** (But not until the page is setup, allowing
    **  page metrics to be calculated)
    */
    CScrollView::OnInitialUpdate();
}

/* Handle the mechanics of scaling a page
 */
void CPDFViewerView::SetScale(ASDouble scaleFactor) {
    /* If it did not change, ignore it
     */
    if (Scale == scaleFactor)
        return;

    /* Prepare the label for the scale factor selected
     */
    wchar_t actualscale[20];
    swprintf_s(actualscale, 20, L"%01.1f", scaleFactor);
    while (actualscale[wcslen(actualscale) - 1] == L'0')
        actualscale[wcslen(actualscale) - 1] = 0;
    if (actualscale[wcslen(actualscale) - 1] == L'.')
        actualscale[wcslen(actualscale) - 1] = 0;

    /* We need to display this in the edit box of the combo box
     */
    int selected = SelectScaleList.FindStringExact(0, actualscale);
    if (selected > 0)
        /* This is one of the defined labels */
        SelectScaleList.SetCurSel(selected);
    else
        SelectScaleList.SetWindowText(actualscale);

    Scale = scaleFactor;

    /* Save old matrix, to allow for scroll position translate
     */
    ASDoubleMatrix oldPageToScreen;
    memmove((char *)&oldPageToScreen, (char *)&PageToScreenMatrix, sizeof(ASDoubleMatrix));

    /* Recalculate to the page to screen metrics
     */
    PageToScreen();

    /* Translate Scroll to keep current image centered
     */
    TranslateScroll(&oldPageToScreen);

    /* Tell parents to redraw the window
     */
    Invalidate();
}

/* Handle the mechanics of changing the rotation
 */
void CPDFViewerView::SetRotate(ASDouble Angle) {
    /* Limit the angle to 360 degrees
     */
    while (Angle >= 360.0)
        Angle -= 360.0;
    while (Angle < 0)
        Angle += 360.0;

    /* Set pull down
     */
    SelectRotationList.SetCurSel((ASUns32)(Rotation / 90.0));

    /* Save old matrix, to allow for scroll position translate
     */
    ASDoubleMatrix oldPageToScreen;
    memmove((char *)&oldPageToScreen, (char *)&PageToScreenMatrix, sizeof(ASDoubleMatrix));

    /* Recalculate to the page to screen metrics
     */
    PageToScreen();

    /* Translate Scroll to keep current image centered
     */
    TranslateScroll(&oldPageToScreen);

    /* Inform parents to redraw window
     */
    OnDraw(GetDC());
}

/* Handle the mechanics of begining a new page
 */
void CPDFViewerView::SetPage(ASUns32 page) {
    /* It must not be more than the number of pages
     */
    if (page >= pDoc->NumberPages)
        return;

    /* Ignore the selection if it is for
    ** the currently displayed page
    */
    if ((page == CurrentPage) && (pDoc->Page))
        return;

    /* Save as the current page
     */
    CurrentPage = page;

    /* If this is from a keyboard shortcut, the
    ** page selector may not match. Make it match.
    */
    SelectPageList.SetCurSel(page);

    /* Make certain we are not rendering to the offscreen DC
    ** before we destroy it
    */
    WaitForSingleObject(NextBitmapComplete, INFINITE);
    ReleaseMutex(NextBitmapComplete);

    /* Do not use a previous page rendering */
    if (OffScreenDC)
        DeleteDC(OffScreenDC);
    OffScreenDC = NULL;

    /* If there was a previously displayed page,
    ** free it
    */
    if (pDoc->Page)
        PDPageRelease(pDoc->Page);

    /* Acquire the page to be displayed
     */
    pDoc->Page = PDDocAcquirePage(pDoc->Doc, CurrentPage);

    /* Get the page size in Points
     */
    ASFixedRect cropF;
    PDPageGetCropBox(pDoc->Page, &cropF);
    ASFixedToDoubleRect(&PageSize, &cropF);

    /* Reset view to show page full size,
    ** in portrait orientation
    */
    SelectScaleList.SetCurSel(SelectPage100Percent);
    SelectRotationList.SetCurSel(0);

    /* Reset Scale and Rotation as well */
    Rotation = 0;
    Scale = 1.0;

    /* Calculate matrices and sizes
     */
    PageToScreen();

    /* Set the size of the scroll view to the size of the
    ** page, in pixels, when rendered
    */
    SetScrollSizes(MM_TEXT, CSize(WindowWide, WindowDeep));

    /* Set the scroll bars to top left
     */
    ScrollPosition.x = ScrollPosition.y = 0;
    ScrollToPosition(ScrollPosition);

    /* Tell the parent to re-draw this window
     */
    Invalidate();
}

/* Handle a change in Scaling List Selector
 */
void CPDFViewerView::OnScaleSelectChange() {
    /* Get the current scale selector
     */
    int selected = SelectScaleList.GetCurSel();

    /* If it's not valid, ignore it
     */
    if (selected >= SelectScaleList.GetCount())
        return;

    /* Convert the text of the scale selector to a
    ** numeric scale factor
    */
    CString scale;
    wchar_t *endPtr;
    ASUns32 scaleLen = SelectScaleList.GetLBTextLen(selected);
    SelectScaleList.GetLBText(selected, scale.GetBuffer(scaleLen));
    ASDouble scaleFactor = wcstod(scale, &endPtr);

    SetScale(scaleFactor);
}

/* Handle a change in the rotation list selector
 */
void CPDFViewerView::OnRotationSelectChange() {
    /* Get the selected rotation
     */
    Rotation = SelectRotationList.GetCurSel() * 90.0;

    /* If we did not really change the Rotation.
    ** there is nothing to change
    */
    if (Rotation == LastRotation)
        return;
    LastRotation = Rotation;

    SetRotate(Rotation);
}

/* Handle a change to the page selector
 */
void CPDFViewerView::OnPageSelectChange() {
    /* Get the selected entry number
    **
    ** This will always be the zero based number of
    ** the page to display
    */
    ASUns32 selected = SelectPageList.GetCurSel();

    SetPage(selected);
}

/* These are the "hot key" handlers
 */

/* Handle a "Next Page" Request
 */
void CPDFViewerView::OnNextPage() { SetPage(CurrentPage + 1); }

/* Handle a "Prev Page" Request
 */
void CPDFViewerView::OnPrevPage() {
    if (CurrentPage != 0)
        SetPage(CurrentPage - 1);
}

/* Handle a "First Page" Request
 */
void CPDFViewerView::OnFirstPage() { SetPage(0); }

/* Handle a "Last Page" Request
 */
void CPDFViewerView::OnLastPage() {
    if (!pDoc)
        return;
    SetPage(pDoc->NumberPages - 1);
}

/* These are the mouse button handlers
 */
/* We want to be able to select left mouse button to "drag"
** the image of the page around on the screen.
** We do this by noting when and where the left button comes down,
** and when the mouse moves, move the scroll position.
*/
void CPDFViewerView::OnMouseMove(UINT flag, CPoint position) {
    ASBool ForceDraw = false;

    if (TrackMouse) {
        /* We are tracking the image to the mouse
        ** Calculate the displacement of the mouse from the point where the
        ** Left Button was pressed, and calculate a new scroll position for
        ** that displacement, based on the scroll position where the Left Button
        ** was pressed */
        POINT NewScroll;
        NewScroll.x = ScrollLBDownPosition.x + (MouseLBDownPosition.x - position.x);
        NewScroll.y = ScrollLBDownPosition.y + (MouseLBDownPosition.y - position.y);

        /* Scroll to the new position.
        ** Note that this will "limit" scrolling to the valid
        ** range to keep the entire view filled.
        ** So get the position we really ended up with after
        ** applying this change.
        */
        ScrollToPosition(NewScroll);
        NewScroll = GetScrollPosition();

        /* If the scroll region really moved, redraw the page
         */
        if ((abs(ScrollPosition.x - NewScroll.x) > 10) || (abs(ScrollPosition.y - NewScroll.y) > 10)) {
            ScrollPosition = GetScrollPosition();
            OnDraw(GetDC());
        }
    }

    /* Save the mouse position */
    MousePosition.x = position.x;
    MousePosition.y = position.y;

    /* Inform the parent of the mouse position
     */
    CScrollView::OnMouseMove(flag, position);
}

/* When the left button is held down, move the view to
** track the mouse
**
** If shift LB is selected, center on mouse, scale up 100%
**
*/
void CPDFViewerView::OnLButtonDown(UINT flag, CPoint position) {
    if (flag & MK_CONTROL) {
        /* Center on the mouse */
        CenterOnMouse(&position);

        /* Pass modified point to parents */
        CScrollView::OnRButtonDown(flag, position);

        /* Scale up the page */
        SetScale(Scale * 2.0);

        return;
    }

    if (flag & MK_SHIFT) {
        /* Center on mouse */
        CenterOnMouse(&position);

        /* Pass modified point to parents */
        CScrollView::OnRButtonDown(flag, position);

        /* Change rotation 90 degrees clockwise
         */
        Rotation += 90.0;
        if (Rotation >= 360)
            Rotation = 0;
        LastRotation = Rotation;

        SetRotate(Rotation);

        return;
    }

    /* If neither shift nor control is selected then
    ** activate mouse tracking. As the mouse moves
    ** shift the scrolling to keep the point originally
    ** selected under the mouse
    */
    TrackMouse = true;

    /* Save the mouse position where the left button down occurred
     */
    MouseLBDownPosition.x = position.x;
    MouseLBDownPosition.y = position.y;

    /* Save the scroll position where the left button down occurred
     */
    ScrollLBDownPosition = GetScrollPosition();

    /* Inform the parent */
    CScrollView::OnLButtonDown(flag, position);
}

/* If we are "tracking" the mouse, moving the page
** around as the mouse moves, and the user releases
** the left mouse button. Stop tracking
*/
void CPDFViewerView::OnLButtonUp(UINT flag, CPoint position) {
    /* Turn off tracking
     */
    TrackMouse = false;

    /* Inform parent */
    CScrollView::OnLButtonUp(flag, position);
}

/* If the right mouse button is pressed, without shift or control,
** center the current mouse point to the center of the view
** (within the limits of scrolling)
**
** If it is pressed with control, scale down by 50%, center on the
** mouse, and redraw the screen
*/
void CPDFViewerView::OnRButtonDown(UINT flag, CPoint position) {
    /* Right button, no control or shift
    **    Center cursor point in view
    */
    if ((flag & (MK_CONTROL | MK_SHIFT)) == 0) {
        /* Center on mouse */
        CenterOnMouse(&position);

        /* Pass to parent (With modified point)
         */
        CScrollView::OnRButtonDown(flag, position);

        /* Rebuild the page metrics */
        PageToScreen();

        /* Force a screen redraw */
        OnDraw(GetDC());

        return;
    }

    /* Mouse right button and control
    **    Center cursor point in view and
    **    scale to half size
    */
    if (flag & MK_CONTROL) {
        /* Center on mouse */
        CenterOnMouse(&position);

        /* Pass modified point to parents */
        CScrollView::OnRButtonDown(flag, position);

        /* Scale up the page */
        SetScale(Scale * 0.5);

        return;
    }

    /* Mouse right button and control
    **    Center cursor point in view and
    **    rotate 90 degrees clockwise
    */
    if (flag & MK_SHIFT) {
        /* Center on mouse */
        CenterOnMouse(&position);

        /* Pass modified point to parents */
        CScrollView::OnRButtonDown(flag, position);

        /* Change rotation 90 degrees clockwise
         */
        Rotation -= 90.0;
        if (Rotation < 0)
            Rotation = 270;
        LastRotation = Rotation;

        SetRotate(Rotation);
        return;
    }

    /* pass original points on to parent, if we did nothing
     */
    CScrollView::OnRButtonDown(flag, position);
}

/* This is all of the calculations to transform from a
** PDF page to a view on the screen
*/
void CPDFViewerView::PageToScreen() {
    /* The rotate matrix will take into account the pages
    ** specification that it should be rendered rotated from (The PDRotate
    ** attribute) the orientation it is defined in, and the user's specification
    ** of how he would like to see the page. It also has to account for
    ** "mirroring" the page vertically, since Windows draws from the
    ** upper left, and APDFL draws from the lower left
    **
    ** All of that will be taken into account in this matrix
    */
    ASDoubleMatrix RotateMatrix;

    /* Start with an identity matrix, with the vertical inverted
     */
    ASDoubleIdentity(&RotateMatrix);
    RotateMatrix.d = -1.0;

    /* Get the pages description of how it believes it should be
    ** rendered to appear "upright"
    **
    ** This is a value between 0 and 270, in 90 degree increments
    */

    ASInt32 PageRotate = 0;
    if ((pDoc) && (pDoc->Page))
        PageRotate = PDPageGetRotate(pDoc->Page);

    /* Add to this the user specified rotation. Also
    ** 0 to 270, in 90 degree increments.
    */
    PageRotate += (ASInt32)Rotation;

    /* Limit to 360 degrees */
    PageRotate %= 360;

    /* Set the translation for the lower left corner of the page, to render
    ** properly to align to the upper right
    */
    switch (PageRotate) {
    case 0:
        ASDoubleMatrixTranslate(&RotateMatrix, &RotateMatrix, -PageSize.left, -PageSize.top);
        break;

    case 90:
        ASDoubleMatrixTranslate(&RotateMatrix, &RotateMatrix, PageSize.top, -PageSize.top);
        break;

    case 180:
        ASDoubleMatrixTranslate(&RotateMatrix, &RotateMatrix, PageSize.right, 0);
        break;

    case 270:
        ASDoubleMatrixTranslate(&RotateMatrix, &RotateMatrix, 0, 0);
        break;
    }

    /* Rotate the matrix by the specified angle
     */
    ASDoubleMatrixRotate(&RotateMatrix, &RotateMatrix, PageRotate * 1.0);

    /* Scale the matrix by both the screen resolution (72 / Screen DPI)
    ** and the user supplied scale factor
    */
    ASDoubleMatrixScale(&PageToScreenMatrix, &RotateMatrix, Scale);
    ASDoubleMatrixScale(&PageToScreenMatrix, &PageToScreenMatrix, ScreenResolution);

    /* Transform the page size to the needed window size
    ** The window will always be based at 0, 0.
    */
    ASDoubleRect WindowSize;
    WindowSize.left = WindowSize.bottom = 0;
    WindowSize.right = PageSize.right - PageSize.left;
    WindowSize.top = PageSize.top - PageSize.bottom;

    WindowSize.right *= Scale * ScreenResolution;
    WindowSize.top *= Scale * ScreenResolution;

    /* If the page rotation is 90 or 270, flip the page width and depth
     */
    if ((PageRotate == 90) || (PageRotate == 270)) {
        ASDouble hold = WindowSize.right;
        WindowSize.right = WindowSize.top;
        WindowSize.top = hold;
    }

    /* Keep the page width and depth in pixels, for convenience
     */
    WindowWide = (ASUns32)floor((WindowSize.right - WindowSize.left) + 0.5);
    WindowDeep = (ASUns32)floor((WindowSize.top - WindowSize.bottom) + 0.5);

    /* There is a second solution built for this issue in OnDraw.
    ** It detects that we cannot build a Bitmap, and uses the existing
    ** map, and StretchBlt, to "show" the page at larger scale sizes.
    **
    ** This parameter is in place to limit the scale size.
    */

    /* Windows will not support a Compatible Bitmap > 2GB in size. So if this is
    ** going to be greater, scale it down.
    */
    if (((ASUns64)WindowWide * (ASUns64)WindowDeep * 3) > 0x20000000) {
        double ScaleDown = (0x1e000000 * 1.0) / (((ASUns64)WindowWide * (ASUns64)WindowDeep * 3) * 1.0);
        Scale *= sqrt(ScaleDown);
        PageToScreen();
    }
}

/* Translate scroll positon to match
** page scale or rotation
**
** This MUST be called after the call to
** PageToScreen, to set up the new page metrics.
** The supplied matrix must be the value in the
** PageToScreenMatrix BEFORE that call.
*/
void CPDFViewerView::TranslateScroll(ASDoubleMatrix *oldMatrix) {
    /* Get the current scroll position */
    POINT ScrollBefore = GetScrollPosition();

    RECT viewSize;
    GetClientRect(&viewSize);

    /* Find the mid point of the view, which will be
    ** the center of the current screen. The scroll handle will
    ** always be the size of the view display
    */
    ASDoublePoint midPoint;
    midPoint.h = ScrollBefore.x + ((viewSize.right * 1.0) / 2.0);
    midPoint.v = ScrollBefore.y + ((viewSize.bottom * 1.0) / 2.0);

    /* Convert the screen position to page position,
    ** using the matrix before the change that is happening
    */
    ASDoubleMatrix ScreenToPage;
    ASDoubleMatrixInvert(&ScreenToPage, oldMatrix);
    ASDoubleMatrixTransform(&midPoint, &ScreenToPage, &midPoint);

    /* Convert the page point to a screen point, using the new matrix
     */
    ASDoubleMatrixTransform(&midPoint, &PageToScreenMatrix, &midPoint);

    /* Move the midPoint back to the trailing edges of the
    ** view, to get scroll position
    */
    POINT NewScroll;
    NewScroll.x = (ASInt32)(floor)(midPoint.h) - (viewSize.right / 2);
    NewScroll.y = (ASInt32)(floor)(midPoint.v) - (viewSize.bottom / 2);

    /* The scroll positions should never be less than
    ** zero, and not greater than Windowsize - Size
    */
    if (NewScroll.x < 0)
        NewScroll.x = 0;
    if (NewScroll.x > (ASInt64)(WindowWide - viewSize.right))
        NewScroll.x = WindowWide - viewSize.right;
    if (NewScroll.y < 0)
        NewScroll.y = 0;
    if (NewScroll.y > (ASInt64)(WindowDeep - viewSize.bottom))
        NewScroll.y = WindowDeep - viewSize.bottom;

    /* Set the new scroll position
     */
    ScrollToPosition(NewScroll);

    /* Set the scroll sizes to match the newly scaled page size
     */
    SetScrollSizes(MM_TEXT, CSize(WindowWide, WindowDeep));
}

void CPDFViewerView::CenterOnMouse(CPoint *point) {
    /* What portion of the document can we view */
    RECT viewPort;
    GetWindowRect(&viewPort);

    /* Where we we want the view (and the mouse cursor)
    ** to end up
    */
    POINT displacement;
    displacement.x = ((viewPort.right - viewPort.left) / 2) - point->x;
    displacement.y = ((viewPort.bottom - viewPort.top) / 2) - point->y;

    /* Where is the current scroll position
     */
    POINT scroll = GetScrollPosition();

    /* What is the desired scroll position, to
    ** center the point where the cursor is
    ** in the view
    */
    POINT newScroll;
    newScroll.x = scroll.x - displacement.x;
    newScroll.y = scroll.y - displacement.y;

    /* Set the scroll there
    **
    ** NOTE: If this would scroll the image such that some
    ** portion of the view is NOT covered by the image, limit
    ** the motion to keep the entire view covered. This will
    ** happen automatically when we request the new scroll position.
    **
    */
    ScrollToPosition(newScroll);

    /* Since the scroll may not have moved as far as we wanted,
    ** find out how much it really did move
    */
    newScroll = GetScrollPosition();
    displacement.x = newScroll.x - scroll.x;
    displacement.y = newScroll.y - scroll.y;

    /* Move the cursor by the amount the scroll really moved
     */
    POINT curPos;
    GetCursorPos(&curPos);
    curPos.x -= displacement.x;
    curPos.y -= displacement.y;
    SetCursorPos(curPos.x, curPos.y);

    /* Move the mouse point (found in
    ** window position values) by the same
    ** displacement we moved the cursor.
    ** Return this value, so it can be passed
    ** to the parent windows.
    */
    point->x -= displacement.x;
    point->y -= displacement.y;

    Invalidate();
}

// Some utility routines

/* Rotate the image in a DC in 90 degree increments
 */
void CPDFViewerView::RotateDC(HDC dc, HBITMAP bitmap, ASDouble Rotation, ASUns32 mapWide, ASUns32 mapDeep) {
    /* Find the angle */
    ASInt16 angle = (ASUns16)floor(Rotation);

    /* If angle is not 90 degree increment, return */
    if ((angle % 90) != 0)
        return;

    /* Clamp the rotation to 270 degrees
     */
    if (angle < 0)
        angle += 360;
    if (angle >= 360)
        angle -= 360;

    /* If no rotation, then just exit */
    if (angle == 0)
        return;

    /* Get the source image Info */
    BITMAP bitmapInfo;
    memset((char *)&bitmapInfo, 0, sizeof(BITMAP));
    GetObject(bitmap, sizeof(BITMAP), &bitmapInfo);

    BITMAPINFO sourceInfo;
    memset((char *)&sourceInfo, 0, sizeof(BITMAPINFO));
    sourceInfo.bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
    sourceInfo.bmiHeader.biWidth = bitmapInfo.bmWidth;
    sourceInfo.bmiHeader.biHeight = bitmapInfo.bmHeight;
    sourceInfo.bmiHeader.biPlanes = bitmapInfo.bmPlanes;
    sourceInfo.bmiHeader.biBitCount = 24;
    sourceInfo.bmiHeader.biCompression = BI_RGB;
    ASUns32 sourceLines = GetDIBits(dc, bitmap, 0, bitmapInfo.bmHeight, NULL, &sourceInfo, DIB_RGB_COLORS);

    /* Allocate space to hold the source image */
    ASUns8 *sourceData = (ASUns8 *)malloc(sourceInfo.bmiHeader.biSizeImage);

    /* Get the source data */
    GetDIBits(dc, bitmap, 0, bitmapInfo.bmHeight, sourceData, &sourceInfo, DIB_RGB_COLORS);

    /* Bytes per pixel.
    ** This is how many bytes we copy for one pixel, and
    ** how many bytes we skip per column
    */
    ASUns8 bytesPerPixel = sourceInfo.bmiHeader.biBitCount / 8;

    /* This is how many bytes we skip per row
    ** Input and output row widths may not be the same
    */
    ASUns32 rowWideIn = (((sourceInfo.bmiHeader.biWidth * sourceInfo.bmiHeader.biBitCount) + 31) / 32) * 4;
    ASUns32 rowWideOut = (((mapWide * sourceInfo.bmiHeader.biBitCount) + 31) / 32) * 4;

    /* The output image will have the same number of pixels as the input image,
    ** but width and depth will swap. This may cause the images to be differently
    ** sized, because of row alignment, and the 32 bit alignment required for a Windows bitmap.
    */
    sourceInfo.bmiHeader.biSizeImage = rowWideOut * mapDeep;
    ASUns8 *destData = (ASUns8 *)malloc(sourceInfo.bmiHeader.biSizeImage);

    /* The copy is different for each orientation
     */
    switch (angle) {
    case 90:
        /* Step through each pixel of each row, top to bottom,
        ** right to left. The source for each pixel step left to right,
        ** bottom to top
        */
        for (ASUns32 row = 0; row < mapDeep; row++)
            for (ASUns32 col = 0; col < mapWide; col++)
                memmove(&destData[(row * rowWideOut) + (col * bytesPerPixel)],
                        &sourceData[((mapWide - col - 1) * rowWideIn) + (row * bytesPerPixel)], bytesPerPixel);
        break;
    case 180:
        /* Step through each pixel of each row, top to bottom,
        ** right to left. The source for each pixel step bottom to top,
        ** right to left
        */
        for (ASUns32 row = 0; row < mapDeep; row++)
            for (ASUns32 col = 0; col < mapWide; col++)
                memmove(&destData[(row * rowWideOut) + (col * bytesPerPixel)],
                        &sourceData[((mapDeep - row - 1) * rowWideIn) + ((mapWide - col - 1) * bytesPerPixel)],
                        bytesPerPixel);
        break;
    case 270:
        /* Step through each pixel of each row, top to bottom,
        ** right to left. The source for each pixel step right to left,
        ** bottom to top
        */
        for (ASUns32 row = 0; row < mapDeep; row++)
            for (ASUns32 col = 0; col < mapWide; col++)
                memmove(&destData[(row * rowWideOut) + (col * bytesPerPixel)],
                        &sourceData[(col * rowWideIn) + ((mapDeep - row - 1) * bytesPerPixel)], bytesPerPixel);
        break;
    }

    /* If we rotated 180 degrees, we can use the same bitmap. Otherwise, we need to create a
    ** new bitmap, with the new orientation.
    */
    if (sourceInfo.bmiHeader.biHeight == mapDeep)
        SetDIBits(dc, bitmap, 0, sourceInfo.bmiHeader.biHeight, destData, &sourceInfo, DIB_RGB_COLORS);
    else {
        HBITMAP newBitmap = CreateCompatibleBitmap(dc, mapWide, mapDeep);
        HBITMAP oldBitmap = (HBITMAP)SelectObject(dc, newBitmap);
        DeleteObject((HGDIOBJ)oldBitmap);
        sourceInfo.bmiHeader.biWidth = mapWide;
        sourceInfo.bmiHeader.biHeight = mapDeep;
        SetDIBits(dc, newBitmap, 0, mapDeep, destData, &sourceInfo, DIB_RGB_COLORS);
    }

    free(sourceData);
    free(destData);
}

void ASFixedToDoubleRect(ASDoubleRect *out, ASFixedRect *in) {
    out->left = ASFixedToFloat(in->left);
    out->right = ASFixedToFloat(in->right);
    out->top = ASFixedToFloat(in->top);
    out->bottom = ASFixedToFloat(in->bottom);
}

void ASDoubleMatrixScale(ASDoubleMatrix *out, ASDoubleMatrix *in, ASDouble scale) {
    out->a = in->a * scale;
    out->b = in->b * scale;
    out->c = in->c * scale;
    out->d = in->d * scale;
    out->h = in->h * scale;
    out->v = in->v * scale;
}

void ASDoubleMatrixRotate(ASDoubleMatrix *out, ASDoubleMatrix *in, ASDouble angle) {
    ASDoubleMatrix inCopy;
    memmove((char *)&inCopy, (char *)in, sizeof(ASDoubleMatrix));

    while (angle < 0)
        angle += 360;

    while (angle > 360.0)
        angle -= 360.0;

    if (angle < 0.0001)
        return;

    angle *= (M_PI / 180.0);
    double Sina = sin(angle);
    double Cosa = cos(angle);

    out->a = (Cosa * inCopy.a) + (Sina * inCopy.c);
    out->b = (Cosa * inCopy.b) + (Sina * inCopy.d);
    out->c = (Cosa * inCopy.c) - (Sina * inCopy.a);
    out->d = (Cosa * inCopy.d) - (Sina * inCopy.b);
    out->h = in->h;
    out->v = in->v;
}

void ASDoubleMatrixTranslate(ASDoubleMatrix *out, ASDoubleMatrix *in, ASDouble tX, ASDouble tY) {
    out->a = in->a;
    out->b = in->b;
    out->c = in->c;
    out->d = in->d;
    out->h = (in->a * tX) + (in->c * tY) + in->h;
    out->v = (in->b * tX) + (in->d * tY) + in->v;
}
