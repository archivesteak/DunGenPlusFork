using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunGenPlus {
  internal class PluginConfig {

    public static ConfigEntry<bool> EnableDevDebugTools;

    public static void SetupConfig(ConfigFile cfg) {
      EnableDevDebugTools = cfg.Bind(new ConfigDefinition("Dev", "Enable Dev Debug Tools"), false, new ConfigDescription("If enabled, allows the dev debug tools to be usable in the ship.\n\nPress LeftAlt + M to activate."));
    }

  }
}
