#region license

// Copyright (c) 2021, andreakarasho
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. All advertising materials mentioning features or use of this software
//    must display the following acknowledgement:
//    This product includes software developed by andreakarasho - https://github.com/andreakarasho
// 4. Neither the name of the copyright holder nor the
//    names of its contributors may be used to endorse or promote products
//    derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#endregion

using Microsoft.Xna.Framework;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using uoAvox.Configuration.Json;
using uoAvox.Utility;

namespace uoAvox.Configuration
{
    [JsonSourceGenerationOptions(WriteIndented = true, GenerationMode = JsonSourceGenerationMode.Metadata)]
    [JsonSerializable(typeof(Settings), GenerationMode = JsonSourceGenerationMode.Metadata)]
    sealed partial class SettingsJsonContext : JsonSerializerContext { }

    internal sealed class Settings
    {
        public const string SETTINGS_FILENAME = "settings.json";

        public static Settings GlobalSettings = new();

        public static string CustomSettingsFilepath = null;

        [JsonPropertyName("username")] public string Username { get; set; } = "Administrator";

        [JsonPropertyName("password")] public string Password { get; set; } = Crypter.Encrypt("123");

        [JsonPropertyName("ip")] public string IP { get; set; } = "localhost";

        [JsonPropertyName("port"), JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] public ushort Port { get; set; } = 2593;

        [JsonPropertyName("gamedirectory")] public string GameDirectory { get; set; } = ".\\ultima";

        [JsonPropertyName("patchdirectory")] public string PatchDirectory { get; set; } = ".\\patches";

        [JsonPropertyName("serverdirectory")] public string ServerDirectory { get; set; } = ".\\server";

        [JsonPropertyName("profilesdirectory")] public string ProfilesDirectory { get; set; } = ".\\profiles";

        [JsonPropertyName("pluginsdirectory")] public string PluginsDirectory { get; set; } = ".\\plugins";

        [JsonPropertyName("clientversion")] public string ClientVersion { get; set; } = string.Empty;

        [JsonPropertyName("lang")] public string Language { get; set; } = "ENU";

        [JsonPropertyName("lastservernum")] public ushort LastServerNum { get; set; } = 1;

        [JsonPropertyName("last_server_name")] public string LastServerName { get; set; } = string.Empty;

        [JsonPropertyName("fps")] public int FPS { get; set; } = 120;

        [JsonConverter(typeof(NullablePoint2Converter))][JsonPropertyName("window_position")] public Point? WindowPosition { get; set; }

        [JsonConverter(typeof(NullablePoint2Converter))][JsonPropertyName("window_size")] public Point? WindowSize { get; set; }

        [JsonPropertyName("is_win_maximized")] public bool IsWindowMaximized { get; set; } = true;

        [JsonPropertyName("saveaccount")] public bool SaveAccount { get; set; } = true;

        [JsonPropertyName("autologin")] public bool AutoLogin { get; set; } = true;

        [JsonPropertyName("reconnect")] public bool Reconnect { get; set; } = true;

        [JsonPropertyName("reconnect_time")] public int ReconnectTime { get; set; } = 1;

        [JsonPropertyName("login_music")] public bool LoginMusic { get; set; } = true;

        [JsonPropertyName("login_music_volume")] public int LoginMusicVolume { get; set; } = 70;

        [JsonPropertyName("fixed_time_step")] public bool FixedTimeStep { get; set; } = true;

        [JsonPropertyName("run_mouse_in_separate_thread")] public bool RunMouseInASeparateThread { get; set; } = true;

        [JsonPropertyName("force_driver")] public byte ForceDriver { get; set; }

        [JsonPropertyName("use_verdata")] public bool UseVerdata { get; set; }

        [JsonPropertyName("maps_layouts")] public string MapsLayouts { get; set; }

        [JsonPropertyName("encryption")] public byte Encryption { get; set; }

        [JsonPropertyName("plugins")] public string[] Plugins { get; set; } = [];

        public static string GetSettingsFilepath()
        {
            if (CustomSettingsFilepath != null)
            {
                if (Path.IsPathRooted(CustomSettingsFilepath))
                {
                    return CustomSettingsFilepath;
                }

                return Path.Combine(UOAEnvironment.ExecutablePath, CustomSettingsFilepath);
            }

            return Path.Combine(UOAEnvironment.ExecutablePath, SETTINGS_FILENAME);
        }

        public void Save()
        {
            // Make a copy of the settings object that we will use in the saving process
            var json = JsonSerializer.Serialize(this, typeof(Settings), SettingsJsonContext.Default);
            var settingsToSave = JsonSerializer.Deserialize(json, typeof(Settings), SettingsJsonContext.Default) as Settings;

            // Make sure we don't save username and password if `saveaccount` flag is not set
            // NOTE: Even if we pass username and password via command-line arguments they won't be saved
            if (!settingsToSave.SaveAccount)
            {
                settingsToSave.Username = string.Empty;
                settingsToSave.Password = string.Empty;
            }

            // NOTE: We can do any other settings clean-ups here before we save them

            ConfigurationResolver.Save(settingsToSave, GetSettingsFilepath(), SettingsJsonContext.Default);
        }
    }
}