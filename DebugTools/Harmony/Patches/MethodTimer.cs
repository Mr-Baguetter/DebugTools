using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using HarmonyLib;
using LabApi.Features.Wrappers;
using LabApi.Loader;

namespace DebugTools.Commands.Patches
{
    public static class MethodTimer
    {
        private static readonly StringBuilder sb = new();

        private static readonly Dictionary<string, Harmony> harmonyInstances = [];
        private static readonly Dictionary<string, LabApi.Loader.Features.Plugins.Plugin> patchedPlugins = new();

        public static bool IsPatched => patchedPlugins.Count > 0;

        public static IReadOnlyCollection<string> PatchedPluginNames => patchedPlugins.Keys;

        public static bool PatchPlugin(string pluginName)
        {
            if (patchedPlugins.ContainsKey(pluginName))
            {
                Logger.Debug($"Plugin '{pluginName}' is already patched.");
                return false;
            }

            var pluginKey = PluginLoader.Plugins.Keys.FirstOrDefault(p => p.Name == pluginName);
            if (pluginKey == null)
            {
                Logger.Debug($"Plugin '{pluginName}' not found.");
                return false;
            }

            if (!PluginLoader.Plugins.TryGetValue(pluginKey, out Assembly assembly))
            {
                Logger.Debug($"Failed to get assembly for plugin '{pluginName}'.");
                return false;
            }

            string harmonyId = $"debugtools-methodtimer-{pluginName}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
            Harmony harmony = new(harmonyId);

            int patchedMethodCount = 0;

            foreach (Type type in assembly.GetTypes())
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
                        patchedMethodCount++;
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug($"Failed to patch {type.Name}.{method.Name}: {ex.Message}");
                    }
                }
            }

            harmonyInstances[pluginName] = harmony;
            patchedPlugins[pluginName] = pluginKey;

            Logger.Debug($"MethodTimer profiling enabled for plugin '{pluginName}' ({patchedMethodCount} methods patched).");
            return true;
        }

        public static bool UnpatchPlugin(string pluginName)
        {
            if (!patchedPlugins.ContainsKey(pluginName))
            {
                Logger.Debug($"Plugin '{pluginName}' is not currently patched.");
                return false;
            }

            if (harmonyInstances.TryGetValue(pluginName, out Harmony harmony))
            {
                harmony.UnpatchAll(harmony.Id);
                harmonyInstances.Remove(pluginName);
                patchedPlugins.Remove(pluginName);

                Logger.Debug($"MethodTimer profiling disabled for plugin '{pluginName}'.");
                return true;
            }

            return false;
        }

        public static void UnpatchAll()
        {
            var pluginNames = patchedPlugins.Keys.ToList();

            foreach (string pluginName in pluginNames)
                UnpatchPlugin(pluginName);

            Logger.Debug("MethodTimer profiling has been disabled for all plugins.");
        }

        public static void Prefix(out Stopwatch __state)
        {
            __state = Stopwatch.StartNew();
        }

        public static void Postfix(Stopwatch __state, MethodBase __originalMethod)
        {
            __state.Stop();
            if (__state.Elapsed.TotalMilliseconds > 0.3)
            {
                var localSb = new StringBuilder();
                localSb.AppendLine();
                localSb.AppendLine("Thread | RoundTime | TPS | Max TPS | MethodInfo");
                localSb.AppendLine($"{Thread.CurrentThread.ManagedThreadId} - {Round.Duration.TotalSeconds} - {Server.Tps} - {Server.MaxTps} - {__originalMethod.DeclaringType}.{__originalMethod.Name} took {__state.Elapsed.TotalMilliseconds:F2} ms");
                Logger.Debug(localSb);
            }
        }
    }
}