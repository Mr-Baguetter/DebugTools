using CommandSystem;
using DebugTools.API.Interfaces;
using DebugTools.Commands.Patches;
using System.Collections.Generic;

namespace DebugTools.Commands.Debug
{
    internal class MethodTiming : ISubcommand
    {
        public string Name { get; } = "debugtiming";

        public string Description { get; } = "Shows the time that methods used to execute";

        public string VisibleArgs { get; } = "<Plugin Name>";

        public int RequiredArgsCount { get; } = 1;

        public string RequiredPermission { get; } = "debug.timing";

        public string[] Aliases { get; } = ["timing"];

        public bool Execute(List<string> args, ICommandSender sender, out string response)
        {            
            if (MethodTimer.isPatched)
            {
                MethodTimer.UnpatchAll();
                response = $"Unpatched";
                return true;
            }
            else
            {
                MethodTimer.PatchAssembly(string.Join(" ", args[0]));
                response = $"Patched";
                return true;
            }
        }
    }
}