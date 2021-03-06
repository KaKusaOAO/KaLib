using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using KaLib.Utils.Extensions;

namespace KaLib.Texts
{
    public class TranslateText : Text<TranslateText>
    {
        public string Translate { get; set; }
        public ICollection<Text> With { get; set; } = new List<Text>();

        public TranslateText(string translate, params Text[] texts)
        {
            Translate = translate;
            foreach (Text t in texts)
            {
                With.Add(t);
                t.Parent = this;
            }
        }

        public TranslateText AddWith(params Text[] texts)
        {
            foreach (Text text in texts)
            {
                With.Add(text);
                text.Parent = this;
            }

            return this;
        }

        public static TranslateText Of(string format, params Text[] texts)
        {
            return new TranslateText(format, texts);
        }

        protected override TranslateText ResolveThis()
        {
            return this;
        }

        public override TranslateText Clone()
        {
            TranslateText result = Of(Translate, With.ToArray());
            result.AddExtra(result.Extra.ToArray());
            return result;
        }

        private string Format(string fmt, params object[] obj)
        {
            var offset = -1;
            var counter = 0;
            var matches = new Regex("%(?:(?:(\\d*?)\\$)?)s").Matches(fmt);
            foreach (Match m in matches)
            {
                var c = m.Groups[1].Value;
                if (c.Length == 0)
                {
                    c = counter++ + "";
                }

                offset += c.Length + 2 - m.Value.Length;
                // fmt = fmt[..(m.Index + offset)] + "{" + c + "}" + fmt[(m.Index + offset + m.Value.Length)..];
                // Need to use legacy syntax to support older versions of .NET
                fmt = fmt.Substring(0, m.Index + offset) + $"{{{c}}}" +
                      fmt.Substring(m.Index + offset + m.Value.Length);
            }

            var o = obj.ToList();
            for (var i = 0; i < counter; i++) o.Add("");
            return string.Format(fmt, o.ToArray());
        }

        internal override string ToAscii()
        {
            var extra = base.ToAscii();
            var color = (Color ?? ParentColor).ToAsciiCode();
            var withAscii = With.Map(text => text.ToAscii() + color).ToArray();
            return color + Format(Translate, withAscii) + extra;
        }

        public override string ToPlainText()
        {
            string extra = base.ToPlainText();

            string result = "";
            for (int i = 0; i < Translate.Length; i++)
            {
                string b = Translate;
                if (b[i] == TextColor.ColorChar && TextColor.McCodes().ToList().IndexOf(b[i + 1]) > -1)
                {
                    i += 2;
                }
                else
                {
                    result += b[i];
                }
            }

            string[] withAscii = With.Map(text => { return text.ToPlainText(); }).ToArray();

            return Format(result, withAscii) + extra;
        }
    }
}