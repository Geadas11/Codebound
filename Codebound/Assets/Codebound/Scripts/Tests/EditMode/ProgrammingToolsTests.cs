using System.IO;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    /// <summary>Etapa 3 tools: extra files, the terminal, watches, run history and coding help.</summary>
    public class ProgrammingToolsTests
    {
        [Test]
        public void FileNames_AreValidated()
        {
            Assert.IsTrue(ComputerState.ValidFileName("utils.js"));
            Assert.IsTrue(ComputerState.ValidFileName("my_lib.h"));
            Assert.IsFalse(ComputerState.ValidFileName("main"));
            Assert.IsFalse(ComputerState.ValidFileName("../x.js"));
            Assert.IsFalse(ComputerState.ValidFileName("1abc.py"));
            Assert.IsFalse(ComputerState.ValidFileName("a b.js"));
            Assert.IsFalse(ComputerState.ValidFileName(null));
        }

        [Test]
        public void Terminal_ManagesFiles_AndRespectsTheLimit()
        {
            var cs = new ComputerState { Code = "log(1)" };
            Assert.AreEqual("created utils.js", Terminal.Execute(cs, "new utils.js", false).output);
            StringAssert.Contains("already exists", Terminal.Execute(cs, "new utils.js", false).output);
            StringAssert.Contains("file names", Terminal.Execute(cs, "new ../evil", false).output);
            StringAssert.Contains("utils.js", Terminal.Execute(cs, "ls", false).output);
            Assert.AreEqual("log(1)", Terminal.Execute(cs, "cat main", false).output);
            for (int i = 0; i < ComputerState.MaxFiles; i++) Terminal.Execute(cs, $"new f{i}.js", false);
            Assert.AreEqual(ComputerState.MaxFiles, cs.Files.Count);
            Assert.AreEqual("removed utils.js", Terminal.Execute(cs, "rm utils.js", false).output);
            Assert.IsFalse(cs.Files.ContainsKey("utils.js"));
        }

        [Test]
        public void Terminal_Commands_MapToActions()
        {
            var cs = new ComputerState();
            Assert.AreEqual(Terminal.Action.Run, Terminal.Execute(cs, "run", false).action);
            Assert.AreEqual(Terminal.Action.None, Terminal.Execute(cs, "run", true).action);
            Assert.AreEqual(Terminal.Action.Stop, Terminal.Execute(cs, "stop", true).action);
            Assert.AreEqual(Terminal.Action.Debug, Terminal.Execute(cs, "debug", false).action);
            Assert.AreEqual(Terminal.Action.Clear, Terminal.Execute(cs, "clear", false).action);
            Terminal.Execute(cs, "lang python", false);
            Assert.AreEqual("python", cs.Language);
            Terminal.Execute(cs, "lang cobol", false);
            Assert.AreEqual("python", cs.Language);
            Terminal.Execute(cs, "watch items[0].count", false);
            CollectionAssert.AreEqual(new[] { "items[0].count" }, cs.Watches);
            Terminal.Execute(cs, "unwatch items[0].count", false);
            Assert.IsEmpty(cs.Watches);
        }

        [Test]
        public void Terminal_Snippets_ShowExpressionValues()
        {
            var cs = new ComputerState { Language = "python" };
            var r = Terminal.Execute(cs, "2 + 3", false);
            Assert.AreEqual(Terminal.Action.Snippet, r.action);
            Assert.AreEqual("print(2 + 3)", r.snippet);
            Assert.AreEqual("x = 1", Terminal.Snippet("python", "x = 1"));
            Assert.AreEqual("log(machines.list());", Terminal.Snippet("javascript", "machines.list()"));
            Assert.AreEqual("Log(1 + 1);", Terminal.Snippet("csharp", "1 + 1"));
            StringAssert.StartsWith("-- @once", Terminal.Snippet("sql", "SELECT 1"));
            Assert.IsNull(Terminal.Snippet("c", "1 + 1"));
            cs.Language = "cpp";
            Assert.AreEqual(Terminal.Action.None, Terminal.Execute(cs, "1 + 1", false).action);
            Assert.AreEqual(Terminal.Action.None, Terminal.Execute(new ComputerState(), "1 + 1", true).action, "no snippets while running");
        }

        [Test]
        public void History_IsCapped()
        {
            var cs = new ComputerState();
            for (int i = 0; i < ComputerState.MaxHistory + 5; i++) cs.Record(new RunRecord { When = "t" + i, Language = "javascript", Result = "done" });
            Assert.AreEqual(ComputerState.MaxHistory, cs.History.Count);
            Assert.AreEqual("t" + (ComputerState.MaxHistory + 4), cs.History[cs.History.Count - 1].When);
        }

        [Test]
        public void FilesWatchesAndHistory_SurviveSaveAndLoad()
        {
            var g = new GameState(1);
            var s = g.World.Generator.FindSpawn();
            PlacedBuilding pc = null;
            for (int x = 1; pc == null && x < 40; x++) pc = g.Grid.Place(BuildingDatabase.Get("computer"), new Cell(s.X + x, s.Y), Rotation.R0);
            var cs = (ComputerState)pc.State;
            cs.Files["utils.js"] = "module.exports = { twice: x => 2 * x };\n\"quoted\" \\ back";
            cs.Watches.Add("ore.count");
            cs.Record(new RunRecord { When = "12:00:00", Language = "javascript", Result = "error", Message = "line 3: boom", Seconds = 1.5f, Calls = 7 });
            var sw = new StringWriter(); SaveSystem.Write(g, sw);
            var loaded = (ComputerState)SaveSystem.Read(new StringReader(sw.ToString())).Grid.At(pc.Origin).State;
            Assert.AreEqual(cs.Files["utils.js"], loaded.Files["utils.js"]);
            CollectionAssert.AreEqual(cs.Watches, loaded.Watches);
            Assert.AreEqual(1, loaded.History.Count);
            Assert.AreEqual("error", loaded.History[0].Result);
            Assert.AreEqual("line 3: boom", loaded.History[0].Message);
            Assert.AreEqual(7, loaded.History[0].Calls);
            Assert.AreEqual(Encoding("log(1)") + Encoding(cs.Files["utils.js"]), new ComputerState { Code = "log(1)", Files = { ["utils.js"] = cs.Files["utils.js"] } }.TotalCodeBytes);
        }

        static int Encoding(string s) => System.Text.Encoding.UTF8.GetByteCount(s);

        [Test]
        public void Assist_GivesHintsPerLanguage_AndGuidedTips()
        {
            Assert.IsNotNull(Assist.Hint("python", "NameError: name 'x' is not defined"));
            Assert.IsNotNull(Assist.Hint("javascript", "ReferenceError: x is not defined"));
            StringAssert.Contains("power", Assist.Tip(false, 0, false, false, null).ToLowerInvariant());
            Assert.AreEqual(3, Assist.Levels.Length);
        }
    }
}
