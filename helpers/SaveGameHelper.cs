using DiskCardGame;

namespace Infiniscryption.Helpers
{
    public static class SaveGameHelper
    {
        private const string SaveKey = "Infiniscryption";

        public static string GetValue(string key)
        {
            string keyVal = ProgressionData.Data.introducedConsumables.Find(str => str.StartsWith($"{SaveKey}.{key}"));
            if (keyVal != default(string))
                return keyVal.Replace($"{SaveKey}.{key}=", "");
            return default(string);
        }

        public static void SetValue(string key, string value)
        {
            for (int i = 0; i < ProgressionData.Data.introducedConsumables.Count; i++)
            {
                if (ProgressionData.Data.introducedConsumables[i].StartsWith($"{SaveKey}.{key}"))
                {
                    ProgressionData.Data.introducedConsumables[i] = $"{SaveKey}.{key}={value}";
                    return;
                }
            }
            ProgressionData.Data.introducedConsumables.Add($"{SaveKey}.{key}={value}");
        }
    }
}