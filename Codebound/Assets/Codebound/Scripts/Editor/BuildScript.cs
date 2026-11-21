using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Codebound.EditorTools
{
    /// <summary>
    /// Player builds that also ship the JavaScript/TypeScript runtime's Node.js binary next to the scripts
    /// (StreamingAssets/Runtime/node). Node is copied at build time so the repository stays small.
    /// Node source: env CODEBOUND_NODE (path to node executable) or the `node` on PATH.
    /// </summary>
    public static class BuildScript
    {
        public const string WindowsOutput = "Builds/Windows/Codebound.exe";

        [MenuItem("Codebound/Build Windows (with Node runtime)")]
        public static void BuildWindows() => Build(BuildTarget.StandaloneWindows64, WindowsOutput, "node.exe");

        /// <summary>Entry point for `unity build --execute-method` and CI; returns the report summary.</summary>
        public static string Build(BuildTarget target, string output, string nodeName)
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes, locationPathName = output, target = target, options = BuildOptions.None,
            });
            var sum = report.summary;
            if (sum.result != BuildResult.Succeeded) throw new Exception($"Build failed: {sum.result}, {sum.totalErrors} errors");

            string dataDir = Path.Combine(Path.GetDirectoryName(output), Path.GetFileNameWithoutExtension(output) + "_Data");
            string nodeDir = Path.Combine(dataDir, "StreamingAssets", "Runtime", "node");
            Directory.CreateDirectory(nodeDir);
            string node = FindNode(nodeName);
            if (node == null) throw new Exception("Node.js not found: set CODEBOUND_NODE or put node on PATH");
            File.Copy(node, Path.Combine(nodeDir, nodeName), true);

            // Redistribution requires Node's LICENSE (MIT + bundled third-party notices) next to the binary.
            string license = new[]
            {
                Environment.GetEnvironmentVariable("CODEBOUND_NODE_LICENSE"),
                Path.Combine(Path.GetDirectoryName(node), "LICENSE"),
                Path.Combine(ToolsDir, "node", "LICENSE"),   // downloaded for the installed Node version
            }.FirstOrDefault(p => !string.IsNullOrEmpty(p) && File.Exists(p));
            if (license != null) File.Copy(license, Path.Combine(nodeDir, "LICENSE"), true);
            else Debug.LogWarning("[Build] Node.js LICENSE not found — OK for development builds, REQUIRED before any release (set CODEBOUND_NODE_LICENSE).");

            // Python runtime: the embeddable CPython in Tools/python (includes its own LICENSE.txt). Windows only for now.
            string py = Path.Combine(ToolsDir, "python");
            bool pyOk = target == BuildTarget.StandaloneWindows64 && File.Exists(Path.Combine(py, "python.exe"));
            if (pyOk) CopyDir(py, Path.Combine(dataDir, "StreamingAssets", "Runtime", "python"));
            else Debug.LogWarning("[Build] Python runtime not bundled (needs Tools/python for Windows) — Python programs will not run in this build.");

            // PHP: php.exe + php8.dll are enough with -n (no extensions); VC++ runtime copied app-locally (Microsoft redistributable)
            string php = Path.Combine(ToolsDir, "php"), phpBin = Path.Combine(dataDir, "StreamingAssets", "Runtime", "php", "bin");
            bool phpOk = target == BuildTarget.StandaloneWindows64 && File.Exists(Path.Combine(php, "php.exe"));
            if (phpOk)
            {
                Directory.CreateDirectory(phpBin);
                foreach (var f in new[] { "php.exe", "php8.dll", "license.txt" }) File.Copy(Path.Combine(php, f), Path.Combine(phpBin, f), true);
                foreach (var f in new[] { "vcruntime140.dll", "vcruntime140_1.dll" })
                {
                    string sys = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), f);
                    if (File.Exists(sys)) File.Copy(sys, Path.Combine(phpBin, f), true);
                }
            }
            else Debug.LogWarning("[Build] PHP runtime not bundled (needs Tools/php) — PHP programs will not run in this build.");

            // C#: the .NET 8 runtime from Tools/dotnet (copied from the Unity Editor's DotNetSdk) + the Roslyn host built into Tools/cshost/out
            string dn = Path.Combine(ToolsDir, "dotnet"), csOut = Path.Combine(ToolsDir, "cshost", "out"), csDir = Path.Combine(dataDir, "StreamingAssets", "Runtime", "csharp");
            bool csOk = target == BuildTarget.StandaloneWindows64 && File.Exists(Path.Combine(dn, "dotnet.exe")) && File.Exists(Path.Combine(csOut, "cshost.dll"));
            if (csOk)
            {
                CopyDir(dn, Path.Combine(csDir, "dotnet"));
                Directory.CreateDirectory(Path.Combine(csDir, "host"));
                foreach (var f in Directory.GetFiles(csOut)) if (!f.EndsWith(".pdb")) File.Copy(f, Path.Combine(csDir, "host", Path.GetFileName(f)), true);   // no satellite resource folders
            }
            else Debug.LogWarning("[Build] C# runtime not bundled (needs Tools/dotnet and a built Tools/cshost) — C# programs will not run in this build.");

            // C/C++: the parts of wasi-sdk the compiler needs (clang, wasm-ld, builtin headers, the wasm32-wasip1 sysroot) + licenses
            string wsdk = Path.Combine(ToolsDir, "wasi-sdk"), cDir = Path.Combine(dataDir, "StreamingAssets", "Runtime", "c", "sdk");
            bool cOk = target == BuildTarget.StandaloneWindows64 && File.Exists(Path.Combine(wsdk, "bin", "clang.exe"));
            if (cOk)
            {
                Directory.CreateDirectory(Path.Combine(cDir, "bin"));
                foreach (var f in new[] { "clang.exe", "wasm-ld.exe" }) File.Copy(Path.Combine(wsdk, "bin", f), Path.Combine(cDir, "bin", f), true);
                CopyDir(Path.Combine(wsdk, "lib", "clang"), Path.Combine(cDir, "lib", "clang"));
                CopyDir(Path.Combine(wsdk, "share", "wasi-sysroot", "include", "wasm32-wasip1"), Path.Combine(cDir, "share", "wasi-sysroot", "include", "wasm32-wasip1"));
                CopyDir(Path.Combine(wsdk, "share", "wasi-sysroot", "lib", "wasm32-wasip1"), Path.Combine(cDir, "share", "wasi-sysroot", "lib", "wasm32-wasip1"));
                Directory.CreateDirectory(Path.Combine(cDir, "share", "wasi-sysroot", "include", "c++", "v1"));   // the driver looks for it before the per-target libc++ headers
                CopyDir(Path.Combine(wsdk, "licenses"), Path.Combine(cDir, "licenses"));
            }
            else Debug.LogWarning("[Build] C/C++ compiler not bundled (needs Tools/wasi-sdk) — C and C++ programs will not run in this build.");

            string msg = $"[Build] {sum.result} {target} -> {output} ({sum.totalSize / (1024 * 1024)} MB player, {sum.totalTime.TotalSeconds:0}s); node: {node}; node license: {(license != null ? "yes" : "MISSING")}; python: {(pyOk ? "bundled" : "no")}; php: {(phpOk ? "bundled" : "no")}; csharp: {(csOk ? "bundled" : "no")}; c/c++: {(cOk ? "bundled" : "no")}";
            Debug.Log(msg);
            return msg;
        }

        static string ToolsDir => Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Tools"));

        static void CopyDir(string from, string to)
        {
            foreach (var dir in Directory.GetDirectories(from, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dir.Replace(from, to));
            Directory.CreateDirectory(to);
            foreach (var file in Directory.GetFiles(from, "*", SearchOption.AllDirectories))
                File.Copy(file, file.Replace(from, to), true);
        }

        static string FindNode(string nodeName)
        {
            string env = Environment.GetEnvironmentVariable("CODEBOUND_NODE");
            if (!string.IsNullOrEmpty(env) && File.Exists(env)) return env;
            foreach (var dir in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator))
            {
                try { string p = Path.Combine(dir.Trim('"'), nodeName); if (File.Exists(p)) return p; } catch (ArgumentException) { }
            }
            return null;
        }
    }
}
