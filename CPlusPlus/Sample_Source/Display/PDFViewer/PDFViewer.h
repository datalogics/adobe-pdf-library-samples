// DLViewer.h : main header file for the DLVIEWER application
//

#if !defined(AFX_DLVIEWER_H_INCLUDED_)
#define AFX_DLVIEWER_H_INCLUDED_

/* Supported only for windows versions XP or later */
#ifndef WINVER
#define WINVER 0x0501
#endif

/* MFC Definitions */
#define VC_EXTRALEAN  // Exclude rarely-used stuff from Windows headers
#include <afxext.h>   // MFC extensions
#include <afxdtctl.h> // MFC support for Internet Explorer 4 Common Controls
#include <afxcmn.h>   // MFC support for Windows Common Controls

// APDF Library headers...
#include "PDFInit.h"       /* Initialize and terminate the library */
#include "PDFLCalls.h"     /* Rendering calls */
#include "ASExtraCalls.h"  /* Support for ASText, used to convert strings to paths */
#include "DLExtrasCalls.h" /* Render with ASDouble Matrices calls */

#define _USE_MATH_DEFINES 1 /* Include math defines so we can use M_PI */
#include <math.h>           /* Support sin/cos/sqrt/floor */

/* Application Resources */
#include "resource.h"

class CPDFViewerView;

/* The information set needed to control a rendering thread
 */
typedef struct threadcommunication {
    wchar_t *DocName;       /* The path to the document to be rendered */
    ASUns32 Page;           /* The page number to render */
    ASDoubleMatrix *Matrix; /* The matrix to render to */
    HDC TargetDC;           /* The Device Context to render to */
    HANDLE DoneMutex;       /* Mutex to take ownership of while rendering a page */
    HANDLE StartMutex;      /* Mutext to wait for before rendering a page */
    HANDLE ReadyMutex;      /* Mutext to take ownership of during startup */
    ASBool Done;            /* When true, exit the thread */
    CPDFViewerView *Viewer; /* View to inform when a new page rendering is complete */
    ASBool DrawToScreen;    /* Draw that page to a window */
    ASBool AAText, AATextDDR, AATextPreview, AAArt, AAThinLine, AAImage, AABiCubic;
} ThreadCommunication;

/* The definition of the viewer application itself */
class CPDFViewerApp : public CWinApp {
  public:
    CPDFViewerApp(); /* Construct the viewer */

  public:
    virtual BOOL InitInstance(); /* Start the viewer */
    virtual int ExitInstance();  /* Destruct the viewer */
    afx_msg void OnAppAbout();   /* Display the "About" box */
    DECLARE_MESSAGE_MAP()

  private:
    bool APDFLInitialized; /* APDFL is open and initialized when true */
};

/* The definition of the parent window, that contains the controls
** and status.
*/
class CMainFrame : public CMDIFrameWnd {
    DECLARE_DYNAMIC(CMainFrame)
  public:
    CMainFrame();                                   /* Create and display the main window */
    virtual ~CMainFrame();                          /* Close and destroy the main window */
    virtual BOOL PreCreateWindow(CREATESTRUCT &cs); /* Set up values prior to opening the main window */
    CStatusBar m_wndStatusBar;                      /* The status bar display at bottom of screen */

  protected:
    CToolBar Toolbar; /* The tool bar at the top of the screen */
    afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);

    DECLARE_MESSAGE_MAP()
};

/* The definition of the child window that will display a single
** PDF Document
*/
class CDocumentFrame : public CMDIChildWnd {
    DECLARE_DYNCREATE(CDocumentFrame)
  public:
    CDocumentFrame();          /* Create the document window */
    virtual ~CDocumentFrame(); /* Destroy the document window */

  protected:
    DECLARE_MESSAGE_MAP()
};

/* Defintion of DLViewerDocument.
** This is an instance of a PDF document,
** annotated with information about that document,
** displayed in a window within the viewer
*/
class CPDFViewerDoc : public CDocument {
  protected:
    /* Create a Document Display */
    CPDFViewerDoc();

    /* Destroy a Document Display */
    virtual ~CPDFViewerDoc();
    DECLARE_DYNCREATE(CPDFViewerDoc)

  public:
    PDDoc Doc;            /* The APDFL Representation of a PDF Document */
    PDPage Page;          /* The APDFL Representation of the page currently displayed */
    wchar_t DocName[512]; /* The name of the document being displayed */
    ASUns32 NumberPages;  /* The count of pages in the document */

    /* Open a PDF document within a window */
    virtual BOOL OnOpenDocument(LPCTSTR lpszPathName);

  protected:
    afx_msg void OnFileClose();
    DECLARE_MESSAGE_MAP()
};

/* The definition of a single view of a single document */
class CPDFViewerView : public CScrollView {
  protected:
    CPDFViewerView(); /* Create a view */
    DECLARE_DYNCREATE(CPDFViewerView)
    virtual ~CPDFViewerView(); /* Destroy a view */

  public:
    /* Allow view to be accessed from Main Frame */
    static CPDFViewerView *GetView();

    /* Initialize variables on first update */
    virtual void OnInitialUpdate();

    /* Give the current document to a caller */
    inline CPDFViewerDoc *GetDocument() { return (CPDFViewerDoc *)m_pDocument; }

    /* Redraw the page on the screen */
    virtual void OnDraw(CDC *pDC);

    /* Window requires update */
    virtual void OnUpdate(CView *pSender, LPARAM lHint, CObject *pHint);

    /* Handle the mechanics of changing to a new page */
    void SetPage(ASUns32 page);

    /* Handle the mechanics of changing scaling */
    void SetScale(ASDouble scale);

    /* Handle the mechanics of changing rotation */
    void SetRotate(ASDouble Angle);

    /* Handle change in rotation */
    void OnRotationSelectChange();

    /* Handle change in scaling */
    void OnScaleSelectChange();

    /* Handle change in page selection */
    void OnPageSelectChange();

    /* Page Selection Shortcuts */
    void OnNextPage();
    void OnPrevPage();
    void OnFirstPage();
    void OnLastPage();

    CPDFViewerDoc *pDoc; /* The document being viewed */

    ASBool AAArt;
    ASBool AAText;
    ASBool AATextDDR;
    ASBool AATextPreview;
    ASBool AAImage;
    ASBool AABiCubic;
    ASBool AAThinLine;

  private:
    /* Calculate Page To Screen Matrix */
    void PageToScreen();

    /* Translate scroll position to match
    ** page scale or rotation
    */
    void TranslateScroll(ASDoubleMatrix *oldMatrix);

    /* Track the position of the mouse when it moves within the
    ** view window
    */
    afx_msg void OnMouseMove(UINT flags, CPoint position);

    /* Track the view to the mouse when the left button is held down
     */
    afx_msg void OnLButtonDown(UINT flag, CPoint position);
    afx_msg void OnLButtonUp(UINT flag, CPoint position);

    /* Scale and center of Right Mouse Button */
    afx_msg void OnRButtonDown(UINT nFlags, CPoint point);

    /* Center the view, and the cursor, on the specified
    ** cursor point
    */
    void CenterOnMouse(CPoint *position);

    /* Rotate DC
    ** Rotate the image in a DC by 90 degree increments
    */
    void RotateDC(HDC dc, HBITMAP bitmap, ASDouble Rotation, ASUns32 width, ASUns32 depth);

    HDC OffScreenDC; /* We draw to this DC, then blit from it to the screen */

    HDC OldScreenDC;       /* Save bitmap from Previous rendering scale */
    ASDouble OldScale;     /* Scale factor from the old page rendering, to the new page rendering */
    ASDouble OldRotation;  /* Rotation last drawn at */
    ASUns32 OldWindowWide; /* Width and Depth of the window in Pixels for the old bitmap */
    ASUns32 OldWindowDeep;
    bool OldMapValid; /* Is the map in the OldScreen DC a scaled rendering of the desired page? */
    ASDouble PendingRotate; /* If we rotated the OLD map, note that we need to rotate the new one as well */

    ASDouble Rotation;         /* 0 to 270, in 90 degree increments */
    ASDouble Scale;            /* User selected scale factor */
    ASDouble ScreenResolution; /* Scale from points to screen resolution */
    ASUns32 CurrentPage;       /* The page number (zero based) of the page being displayed */

    ASDoubleMatrix PageToScreenMatrix; /* Matrix used to render a page */
    ASDoubleMatrix PageToWindowMatrix; /* Matrix used to render a page directly to the window */

    ASDoubleRect PageSize; /* Size of the page in 1/72nd of an inch, erect */
    ASUns32 WindowWide;    /* Width and Depth of the window in Pixels */
    ASUns32 WindowDeep;

    POINT ScrollPosition; /* H and V Scroll positions */

    ASDouble LastRotation;                 /* The Rotation in effect when we last rendered */
    ASDoubleMatrix LastPageToScreenMatrix; /* Last matrix used to render a page */

    ThreadCommunication ThreadData; /* Structure to communicate with a rendering thread */

    HANDLE RenderThreadReady; /* Handle to a Mutex, Owned when the render thread is ready to draw a page */
    HANDLE DrawNextBitmap; /* Handle to a Mutex, When owned, render thread will pause before drawing */
    HANDLE NextBitmapComplete; /* Handle to a Mutex, When Owned, page has been rendered completely */

    POINT MousePosition;
    POINT MouseLBDownPosition;
    POINT ScrollLBDownPosition;

    ASBool TrackMouse;

  protected:
    DECLARE_MESSAGE_MAP()
};

// Some utility routines
void ASFixedToDoubleRect(ASDoubleRect *out, ASFixedRect *in);
void ASDoubleIdentity(ASDoubleMatrix *out) {
    out->a = out->d = 1.0;
    out->b = out->c = out->h = out->v = 0;
}
void ASDoubleMatrixScale(ASDoubleMatrix *out, ASDoubleMatrix *in, ASDouble scale);
void ASDoubleMatrixRotate(ASDoubleMatrix *out, ASDoubleMatrix *in, ASDouble angle);
void ASDoubleMatrixTranslate(ASDoubleMatrix *out, ASDoubleMatrix *in, ASDouble tX, ASDouble tY);

void ASDoubleToRealMatrix(ASRealMatrix *out, ASDoubleMatrix *in) {
    out->a = (ASReal)in->a;
    out->b = (ASReal)in->b;
    out->c = (ASReal)in->c;
    out->d = (ASReal)in->d;
    out->tx = (ASReal)in->h;
    out->ty = (ASReal)in->v;
}

#endif
