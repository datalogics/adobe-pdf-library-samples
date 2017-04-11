using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using Datalogics.PDFL;

namespace DotNETViewerComponent
{
    public class EditCommand
    {
        #region Constructors
        public EditCommand(DocumentView view)
        {
            this.view = view;
        }
        #endregion
        #region Methods
        public virtual void Do()
        {
            Debug.WriteLine(this.GetType().Name + ": Do");
        }
        public virtual void Undo()
        {
            Debug.WriteLine(this.GetType().Name + ": Undo");
        }
        public virtual bool Merge(EditCommand command)
        {
            return false;
        }
        public virtual void Clear() // frees any resources if need
        {
        }
        #endregion
        #region Members
        protected DocumentView view;
        #endregion
    }

    public class EditAnnCommand : EditCommand
    {
        public class State
        {
            public System.Drawing.Point position;
            public int pageNum;
            public int index;
            public AnnotationProperties properties;

            public State() { }

            public State(DocumentView view, BaseAnnotationEditor editor)
            {
                Init(view, editor);
            }

            public void Init(DocumentView view, BaseAnnotationEditor editor)
            {
                position = view.Scroll;
                pageNum = view.EditPage;
                index = editor.Index;
                properties = new AnnotationProperties(editor.Properties);
            }

            public override bool Equals(object obj)
            {
                if (!(obj is EditAnnCommand.State)) return false;
                EditAnnCommand.State other = obj as EditAnnCommand.State;
                return pageNum.Equals(other.pageNum) && index.Equals(other.index) && properties.Equals(other.properties);
            }
        }
        #region Constructors
        public EditAnnCommand(DocumentView view) : base(view) { }
        public EditAnnCommand(DocumentView view, State oldState, State newState) : base(view)
        {
            Init(oldState, newState);
        }
        #endregion
        #region Methods
        public void Init(State oldState, State newState)
        {
            if ((oldState.pageNum != newState.pageNum) || (oldState.index != newState.index))
            {
                throw new Exception("Logic error");
            }
            this.oldState = oldState;
            this.newState = newState;
        }
        public override void Do()
        {
            SetState(newState);
        }
        public override void Undo()
        {
            SetState(oldState);
        }
        public override bool Merge(EditCommand command)
        {
            if (!(command is EditAnnCommand)) return false;
            EditAnnCommand other = command as EditAnnCommand;
            // if new states are equal there is no need to do anything
            return other.newState.Equals(newState);
        }
        private void SetState(State state)
        {
            view.Scroll = state.position;
            view.EditPage = state.pageNum;
            Annotation anno = view.GetEditPage().GetAnnotation(state.index);
            AnnotationAppearance app = new AnnotationAppearance(view.Document, anno);
            app.CaptureAnnotation();
            app.Properties.SetFrom(state.properties);
            app.Properties.Dirty = true;
            app.ReleaseAnnotation();
            view.EditPage = -1;
        }
        #endregion
        #region Members
        private State oldState;
        private State newState;
        #endregion
    }

    public class CreateAnnCommand : EditCommand
    {
        public class State
        {
            public System.Drawing.Point position;
            public int pageNum;
            public int index;
            public AnnotationProperties properties;

            public State() { }

            public State(DocumentView view, BaseAnnotationEditor editor)
            {
                Init(view, editor);
            }

            public void Init(DocumentView view, BaseAnnotationEditor editor)
            {
                position = view.Scroll;
                pageNum = view.EditPage;
                index = editor.Index;
                properties = new AnnotationProperties(editor.Properties);
            }
        }
        #region Constructors
        public CreateAnnCommand(DocumentView view) : base(view) { }
        public CreateAnnCommand(DocumentView view, State state) : base(view)
        {
            Init(state);
        }
        #endregion
        #region Methods
        public void Init(State state)
        {
            this.state = state;
        }
        public override void Do()
        {
            view.Scroll = state.position;
            view.EditPage = state.pageNum;
            Annotation ann = AnnotationFactory.CreateAnnotation(view.Document, view.GetEditPage(), state.index, state.properties);
            view.EditPage = -1;
        }
        public override void Undo()
        {
            view.Scroll = state.position;
            view.EditPage = state.pageNum;
            Page page = view.GetEditPage();
            page.RemoveAnnotation(state.index);
            view.EditPage = -1;
        }

        #endregion
        #region Members
        private State state;
        #endregion
    }

    public class DeleteAnnCommand : EditCommand
    {
        public class State
        {
            public System.Drawing.Point position;
            public int pageNum;
            public int index;
            public AnnotationProperties properties;

            public State() { }

            public State(DocumentView view, BaseAnnotationEditor editor)
            {
                Init(view, editor);
            }

            public void Init(DocumentView view, BaseAnnotationEditor editor)
            {
                position = view.Scroll;
                pageNum = view.EditPage;
                index = editor.Index;
                properties = new AnnotationProperties(editor.Properties);
            }
        }
        #region Constructors
        public DeleteAnnCommand(DocumentView view) : base(view) { }
        public DeleteAnnCommand(DocumentView view, State state) : base(view)
        {
            Init(state);
        }
        #endregion
        #region Methods
        public void Init(State state)
        {
            this.state = state;
        }
        public override void Do()
        {
            view.Scroll = state.position;
            view.EditPage = state.pageNum;
            Page page = view.GetEditPage();
            page.RemoveAnnotation(state.index);
            view.EditPage = -1;
        }
        public override void Undo()
        {
            view.Scroll = state.position;
            view.EditPage = state.pageNum;
            Annotation ann = AnnotationFactory.CreateAnnotation(view.Document, view.GetEditPage(), state.index, state.properties);
            view.EditPage = -1;
        }
        #endregion
        #region Members
        private State state;
        #endregion
    }

    public class AppendDocCommand : EditCommand
    {
        public class State
        {
            public System.Drawing.Point scroll;
            public int bookmarks;
            public int layers;
            public int pages;
            public string fileName;

            public State() { }
            public State(DocumentView view, Document append)
            {
                Init(view, append);
            }

            public void Init(DocumentView view, Document append)
            {
                scroll = view.Scroll;
                Document doc = view.Document;
                pages = doc.NumPages;
                Bookmark root = doc.BookmarkRoot;
                if (root != null) bookmarks = root.Count;
                else bookmarks = 0;
                IList<OptionalContentGroup> groups = doc.OptionalContentGroups;
                if (groups != null) layers = groups.Count;
                else layers = 0;
                fileName = append.FileName;
            }
        }
        #region Constructors
        public AppendDocCommand(DocumentView view) : base(view) { }
        public AppendDocCommand(DocumentView view, State state) : base(view)
        {
            Init(state);
        }
        #endregion
        #region Methods
        public void Init(State state)
        {
            this.state = state;
        }
        public override void Do()
        {
            Document doc = view.Document;
            FitModes fit = view.CurrentFit;
            double zoom = view.Zoom;
            Document newDoc = new Document(state.fileName);
            doc.InsertPages(state.pages - 1, newDoc, 0, newDoc.NumPages, PageInsertFlags.All);
            newDoc.Dispose();
            view.Document = doc;
            view.setZoom(fit, zoom);
            view.Scroll = state.scroll;
        }
        public override void Undo()
        {
            view.Scroll = state.scroll;
            Document doc = view.Document;
            FitModes fit = view.CurrentFit;
            double zoom = view.Zoom;
            Bookmark root = doc.BookmarkRoot;
            if (root != null)
            {
                Bookmark child = GetRootBookmark(root, state.bookmarks);
                if (child != null) child.Destroy();
            }
            if (doc.OptionalContentGroups.Count > state.layers)
            {
                IList<OptionalContentGroup> groups = doc.OptionalContentGroups;
                for (int i = state.layers; i < groups.Count; ++i)
                {
                    doc.RemoveOCG(groups[i]);
                }
            }
            view.ClearPageCache();
            doc.DeletePages(state.pages, doc.NumPages - 1);
            view.Document = doc;
            view.setZoom(fit, zoom);
        }
        private Bookmark GetRootBookmark(Bookmark root, int index)
        {
            if ((root == null) || (root.Count <= index)) return null;

            Bookmark child = root.FirstChild;
            while (index > 0)
            {
                child = child.Next;
                if (child == null) return null;
                --index;
            }
            return child;
        }
        #endregion
        #region Members
        private State state;
        #endregion
    }

    public class NewAppendDocCommand : EditCommand
    {
        public class State
        {
            public System.Drawing.Point scroll;
            public OpenDocumentData origin;
            public OpenDocumentData append;
            public string tmpFileName;

            public State() { }
            public State(DocumentView view, OpenDocumentData origin, OpenDocumentData append)
            {
                Init(view, origin, append);
            }

            public void Init(DocumentView view, OpenDocumentData origin, OpenDocumentData append)
            {
                this.scroll = view.Scroll;
                this.origin = origin;
                this.append = append;
                this.tmpFileName = null;
            }
        }
        #region Constructors
        public NewAppendDocCommand(DocumentView view) : base(view) { }
        public NewAppendDocCommand(DocumentView view, State state) : base(view)
        {
            Init(state);
        }
        #endregion
        #region Methods
        void Init(State state)
        {
            this.state = state;
        }
        public override void Do()
        {
            view.Scroll = new System.Drawing.Point(0, 0);
            if (state.tmpFileName == null)
            {
                state.tmpFileName = System.IO.Path.GetTempFileName();
                view.Document.Save(SaveFlags.Full, state.tmpFileName);
                state.origin.fileName = state.tmpFileName;
            }

            view.Document = null;
            OpenDocumentData first = view.OpenDocument(state.origin);  // first doc - original doc saved in tmp file
            OpenDocumentData second = view.OpenDocument(state.append); // second doc - it will be appended to first
            first.document.InsertPages(Document.LastPage, second.document, 0, Document.AllPages, PageInsertFlags.All);
            second.document.Dispose();

            view.Document = null;
            view.Document = first.document;
        }
        public override void Undo()
        {
            view.Scroll = new System.Drawing.Point(0, 0);
            view.Document = null;
            OpenDocumentData open = view.OpenDocument(state.origin);
            view.Document = open.document;
        }
        public override void Clear()
        {
            System.IO.File.Delete(state.origin.fileName);
        }
        #endregion
        #region Members
        State state;
        #endregion
    }

    public class GroupCommand : EditCommand
    {
        #region Constructors
        public GroupCommand(DocumentView view) : base(view)
        {
            commands = new List<EditCommand>();
        }
        #endregion
        #region Methods
        public void Add(EditCommand command)
        {
            commands.Add(command);
        }
        public override void Do()
        {
            for (int i = 0; i <= commands.Count - 1; ++i) commands[i].Do();
        }
        public override void Undo()
        {
            for (int i = commands.Count - 1; i >= 0; --i) commands[i].Undo();
        }
        #endregion
        #region Members
        private IList<EditCommand> commands;
        #endregion
    }

    public class EditCommandStack
    {
        #region Constructors
        public EditCommandStack()
        {
            Reset();
        }
        #endregion
        #region Properties
        public bool CanRedo { get { return commandIndex < commands.Count - 1; } }
        public bool CanUndo { get { return commandIndex >= 0; } }
        #endregion
        #region Methods
        public void Reset()
        {
            foreach (EditCommand command in commands) command.Clear();
            commands = new List<EditCommand>();
            commandIndex = -1;
            UpdateEvent = null;
        }
        public void Push(EditCommand command)
        {
            Add(command);
            ++commandIndex;
            Update();
        }
        public void Execute(EditCommand command)
        {
            Add(command);
            Redo();
        }
        public void Redo()
        {
            if (!CanRedo) return;
            ++commandIndex;
            commands[commandIndex].Do();
            Update();
        }
        public void Undo()
        {
            if (!CanUndo) return;
            commands[commandIndex].Undo();
            --commandIndex;
            Update();
        }
        public void OnUpdate(UpdateCallback Update)
        {
            this.UpdateEvent += Update;
            this.Update();
        }
        private void Update()
        {
            if (UpdateEvent != null) UpdateEvent(this);
        }
        private void Add(EditCommand command)
        {
            // if current command is not the last - remove rest commands
            while (commandIndex + 1 < commands.Count) commands.RemoveAt(commandIndex + 1);
            // first try merge new command with last command
            bool merged = false;
            if (commandIndex >= 0) merged = commands[commandIndex].Merge(command);

            if (merged) --commandIndex;
            else commands.Add(command);
        }
        #endregion
        #region Members
        private int commandIndex;            // index of last command that was executed
        private IList<EditCommand> commands = new List<EditCommand>(); // this list is always not null

        public delegate void UpdateCallback(EditCommandStack editor);
        private event UpdateCallback UpdateEvent;
        #endregion
    }
}
