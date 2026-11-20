using System;
using System.Collections.Generic;

namespace Codebound.Core
{
    /// <summary>
    /// Minimal localization lookup. Every player-visible string goes through a key.
    /// Tables are registered per language code (e.g. "pt-PT", "en"); missing keys fall back
    /// to the fallback language, then to the key itself so missing strings are visible.
    /// Programming-language syntax is never localized.
    /// </summary>
    public static class Loc
    {
        public const string FallbackLanguage = "en";
        private static readonly Dictionary<string, Dictionary<string, string>> Tables =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        public static string Language { get; set; } = "pt-PT";

        public static event Action LanguageChanged;

        public static void SetLanguage(string code)
        {
            Language = code;
            LanguageChanged?.Invoke();
        }

        public static void Register(string language, IDictionary<string, string> entries)
        {
            if (!Tables.TryGetValue(language, out var table))
            {
                table = new Dictionary<string, string>();
                Tables[language] = table;
            }
            foreach (var kv in entries) table[kv.Key] = kv.Value;
        }

        public static bool Has(string key) =>
            (Tables.TryGetValue(Language, out var t) && t.ContainsKey(key)) ||
            (Tables.TryGetValue(FallbackLanguage, out var f) && f.ContainsKey(key));

        public static string Get(string key)
        {
            if (key == null) return string.Empty;
            if (Tables.TryGetValue(Language, out var t) && t.TryGetValue(key, out var s)) return s;
            if (Tables.TryGetValue(FallbackLanguage, out var f) && f.TryGetValue(key, out s)) return s;
            return key;
        }

        public static string Format(string key, params object[] args) => string.Format(Get(key), args);

        public static IEnumerable<string> Languages => Tables.Keys;
    }
}
