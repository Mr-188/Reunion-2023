﻿using System;
using System.Collections.Generic;
using System.Linq;
using ClientCore.Enums;
using ClientCore.Settings;
using Rampastring.Tools;


namespace ClientCore
{
    public class UserINISettings
    {
        private static UserINISettings _instance;

        public const string VIDEO = "Video";
        public const string MULTIPLAYER = "MultiPlayer";
        public const string OPTIONS = "Options";
        public const string AUDIO = "Audio";
        public const string COMPATIBILITY = "Compatibility";
        public const string GAME_FILTERS = "GameFilters";
        public const string GAMEMOD = "GameMod";
        private const string PHOBOS = "Phobos";
        private const string SKIN = "Skin";
        private const bool DEFAULT_SHOW_FRIENDS_ONLY_GAMES = false;
        private const bool DEFAULT_HIDE_LOCKED_GAMES = false;
        private const bool DEFAULT_HIDE_PASSWORDED_GAMES = false;
        private const bool DEFAULT_HIDE_INCOMPATIBLE_GAMES = false;
        private const int DEFAULT_MAX_PLAYER_COUNT = 8;

        public static UserINISettings Instance
        {
            get
            {
                if (_instance == null)
                    throw new InvalidOperationException("UserINISettings not initialized!");

                return _instance;
            }
        }

        public static void Initialize(string iniFileName)
        {
            if (_instance != null)
                throw new InvalidOperationException("UserINISettings has already been initialized!");

            var iniFile = new IniFile(SafePath.CombineFilePath(ProgramConstants.GamePath, iniFileName));

            _instance = new UserINISettings(iniFile);
        }

        protected UserINISettings(IniFile iniFile)
        {
            SettingsIni = iniFile;

            const string WINDOWED_MODE_KEY = "Video.Windowed";
#if TS
            BackBufferInVRAM = new BoolSetting(iniFile, VIDEO, "UseGraphicsPatch", true);
#else
            BackBufferInVRAM = new BoolSetting(iniFile, VIDEO, "VideoBackBuffer", false);
#endif

            IngameScreenWidth = new IntSetting(iniFile, VIDEO, "ScreenWidth", 1024);
            IngameScreenHeight = new IntSetting(iniFile, VIDEO, "ScreenHeight", 768);
            ClientTheme = new StringSetting(iniFile, MULTIPLAYER, "Theme", string.Empty);
            Language = new StringSetting(iniFile, MULTIPLAYER, "Language", string.Empty);
            Voice = new StringSetting(iniFile, MULTIPLAYER, "Voice", string.Empty);

            DetailLevel = new IntSetting(iniFile, OPTIONS, "DetailLevel", 2);
            Game = new StringSetting(iniFile, OPTIONS, "Game", "SkirmishLobby");

            Renderer = new StringSetting(iniFile, COMPATIBILITY, "Renderer", string.Empty);
            WindowedMode = new BoolSetting(iniFile, VIDEO, WINDOWED_MODE_KEY, false);
            BorderlessWindowedMode = new BoolSetting(iniFile, VIDEO, "NoWindowFrame", false);
            BorderlessWindowedClient = new BoolSetting(iniFile, VIDEO, "BorderlessWindowedClient", true);
            ClientFPS = new IntSetting(iniFile, VIDEO, "ClientFPS", 60);
            DisplayToggleableExtraTextures = new BoolSetting(iniFile, VIDEO, "DisplayToggleableExtraTextures", true);
            CampaignDefaultGameSpeed = new IntSetting(iniFile, PHOBOS, "CampaignDefaultGameSpeed", 4);

            ScoreVolume = new DoubleSetting(iniFile, AUDIO, "ScoreVolume", 0.7);
            SoundVolume = new DoubleSetting(iniFile, AUDIO, "SoundVolume", 0.7);
            VoiceVolume = new DoubleSetting(iniFile, AUDIO, "VoiceVolume", 0.7);
            IsScoreShuffle = new BoolSetting(iniFile, AUDIO, "IsScoreShuffle", true);
            ClientVolume = new DoubleSetting(iniFile, AUDIO, "ClientVolume", 1.0);
            PlayMainMenuMusic = new BoolSetting(iniFile, AUDIO, "PlayMainMenuMusic", true);
            StopMusicOnMenu = new BoolSetting(iniFile, AUDIO, "StopMusicOnMenu", true);
            MessageSound = new BoolSetting(iniFile, AUDIO, "ChatMessageSound", true);

            ScrollRate = new IntSetting(iniFile, OPTIONS, "ScrollRate", 3);
            DragDistance = new IntSetting(iniFile, OPTIONS, "DragDistance", 4);
            DoubleTapInterval = new IntSetting(iniFile, OPTIONS, "DoubleTapInterval", 30);
            Win8CompatMode = new StringSetting(iniFile, OPTIONS, "Win8Compat", "No");

            PlayerName = new StringSetting(iniFile, MULTIPLAYER, "Handle", string.Empty);

            ChatColor = new IntSetting(iniFile, MULTIPLAYER, "ChatColor", -1);
            LANChatColor = new IntSetting(iniFile, MULTIPLAYER, "LANChatColor", -1);
            PingUnofficialCnCNetTunnels = new BoolSetting(iniFile, MULTIPLAYER, "PingCustomTunnels", true);
            WritePathToRegistry = new BoolSetting(iniFile, OPTIONS, "WriteInstallationPathToRegistry", true);
            PlaySoundOnGameHosted = new BoolSetting(iniFile, MULTIPLAYER, "PlaySoundOnGameHosted", true);
            SkipConnectDialog = new BoolSetting(iniFile, MULTIPLAYER, "SkipConnectDialog", false);
            PersistentMode = new BoolSetting(iniFile, MULTIPLAYER, "PersistentMode", false);
            AutomaticCnCNetLogin = new BoolSetting(iniFile, MULTIPLAYER, "AutomaticCnCNetLogin", false);
            DiscordIntegration = new BoolSetting(iniFile, MULTIPLAYER, "DiscordIntegration", true);
            AllowGameInvitesFromFriendsOnly = new BoolSetting(iniFile, MULTIPLAYER, "AllowGameInvitesFromFriendsOnly", false);
            NotifyOnUserListChange = new BoolSetting(iniFile, MULTIPLAYER, "NotifyOnUserListChange", true);
            DisablePrivateMessagePopups = new BoolSetting(iniFile, MULTIPLAYER, "DisablePrivateMessagePopups", false);
            AllowPrivateMessagesFromState = new IntSetting(iniFile, MULTIPLAYER, "AllowPrivateMessagesFromState", (int)AllowPrivateMessagesFromEnum.All);
            EnableMapSharing = new BoolSetting(iniFile, MULTIPLAYER, "EnableMapSharing", true);
            AlwaysDisplayTunnelList = new BoolSetting(iniFile, MULTIPLAYER, "AlwaysDisplayTunnelList", false);
            MapSortState = new IntSetting(iniFile, MULTIPLAYER, "MapSortState", (int)SortDirection.None);

            CheckForUpdates = new BoolSetting(iniFile, OPTIONS, "CheckforUpdates", true);

            PrivacyPolicyAccepted = new BoolSetting(iniFile, OPTIONS, "PrivacyPolicyAccepted", false);

            //随机壁纸
            Random_wallpaper = new BoolSetting(iniFile, OPTIONS, "Random_wallpaper", false);
            //壁纸还是视频
            video_wallpaper = new BoolSetting(iniFile, OPTIONS, "video_wallpaper", false);
            IsFirstRun = new BoolSetting(iniFile, OPTIONS, "IsFirstRun", true);
            CustomComponentsDenied = new BoolSetting(iniFile, OPTIONS, "CustomComponentsDenied", false);
            Difficulty = new IntSetting(iniFile, OPTIONS, "Difficulty", 1);
            ScrollDelay = new IntSetting(iniFile, OPTIONS, "ScrollDelay", 4);
            GameSpeed = new IntSetting(iniFile, OPTIONS, "GameSpeed", 1);
            PreloadMapPreviews = new BoolSetting(iniFile, VIDEO, "PreloadMapPreviews", false);
            ForceLowestDetailLevel = new BoolSetting(iniFile, VIDEO, "ForceLowestDetailLevel", false);
            MinimizeWindowsOnGameStart = new BoolSetting(iniFile, OPTIONS, "MinimizeWindowsOnGameStart", true);
            AutoRemoveUnderscoresFromName = new BoolSetting(iniFile, OPTIONS, "AutoRemoveUnderscoresFromName", true);

            SortState = new IntSetting(iniFile, GAME_FILTERS, "SortState", (int)SortDirection.None);
            ShowFriendGamesOnly = new BoolSetting(iniFile, GAME_FILTERS, "ShowFriendGamesOnly", DEFAULT_SHOW_FRIENDS_ONLY_GAMES);
            HideLockedGames = new BoolSetting(iniFile, GAME_FILTERS, "HideLockedGames", DEFAULT_HIDE_LOCKED_GAMES);
            HidePasswordedGames = new BoolSetting(iniFile, GAME_FILTERS, "HidePasswordedGames", DEFAULT_HIDE_PASSWORDED_GAMES);
            HideIncompatibleGames = new BoolSetting(iniFile, GAME_FILTERS, "HideIncompatibleGames", DEFAULT_HIDE_INCOMPATIBLE_GAMES);
            MaxPlayerCount = new IntRangeSetting(iniFile, GAME_FILTERS, "MaxPlayerCount", DEFAULT_MAX_PLAYER_COUNT, 2, 8);

            FavoriteMaps = new StringListSetting(iniFile, OPTIONS, "FavoriteMaps", new List<string>());

            //皮肤
            Skin = new StringListSetting(iniFile, SKIN, "Skin", new List<string>());

            Mod_cath = new BoolSetting(iniFile, OPTIONS, "Mod_cath", true);


            StartCap = new BoolSetting(iniFile, OPTIONS, "StartCap", true);
            GameModName = new StringSetting(iniFile, GAMEMOD, "items", string.Empty);
            GameModPath = new StringSetting(iniFile, GAMEMOD, "Mod", string.Empty);
        }

        public IniFile SettingsIni { get; private set; }

        public event EventHandler SettingsSaved;

        /*********/
        /* VIDEO */
        /*********/

        public IntSetting IngameScreenWidth { get; private set; }
        public IntSetting IngameScreenHeight { get; private set; }
        public StringSetting ClientTheme { get; private set; }

        public StringSetting Language { get; private set; }

        public StringSetting Voice { get; private set; }
        public IntSetting DetailLevel { get; private set; }
        public StringSetting Renderer { get; private set; }
        public BoolSetting WindowedMode { get; private set; }
        public BoolSetting BorderlessWindowedMode { get; private set; }
        public BoolSetting BackBufferInVRAM { get; private set; }
        public IntSetting ClientResolutionX { get; set; }
        public IntSetting ClientResolutionY { get; set; }
        public BoolSetting BorderlessWindowedClient { get; private set; }
        public IntSetting ClientFPS { get; private set; }
        public BoolSetting DisplayToggleableExtraTextures { get; private set; }

        /*********/
        /* AUDIO */
        /*********/

        public DoubleSetting ScoreVolume { get; private set; }
        public DoubleSetting SoundVolume { get; private set; }
        public DoubleSetting VoiceVolume { get; private set; }
        public BoolSetting IsScoreShuffle { get; private set; }
        public DoubleSetting ClientVolume { get; private set; }
        public BoolSetting PlayMainMenuMusic { get; private set; }
        public BoolSetting StopMusicOnMenu { get; private set; }
        public BoolSetting MessageSound { get; private set; }

        /********/
        /* GAME */
        /********/
        public StringSetting Game { get; private set; }

        public IntSetting ScrollRate { get; private set; }
        public IntSetting DragDistance { get; private set; }
        public IntSetting DoubleTapInterval { get; private set; }
        public StringSetting Win8CompatMode { get; private set; }

        /************************/
        /* MULTIPLAYER (CnCNet) */
        /************************/

        public StringSetting PlayerName { get; private set; }

        public IntSetting ChatColor { get; private set; }
        public IntSetting LANChatColor { get; private set; }
        public BoolSetting PingUnofficialCnCNetTunnels { get; private set; }
        public BoolSetting WritePathToRegistry { get; private set; }
        public BoolSetting PlaySoundOnGameHosted { get; private set; }

        public BoolSetting SkipConnectDialog { get; private set; }
        public BoolSetting PersistentMode { get; private set; }
        public BoolSetting AutomaticCnCNetLogin { get; private set; }
        public BoolSetting DiscordIntegration { get; private set; }
        public BoolSetting AllowGameInvitesFromFriendsOnly { get; private set; }

        public BoolSetting NotifyOnUserListChange { get; private set; }

        public BoolSetting DisablePrivateMessagePopups { get; private set; }

        public IntSetting AllowPrivateMessagesFromState { get; private set; }

        public BoolSetting EnableMapSharing { get; private set; }

        public BoolSetting AlwaysDisplayTunnelList { get; private set; }

        public IntSetting MapSortState { get; private set; }

        /*********************/
        /* GAME LIST FILTERS */
        /*********************/

        public IntSetting SortState { get; private set; }

        public BoolSetting ShowFriendGamesOnly { get; private set; }

        public BoolSetting HideLockedGames { get; private set; }

        public BoolSetting HidePasswordedGames { get; private set; }

        public BoolSetting HideIncompatibleGames { get; private set; }

        public IntRangeSetting MaxPlayerCount { get; private set; }

        /********/
        /* MISC */
        /********/

        public BoolSetting CheckForUpdates { get; private set; }

        public BoolSetting PrivacyPolicyAccepted { get; private set; }
        public BoolSetting IsFirstRun { get; private set; }

        //随机壁纸
        public BoolSetting Random_wallpaper { get; private set; }

        //壁纸或视频
        public BoolSetting video_wallpaper { get; private set; }
        //Mod缓存机制
        public BoolSetting Mod_cath { get; private set; }


        public StringListSetting Skin { get; private set; }
        public BoolSetting CustomComponentsDenied { get; private set; }

        public IntSetting Difficulty { get; private set; }

        public IntSetting GameSpeed { get; private set; }

        public IntSetting ScrollDelay { get; private set; }

        public BoolSetting PreloadMapPreviews { get; private set; }

        public BoolSetting ForceLowestDetailLevel { get; private set; }

        public BoolSetting MinimizeWindowsOnGameStart { get; private set; }

        public BoolSetting AutoRemoveUnderscoresFromName { get; private set; }

        public StringListSetting FavoriteMaps { get; private set; }

        public IntSetting CampaignDefaultGameSpeed { get; private set; }


        //启动时检查任务包

        public BoolSetting StartCap { get; private set; }

        public StringSetting GameModName { get; private set; }

        public StringSetting GameModPath { get; private set; }

        public void SetValue(string section, string key, string value)
               => SettingsIni.SetStringValue(section, key, value);

        public void SetValue(string section, string key, bool value)
            => SettingsIni.SetBooleanValue(section, key, value);

        public void SetValue(string section, string key, int value)
            => SettingsIni.SetIntValue(section, key, value);

        public string GetValue(string section, string key, string defaultValue)
            => SettingsIni.GetStringValue(section, key, defaultValue);

        public bool GetValue(string section, string key, bool defaultValue)
            => SettingsIni.GetBooleanValue(section, key, defaultValue);

        public int GetValue(string section, string key, int defaultValue)
            => SettingsIni.GetIntValue(section, key, defaultValue);

        public bool IsGameFollowed(string gameName)
        {
            return SettingsIni.GetBooleanValue("Channels", gameName, false);
        }

        public string[] GetTypes()
        {
            List<string> SkinList = Skin.Value;

            List<string> Types = new List<string>();
            for (int i = 0; i < SkinList.Count; i++)
            {
                foreach (string type in new StringSetting(SettingsIni, SkinList[i], "Type", "").Value.Split(','))
                    Types.Add(type);
            }
            return Types.ToArray().GroupBy(p => p).Select(p => p.Key).ToArray();
        }

        public int GetSkinBy(string name, string m)
        {

            return new IntSetting(SettingsIni, name, m, 0).Value;
        }

        public List<string> GetSkinName(string Types)
        {
            List<string> SkinList = Skin.Value;

            List<string> SkinName = new List<string>();

            for (int i = 0; i < SkinList.Count; i++)
            {

                string s = new StringSetting(SettingsIni, SkinList[i], "Type", "").Value;
                if (Types == "All" || s.IndexOf(Types) != -1)
                    SkinName.Add(new StringSetting(SettingsIni, SkinList[i], "Text", "").Value);
            }

            return SkinName;
        }


        public List<string[]> GetAIISkin()
        {
            List<string> SkinList = Skin.Value;

            List<string[]> AllSkin = new List<string[]>();

            for (int i = 0; i < SkinList.Count; i++)
            {
                string[] skin = new string[11];
                skin[0] = new StringSetting(SettingsIni, SkinList[i], "Text", "").Value.ToString();
                skin[1] = new StringSetting(SettingsIni, SkinList[i], "Folder", "").Value.ToString();
                skin[2] = new StringSetting(SettingsIni, SkinList[i], "Options", "").Value.ToString();
                skin[3] = new StringSetting(SettingsIni, SkinList[i], "Select", "").Value.ToString();
                skin[4] = new StringSetting(SettingsIni, SkinList[i], "Image", "").Value.ToString();
                skin[5] = SkinList[i];
                skin[6] = new StringSetting(SettingsIni, SkinList[i], "Delete", "").Value.ToString();
                skin[7] = new StringSetting(SettingsIni, SkinList[i], "RulesIni", "").Value.ToString();
                skin[8] = new StringSetting(SettingsIni, SkinList[i], "ArtIni", "").Value.ToString();
                skin[9] = new StringSetting(SettingsIni, SkinList[i], "AllText", "").Value.ToString();
                skin[10] = new StringSetting(SettingsIni, SkinList[i], "Index", "").Value.ToString();
                AllSkin.Add(skin);
            }
            return AllSkin;
        }

        public List<string> GetSkinIni(string types)
        {
            List<string> SkinList = Skin.Value;
            List<string> rules = new List<string>();
            for (int i = 0; i < SkinList.Count; i++)
            {
                rules.Add(new StringSetting(SettingsIni, SkinList[i], types, "").Value);
            }
            return rules;
        }

        public void SetSkinIndex(string name, int value)
        {
            SettingsIni.SetIntValue(name, "Select", value);
        }

        public bool ToggleFavoriteMap(string mapName, string gameModeName, bool isFavorite)
        {
            if (string.IsNullOrEmpty(mapName))
                return isFavorite;

            var favoriteMapKey = FavoriteMapKey(mapName, gameModeName);
            isFavorite = IsFavoriteMap(mapName, gameModeName);
            if (isFavorite)
                FavoriteMaps.Remove(favoriteMapKey);
            else
                FavoriteMaps.Add(favoriteMapKey);

            Instance.SaveSettings();

            return !isFavorite;
        }

        /// <summary>
        /// Checks if a specified map name and game mode name belongs to the favorite map list.
        /// </summary>
        /// <param name="nameName">The name of the map.</param>
        /// <param name="gameModeName">The name of the game mode</param>
        public bool IsFavoriteMap(string nameName, string gameModeName) => FavoriteMaps.Value.Contains(FavoriteMapKey(nameName, gameModeName));

        private string FavoriteMapKey(string nameName, string gameModeName) => $"{nameName}:{gameModeName}";

        public void ReloadSettings()
        {
            SettingsIni.Reload();
        }

        public void ApplyDefaults()
        {
            ForceLowestDetailLevel.SetDefaultIfNonexistent();
            DoubleTapInterval.SetDefaultIfNonexistent();
            ScrollDelay.SetDefaultIfNonexistent();
        }

        public void SaveSettings()
        {
            Logger.Log("Writing settings INI.");

            ApplyDefaults();
            // CleanUpLegacySettings();

            SettingsIni.WriteIniFile();

            SettingsSaved?.Invoke(this, EventArgs.Empty);
        }

        public bool IsGameFiltersApplied()
        {
            return ShowFriendGamesOnly.Value != DEFAULT_SHOW_FRIENDS_ONLY_GAMES ||
                   HideLockedGames.Value != DEFAULT_HIDE_LOCKED_GAMES ||
                   HidePasswordedGames.Value != DEFAULT_HIDE_PASSWORDED_GAMES ||
                   HideIncompatibleGames.Value != DEFAULT_HIDE_INCOMPATIBLE_GAMES ||
                   MaxPlayerCount.Value != DEFAULT_MAX_PLAYER_COUNT;
        }

        public void ResetGameFilters()
        {
            ShowFriendGamesOnly.Value = DEFAULT_SHOW_FRIENDS_ONLY_GAMES;
            HideLockedGames.Value = DEFAULT_HIDE_LOCKED_GAMES;
            HideIncompatibleGames.Value = DEFAULT_HIDE_INCOMPATIBLE_GAMES;
            HidePasswordedGames.Value = DEFAULT_HIDE_PASSWORDED_GAMES;
            MaxPlayerCount.Value = DEFAULT_MAX_PLAYER_COUNT;
        }
    }
}