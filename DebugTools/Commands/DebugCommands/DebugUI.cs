using CommandSystem;
using LabApi.Features.Wrappers;
using System;
using System.Collections.Generic;
using System.Text;
using DebugTools.API.Enums;
using DebugTools.API.Features.Components;
using DebugTools.API.Interfaces;
using UnityEngine;

namespace DebugTools.Commands.Debug
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class DebugUI : ISubcommand
    {
        public string Name { get; } = "ui";
        public string Description { get; } = "Enables a UI that shows information about a activated section.";
        public string VisibleArgs { get; } = "<DebugUISection> || <disable> <DebugUISection>";
        public int RequiredArgsCount { get; } = 0;
        public string RequiredPermission { get; } = "debug.ui";
        public string[] Aliases { get; } = ["interface", "debugui"];

        public bool Execute(List<string> arguments, ICommandSender sender, out string response)
        {
            if (!Player.TryGet(sender, out Player player))
            {
                response = "This command can only be executed by a player.";
                return false;
            }

            if (!player.GameObject.TryGetComponent<DebugUIComponent>(out var ui))
            {
                ui = player.GameObject.AddComponent<DebugUIComponent>();
                ui.Initialize(player);
            }

            if (arguments.Count == 0)
            {
                response = "Started Debug UI with default sections.";
                ui.SetSectionActive(DebugUISections.RaycastInfo, true);
                return true;
            }

            string arg = arguments[0].ToLower();

            if (arg == "list")
            {
                response = $"{string.Join(", ", Enum.GetNames(typeof(DebugUISections)))}";
                return true;
            }

            if (Enum.TryParse(arg, true, out DebugUISections section))
            {
                ui.SetSectionActive(section, true);
                response = $"Enabled debug UI section: {section}";
                return true;
            }

            if (arg == "disable" && arguments.Count > 1 && Enum.TryParse(arguments[1], true, out DebugUISections disableSection))
            {
                ui.SetSectionActive(disableSection, false);
                response = $"Disabled debug UI section: {disableSection}";
                return true;
            }

            response = $"Unknown section. Try: {string.Join(", ", Enum.GetNames(typeof(DebugUISections)))}, or disable PlayerInfo";
            return false;
        }
    }
}