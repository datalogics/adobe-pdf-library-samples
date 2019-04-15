/*
 * Copyright (C) 2011-2017, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

package com.datalogics.pdfl.javaviewer.Presentation;

import java.awt.Color;
import java.awt.Graphics;
import java.awt.Rectangle;
import java.util.HashMap;
import java.util.Iterator;
import java.util.LinkedHashMap;
import java.util.LinkedList;
import java.util.List;
import java.util.Map;

import javax.management.RuntimeErrorException;

import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.LineEndingStyle;
import com.datalogics.PDFL.OptionalContentGroup;
import com.datalogics.PDFL.Rect;
import com.datalogics.pdfl.javaviewer.Document.JavaDocument;
import com.datalogics.pdfl.javaviewer.Document.DocumentListener;
import com.datalogics.pdfl.javaviewer.Document.Annotations.AnnotationConsts;
import com.datalogics.pdfl.javaviewer.Document.Annotations.AnnotationFactory;
import com.datalogics.pdfl.javaviewer.Document.Annotations.AnnotationHolder;
import com.datalogics.pdfl.javaviewer.Document.Annotations.AnnotationListener;
import com.datalogics.pdfl.javaviewer.Document.Annotations.AnnotationProperties;
import com.datalogics.pdfl.javaviewer.Document.Command.BaseAnnotationCommand;
import com.datalogics.pdfl.javaviewer.Document.Command.CloseCommand;
import com.datalogics.pdfl.javaviewer.Document.Command.CommandStack;
import com.datalogics.pdfl.javaviewer.Document.Command.CommandType;
import com.datalogics.pdfl.javaviewer.Document.Command.CreateAnnotationCommand;
import com.datalogics.pdfl.javaviewer.Document.Command.DocumentCommand;
import com.datalogics.pdfl.javaviewer.Document.Command.MarqueeZoomCommand;
import com.datalogics.pdfl.javaviewer.Document.Command.PendingCommand;
import com.datalogics.pdfl.javaviewer.Document.Command.ViewCommand;
import com.datalogics.pdfl.javaviewer.Document.Command.CommandStack.StackListener;
import com.datalogics.pdfl.javaviewer.Document.Command.DocumentCommand.State;
import com.datalogics.pdfl.javaviewer.Presentation.Cache.PageModel;
import com.datalogics.pdfl.javaviewer.Presentation.Enums.ApplicationMode;
import com.datalogics.pdfl.javaviewer.Presentation.Interactive.BaseInteractive;
import com.datalogics.pdfl.javaviewer.Presentation.Interactive.EditAnnotationInteractive;
import com.datalogics.pdfl.javaviewer.Presentation.Interactive.Interactive;
import com.datalogics.pdfl.javaviewer.Presentation.Interactive.MarqueeZoomInteractive;
import com.datalogics.pdfl.javaviewer.Presentation.Interactive.ScrollModeInteractive;
import com.datalogics.pdfl.javaviewer.Presentation.Interactive.ZoomModeInteractive;
import com.datalogics.pdfl.javaviewer.Presentation.Interactive.Interactive.InteractiveContext;
import com.datalogics.pdfl.javaviewer.Presentation.PDFPresenter.ViewListener;
import com.datalogics.pdfl.javaviewer.Views.Interfaces.ColorPicker;
import com.datalogics.pdfl.javaviewer.Views.Interfaces.InputData;
import com.datalogics.pdfl.javaviewer.Views.Interfaces.PDF;
import com.datalogics.pdfl.javaviewer.Views.Interfaces.Viewer;

/**
 * ApplicationController - service class which plays the role of the nexus
 * between different parts of the application such as views, presentations etc.
 * 
 * It creates an instance of JavaDocument, and all the presenters which handle
 * bookmarks, text search, layer functionality etc. This class handles command
 * processing and main user (mouse and keyboard) input.
 * 
 * It implements most of existing interfaces.
 */
public class ApplicationController implements Application, DocumentListener, ViewListener, StackListener, Interactive, InteractiveContext, AnnotationListener {
    public ApplicationController() {
        library = new Library();

        /* You may find that you receive exceptions when you attempt to open
         * PDF files that contain permissions restrictions on content or image
         * extraction.  This is due to the APIs used for viewing: these can
         * also be used in other contexts for content extraction or enabling
         * save-as-image capabilities. If you are making a PDF file viewer and
         * you encounter this situation, please contact your support
         * representative or support@datalogics.com to request a key to enable
         * bypassing this restriction check.
         */
        //if (!Library.enableLicensedBehavior("xxxxxxxxxxxxxxxxxxxxx="))
        //    throw new RuntimeErrorException(new Error());

        this.pdfPresenter = new PDFPresenter(this);
        this.bookmarksPresenter = new BookmarksPresenter(this);
        this.layersPresenter = new LayersPresenter(this);
        this.textSearchPresenter = new TextSearchPresenter(this);

        this.handlerList = new LinkedList<Interactive>();
        this.commandStack = new CommandStack(this);
        this.cursorHandlers = new LinkedHashMap<Interactive, PDF.Cursor>();

        this.navigationPresenter = new KeyNavigationPresenter();

        setDefaultProperties();

        this.activeProps = new AnnotationProperties();
        activeProps.addAnnotationListener(this);
        activeProps.setFrom(defaultProps);
        activeProps.setUpdateAlways(true);

        initModeHandlers();
    }

    public void setView(Viewer view) {
        this.view = view;
        pdfPresenter.setView(view.getPDF());
        bookmarksPresenter.setView(view.getBookmarks());
        layersPresenter.setView(view.getLayers());

        fullUpdate();
    }

    public PDFPresenter getPDFPresenter() {
        return this.pdfPresenter;
    }

    public BookmarksPresenter getBookmarksPresenter() {
        return this.bookmarksPresenter;
    }

    public LayersPresenter getLayersPresenter() {
        return this.layersPresenter;
    }

    public TextSearchPresenter getTextSearchPresenter() {
        return this.textSearchPresenter;
    }

    public boolean close() {
        final boolean isClosed = (this.executeCommand(new CloseCommand()).getState() != State.Canceled);
        if (isClosed) {
            if (library != null && Library.initialized()) {
                library.terminate();
            }
            library = null;
        }
        return isClosed;
    }

    public void setDpi(int dpi) {
        pdfPresenter.setDpi(dpi);
    }

    public int getDpi() {
        return pdfPresenter.getDpi();
    }

    public void setZoomArray(double[] zoomArray) {
        try {
            ((ZoomModeInteractive) getHandler(ApplicationMode.ZOOM_MODE)).setZoomArray(zoomArray);
        } catch (NullPointerException e) {
        } catch (ClassCastException e) {
        }
    }

    public ApplicationMode getMode() {
        return mode;
    }

    public void setMode(ApplicationMode newMode) {
        if (mode != newMode && newMode != null) {
            final ApplicationMode oldMode = mode;
            mode = newMode;

            if (getHandler(oldMode) != null) {
                getHandler(oldMode).activate(false);
            }

            // reset registered handlers list
            handlerList.clear();
            registerInteractive(navigationPresenter, true);
            registerInteractive(getHandler(mode), true);

            if (getHandler(mode) != null)
                getHandler(mode).activate(true);

            view.updateApplicationMode(newMode);

            if (oldMode.equals(ApplicationMode.CUSTOM_MODE)) {
                modeHandlers.remove(oldMode);
            }
        }
    }

    public void setCustomMode(BaseInteractive mode) {
        addHandler(ApplicationMode.CUSTOM_MODE, mode);
        setMode(ApplicationMode.CUSTOM_MODE);
    }

    public ApplicationMode getDefaultMode() {
        return JavaDocument.isCommandPermitted(getActiveDocument(), DEFAULT_EDIT_MODE.getCommandType()) ? DEFAULT_EDIT_MODE : DEFAULT_VIEW_MODE;
    }

    public void mousePressed(InputData input) {
        // handle clicks on pages to change active page
        final int page = pdfPresenter.getPageByPoint(input.getLocation());
        pdfPresenter.setActivePage(page);

        final Interactive[] handlers = getHandlers();
        for (Interactive handler : handlers) {
            handler.mousePressed(input);
            if (input.isProcessed())
                break;
        }
    }

    public void mouseMoved(InputData input) {
        final Interactive[] handlers = getHandlers();
        for (Interactive handler : handlers) {
            handler.mouseMoved(input);
            if (input.isProcessed())
                break;
        }
    }

    public void mouseReleased(InputData input) {
        final Interactive[] handlers = getHandlers();
        for (Interactive handler : handlers) {
            handler.mouseReleased(input);
            if (input.isProcessed())
                break;
        }
    }

    public void mouseDoubleClicked(InputData input) {
        final Interactive[] handlers = getHandlers();
        for (Interactive handler : handlers) {
            handler.mouseDoubleClicked(input);
            if (input.isProcessed())
                break;
        }
    }

    public void keyPressed(InputData input) {
        final Interactive[] handlers = getHandlers();
        for (Interactive handler : handlers) {
            handler.keyPressed(input);
            if (input.isProcessed())
                break;
        }
    }

    public void keyReleased(InputData input) {
        final Interactive[] handlers = getHandlers();
        for (Interactive handler : handlers) {
            handler.keyReleased(input);
            if (input.isProcessed())
                break;
        }
    }

    public void keyTyped(InputData input) {
        final Interactive[] handlers = getHandlers();
        for (Interactive handler : handlers) {
            handler.keyTyped(input);
            if (input.isProcessed())
                break;
        }
    }

    public void draw(Graphics g, Rectangle updateRect) {
        pdfPresenter.drawPDF(g, updateRect);
        textSearchPresenter.drawSearchedRects(g);

        // draw in the backwards order
        final Interactive[] handlers = getHandlers();
        for (int i = handlers.length - 1; i >= 0; --i) {
            handlers[i].draw(g, updateRect);
        }
    }

    public void setInteractiveContext(InteractiveContext context) {
        // infrastructure, not used
    }

    public Application getApplication() {
        return this;
    }

    public PDF getPdfView() {
        return view.getPDF();
    }

    public PageModel getPageModel() {
        return getPDFPresenter().getPageModel(getPDFPresenter().getActivePage());
    }

    public void changeCursor(Interactive handler, PDF.Cursor cursor) {
        // remove previous entry
        revertCursor(handler);

        // save the last handler on top
        cursorHandlers.put(handler, getPdfView().getViewCursor());
        getPdfView().setViewCursor(cursor);
    }

    public void revertCursor(Interactive handler) {
        if (cursorHandlers.containsKey(handler)) {
            final PDF.Cursor cursor = cursorHandlers.get(handler);
            Iterator<Interactive> handlers = cursorHandlers.keySet().iterator();
            // find current handler in the list of all of them
            while (handlers.hasNext()) {
                if (!handlers.next().equals(handler))
                    continue;

                // found current handler in the list
                if (handlers.hasNext()) {
                    // correct chain if some handler are higher than the current
                    // one
                    cursorHandlers.put(handlers.next(), cursor);
                } else {
                    // revert cursor if the current handler was the topmost
                    getPdfView().setViewCursor(cursor);
                }
                break;
            }
            cursorHandlers.remove(handler);
        }
    }

    public void addDocument(JavaDocument document, boolean activate) {
        this.document = document;
        document.addDocumentListener(this);
        document.addDocumentListener(pdfPresenter);
        document.addDocumentListener(layersPresenter);

        pdfPresenter.addListener(this);
        pdfPresenter.activeDocumentChanged();
        layersPresenter.activeDocumentChanged();
        textSearchPresenter.activeDocumentChanged();

    }

    public void removeDocument(JavaDocument document) {
        if (document != null && document.equals(this.document)) {
            document.removeDocumentListener(layersPresenter);
            document.removeDocumentListener(pdfPresenter);
            document.removeDocumentListener(this);
            this.document = null;

            commandStack.clear();
            pdfPresenter.removeListener(this);
            pdfPresenter.activeDocumentChanged();
            layersPresenter.activeDocumentChanged();
            textSearchPresenter.activeDocumentChanged();
        }
    }

    public JavaDocument getActiveDocument() {
        return document;
    }

    public Viewer getView() {
        return view;
    }

    public DocumentCommand executeCommand(DocumentCommand command) {
        if (command == null)
            command = (DocumentCommand) pendingCommand;

        if (command == null) // no pending command available - simply return
            return null;

        // configure new command with specific parameters
        final boolean editCommand = (command.getType() == CommandType.EDIT);
        boolean consumed = false;
        if (command.getState().isNew()) {
            command.setApplication(this);

            if (command instanceof ViewCommand) {
                ((ViewCommand) command).setPDFPresenter(pdfPresenter);
            }

            if (command instanceof BaseAnnotationCommand) {
                setMode(ApplicationMode.EDIT_ANNOTATION_MODE); // change mode to annotation editing if new specific command came
                ((BaseAnnotationCommand) command).setAnnotationInteractive(annotationInteractive);
                if (command instanceof CreateAnnotationCommand) {
                    ((BaseAnnotationCommand) command).setProperties(getDefaultProps());
                }
            }

            // complete pending command if new edit command came
            if (editCommand && pendingCommand != command && pendingCommand != null) {
                if (pendingCommand.consume(command.getClass())) { // try consuming current command by pending one
                    command = (DocumentCommand) pendingCommand; // forget about current command, return pending instead
                    consumed = true;
                } else {
                    completePendingCommand();
                }
            }
        }

        // execute command if possible
        if (!command.getState().isFinished() && !consumed) {
            if (JavaDocument.isCommandPermitted(getActiveDocument(), command.getType())) {
                command.execute();
            } else {
                command.reject();
            }
        }

        // push command to the stack
        if (editCommand && command.getState().isFinished() && command.getState() == State.Succeeded) {
            commandStack.add(command);
        }

        // set pending command for edit annotation mode
        if (editCommand && command instanceof PendingCommand && command.getState().equals(State.Pending)) {
            pendingCommand = (PendingCommand) command;
        } else if (command instanceof PendingCommand && command.getState().isFinished()) {
            pendingCommand = null;
        }

        if (command.getState().isFinished()) {
            if (command instanceof MarqueeZoomCommand) // if executed marquee zoom command - switch back to default mode
                setDefaultMode();
        }

        return command;
    }

    public void registerInteractive(Interactive interactive, boolean register) {
        if (interactive != null) {
            // remove handler always, since if it is actually available and
            // needed to be re-registered, it will be put to the end of the list
            handlerList.remove(interactive);

            // insert a handler to the first (main) position
            if (register) {
                handlerList.add(0, interactive);
                interactive.setInteractiveContext(this);
            }
        }
    }

    public void pdfChanged(Document pdfDoc) {
        bookmarksPresenter.setPDF(pdfDoc);
        fullUpdate();

        final boolean hasPDF = (getActiveDocument().isOpened());
        if (hasPDF && mode == ApplicationMode.NONE_MODE) {
            setDefaultMode();
        } else if (!hasPDF) {
            setMode(ApplicationMode.NONE_MODE);
        }
    }

    public void permissionChanged() {
        fullUpdate();

        // re-switch default mode in order permissions were restricted
        if (!JavaDocument.isCommandPermitted(getActiveDocument(), mode.getCommandType()) ||
        // re-switch default mode in order permissions were widened
                (mode == DEFAULT_VIEW_MODE && getDefaultMode() == DEFAULT_EDIT_MODE))
            setDefaultMode();
    }

    public void pageChanged(int index, Rect changedArea) {
    }

    public void selectionChanged(Object oldSelection) {
        Object selection = getActiveDocument().getSelection();
        boolean hasSelection = selection instanceof AnnotationHolder;
        final AnnotationHolder holder = hasSelection ? ((AnnotationHolder) selection) : null;
        activeProps.setFrom(hasSelection ? holder.getProperties() : defaultProps);
        activeProps.raiseUpdates(false);

        if (hasSelection)
            holder.addAnnotationListener(new AnnotationListener() {
                public void update(Object updateData) {
                    activeProps.setDirty(false);
                    Object selection = getActiveDocument().getSelection();
                    if (selection != null) {
                        activeProps.setFrom(((AnnotationHolder) selection).getProperties());
                        activeProps.setDirty(false);
                    }
                    updateProperties();
                }
            });
    }

    public void pdfLayerUpdated(List<OptionalContentGroup> ocgList) {
    }

    public void annotationUpdated(AnnotationHolder holder, boolean created) {
        updateControls();
    }

    public void annotationDeleted() {
        updateControls();
    }

    public void viewUpdated(boolean changed) {
        updateControls();
        if (changed) {
            final int activePage = pdfPresenter.getActivePage();
            textSearchPresenter.setActivePage(activePage);

            for (Interactive handler : this.modeHandlers.values()) {
                if (handler instanceof BaseInteractive)
                    ((BaseInteractive) handler).setActivePage(activePage);
            }
        }
    }

    public void stackUpdated() {
        view.updateUndoRedoButtons(commandStack.canPrev(), commandStack.canNext());
    }

    public void searchText(String searchPhrase) {
        if (searchPhrase == null)
            searchPhrase = "";

        textSearchPresenter.setSearchPhrase(searchPhrase);
        view.getPDF().repaintView();
    }

    public void undo() {
        completePendingCommand();
        commandStack.prev().undo();
    }

    public void redo() {
        completePendingCommand();
        commandStack.next().redo();
    }

    public AnnotationProperties getActiveProps() {
        return this.activeProps;
    }

    public AnnotationProperties getDefaultProps() {
        return this.defaultProps;
    }

    public void update(Object updateData) {
        if (getActiveDocument() != null) {
            if (getActiveDocument().getSelection() instanceof AnnotationHolder) {
                AnnotationHolder holder = (AnnotationHolder) getActiveDocument().getSelection();
                holder.getProperties().updateFrom(activeProps);
                holder.getProperties().raiseUpdates(true);
            } else {
                defaultProps.setFrom(activeProps);
            }
        }
        updateProperties();
    }

    public void setAvailableFonts(List<String> availableFonts) {
        AnnotationFactory.setAvailableFonts(availableFonts);
    }

    public void setBlockIncrement(int increment) {
        navigationPresenter.setBlockIncrement(increment);
    }

    protected BaseInteractive getHandler(ApplicationMode forMode) {
        return modeHandlers.containsKey(forMode) ? (BaseInteractive) modeHandlers.get(forMode) : null;
    }

    private void completePendingCommand() {
        final DocumentCommand tempCommand = (DocumentCommand) pendingCommand;
        if (tempCommand != null && !tempCommand.getState().isFinished()) {
            pendingCommand = null;
            executeCommand(tempCommand);
        }
    }

    private void initModeHandlers() {
        this.mode = ApplicationMode.NONE_MODE;
        this.annotationInteractive = new EditAnnotationInteractive();

        this.modeHandlers = new HashMap<ApplicationMode, Interactive>();
        addHandler(ApplicationMode.SCROLL_MODE, new ScrollModeInteractive());
        addHandler(ApplicationMode.ZOOM_MODE, new ZoomModeInteractive());
        addHandler(ApplicationMode.MARQUEE_ZOOM_MODE, new MarqueeZoomInteractive());
        addHandler(ApplicationMode.EDIT_ANNOTATION_MODE, annotationInteractive);
    }

    private void addHandler(ApplicationMode mode, BaseInteractive handler) {
        this.modeHandlers.put(mode, handler);
        handler.setInteractiveContext(this);
        handler.setActivePage(getPDFPresenter().getActivePage());
    }

    private Interactive[] getHandlers() {
        Interactive[] handlers = new Interactive[handlerList.size()];
        handlerList.toArray(handlers);
        return handlers;
    }

    private void setDefaultMode() {
        setMode(getDefaultMode());
    }

    private void updateControls() {
        view.updateControls();
        view.updatePage(pdfPresenter.getActivePage(), pdfPresenter.getPageCount());
        view.updateZoom(pdfPresenter.getZoomMode(), pdfPresenter.getZoom());
        view.updateViewMode(pdfPresenter.getViewMode());
        view.updateUnlock(this.document != null ? this.document.isLocked() : false);
        view.updateTitle((this.document != null && this.document.isOpened()) ? this.document.getDocumentPath() : "");
        view.updateUndoRedoButtons(commandStack.canPrev(), commandStack.canNext());
    }

    private void updateProperties() {
        view.updateLineStyleButton(getActiveProps().getDashPattern());
        view.updateLineWidthButton(getActiveProps().getLineWidth());
        view.updateColorButton(new ColorPicker(getActiveProps().getSubtype().equals(AnnotationConsts.Subtypes.FREETEXT) ? getActiveProps().getTextColor() : getActiveProps().getForeColor(), getActiveProps().getInteriorColor()));
        view.updateArrowMenus(getActiveProps().getSubtype(), getActiveProps().getStartEndingStyle(), getActiveProps().getEndEndingStyle());
        view.updateFontProperty(getActiveProps().getFontFace(), (int) getActiveProps().getFontSize(), getActiveProps().getQuadding());
        String subtype = getActiveProps().getSubtype();
        if (getActiveProps().getSubtype().equals(AnnotationConsts.Subtypes.LINE)) {
            subtype = getActiveProps().getStartEndingStyle().equals(LineEndingStyle.NONE) && getActiveProps().getEndEndingStyle().equals(LineEndingStyle.NONE) ? AnnotationConsts.Subtypes.LINE : AnnotationConsts.Subtypes.ARROW;
        }
        view.updateAnnotationButton(subtype);
    }

    private void fullUpdate() {
        updateControls();
        updateProperties();
    }

    private void setDefaultProperties() {
        defaultProps = new AnnotationProperties();
        defaultProps.setForeColor(Color.BLACK);
        defaultProps.setInteriorColor(Color.WHITE);

        defaultProps.setLineWidth(1);
        defaultProps.setStyle("S");

        defaultProps.setStartEndingStyle(LineEndingStyle.NONE);
        defaultProps.setEndEndingStyle(LineEndingStyle.NONE);

        defaultProps.setUpdateAlways(true);
    }

    private final static ApplicationMode DEFAULT_VIEW_MODE = ApplicationMode.SCROLL_MODE;
    private final static ApplicationMode DEFAULT_EDIT_MODE = ApplicationMode.EDIT_ANNOTATION_MODE;

    private Library library;
    private Viewer view;
    private JavaDocument document;
    private PDFPresenter pdfPresenter;
    private BookmarksPresenter bookmarksPresenter;
    private LayersPresenter layersPresenter;
    private TextSearchPresenter textSearchPresenter;

    private PendingCommand pendingCommand;
    private CommandStack commandStack;

    private ApplicationMode mode;
    private Map<ApplicationMode, Interactive> modeHandlers;
    private final List<Interactive> handlerList;

    private Map<Interactive, PDF.Cursor> cursorHandlers;

    private EditAnnotationInteractive annotationInteractive;
    private KeyNavigationPresenter navigationPresenter;

    private AnnotationProperties activeProps;
    private AnnotationProperties defaultProps;
}
