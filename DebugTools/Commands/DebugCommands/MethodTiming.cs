using CommandSystem;
using DebugTools.API.Interfaces;
using DebugTools.Commands.Patches;
using System.Collections.Generic;
using System.Linq;

namespace DebugTools.Commands.Debug
{
    internal class MethodTiming : ISubcommand
    {
        public string Name { get; } = "debugtiming";
        public string Description { get; } = "Shows the time that methods used to execute";
        public string VisibleArgs { get; } = "<action> [plugin name]";
        public int RequiredArgsCount { get; } = 1;
        public string RequiredPermission { get; } = "debug.timing";
        public string[] Aliases { get; } = ["timing"];

        public bool Execute(List<string> args, ICommandSender sender, out string response)
        {
            string action = args[0].ToLower();

            switch (action)
            {
                case "patch":
                case "enable":
                case "on":
                    return HandlePatch(args.Skip(1).ToList(), out response);

                case "unpatch":
                case "disable":
                case "off":
                    return HandleUnpatch(args.Skip(1).ToList(), out response);

                case "list":
                case "status":
                    return HandleList(out response);

                case "unpatchall":
                case "clear":
                case "stop":
                    return HandleUnpatchAll(out response);

                case "help":
                case "?":
                    return HandleHelp(out response);

                default:
                    response = $"Error: Unknown action '{action}'. Use 'debugtiming help' for a list of commands.";
                    return false;
            }
        }

        private bool HandlePatch(List<string> pluginArgs, out string response)
        {
            if (pluginArgs.Count == 0)
            {
                response = "Error: No plugin name specified. Usage: debugtiming patch <plugin name>";
                return false;
            }

            string pluginName = string.Join(" ", pluginArgs);

            if (MethodTimer.PatchPlugin(pluginName))
            {
                response = $"Started profiling '{pluginName}'.";
                return true;
            }
            else
            {
                response = $"Failed to start profiling '{pluginName}' (already patched or not found).";
                return false;
            }
        }

        private bool HandleUnpatch(List<string> pluginArgs, out string response)
        {
            if (pluginArgs.Count == 0)
            {
                response = "Error: No plugin name specified. Usage: debugtiming unpatch <plugin name>";
                return false;
            }

            string pluginName = string.Join(" ", pluginArgs);

            if (MethodTimer.UnpatchPlugin(pluginName))
            {
                response = $"Stopped profiling '{pluginName}'.";
                return true;
            }
            else
            {
                response = $"Failed to stop profiling '{pluginName}' (not currently patched).";
                return false;
            }
        }

        private bool HandleList(out string response)
        {
            if (!MethodTimer.IsPatched)
            {
                response = "No plugins are currently being profiled.";
                return true;
            }

            var patchedPlugins = MethodTimer.PatchedPluginNames.ToList();
            response = $"Currently profiling {patchedPlugins.Count} plugin(s):\n" +
                      string.Join("\n", patchedPlugins.Select(p => $"• {p}"));
            return true;
        }

        private bool HandleUnpatchAll(out string response)
        {
            if (!MethodTimer.IsPatched)
            {
                response = "No plugins are currently being profiled.";
                return true;
            }

            var count = MethodTimer.PatchedPluginNames.Count;
            MethodTimer.UnpatchAll();
            response = $"Stopped profiling all {count} plugin(s).";
            return true;
        }

        private bool HandleHelp(out string response)
        {
            response = "DebugTiming Command Usage:\n\n" +
                      "Actions:\n" +
                      "  patch <plugin name>              - Start profiling specified plugin\n" +
                      "  unpatch <plugin name>            - Stop profiling specified plugin\n" +
                      "  list / status                    - Show currently profiled plugins\n" +
                      "  unpackall / clear / stop         - Stop profiling all plugins\n" +
                      "  help / ?                         - Show this help message\n\n" +
                      "Aliases for actions:\n" +
                      "  patch: enable, on\n" +
                      "  unpatch: disable, off\n\n" +
                      "Examples:\n" +
                      "  debugtiming patch MyPlugin\n" +
                      "  debugtiming unpatch MyPlugin\n" +
                      "  debugtiming list\n" +
                      "  debugtiming unpackall\n\n";
            return true;
        }
    }
}