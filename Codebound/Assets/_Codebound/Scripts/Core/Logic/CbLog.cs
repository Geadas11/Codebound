using System;

namespace Codebound.Core
{
    public enum LogLevel { Info, Warning, Error }

    /// <summary>
    /// Module-tagged logging facade. The Unity layer installs a sink that forwards to Debug.Log;
    /// tests and tools can install their own.
    /// </summary>
    public static class CbLog
    {
        public static Action<LogLevel, string, string> Sink = (level, module, msg) =>
            Console.WriteLine($"[{level}] [{module}] {msg}");

        public static void Info(string module, string msg) => Sink?.Invoke(LogLevel.Info, module, msg);
        public static void Warn(string module, string msg) => Sink?.Invoke(LogLevel.Warning, module, msg);
        public static void Error(string module, string msg) => Sink?.Invoke(LogLevel.Error, module, msg);
    }
}
