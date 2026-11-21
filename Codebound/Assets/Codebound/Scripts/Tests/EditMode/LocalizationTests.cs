using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class LocalizationTests
    {
        static Dictionary<string, object> Table(string code) =>
            (Dictionary<string, object>)Json.Parse(File.ReadAllText(Path.Combine("Assets", "StreamingAssets", "Localization", code + ".json")));

        [TearDown] public void Reset() => Loc.Load("en", null);

        [Test]
        public void EveryLanguage_HasTheEnglishKeys_AndSamePlaceholders()
        {
            var en = Table("en");
            foreach (var (code, _) in Loc.Languages)
            {
                var t = Table(code);
                CollectionAssert.AreEquivalent(en.Keys, t.Keys, code);
                foreach (var kv in en)
                {
                    var s = (string)t[kv.Key];
                    Assert.IsNotEmpty(s, code + " " + kv.Key);
                    Assert.AreEqual(Regex.Matches((string)kv.Value, @"\{\d\}").Count, Regex.Matches(s, @"\{\d\}").Count, code + " " + kv.Key);
                }
                Assert.IsTrue(Loc.Load(code, File.ReadAllText(Path.Combine("Assets", "StreamingAssets", "Localization", code + ".json"))));
            }
        }

        [Test]
        public void Lookup_FallsBackToEnglish_AndBadFormatNeverThrows()
        {
            Loc.Load("pt-PT", "{\"menu.resume\":\"Continuar\",\"x\":\"bad {1}\"}");
            Assert.AreEqual("Continuar", Loc.T("menu.resume", "Resume"));
            Assert.AreEqual("Quit game", Loc.T("menu.quit", "Quit game"));
            Assert.AreEqual("ok 5", Loc.T("x", "ok {0}", 5));
            Assert.IsFalse(Loc.Load("fr", "not json"));
            Assert.AreEqual("pt-PT", Loc.Current, "bad table keeps the previous one");
        }
    }
}
