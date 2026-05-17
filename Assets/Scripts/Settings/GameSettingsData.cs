using System;
using UnityEngine;

namespace Core.Settings
{
    [Serializable]
    public class GameSettingsData
    {
        public int Version = 1;
        public float MasterVolume = 1f;
        public float MusicVolume = 1f;
        public float SfxVolume = 1f;
        public string LocaleCode = string.Empty;

        public void Normalize()
        {
            MasterVolume = Mathf.Clamp01(MasterVolume);
            MusicVolume = Mathf.Clamp01(MusicVolume);
            SfxVolume = Mathf.Clamp01(SfxVolume);
            LocaleCode ??= string.Empty;
        }

        public GameSettingsData Clone()
        {
            return new GameSettingsData
            {
                Version = Version,
                MasterVolume = MasterVolume,
                MusicVolume = MusicVolume,
                SfxVolume = SfxVolume,
                LocaleCode = LocaleCode
            };
        }
    }
}
