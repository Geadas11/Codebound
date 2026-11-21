using System;
using System.Collections.Generic;
using System.Globalization;

namespace Codebound.Core
{
    /// <summary>
    /// UI text tables. English is written inline at each call site (and in en.json, which the tests check against);
    /// other languages come from StreamingAssets/Localization/&lt;code&gt;.json. Programming-language syntax and the
    /// API reference are never translated (Prompt §18).
    /// </summary>
    public static class Loc
    {
        public static readonly (string code, string native)[] Languages =
        {
            ("en", "English"), ("pt-PT", "Português (Portugal)"), ("pt-BR", "Português (Brasil)"), ("es", "Español"),
            ("fr", "Français"), ("de", "Deutsch"), ("it", "Italiano"), ("ja", "日本語"), ("zh-Hans", "简体中文"), ("ko", "한국어"),
        };

        static Dictionary<string, string> table = new Dictionary<string, string>();
        public static string Current { get; private set; } = "en";

        /// <summary>True for languages whose glyphs need a CJK-capable font.</summary>
        public static bool NeedsCjkFont => Current == "ja" || Current == "zh-Hans" || Current == "ko";

        /// <summary>Loads a language table (JSON object of key → text). Bad input leaves English in place.</summary>
        public static bool Load(string code, string json)
        {
            var t = new Dictionary<string, string>();
            if (code != "en")
            {
                try
                {
                    if (!(Json.Parse(json ?? "{}") is Dictionary<string, object> d)) return false;
                    foreach (var kv in d) if (kv.Value is string s && s.Length > 0) t[kv.Key] = s;
                }
                catch (FormatException) { return false; }
            }
            table = t;
            Current = code;
            return true;
        }

        /// <summary>Translated text for a key, falling back to the given English; args fill {0}, {1}...</summary>
        public static string T(string key, string english, params object[] args)
        {
            string s = table.TryGetValue(key, out var v) ? v : english;
            if (args == null || args.Length == 0) return s;
            try { return string.Format(CultureInfo.InvariantCulture, s, args); }
            catch (FormatException) { return string.Format(CultureInfo.InvariantCulture, english, args); }   // a bad translation never breaks the UI
        }

        public static string Building(BuildingDef d) => T("building." + d.Id, d.DisplayName);
        public static string Item(string id) => T("item." + id, ItemDatabase.Get(id).DisplayName);
        public static string Tech(Tech t) => T("tech." + t.Id, t.Name);
    }
}
