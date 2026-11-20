using System;
using System.Collections;
using System.Linq;
using Codebound.Core;
using UnityEngine;

namespace Codebound.Game
{
    /// <summary>
    /// `Codebound.exe -selftest`: smoke test for release builds. Builds a powered computer, runs a JavaScript and a
    /// TypeScript program in the bundled runtime, logs "[SELFTEST] PASS/FAIL" to Player.log and quits (exit code 0/1).
    /// </summary>
    public sealed class SelfTest : MonoBehaviour
    {
        public static bool Requested => Environment.GetCommandLineArgs().Contains("-selftest");

        string failure;

        IEnumerator Start()
        {
            yield return null;
            Debug.Log("[SELFTEST] start; node = " + ProgramManager.NodeExecutable() + "; python = " + ProgramManager.PythonExecutable() + "; runtime = " + ProgramManager.RuntimeDir);
            var s = GameBootstrap.Session; var g = s.State;
            var pm = FindAnyObjectByType<ProgramManager>();
            var sp = g.World.Generator.FindSpawn();
            PlacedBuilding gen = null, pc = null;
            for (int r = 2; r < 60 && pc == null; r++)
                for (int x = -r; x <= r && pc == null; x++)
                {
                    var c = new Cell(sp.X + x, sp.Y + r);
                    if (!g.CanPlace(BuildingDatabase.Get("generator"), c, Rotation.R0) ||
                        !g.CanPlace(BuildingDatabase.Get("computer"), new Cell(c.X + 2, c.Y), Rotation.R0)) continue;
                    gen = g.Grid.Place(BuildingDatabase.Get("generator"), c, Rotation.R0);
                    pc = g.Grid.Place(BuildingDatabase.Get("computer"), new Cell(c.X + 2, c.Y), Rotation.R0);
                }
            if (pc == null) { Finish("no room to place test machines"); yield break; }
            gen.State.TryInsert(ItemDatabase.Coal, 5);
            var cs = (ComputerState)pc.State;

            yield return Run(pm, pc, cs, "javascript", "log('selftest-js', 6 * 7, energy.available());", "selftest-js 42 true");
            if (failure == null)
                yield return Run(pm, pc, cs, "typescript", "const n: number = 40 + 2;\nlog('selftest-ts', n);", "selftest-ts 42");
            if (failure == null)
                yield return Run(pm, pc, cs, "python", "import math\nlog('selftest-py', int(math.sqrt(1764)), energy.available())", "selftest-py 42 True");
            if (failure == null)
                yield return Run(pm, pc, cs, "sql", "-- @once\nSELECT 'selftest-sql' AS t, 6 * 7 AS answer, available FROM energy;",
                    "selftest-sql | 42");
            Finish(failure);
        }

        IEnumerator Run(ProgramManager pm, PlacedBuilding pc, ComputerState cs, string lang, string code, string expect)
        {
            cs.Language = lang; cs.Code = code; cs.Log.Clear();
            if (!pm.StartProgram(pc)) { failure = $"{lang}: could not start: {cs.LastError}"; yield break; }
            float t0 = Time.realtimeSinceStartup;
            while (pm.IsRunning(pc) && Time.realtimeSinceStartup - t0 < 30f) yield return null;
            Debug.Log($"[SELFTEST] {lang} log: " + string.Join(" | ", cs.Log));
            if (!cs.Log.Exists(l => l.Contains(expect))) failure = $"{lang}: expected '{expect}', error: {cs.LastError ?? pm.RuntimeError ?? "timeout"}";
        }

        void Finish(string fail)
        {
            Debug.Log(fail == null ? "[SELFTEST] PASS" : "[SELFTEST] FAIL: " + fail);
            Application.Quit(fail == null ? 0 : 1);
        }
    }
}
