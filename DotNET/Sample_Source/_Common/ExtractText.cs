using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Datalogics.PDFL;

/*
 * ===============================================================================
 * This class is intended to assist with operations common to
 * text extraction samples. The class contains methods to control types of words
 * found and what information is returned to the user.
 ===============================================================================
 *
 * Copyright (c) 2022-2023, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace ExtractTextNameSpace
{
    // This class represents the text info.
    public class TextObject
    {
        public string Text { get; set; }
    }

    public class DLColorSpace
    {
        public string Name { get; set; }
        public int NumComponents { get; set; }
    }

    public class DLColor
    {
        public IList<double> Value { get; set; }
        public DLColorSpace Space { get; set; }
    }
    public class DLStyle
    {
        public DLColor Color { get; set; }
        public double FontSize { get; set; }
        public string FontName { get; set; }
    }
    public class DLStyleTransition
    {
        public int CharIndex { get; set; }
        public DLStyle Style { get; set; }
    }

    // This class represents the text and details info.
    public class TextAndDetailsObject
    {
        public string Text { get; set; }
        public IList<Quad> CharQuads { get; set; }
        public IList<Quad> Quads { get; set; }
        public IList<DLStyleTransition> StyleList { get; set; }
    }

    // This class represents the AcroForm text info.
    public class AcroFormTextFieldObject
    {
        [JsonPropertyName("field-name")]
        public string AcroFormFieldName { get; set; }
        [JsonPropertyName("field-text")]
        public string AcroFormFieldText { get; set; }
    }

    // This class represents the Annotation text info.
    public class AnnotationTextObject
    {
        [JsonPropertyName("annotation-type")]
        public string AnnotationType { get; set; }
        [JsonPropertyName("annotation-text")]
        public string AnnotationText { get; set; }
    }

    public class ExtractText : IDisposable
    {
        private Document doc;

        private WordFinder wordFinder;
        private IList<Word> pageWords = null;

        public ExtractText(Document inputDoc)
        {
            doc = inputDoc;
            // Use WordFinder with default settings to find words
            WordFinderConfig wordConfig = new WordFinderConfig();
            wordFinder = new WordFinder(doc, WordFinderVersion.Latest, wordConfig);
        }

        public void Dispose()
        {
            wordFinder.Dispose();
        }

        //===============================================================================
        // GetText() - Gets the text for the entire document.
        //===============================================================================
        public List<TextObject> GetText()
        {
            List<TextObject> resultText = new List<TextObject>();

            for (int pageNum = 0; pageNum < doc.NumPages; ++pageNum)
            {
                List<TextObject> pageText = GetText(pageNum);
                resultText.AddRange(pageText);
            }
            return resultText;
        }

        //===============================================================================
        // GetText() - Gets the text on a specified page.
        //===============================================================================
        public List<TextObject> GetText(int pageNum)
        {
            List<TextObject> pageText = new List<TextObject>();

            pageWords = wordFinder.GetWordList(pageNum);

            foreach (Word wordInfo in pageWords)
            {
                TextObject textObject = new TextObject();
                textObject.Text = wordInfo.Text;
                pageText.Add(textObject);
            }
            // Release requested WordList
            for (int wordnum = 0; wordnum < pageWords.Count; ++wordnum)
            {
                pageWords[wordnum].Dispose();
            }

            return pageText;
        }
        //===============================================================================
        // GetTextAndDetails() - Gets the text and detail info for the entire document.
        //===============================================================================
        public List<TextAndDetailsObject> GetTextAndDetails()
        {
            List<TextAndDetailsObject> resultTextAndDetails = new List<TextAndDetailsObject>();

            for (int pageNum = 0; pageNum < doc.NumPages; ++pageNum)
            {
                List<TextAndDetailsObject> pageText = GetTextAndDetails(pageNum);
                resultTextAndDetails.AddRange(pageText);
            }
            return resultTextAndDetails;
        }

        //===============================================================================
        // GetTextAndDetails() - Gets the text and detail info for a specific page.
        //===============================================================================
        public List<TextAndDetailsObject> GetTextAndDetails(int pageNum)
        {
            List<TextAndDetailsObject> resultTextAndDetails = new List<TextAndDetailsObject>();

            pageWords = wordFinder.GetWordList(pageNum);

            foreach (Word wordInfo in pageWords)
            {
                TextAndDetailsObject textObject = new TextAndDetailsObject();
                textObject.Text = wordInfo.Text;
                textObject.CharQuads = wordInfo.CharQuads;
                textObject.Quads = wordInfo.Quads;
                List<DLStyleTransition> trans = new List<DLStyleTransition>();
                foreach (StyleTransition st in wordInfo.StyleTransitions)
                {
                    DLStyleTransition dlStyleTrans =  new DLStyleTransition();
                    dlStyleTrans.CharIndex = st.CharIndex;
                    DLStyle dlStyle= new DLStyle();
                    dlStyle.FontSize = st.Style.FontSize;
                    dlStyle.FontName = st.Style.FontName;
                    dlStyle.Color = new DLColor();
                    dlStyle.Color.Space = new DLColorSpace();
                    dlStyle.Color.Space.Name = st.Style.Color.Space.Name;
                    dlStyle.Color.Space.NumComponents = st.Style.Color.Space.NumComponents;
                    dlStyle.Color.Value = new List<double>();
                    foreach (double val in st.Style.Color.Value )
                    {
                        dlStyle.Color.Value.Add(val);
                    }
                    dlStyleTrans.Style = dlStyle;
                    trans.Add(dlStyleTrans);
                }
                textObject.StyleList = trans;
                resultTextAndDetails.Add(textObject);
            }
            // Release requested WordList
            for (int wordnum = 0; wordnum < pageWords.Count; ++wordnum)
            {
                pageWords[wordnum].Dispose();
            }
            return resultTextAndDetails;
        }

        //===============================================================================
        // GetAcroFormFieldData() - Gets the AcroForm field data.
        //===============================================================================
        public List<AcroFormTextFieldObject> GetAcroFormFieldData()
        {
            List<AcroFormTextFieldObject> resultAcroFormText = new List<AcroFormTextFieldObject>();

            PDFObject form_entry = doc.Root.Get("AcroForm");
            if (form_entry is PDFDict)
            {
                PDFDict form_root = (PDFDict)form_entry;

                PDFObject fields_entry = form_root.Get("Fields");
                if (fields_entry is PDFArray)
                {
                    PDFArray fields = (PDFArray)fields_entry;
                    for (int fieldIndex = 0; fieldIndex < fields.Length; ++fieldIndex)
                    {
                        PDFObject field_entry = fields.Get(fieldIndex);
                        EnumerateAcroFormField(field_entry, "", resultAcroFormText);
                    }
                }
            }
            return resultAcroFormText;
        }

        string GetAcroFormFieldText(PDFDict field)
        {
            PDFObject entry = field.Get("V");
            string svalue = "";
            if (entry is PDFString)
            {
                PDFString value = (PDFString)entry;
                svalue = value.Value;
            }
            return svalue;
        }
        void EnumerateAcroFormField(PDFObject field_entry, string prefix, List<AcroFormTextFieldObject> result)
        {
            string name_part;
            string field_name;
            string field_text;
            PDFObject entry;

            if (field_entry is PDFDict)
            {
                PDFDict field = (PDFDict)field_entry;
                entry = (PDFString)field.Get("T");
                if (entry is PDFString)
                {
                    PDFString t = (PDFString)entry;
                    name_part = t.Value;
                }
                else
                    return;

                if (prefix == "")
                {
                    field_name = name_part;
                }
                else
                {
                    // Concatenate field name for 'Kids'
                    field_name = string.Format("{0}.{1}", prefix, name_part);
                }

                // Recursively handle 'Kids'
                entry = field.Get("Kids");
                if (entry is PDFArray)
                {
                    PDFArray kids = (PDFArray)entry;
                    for (int kidIndex = 0; kidIndex < kids.Length; ++kidIndex)
                    {
                        PDFObject kid_entry = kids.Get(kidIndex);
                        EnumerateAcroFormField(kid_entry, field_name, result);
                    }
                }

                // We are at an end-node
                entry = field.Get("FT");
                if (entry is PDFName)
                {
                    PDFName field_type_name = (PDFName)entry;
                    if (field_type_name.Value == "Tx")
                    {
                        field_text = GetAcroFormFieldText(field);
                        AcroFormTextFieldObject textObject = new AcroFormTextFieldObject();
                        textObject.AcroFormFieldName = field_name;
                        textObject.AcroFormFieldText = field_text;
                        result.Add(textObject);
                    }
                }
            }
        }

        //===============================================================================
        // GetAnnotationText() - Gets the Annotation text.
        //===============================================================================
        public List<AnnotationTextObject> GetAnnotationText()
        {
            List<AnnotationTextObject> resultAnnotationText = new List<AnnotationTextObject>();

            for (int pageNum = 0; pageNum < doc.NumPages; ++pageNum)
            {
                Page page = doc.GetPage(pageNum);
                for (int annotNum = 0; annotNum < page.NumAnnotations; ++annotNum)
                {
                    Annotation annot = page.GetAnnotation(annotNum);
                    if (annot.Subtype == "Text" || annot.Subtype == "FreeText")
                    {
                        AnnotationTextObject textObject = new AnnotationTextObject();
                        textObject.AnnotationType = annot.GetType().Name;
                        textObject.AnnotationText = annot.Contents;
                        resultAnnotationText.Add(textObject);
                    }
                }
            }
            return resultAnnotationText;
        }
    }
}
