using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using HarmonyLib;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using LabApi.Loader;

namespace DebugTools.Commands.Patches
{
    public static class MethodTimer
    {
        private static readonly StringBuilder sb = new();
        private static Harmony harmony;
        internal static bool isPatched = false;

        public static void PatchAssembly(string pluginName)
        {
            if (isPatched)
                return;

            PluginLoader.Plugins.TryGetValue(PluginLoader.Plugins.Keys.Where(p => p.Name == pluginName).FirstOrDefault(), out Assembly value);
            harmony = new Harmony($"com.ucs.uci_labapi-debugging-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");

            foreach (Type type in value.GetTypes())
            {
                if (type == typeof(MethodTimer))
                    continue;

                foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                {
                    if (method.IsAbstract || method.IsGenericMethodDefinition)
                        continue;

                    try
                    {
                        HarmonyMethod prefix = new(typeof(MethodTimer).GetMethod(nameof(Prefix)));
                        HarmonyMethod postfix = new(typeof(MethodTimer).GetMethod(nameof(Postfix)));
                        harmony.Patch(method, prefix, postfix);
                    }
                    catch
                    {

                    }
                }
            }

            isPatched = true;
            Logger.Debug("MethodTimer profiling has been enabled.");
        }

        public static void UnpatchAll()
        {
            if (!isPatched || harmony == null)
                return;

            harmony.UnpatchAll(harmony.Id);
            isPatched = false;
            Logger.Debug("MethodTimer profiling has been disabled.");
        }

        public static void Prefix(out Stopwatch __state)
        {
            sb.Clear();
            __state = Stopwatch.StartNew();
        }

        public static void Postfix(Stopwatch __state, MethodBase __originalMethod)
        {
            __state.Stop();
            if (__state.Elapsed.TotalMilliseconds > .3)
            {
                sb.AppendLine();
                sb.AppendLine("Thread | RoundTime | TPS | Max TPS | MethodInfo");
                sb.AppendLine($"{Thread.CurrentThread.ManagedThreadId} - {Round.Duration.TotalSeconds} - {Server.Tps} - {Server.MaxTps} - {__originalMethod.DeclaringType}.{__originalMethod.Name} took {__state.Elapsed.TotalMilliseconds:F2} ms");
                Logger.Debug(sb);
            }
        }
    }
}
