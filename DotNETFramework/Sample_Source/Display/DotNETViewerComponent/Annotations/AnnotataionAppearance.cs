using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using Datalogics.PDFL;
using System.Reflection;

namespace DotNETViewerComponent
{
    /**
     * Class providing annotation editing.
     * It uses AnnotationProperties class for work with annotation.
     * Used for properties serialization, and for generation of annotation's appearance.
     */
    public class AnnotationAppearance
    {
        #region Constructors
        static AnnotationAppearance()
        {
            families = new List<string>();
            InstalledFontCollection collection = new InstalledFontCollection();

            FontFamily[] fams = collection.Families;

            for (int i = 0; i < fams.Length; ++i)
            {
                families.Add(fams[i].Name);
            }
        }

        public AnnotationAppearance(Datalogics.PDFL.Document doc, Datalogics.PDFL.Annotation ann)
        {
            document = doc;
            annotation = ann;
            properties = new AnnotationProperties();
            ReadProperties();
        }
        #endregion

        #region Properties
        public Datalogics.PDFL.Form NormalAppearance
        {
            get { return normalAppearance; }
            set
            {
                if (normalAppearance == value) return;
                normalAppearance = value;
            }
        }
      
        public AnnotationProperties Properties
        {
            get { return properties; }
        }
        #endregion

        #region Methods
        /**
         * Recreates annotation's appearance: writes annotation to pdf,
         * updates appearance, and then reads it again.
         */
        public void UpdateAppearance()
        {
            if (properties.Dirty)
            {
                WriteProperties();

                NormalAppearance = GenereteAppearence();
                setPropertyByName<Datalogics.PDFL.Form>(AnnotationConsts.NORM_APPEARANCE, normalAppearance);

                ReadProperties();
            }
        }
        /**
         * Captures annotation for editing.
         * While captured, annotation made invisible to avoid it's rendering on page
         */
        public void CaptureAnnotation()
        {
            properties.Update += OnPropertyChanged;

            if ((annotation.Flags & AnnotationFlags.Hidden) == 0)
            {
                annotation.Flags |= AnnotationFlags.Hidden;
                OnUpdate(AppearanceUpdate.ObjectHidden);
            }
        }
        /**
         * Releases annotation after editing, restores visibility.
         */
        public void ReleaseAnnotation()
        {
            UpdateAppearance();

            annotation.Flags &= ~AnnotationFlags.Hidden;
            OnUpdate(AppearanceUpdate.ObjectShown);

            properties.Update -= OnPropertyChanged;
        }
        private void ReadProperties()
        {
            readPropertyByName<string>(AnnotationConsts.SUBTYPE, false);
            readPropertyByName<string>(AnnotationConsts.TITLE, false);
            readPropertyByName<string>(AnnotationConsts.CONTENTS, false);
            readPropertyByName<double>(AnnotationConsts.WIDTH, true);
            readPropertyByName<Datalogics.PDFL.Color>(AnnotationConsts.COLOR, false);
            readPropertyByName<Datalogics.PDFL.Color>(AnnotationConsts.INTERIOR_COLOR, false);
            readPropertyByName<Datalogics.PDFL.Color>(AnnotationConsts.TEXT_COLOR, false);
            readPropertyByName<string>(AnnotationConsts.FONT_FACE, false);
            readPropertyByName<double>(AnnotationConsts.FONT_SIZE, false);
            readPropertyByName<IList<double>>(AnnotationConsts.DASH_PATTERN, false);
            readPropertyByName<Datalogics.PDFL.Rect>(AnnotationConsts.BOUNDING_RECT, true);
            readPropertyByName<string>(AnnotationConsts.STYLE, false);

            readPropertyByName<Datalogics.PDFL.Point>(AnnotationConsts.START_POINT, true);
            readPropertyByName<Datalogics.PDFL.Point>(AnnotationConsts.END_POINT, true);
            readPropertyByName<IList<Datalogics.PDFL.Point>>(AnnotationConsts.VERTICES, true);
            readPropertyByName<Datalogics.PDFL.HorizontalAlignment>(AnnotationConsts.QUADDING, false);

            readPropertyByName<Datalogics.PDFL.LineEndingStyle>(AnnotationConsts.START_ENDING_STYLE, false);
            readPropertyByName<Datalogics.PDFL.LineEndingStyle>(AnnotationConsts.END_ENDING_STYLE, false);
            readPropertyByName<IList<Datalogics.PDFL.Quad>>(AnnotationConsts.QUADS, true);

            normalAppearance = getPropertyByName<Datalogics.PDFL.Form>(AnnotationConsts.NORM_APPEARANCE);

            if (annotation is LinkAnnotation)
            {
                Datalogics.PDFL.Action action = (annotation as LinkAnnotation).Action;
                if (action is GoToAction)
                {
                    ActionData actionData = new ActionData(action as GoToAction);
                    properties.SetProperty<ActionData>(AnnotationConsts.ACTION, actionData, true);
                }
                SetShowBorderProperties();
            }
            else if (annotation is InkAnnotation)
            {
                ReadScribbles();
            }

            SetSupportedProperty(annotation);
            properties.Dirty = false;
        }
        private void WriteProperties()
        {
            foreach (string property in properties.Available)
            {
                if ((property == AnnotationConsts.SCRIBBLES) && (annotation is InkAnnotation))
                {
                    WriteScribbles((IList<IList<Datalogics.PDFL.Point>>)properties[AnnotationConsts.SCRIBBLES].Value);
                }
                else if ((property == AnnotationConsts.ACTION) && (annotation is LinkAnnotation))
                {
                    ActionData actionData = properties[property].Value as ActionData;
                    ViewDestination viewDestination = new ViewDestination(document, actionData.pageNum, actionData.fitType, actionData.destRect, actionData.zoom);
                    (annotation as LinkAnnotation).Action = new GoToAction(viewDestination);
                }
                else
                {
                    setPropertyByName(property, properties[property].Key, properties[property].Value);
                }
            }
        }
        private void ReadScribbles()
        {
            InkAnnotation ink = annotation as InkAnnotation;
            IList<IList<Datalogics.PDFL.Point>> scribbles = new List<IList<Datalogics.PDFL.Point>>();
            for (int i = 0; i < ink.NumScribbles; ++i)
            {
                scribbles.Add(ink.GetScribble(i));
            }
            properties.SetProperty<IList<IList<Datalogics.PDFL.Point>>>(AnnotationConsts.SCRIBBLES, scribbles, true);
        }
        private void WriteScribbles(IList<IList<Datalogics.PDFL.Point>> scribbles)
        {
            InkAnnotation ink = annotation as InkAnnotation;
            while (ink.NumScribbles > 0) ink.RemoveScribble(0);
            foreach (IList<Datalogics.PDFL.Point> scribble in scribbles)
            {
                ink.AddScribble(scribble);
            }
        }
        private Datalogics.PDFL.Form GenereteAppearence()
        {
             Datalogics.PDFL.Form appear = null;
             if (annotation is LinkAnnotation)
                 appear = getLinkAnnotationForm();
             else
                 appear = annotation.GenerateAppearance();
            return appear;
        }
        private void SetShowBorderProperties()
        {
            PDFObject width = null;
            // if annotation dictionary includes BS entry, then the Border entry is ignored
            if (annotation.PDFDict.Contains("BS"))
                width = (PDFObject)((PDFDict)annotation.PDFDict.Get("BS")).Get("W");
            else
            {
                try
                {
                    width = (PDFInteger)((PDFArray)annotation.PDFDict.Get("Border")).Get(2);
                }
                catch (Exception)
                {
                }
            }
            double lineWidth = properties.LineWidth; // default value
            if (width != null)
                lineWidth = width is PDFReal ? ((PDFReal)width).Value : ((PDFInteger)width).Value;
            properties.LineWidth = (float)lineWidth; // set proper border value from library properties

            PDFArray color = null;
            if (annotation.PDFDict.Contains("C"))
            {
                try
                {
                    color = (PDFArray)annotation.PDFDict.Get("C");
                }
                catch (Exception)
                {
                }
            }
            int colorLen = (color != null)? color.Length: 3;
            properties.SetProperty<bool>(AnnotationConsts.SHOW_BORDER, colorLen != 0 && lineWidth != 0, false);
        }

        private Datalogics.PDFL.Form getLinkAnnotationForm()
        {
            return this.Properties.ShowBorder ? annotation.GenerateAppearance() : null;
        }

        private void SetFreeTextAnnotationProperties(Datalogics.PDFL.Content content)
        {
            bool useDefaultFont = false;
            Path path = null;
            TextRun currentTextRun = null;
            GraphicState gState = null;
            Datalogics.PDFL.Font dlFont = null;
            for (int i = 0; i < content.NumElements; i++)
            {
                Element element = content.GetElement(i);
                if (element is Text)
                {
                    Text currentText = element as Text;
                    for (int j = 0; j < currentText.NumberOfRuns; j++)
                    {
                        currentTextRun = currentText.GetRun(j);
                        dlFont = currentTextRun.Font;
                        if (dlFont != null)
                            break;
                    }
                }
                else if (element is Path)
                {
                    path = element as Path;
                    gState = path.GraphicState;
                }
            }
            if (dlFont == null)
            {
                dlFont = new Datalogics.PDFL.Font("Arial");
                useDefaultFont = true;
            }

            if (gState == null)
                gState = new GraphicState();

            if (currentTextRun == null)
                currentTextRun = new TextRun("", dlFont, gState, new TextState(), new Datalogics.PDFL.Matrix());

            if (path == null)
                path = new Path(gState);

            string fontName = "";
            if (families.IndexOf(dlFont.Name) == -1)
            {
                foreach (string name in families)
                {
                    string trimmedName = name.Replace(" ", "");
                    if (name.Contains(dlFont.Name) || trimmedName.Contains(dlFont.Name) || dlFont.Name.Contains(name) || dlFont.Name.Contains(trimmedName))
                    {
                        fontName = name;
                        break;
                    }
                }
            }
        }
        private void SetSupportedProperty(Annotation ann)
        {
            bool isSupp = (ann.AnnotationFeatureLevel <= 1.5);

            // check the ending styles of the Line and PolyLine (in case the line is
            // an Arrow) for the endings that DotNETViewer knows how to draw
            LineEndingStyle[] lineStyles = new LineEndingStyle[] { properties.StartEndingStyle, properties.EndEndingStyle };
            LineEndingStyle[] supportedStyles = new LineEndingStyle[] { LineEndingStyle.None, LineEndingStyle.Circle, LineEndingStyle.OpenArrow, LineEndingStyle.ClosedArrow };

            foreach (LineEndingStyle style in lineStyles)
            {
                isSupp = isSupp && (Array.IndexOf(supportedStyles, style) != -1);
                if (!isSupp)
                    break;
            }
            properties.IsAnnotationSupported = isSupp;
        }
        private T getPropertyByName<T>(string property) where T : class
        {
            PropertyInfo prop = annotation.GetType().GetProperty(property, typeof(T));
            return (prop != null && prop.CanRead) ? (T)prop.GetValue(annotation, null) : null;
        }
        private void readPropertyByName<T>(string property, bool shapeChanged)
        {
            PropertyInfo prop = annotation.GetType().GetProperty(property, typeof(T));
            if (prop != null && prop.CanRead)
                properties.SetProperty<T>(property, (T)prop.GetValue(annotation, null), shapeChanged);
        }
        private void setPropertyByName<T>(string property, T value)
        {
            setPropertyByName(property, typeof(T), value);
        }
        private void setPropertyByName(string property, Type type, object value)
        {
            PropertyInfo prop = annotation.GetType().GetProperty(property, type);
            if (prop != null && prop.CanWrite)
                prop.SetValue(annotation, value, null);
        }
        private void OnPropertyChanged(object shapeChanged)
        {
            if (properties.IsAutoSize)
            {
                if (annotation is FreeTextAnnotation)
                {
                    Content content = annotation.NormalAppearance.Content;
                    double newBoundHeight = 0;
                    for (int i = 0; i < content.NumElements; i++)
                    {
                        Element element = content.GetElement(i);
                        if (element is Text)
                        {
                            Text currentText = element as Text;
                            for (int j = 0; j < currentText.NumberOfRuns; j++)
                                newBoundHeight += currentText.GetRun(j).BoundingBox.Height;
                        }
                    }
                    properties.BoundingRect = new Rect(properties.BoundingRect.LLx, properties.BoundingRect.URy - newBoundHeight, properties.BoundingRect.URx, properties.BoundingRect.URy);
                    shapeChanged = true;
                }
                properties.IsAutoSize = false;
            }

            OnUpdate(true.Equals(shapeChanged) ? AppearanceUpdate.ShapeChanged : AppearanceUpdate.PropertiesChanged);
        }
        private void OnUpdate(AppearanceUpdate appearanceUpdate)
        {
            if (Update != null)
                Update(appearanceUpdate);
        }
        #endregion

        #region Members
        public event AnnotationProperties.UpdateCallback Update;
        private Document document;
        private Datalogics.PDFL.Annotation annotation;
        private Datalogics.PDFL.Form normalAppearance;
        private AnnotationProperties properties;
        private static List<string> families;
        #endregion

        #region Subtypes
        public enum AppearanceUpdate
        {
            ObjectHidden,
            ObjectShown,
            ShapeChanged,
            PropertiesChanged
        }
        #endregion
    }
}
