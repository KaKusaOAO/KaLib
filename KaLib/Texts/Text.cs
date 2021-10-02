﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using KaLib.Utils.Extensions;

namespace KaLib.Texts
{
    public abstract class Text : IText
    {
        public ICollection<IText> Extra { get; set; } = new List<IText>();
        public IText Parent { get; set; }
        public TextColor Color { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public bool Obfuscated { get; set; }
        public bool Underline { get; set; }
        public bool Strikethrough { get; set; }
        public bool Reset { get; set; }

        public bool ShouldSerializeExtra() => Extra.Count > 0;
        public TextColor ParentColor
        {
            get
            {
                if (Parent == null) return Environment.OSVersion.Platform == PlatformID.Win32NT ? TextColor.Gray : TextColor.White;
                return Parent.Color ?? Parent.ParentColor;
            }
        }

        internal virtual string ToAscii()
        {
            string extra = "";
            foreach (Text e in Extra)
            {
                extra += e.ToAscii() + (Color ?? ParentColor).ToAsciiCode();
            }
            return extra;
        }

        public virtual string ToPlainText()
        {
            string extra = "";
            foreach (Text e in Extra)
            {
                extra += e.ToPlainText();
            }
            return extra;
        }

        public static Text RepresentType(Type t)
        {
            return LiteralText.Of(t.Namespace + ".").SetColor(TextColor.DarkGray)
                    .AddExtra(LiteralText.Of(t.Name).SetColor(TextColor.Gold));
        }
    }

    public abstract class Text<S> : Text, IText<S> where S : Text<S>
    {
        protected abstract S ResolveThis();
        public abstract S Clone();

        public S AddExtra(params IText[] texts)
        {
            S t = ResolveThis();
            foreach (IText text in texts)
            {
                Extra.Add(text);
                if (text is Text t2)
                {
                    t2.Parent = t;
                }
            }
            return t;
        }

        public S SetColor(TextColor color)
        {
            S t = ResolveThis();
            Color = color;
            return t;
        }

        public S Format(TextFormatFlag flags)
        {
            S t = ResolveThis();
            Bold = flags.HasFlag(TextFormatFlag.Bold);
            Italic = flags.HasFlag(TextFormatFlag.Italic);
            Obfuscated = flags.HasFlag(TextFormatFlag.Obfuscated);
            Strikethrough = flags.HasFlag(TextFormatFlag.Strikethrough);
            Underline = flags.HasFlag(TextFormatFlag.Underline);
            Reset = flags.HasFlag(TextFormatFlag.Reset);
            return t;
        }
    }

    public class LiteralText : Text<LiteralText>
    {
        public string Text { get; set; }

        protected override LiteralText ResolveThis() => this;

        public static LiteralText Of(string text)
        {
            LiteralText result = new LiteralText();
            result.Text = text;
            return result;
        }

        public override LiteralText Clone()
        {
            LiteralText result = Of(Text);
            result.AddExtra(result.Extra.ToArray());
            return result;
        }

        internal override string ToAscii()
        {
            string extra = base.ToAscii();
            string color = (Color ?? ParentColor).ToAsciiCode();
            return color + Text + extra;
        }

        public override string ToPlainText()
        {
            string extra = base.ToAscii();

            string result = "";
            for (int i = 0; i < Text.Length; i++)
            {
                string b = Text;
                if (b[i] == TextColor.ColorChar && TextColor.McCodes().ToList().IndexOf(b[i + 1]) > -1)
                {
                    i += 2;
                }
                else
                {
                    result += b[i];
                }
            }

            return result + extra;
        }
    }

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
            }
        }

        public TranslateText AddWith(params Text[] texts)
        {
            foreach (Text text in texts)
            {
                With.Add(text);
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
                    c = (counter++) + "";
                }
                offset += c.Length + 2 - m.Value.Length;
                fmt = fmt[..(m.Index + offset)] + "{" + c + "}" + fmt[(m.Index + offset + m.Value.Length)..];
            }
            return string.Format(fmt, obj);
        }

        internal override string ToAscii()
        {
            string extra = base.ToAscii();
            string color = (Color ?? ParentColor).ToAsciiCode();
            string[] withAscii = With.Map(text =>
            {
                return text.ToAscii() + color;
            }).ToArray();
            return color + Format(Translate, withAscii) + extra;
        }

        public override string ToPlainText()
        {
            string extra = base.ToAscii();

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

            string[] withAscii = With.Map(text =>
            {
                return text.ToPlainText();
            }).ToArray();

            return Format(result, withAscii) + extra;
        }
    }

    internal static class TextColorAsciiExtension
    {
        internal static string ToAsciiCode(this TextColor color) => AsciiColor.FromTextColor(color).ToAsciiCode();
    }

    internal sealed class AsciiColor
    {
        public char ColorCode { get; set; }
        public int Color { get; set; }
        public bool Bright { get; set; }

        private static Dictionary<char, AsciiColor> byCode = new Dictionary<char, AsciiColor>();

        public static readonly AsciiColor Black = new AsciiColor('0', 30);
        public static readonly AsciiColor DarkBlue = new AsciiColor('1', 30, true);
        public static readonly AsciiColor DarkGreen = new AsciiColor('2', 30, true);
        public static readonly AsciiColor DarkAqua = new AsciiColor('3', 30, true);
        public static readonly AsciiColor DarkRed = new AsciiColor('4', 30, true);
        public static readonly AsciiColor DarkPurple = new AsciiColor('5', 30, true);
        public static readonly AsciiColor Gold = new AsciiColor('6', 33);
        public static readonly AsciiColor Gray = new AsciiColor('7', 37);
        public static readonly AsciiColor DarkGray = new AsciiColor('8', 30, true);
        public static readonly AsciiColor Blue = new AsciiColor('9', 30, true);
        public static readonly AsciiColor Green = new AsciiColor('a', 32, true);
        public static readonly AsciiColor Aqua = new AsciiColor('b', 34, true);
        public static readonly AsciiColor Red = new AsciiColor('c', 31, true);
        public static readonly AsciiColor Purple = new AsciiColor('d', 31, true);
        public static readonly AsciiColor Yellow = new AsciiColor('e', 33, true);
        public static readonly AsciiColor White = new AsciiColor('f', 37, true);

        public const char ColorChar = '\u00a7';

        private AsciiColor(char code, int color, bool isBright = false)
        {
            ColorCode = code;
            Color = color;
            Bright = isBright;

            byCode.Add(code, this);
        }

        public static AsciiColor Of(char c)
        {
            try
            {
                return byCode[c];
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException($"Color of '{c}' is not defined");
            }
        }

        public static AsciiColor FromTextColor(TextColor color)
        {
            TextColor closest = color.ToNearestPredefinedColor();
            char code = closest.ToString()[1..][0];
            return Of(code);
        }

        public string ToAsciiCode()
        {
            string brightPrefix = Bright ? "1;" : "";
            return $"\u001b[{brightPrefix}{Color}m";
        }

        public string ToMcCode()
        {
            return $"{ColorChar}{ColorCode.ToString().ToLower()}";
        }

        public static char[] McCodes()
        {
            return "0123456789abcdef".ToCharArray();
        }
    }
}
