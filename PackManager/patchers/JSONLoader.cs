using System;
using System.IO;
using System.Text;
using BepInEx;
using DiskCardGame;

namespace Infiniscryption.PackManagement.Patchers
{
    public static class JSONLoader
    {
        public static void LoadFromJSON()
        {
            foreach (string fileName in Directory.EnumerateFiles(Paths.PluginPath, "*.jlpk", SearchOption.AllDirectories))
            {
                try
                {
                    string pseudoGuid = fileName.Replace(Paths.PluginPath, "").Replace(Path.PathSeparator, '.');
                    string json = File.ReadAllText(fileName);
                    PackInfoJSON pack = SaveManager.FromJSON<PackInfoJSON>(json);
                    PackManager.Add(pseudoGuid, pack.Convert());
                }
                catch (Exception ex)
                {
                    PackPlugin.Log.LogError($"Error deserializing pack {fileName}: {ex.Message}");
                    PackPlugin.Log.LogError(ex);
                }
            }
        }
    }
}