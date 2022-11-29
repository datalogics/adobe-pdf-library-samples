using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using Datalogics.PDFL;

namespace DotNETViewerComponent
{
    /**
     * Annotation properties descriptors.
     * We use reflection mechanism to access to most of annotation's properties
     */
    public class AnnotationConsts
    {
        public const string SUBTYPE = "Subtype";
        public const string TITLE = "Title";
        public const string CONTENTS = "Contents";
        public const string ACTION = "Action";
        public const string WIDTH = "Width";
        public const string COLOR = "Color";
        public const string INTERIOR_COLOR = "InteriorColor";
        public const string TEXT_COLOR = "TextColor";
        public const string DASH_PATTERN = "DashPattern";
        public const string BOUNDING_RECT = "Rect";
        public const string STYLE = "Style";

        public const string START_POINT = "StartPoint";
        public const string END_POINT = "EndPoint";
        public const string VERTICES = "Vertices";
        public const string QUADS = "Quads";
        public const string NORM_APPEARANCE = "NormalAppearance";

        public const string START_ENDING_STYLE = "StartPointEndingStyle";
        public const string END_ENDING_STYLE = "EndPointEndingStyle";

        public const string FONT_FACE = "FontFace";
        public const string FONT_SIZE = "FontSize";
        public const string QUADDING = "Quadding";

        public const string SCRIBBLES = "Scribbles";

        public const string IS_ANNOT_SUPPORT = "IsAnnotationSupport";

        public const string IS_AUTO_SIZE = "IsAutoSize";
        public const string SAVED_WIDTH = "SavedWidth";
        public const string SHOW_BORDER = "ShowBorder";
    }

    /**
     * Wrapper class for Datalogics.PDFL.Action
     */
    public class ActionData
    {
        public ActionData(GoToAction action) : this(action.Destination) {}
        public ActionData(ViewDestination destination)
        {
            pageNum = destination.PageNumber;
            fitType = destination.FitType;
            destRect = new Rect(destination.DestRect.Left, destination.DestRect.Bottom, destination.DestRect.Right, destination.DestRect.Top);
            zoom = destination.Zoom;
        }
        public int pageNum;
        public string fitType;
        public Rect destRect;
        public double zoom;
    }
    /**
     * Class that allows to work with annotation properties.
     * Any propety can be set to any type of annotation,
     * if annotation hasn't given property, operation has no effect.
     * When property is changed, OnUpdate event is called.
     */
    public class AnnotationProperties
    {
        #region Constructors
        public AnnotationProperties() {}
        public AnnotationProperties(AnnotationProperties other)
        {
            CopyFrom(other, false);
        }
        public AnnotationProperties(string subtype)
        {
            UpdateProperty<string>(AnnotationConsts.SUBTYPE, subtype);
        }
        #endregion
        #region Properties
        public bool UpdateAlways
        {
            get { return updateAlways; }
            set { updateAlways = value; }
        }
        public string Subtype
        {
            get { return GetProperty<string>(AnnotationConsts.SUBTYPE, ""); }
        }
        public Datalogics.PDFL.Rect BoundingRect
        {
            get
            {
                Datalogics.PDFL.Rect rect = GetProperty<Datalogics.PDFL.Rect>(AnnotationConsts.BOUNDING_RECT, new Datalogics.PDFL.Rect(0, 0, 1, 1));
                return new Rect(rect.LLx, rect.LLy, rect.URx, rect.URy);
            }
            set
            {
                if (value == null || (!UpdateAlways && BoundingRect.Equals(value))) return;
                UpdateProperty<Datalogics.PDFL.Rect>(AnnotationConsts.BOUNDING_RECT, new Rect(value.LLx, value.LLy, value.URx, value.URy));
            }
        }
        public string Title
        {
            get { return GetProperty<string>(AnnotationConsts.TITLE, ""); }
            set
            {
                if (!UpdateAlways && (Title == value)) return;
                UpdateProperty<string>(AnnotationConsts.TITLE, value);
            }
        }
        public string Contents
        {
            get { return GetProperty<string>(AnnotationConsts.CONTENTS, ""); }
            set
            {
                if (!UpdateAlways && (Contents == value)) return;
                UpdateProperty<string>(AnnotationConsts.CONTENTS, value);
            }
        }
        public ActionData ActionData
        {
            get { return (ActionData)(GetProperty<ActionData>(AnnotationConsts.ACTION, null)); }
            set { UpdateProperty<ActionData>(AnnotationConsts.ACTION, value); }
        }
        public float LineWidth
        {
            get { return (float)(GetProperty<double>(AnnotationConsts.WIDTH, 1.0)); }
            set
            {
                if (!UpdateAlways && (LineWidth == value)) return;
                UpdateProperty<double>(AnnotationConsts.WIDTH, (double)value);
            }
        }
        public System.Drawing.Color ForeColor
        {
            get { return Utils.Transform(GetProperty<Datalogics.PDFL.Color>(AnnotationConsts.COLOR, new Datalogics.PDFL.Color(0.0, 0.0, 0.0))); }
            set
            {
                if (!UpdateAlways && (ForeColor == value)) return;
                UpdateProperty<Datalogics.PDFL.Color>(AnnotationConsts.COLOR, Utils.Transform(value));
            }
        }
        public System.Drawing.Color InteriorColor
        {
            get { return Utils.Transform(GetProperty<Datalogics.PDFL.Color>(AnnotationConsts.INTERIOR_COLOR, new Datalogics.PDFL.Color(0.0, 0.0, 0.0))); }
            set
            {
                if (!UpdateAlways && (InteriorColor == value)) return;
                UpdateProperty<Datalogics.PDFL.Color>(AnnotationConsts.INTERIOR_COLOR, Utils.Transform(value));
            }
        }
        public System.Drawing.Color TextColor
        {
            get { return Utils.Transform(GetProperty<Datalogics.PDFL.Color>(AnnotationConsts.TEXT_COLOR, new Datalogics.PDFL.Color(0.0, 0.0, 0.0))); }
            set
            {
                if (!UpdateAlways && (TextColor == value)) return;
                UpdateProperty<Datalogics.PDFL.Color>(AnnotationConsts.TEXT_COLOR, Utils.Transform(value));
            }
        }
        public bool HasFill
        {
            get { return GetProperty<Datalogics.PDFL.Color>(AnnotationConsts.INTERIOR_COLOR, new Datalogics.PDFL.Color(0.0, 0.0, 0.0)) != null; }
            set
            {
                if (value == false)
                {
                    UpdateProperty<Datalogics.PDFL.Color>(AnnotationConsts.INTERIOR_COLOR, null);
                }
            }
        }
        public System.Drawing.SolidBrush FillBrush { get { return new SolidBrush(InteriorColor); } }
        public bool Solid
        {
            get { return Style == "S"; }
            set
            {
                if (value == true)
                    Style = "S";
            }
        }
        public string Style
        {
            get { return GetProperty<string>(AnnotationConsts.STYLE, "S"); }
            set
            {
                if ((!UpdateAlways && (Style == value)) || (value != "S" && value != "D" && value != "B" && value != "I" && value != "U")) return;
                UpdateProperty<string>(AnnotationConsts.STYLE, value);
            }
        }
        public float[] DashPattern
        {
            get
            {
                IList<double> dashPattern = GetProperty<IList<double>>(AnnotationConsts.DASH_PATTERN, new List<double>());
                float[] pattern = null;

                if (dashPattern != null && dashPattern.Count != 0)
                {
                    pattern = new float[dashPattern.Count];
                    for (int i = 0; i < dashPattern.Count; ++i)
                        pattern[i] = (float)dashPattern[i];
                }
                return pattern;
            }
            set
            {
                IList<double> dashPattern = GetProperty<IList<double>>(AnnotationConsts.DASH_PATTERN, new List<double>());

                int len = value != null ? value.Length : 0;
                List<double> dash = new List<double>(len);
                bool sameValues = (len == dashPattern.Count);
                for (int i = 0; i < len; ++i)
                {
                    dash.Add(value[i]);
                    sameValues = sameValues && (dash[i] == dashPattern[i]);
                }

                if (!UpdateAlways && sameValues) return;
                //Style = (value != null) ? "D" : "S";
                // first update style without calling update event
                UpdateProperty<string>(AnnotationConsts.STYLE, (value != null) ? "D" : "S", null, false);
                UpdateProperty<IList<double>>(AnnotationConsts.DASH_PATTERN, dash);
            }
        }
        public bool ShowBorder
        {
            get { return GetProperty<bool>(AnnotationConsts.SHOW_BORDER, false); }
            set
            {
                if (!UpdateAlways && (value == ShowBorder)) return;

                if (value && LineWidth == 0)
                    LineWidth = 1;
                else if (!value && LineWidth != 0)
                    LineWidth = 0;

                UpdateProperty<bool>(AnnotationConsts.SHOW_BORDER, value);
            }
        }
        public Datalogics.PDFL.Point StartPoint
        {
            get
            {
                Datalogics.PDFL.Point value = GetProperty<Datalogics.PDFL.Point>(AnnotationConsts.START_POINT, new Datalogics.PDFL.Point());
                return new Datalogics.PDFL.Point(value.H, value.V);
            }
            set
            {
                if (!UpdateAlways && StartPoint.Equals(value)) return;
                UpdateProperty<Datalogics.PDFL.Point>(AnnotationConsts.START_POINT, new Datalogics.PDFL.Point(value.H, value.V));
            }
        }
        public Datalogics.PDFL.Point EndPoint
        {
            get
            {
                Datalogics.PDFL.Point value = GetProperty<Datalogics.PDFL.Point>(AnnotationConsts.END_POINT, new Datalogics.PDFL.Point());
                return new Datalogics.PDFL.Point(value.H, value.V);
            }
            set
            {
                if (!UpdateAlways && EndPoint.Equals(value)) return;
                UpdateProperty<Datalogics.PDFL.Point>(AnnotationConsts.END_POINT, new Datalogics.PDFL.Point(value.H, value.V));
            }
        }
        public IList<Datalogics.PDFL.Point> Vertices
        {
            get
            {
                // can not just return property, need to make a copy
                IList<Datalogics.PDFL.Point> vertices = GetProperty<IList<Datalogics.PDFL.Point>>(AnnotationConsts.VERTICES, new List<Datalogics.PDFL.Point>());
                return Utils.Clone(vertices);
            }
            set
            {
                // can not just assing value, need to make a copy
                IList<Datalogics.PDFL.Point> newVertices = Utils.Clone(value);
                // assing only if new value is not equals to old value
                bool equals = false;
                IList<Datalogics.PDFL.Point> oldVertices = GetProperty<IList<Datalogics.PDFL.Point>>(AnnotationConsts.VERTICES, new List<Datalogics.PDFL.Point>());
                if (oldVertices.Count == newVertices.Count)
                {
                    equals = true;
                    for (int i = 0; i < oldVertices.Count; ++i)
                    {
                        if (!oldVertices[i].Equals(newVertices[i]))
                        {
                            equals = false;
                            break;
                        }
                    }
                }

                if (UpdateAlways || !equals) UpdateProperty<IList<Datalogics.PDFL.Point>>(AnnotationConsts.VERTICES, newVertices);
            }
        }
        public IList<Quad> Quads
        {
            get
            {
                // can not just return property, need to make a copy
                IList<Quad> quads = GetProperty<IList<Quad>>(AnnotationConsts.QUADS, new List<Quad>());
                IList<Quad> value = Utils.Clone(quads);
                return value;
            }
            set
            {
                // can not just assing value, need to make a copy
                IList<Quad> newQuads = Utils.Clone(value);
                // assing only if new value is not equals to old value
                bool equals = false;
                IList<Quad> oldQuads = GetProperty<IList<Quad>>(AnnotationConsts.QUADS, new List<Quad>());
                if (oldQuads.Count == newQuads.Count)
                {
                    equals = true;
                    for (int i = 0; i < oldQuads.Count; ++i)
                    {
                        if (!oldQuads[i].Equals(newQuads[i]))
                        {
                            equals = false;
                            break;
                        }
                    }
                }

                if (UpdateAlways || !equals) UpdateProperty<IList<Quad>>(AnnotationConsts.QUADS, newQuads);
            }
        }
        public LineEndingStyle StartEndingStyle
        {
            get { return GetProperty<LineEndingStyle>(AnnotationConsts.START_ENDING_STYLE, LineEndingStyle.None); }
            set
            {
                if (!UpdateAlways && (StartEndingStyle == value)) return;
                UpdateProperty<LineEndingStyle>(AnnotationConsts.START_ENDING_STYLE, value);
            }
        }
        public LineEndingStyle EndEndingStyle
        {
            get { return GetProperty<LineEndingStyle>(AnnotationConsts.END_ENDING_STYLE, LineEndingStyle.None); }
            set
            {
                if (!UpdateAlways && (EndEndingStyle == value)) return;
                UpdateProperty<LineEndingStyle>(AnnotationConsts.END_ENDING_STYLE, value);
            }
        }
        public string FontFace
        {
            get { return GetProperty<string>(AnnotationConsts.FONT_FACE, "Arial"); }
            set
            {
                if (!UpdateAlways && (FontFace == value)) return;
                UpdateProperty<string>(AnnotationConsts.FONT_FACE, value);
            }
        }
        public double FontSize
        {
            get { return GetProperty<double>(AnnotationConsts.FONT_SIZE, 10); }
            set
            {
                if (!UpdateAlways && (FontSize == value)) return;
                UpdateProperty<double>(AnnotationConsts.FONT_SIZE, value);
            }
        }
        public IList<IList<Datalogics.PDFL.Point>> Scribbles
        {
            get
            {
                IList<IList<Datalogics.PDFL.Point>> scribbles = GetProperty<IList<IList<Datalogics.PDFL.Point>>>
                    (AnnotationConsts.SCRIBBLES, new List<IList<Datalogics.PDFL.Point>>());
                return Utils.Clone(scribbles);
            }
            set
            {
                IList<IList<Datalogics.PDFL.Point>> scribbles = Utils.Clone(value);
                IList<IList<Datalogics.PDFL.Point>> oldScribbles = GetProperty<IList<IList<Datalogics.PDFL.Point>>>
                    (AnnotationConsts.SCRIBBLES, new List<IList<Datalogics.PDFL.Point>>());

                bool equals = false;
                if (!UpdateAlways && (scribbles.Count == oldScribbles.Count))
                {
                    equals = true;
                    for (int i = 0; i < scribbles.Count; ++i)
                    {
                        if (scribbles[i].Count == oldScribbles[i].Count)
                        {
                            for (int j = 0; j < scribbles[i].Count; ++j)
                            {
                                if (!scribbles[i][j].Equals(oldScribbles[i][j]))
                                {
                                    equals = false;
                                    break;
                                }
                            }
                        }
                        else equals = false;

                        if (!equals) break;
                    }
                }
                if (UpdateAlways || !equals) UpdateProperty<IList<IList<Datalogics.PDFL.Point>>>(AnnotationConsts.SCRIBBLES, scribbles);
            }
        }
        public bool IsAutoSize
        {
            get { return GetProperty<bool>(AnnotationConsts.IS_AUTO_SIZE, false); }
            set
            {
                if (!UpdateAlways && (IsAutoSize == value)) return;
                UpdateProperty<bool>(AnnotationConsts.IS_AUTO_SIZE, value);
            }
        }
        public TextFormatFlags Quadding
        {
            get
            {
                Datalogics.PDFL.HorizontalAlignment quadding = GetProperty<Datalogics.PDFL.HorizontalAlignment>(AnnotationConsts.QUADDING, Datalogics.PDFL.HorizontalAlignment.Left);
                TextFormatFlags flags = new TextFormatFlags();
                if (quadding == Datalogics.PDFL.HorizontalAlignment.Left)
                    flags = TextFormatFlags.Left;
                else if (quadding == Datalogics.PDFL.HorizontalAlignment.Center)
                    flags = TextFormatFlags.HorizontalCenter;
                else if (quadding == Datalogics.PDFL.HorizontalAlignment.Right)
                    flags = TextFormatFlags.Right;
                return flags;
            }
            set
            {
                if (!UpdateAlways && (Quadding == value)) return;
                Datalogics.PDFL.HorizontalAlignment ha = new Datalogics.PDFL.HorizontalAlignment();

                if ((value & TextFormatFlags.HorizontalCenter) != 0)
                    ha = Datalogics.PDFL.HorizontalAlignment.Center;
                else if ((value & TextFormatFlags.Right) != 0)
                    ha = Datalogics.PDFL.HorizontalAlignment.Right;
                else if (value == 0)
                    ha = Datalogics.PDFL.HorizontalAlignment.Left;

                UpdateProperty<Datalogics.PDFL.HorizontalAlignment>(AnnotationConsts.QUADDING, ha);
            }
        }
        public StringFormat Alignment
        {
            get
            {
                Datalogics.PDFL.HorizontalAlignment quadding = GetProperty<Datalogics.PDFL.HorizontalAlignment>(AnnotationConsts.QUADDING, Datalogics.PDFL.HorizontalAlignment.Left);
                StringFormat stringFormat = new StringFormat();
                if (quadding == Datalogics.PDFL.HorizontalAlignment.Left)
                    stringFormat.Alignment = StringAlignment.Near;
                else if (quadding == Datalogics.PDFL.HorizontalAlignment.Center)
                    stringFormat.Alignment = StringAlignment.Center;
                else if (quadding == Datalogics.PDFL.HorizontalAlignment.Right)
                    stringFormat.Alignment = StringAlignment.Far;
                return stringFormat;
            }
            set
            {
                if (!UpdateAlways && Alignment.Equals(value)) return;
                Datalogics.PDFL.HorizontalAlignment ha = new Datalogics.PDFL.HorizontalAlignment();

                if ((value.Alignment & StringAlignment.Center) != 0)
                    ha = Datalogics.PDFL.HorizontalAlignment.Center;
                else if ((value.Alignment & StringAlignment.Far) != 0)
                    ha = Datalogics.PDFL.HorizontalAlignment.Right;
                else
                    ha = Datalogics.PDFL.HorizontalAlignment.Left;

                UpdateProperty<Datalogics.PDFL.HorizontalAlignment>(AnnotationConsts.QUADDING, ha);
            }
        }

        public bool IsAnnotationSupported
        {
            get { return GetProperty<bool>(AnnotationConsts.IS_ANNOT_SUPPORT, false); }
            set
            {
                if (!UpdateAlways && (IsAnnotationSupported == value)) return;
                UpdateProperty<bool>(AnnotationConsts.IS_ANNOT_SUPPORT, value);
            }
        }

        public ICollection<string> Available
        {
            get { return properties.Keys; }
        }
        public bool Dirty
        {
            get
            {
                bool dirty = false;

                foreach (Property prop in properties.Values)
                {
                    dirty = dirty || prop.Dirty;
                    if (dirty)
                        break;
                }
                return dirty;
            }
            set
            {
                if (Dirty == value) return;

                string[] keys = new string[properties.Keys.Count];
                properties.Keys.CopyTo(keys, 0);

                for (int i = 0; i < keys.Length; ++i)
                {
                    string property = keys[i];
                    properties[property] = new Property(properties[property].Value, properties[property].Type, value, properties[property].ChangeData);
                }
            }
        }
        public KeyValuePair<Type, object> this[string property]
        {
            get
            {
                return properties.ContainsKey(property) ? new KeyValuePair<Type, object>(properties[property].Type, properties[property].Value) : new KeyValuePair<Type, object>();
            }
        }
        #endregion

        #region Methods
        public override bool Equals(object obj)
        {
            if (!(obj is AnnotationProperties)) return false;
            AnnotationProperties other = obj as AnnotationProperties;
            return properties.Equals(other.properties);
        }
        private bool Equals<T>(IList<IList<T>> a, IList<IList<T>> b)
        {
            if ((a == null) && (b == null)) return true;

            if ((a == null) || (b == null) || (a.Count != b.Count)) return false;

            for (int i = 0; i < a.Count; ++i)
            {
                if ((a[i] == null) != (b[i] == null)) return false;
                if (a[i] == null) continue;
                for (int j = 0; j < a[i].Count; ++j)
                {
                    if (!a[i][j].Equals(b[i][j])) return false;
                }
            }
            return true;
        }
        public void RaiseUpdates(object changeData)
        {
            OnUpdate(changeData);
        }
        public void SetFrom(AnnotationProperties other)
        {
            properties.Clear();
            CopyFrom(other, false);
            Dirty = false;
        }
        public void UpdateFrom(AnnotationProperties other)
        {
            CopyFrom(other, true);
        }
        internal void SetProperty<T>(string property, T value, object change)
        {
            UpdateProperty<T>(property, value, change, false);
        }
        private void CopyFrom(AnnotationProperties other, bool onlyDirties)
        {
            foreach (string property in other.Available)
            {
                if (!onlyDirties || other.properties[property].Dirty)
                {
                    this.properties.Remove(property);
                    this.properties.Add(property, new Property(other.properties[property].Value, other.properties[property].Type, true, other.properties[property].ChangeData));
                }
            }
        }
        private void UpdateProperty<T>(string property, T value)
        {
            UpdateProperty<T>(property, value, properties.ContainsKey(property) ? properties[property].ChangeData : null, true);
        }
        private void UpdateProperty<T>(string property, T value, object change, bool sendUpdate)
        {
            properties.Remove(property);
            properties.Add(property, new Property(value, typeof(T), true, change));

            if (sendUpdate)
                OnUpdate(change);
        }
        private T GetProperty<T>(string property, T defVal)
        {
            if (!properties.ContainsKey(property))
                properties.Add(property, new Property(defVal, typeof(T), true, null));
            return (T)properties[property].Value;
        }
        private void OnUpdate(object changeData)
        {
            if (Update != null)
                Update(changeData);
        }
        #endregion

        #region Members
        public delegate void UpdateCallback(object changeData);
        public event UpdateCallback Update;

        private bool updateAlways = false;

        private Dictionary<string, Property> properties = new Dictionary<string, Property>();
        #endregion

        #region Subtypes
        private struct Property
        {
            public Property(object value, Type type, bool dirty, object change)
            {
                Value = value;
                Type = type;
                Dirty = dirty;
                ChangeData = change;
            }
            public object Value;
            public Type Type;
            public bool Dirty;
            public object ChangeData;
        }
        #endregion
    }
}
