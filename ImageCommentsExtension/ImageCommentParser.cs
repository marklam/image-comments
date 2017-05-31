using System.Globalization;

namespace LM.ImageComments.EditorComponent
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using System.Windows.Media;
    using System.Xml;
    using System.Xml.Linq;

    // TODO [?]: Could make this a non-static class and use instances, but ensure a new instance is created when content type of a view is changed.
    internal static class ImageCommentParser
    {
        private static Regex _xmlImageTagRegex;
        private const string xmlImageTagPattern = @"<image.*>";

        public class SupportedLanguage
        {
            public SupportedLanguage(string comment, string name, string alias = null)
            {
                Name = name;
                Alias = alias;
                Comment = comment;

                FindFirstComment = new Regex(Comment, RegexOptions.Compiled);
                FindFirstCharAfterComment = new Regex(Comment + @"\s*<image", RegexOptions.Compiled);
                FindImageComment = new Regex(Comment + @".*" + xmlImageTagPattern, RegexOptions.Compiled);
            }
            public string Name;
            public string Alias;
            public string Comment;
            public Regex FindFirstCharAfterComment;
            public Regex FindImageComment;
            public Regex FindFirstComment;
        }

        private static List<SupportedLanguage> _langs;


        //      private static Regex _csharpImageCommentRegex;
        //      private static Regex _csharpIndentRegex;
        //      private static Regex _vbImageCommentRegex;
        //      private static Regex _vbIndentRegex;
        //private static Regex _pythonImageCommentRegex;
        //private static Regex _pythonIndentRegex;



        // Initialize regex objects
        static ImageCommentParser()
        {
            _langs = new List< SupportedLanguage > ();
            _langs.Add(new SupportedLanguage("//", "C/C++", "CSharp"));
            _langs.Add(new SupportedLanguage("'", "Basic"));
            _langs.Add(new SupportedLanguage("#", "Python"));

            _xmlImageTagRegex = new Regex(xmlImageTagPattern, RegexOptions.Compiled);
        }

        public static SupportedLanguage GetLanguage(string contentTypeName)
        {
            if (string.IsNullOrWhiteSpace(contentTypeName))
                return null;

            foreach(SupportedLanguage l in _langs)
            {
                if (contentTypeName == l.Name || contentTypeName == l.Alias)
                    return l;
            }

            return null;
        }

        public static string GetLineCommentStart(string contentTypeName)
        {
            SupportedLanguage l = GetLanguage(contentTypeName);
            return l==null ? "" : l.Comment;
        }

        /// <summary>
        /// Tries to match Regex on input text
        /// </summary>
        /// <returns>Position in line at start of matched image comment. -1 if not matched</returns>
        public static int MatchImageTag(string contentTypeName, string lineText, out string matchedText)
        {
            SupportedLanguage l = GetLanguage(contentTypeName);
            if (l == null)
            {
                matchedText = "";
                return -1;
            }
            Match imageCommentMatch = l.FindImageComment.Match(lineText);
            Match indentMatch = l.FindFirstCharAfterComment.Match(lineText);

            matchedText = imageCommentMatch.Value;
            if (matchedText == "")
                return -1;
            int tagStart = indentMatch.Index + indentMatch.Length - 6;

            if (imageCommentMatch.Index >= tagStart - l.Comment.Length - 1)
                return imageCommentMatch.Index;

            return tagStart;
        }

        /// <summary>
        /// Tries to match comment Regex on input text
        /// </summary>
        /// <returns>The position of the comment end if it contains a comment, otherwise -1</returns>
        public static int MatchComment(string contentTypeName, string lineText)
        {
            SupportedLanguage l = GetLanguage(contentTypeName);
            if (l == null)
                return -1;

            Match indentMatch = l.FindFirstComment.Match(lineText);

            return indentMatch.Success ? indentMatch.Index + indentMatch.Length : -1;
        }

        /// <summary>
        /// Looks for well formed image comment in line of text and tries to parse parameters
        /// </summary>
        /// <param name="matchedText">Input: Line of text in editor window</param>
        /// <param name="imageUrl">Output: URL of image</param>
        /// <param name="imageScale">Output: Scale factor of image </param>
        /// <param name="ex">Instance of any exception generated. Null if function finished succesfully</param>
        /// <returns>Returns true if successful, otherwise false</returns>
        public static bool TryParse(string matchedText, out string imageUrl, out double imageScale, ref Color bgColor, out Exception exception)
        {
            exception = null;
            imageUrl = "";
            imageScale = 0; // See MyImage.cs for explanation of default value here
            
            // Try parse text
            if (matchedText != "")
            {
                string tagText = _xmlImageTagRegex.Match(matchedText).Value;
                try
                {
                    XElement imgEl = XElement.Parse(tagText);
                    XAttribute srcAttr = imgEl.Attribute("url");
                    if (srcAttr == null)
                    {
                        exception = new XmlException("url attribute not specified.");
                        return false;
                    }
                    imageUrl = srcAttr.Value;
                    XAttribute scaleAttr = imgEl.Attribute("scale");
                    if (scaleAttr != null)
                    {
                        double.TryParse(scaleAttr.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out imageScale);
                    }

                    XAttribute bgColorAttr = imgEl.Attribute("bgcolor");
                    if (bgColorAttr != null)
                    {
                        UInt32 color;
                        if( UInt32.TryParse(bgColorAttr.Value.Replace("#", "").Replace("0x", ""), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out color) )
                        {
                            bgColor.A = 255;
                            bgColor.B = (byte)color;
                            bgColor.G = (byte)(color>>8);
                            bgColor.R = (byte)(color>>16);
                        }
                    }
                    else
                    {
                        bgColor.A = 0;
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    exception = ex;
                    return false;
                }
            }
            else
            {
                exception = new XmlException("<image... /> tag not in correct format.");
                return false;
            }
        }
    }
}
