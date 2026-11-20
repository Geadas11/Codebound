using System.Collections.Generic;
using System.IO;
using System.Linq;
using Codebound.Core;
using Codebound.Core.Hud;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class HudTests
    {
        [Test]
        public void Html_ParsesNestingAttributesEntitiesAndText()
        {
            var problems = new List<string>();
            var root = HtmlParser.Parse("<div id=\"main\" class=\"card big\">\n  <p>Iron &amp; coal: <b>{{inventory.coal}}</b></p>\n  <br/>\n</div>", problems);
            CollectionAssert.IsEmpty(problems);
            var div = root.Children.Single();
            Assert.AreEqual("div", div.Tag);
            Assert.AreEqual("main", div.Id);
            CollectionAssert.AreEqual(new[] { "card", "big" }, div.Classes.ToArray());
            var p = div.Children[0];
            Assert.AreEqual("p", p.Tag);
            Assert.AreEqual("Iron & coal: ", p.Children[0].Text);
            Assert.AreEqual("b", p.Children[1].Tag);
            Assert.AreEqual("br", div.Children[1].Tag);
            Assert.AreEqual(2, p.Line);
        }

        [Test]
        public void Html_ReportsMistakesWithLines_AndNeverThrows()
        {
            var problems = new List<string>();
            HtmlParser.Parse("<div>\n<p>hello\n</div>\n</span>\n<marquee>x</marquee>\n<div", problems);
            Assert.IsTrue(problems.Any(m => m.Contains("line 2") && m.Contains("<p>")), string.Join(" | ", problems));
            Assert.IsTrue(problems.Any(m => m.Contains("</span>")), string.Join(" | ", problems));
            Assert.IsTrue(problems.Any(m => m.Contains("marquee")), string.Join(" | ", problems));
            Assert.IsTrue(problems.Any(m => m.Contains("'<'")), string.Join(" | ", problems));
        }

        [Test]
        public void Css_CascadeBySpecificityOrderAndInline()
        {
            var problems = new List<string>();
            var root = HtmlParser.Parse("<div class=\"card\"><p id=\"t\" class=\"x\" style=\"font-size: 20px\">a</p><span>b</span></div>", problems);
            var rules = CssParser.Parse("/* c */ p { color: red; font-size: 10px }\n.card p { color: blue }\n#t { color: green }\np { color: yellow; blink: yes }\n.card span, b { color: #abc }", problems);
            Assert.AreEqual(1, problems.Count(m => m.Contains("'blink'") && m.Contains("line 4")), string.Join(" | ", problems));
            var p = root.Children[0].Children[0];
            var st = CssParser.Compute(p, rules, problems);
            Assert.AreEqual("green", st["color"], "id beats class+tag and later tag rules");
            Assert.AreEqual("20px", st["font-size"], "inline style wins");
            var span = root.Children[0].Children[1];
            Assert.AreEqual("#abc", CssParser.Compute(span, rules, problems)["color"], "descendant selector in a list");
        }

        [Test]
        public void CssValues_ColorsLengthsBoxes()
        {
            Assert.IsTrue(CssValue.TryColor("#ff8000", out var c)); Assert.AreEqual(1f, c.R); Assert.AreEqual(128 / 255f, c.G, 1e-4);
            Assert.IsTrue(CssValue.TryColor("#0f08", out c)); Assert.AreEqual(1f, c.G); Assert.AreEqual(0x88 / 255f, c.A, 1e-4);
            Assert.IsTrue(CssValue.TryColor("rgba(255, 0, 0, 0.5)", out c)); Assert.AreEqual(0.5f, c.A);
            Assert.IsTrue(CssValue.TryColor("Orange", out c));
            Assert.IsFalse(CssValue.TryColor("#12", out c));
            Assert.IsTrue(CssValue.TryLength("50%", out var l)); Assert.AreEqual(CssValue.Unit.Percent, l.Unit);
            Assert.IsTrue(CssValue.TryLength("12", out l)); Assert.AreEqual(12f, l.Value);
            Assert.IsTrue(CssValue.TryBox("4px 8px", out var box)); Assert.AreEqual(8f, box[3].Value);
            Assert.IsFalse(CssValue.TryBox("1px 2px 3px 4px 5px", out box));
        }

        [Test]
        public void Template_InterpolatesAndReportsMissingKeys()
        {
            var missing = new HashSet<string>();
            var vals = new Dictionary<string, string> { { "a.b", "7" } };
            string r = HudTemplate.Render("x={{ a.b }} y={{nope}}", k => vals.TryGetValue(k, out var v) ? v : null, missing);
            Assert.AreEqual("x=7 y=", r);
            CollectionAssert.AreEquivalent(new[] { "nope" }, missing);
        }

        [Test]
        public void Layout_ValuesValidated_AndSaveRoundTrip()
        {
            var g = new GameState(9);
            Assert.IsNull(g.Hud.SetValue("ore_rate", "12/s"));
            Assert.IsNotNull(g.Hud.SetValue("player.x", "1"), "built-in names are protected");
            Assert.IsNotNull(g.Hud.SetValue("bad key!", "1"));
            var p = g.Hud.Add(HudLayout.Starter());
            p.X = 33.5f; p.Collapsed = true; p.Css += "\n/* olá */";
            var sw = new StringWriter(); SaveSystem.Write(g, sw);
            var g2 = SaveSystem.Read(new StringReader(sw.ToString()));
            var p2 = g2.Hud.Panels.Single();
            Assert.AreEqual(33.5f, p2.X); Assert.IsTrue(p2.Collapsed);
            Assert.AreEqual(p.Html, p2.Html); Assert.AreEqual(p.Css, p2.Css); Assert.AreEqual("Status", p2.Title);
        }

        [Test]
        public void StarterPanel_ParsesWithoutProblems()
        {
            var problems = new List<string>();
            HtmlParser.Parse(HudLayout.StarterHtml, problems);
            CssParser.Parse(HudLayout.StarterCss, problems);
            CollectionAssert.IsEmpty(problems);
        }
    }
}
