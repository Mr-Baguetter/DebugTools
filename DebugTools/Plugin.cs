global using Logger = LabApi.Features.Console.Logger;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DebugTools
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "DebugTools";
        public override string Description => "Provides a set of debug tools.";

        public override Version RequiredApiVersion => LabApi.Features.LabApiProperties.CurrentVersion;

        public override LoadPriority Priority => LoadPriority.Low;

        public override Version Version { get; } = new(1, 0, 0);

        public override string Author => "Mr. Baguetter";

        public static Plugin Instance;

        public Assembly Assembly => Assembly.GetExecutingAssembly();

        public override void Enable()
        {
            Instance = this;
        }

        public override void Disable()
        {
            Instance = null;
        }
    }
}
