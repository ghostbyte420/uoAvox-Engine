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

using System;
using System.Collections.Generic;
using ClassicUO.Data;
using ClassicUO.Configuration;
using ClassicUO.IO.Audio;
using ClassicUO.IO.Resources;
using ClassicUO.Utility.Logging;
using Microsoft.Xna.Framework.Audio;

namespace ClassicUO.Game.Managers
{
    internal class AudioManager
    {
        private bool _canReproduceAudio = true;
        private readonly LinkedList<UOSound> _currentSounds = new LinkedList<UOSound>();
        private readonly UOMusic[] _currentMusic = { null, null };
        private readonly int[] _currentMusicIndices = { 0, 0 };
        public int LoginMusicIndex { get; private set; }
        public int DeathMusicIndex { get; } = 42;

        public void Initialize()
        {
            try
            {
                new DynamicSoundEffectInstance(0, AudioChannels.Stereo).Dispose();
            }
            catch (NoAudioHardwareException ex)
            {
                Log.Warn(ex.ToString());
                _canReproduceAudio = false;
            }

            #region Client Login Music Is Defined Here [01-01]

            /// MusicIDs Cheatsheet
            /// -------------------
            /// [0] turfin,loop
            /// [1] turfin,loop
            /// [2] turfin,loop
            /// [3] turfin,loop
            /// [4] turfin,loop
            /// [5] turfin,loop
            /// [6] turfin,loop
            /// [7] turfin,loop
            /// [8] stones1
            /// [9] britainpos,loop
            /// -------------------
            /// [10] britain1
            /// [11] bucsden,loop
            /// [12] jhelom
            /// [13] lbc
            /// [14] linelle
            /// [15] newmagincia,loop
            /// [16] minocpos,loop
            /// [17] valoriapos,loop
            /// [18] ambrosia,loop
            /// [19] stones
            /// [20] scarabreapos,loop
            /// [21] trinsicpos,loop
            /// [22] vesper1
            /// [23] yew1
            /// [24] yewpos,loop
            /// [25] dungeon
            /// [26] dragonshi,loop
            /// [27] citynightedit,loop
            /// [28] walking,loop
            /// [29] citynightedit,loop
            /// [30] walking,loop
            /// [31] citynightedit,loop
            /// [32] boattravel
            /// [33] citynightedit,loop
            /// [34] tavern1
            /// [35] tavern2
            /// [36] tavern3
            /// [37] pubtune,loop
            /// [38] goodevil,loop
            /// [39] humanoids,loop
            /// [40] gargoyles,loop
            /// [41] turfin,loop
            /// [42] deathtune
            /// [43] victory
            /// [44] overlordv2
            /// [45] nujelm
            /// [46] dragonslo,loop
            /// [47] cove,loop
            /// [48] moonglowpos,loop
            /// [49] zento,loop
            /// [50] tokunodungeon,loop
            /// [51] taiko,loop
            /// [52] dread_horn_area,loop
            /// [53] elf_city_1,loop
            /// [54] grizzle_dungeon,loop
            /// [55] melisandes_lair,loop
            /// [56] paroxysmus_lair,loop
            /// [57] ConversationWithGwenno.mp3
            /// [58] GoodEndGame.mp3
            /// [59] GoodVsEvil.mp3
            /// [60] GreatEarthSerpentsTheme.mp3
            /// [61] HumanoidsU9.mp3
            /// [62] MinocNegative.mp3
            /// [63] Paws.mp3
            /// [64] SelimsBar.mp3
            /// [65] UltimaVIISerpentIsleCombat.mp3
            /// [66] ValoriaShips.mp3
            /// [67] TheWanderer.mp3
            /// [68] Castle.mp3
            /// [69] Festival.mp3
            /// [70] Honor.mp3
            /// [71] Medieval.mp3
            /// [72] BattleOnStones.mp3
            /// [73] Docktown.mp3
            /// [74] GargoyleQueen.mp3
            /// [75] GenericCombat.mp3
            /// [76] Holycity.mp3
            /// [77] HumanLevel.mp3
            /// [78] LoginLoop.mp3,loop
            /// [79] NorthernForestBattleonStones.mp3
            /// [80] PrimevalLich.mp3
            /// [81] QueenPalace.mp3
            /// [82] RoyalCity.mp3
            /// [83] SlasherVeil.mp3
            /// [84] StygianAbyss.mp3
            /// [85] StygianDragon.mp3
            /// [86] Void.mp3
            /// [87] CodexShrine.mp3
            /// [88] AnvilStrikeInMinoc.mp3
            /// [89] ASkaranLullaby.mp3
            /// [90] BlackthornsMarch.mp3
            /// [91] DupresNightInTrinsic.mp3
            /// [92] FayaxionAndTheSix.mp3
            /// [93] FlightOfTheNexus.mp3
            /// [94] GalehavenJaunt.mp3
            /// [95] JhelomToArms.mp3
            /// [96] MidnightInYew.mp3
            /// [97] MoonglowSonata.mp3
            /// [98] NewMaginciaMarch.mp3
            /// [99] NujelmWaltz.mp3
            /// -------------------            
            /// [100] SherrysSong.mp3
            /// [101] StarlightInBritain.mp3
            /// [102] TheVesperMist.mp3

            LoginMusicIndex = Client.Version >= ClientVersion.CV_7000 ? 78 : Client.Version > ClientVersion.CV_308Z ? 0 : 8;

            #endregion

            Client.Game.Activated += OnWindowActivated;
            Client.Game.Deactivated += OnWindowDeactivated;
        }

        private void OnWindowDeactivated(object sender, EventArgs e)
        {
            if (!_canReproduceAudio || ProfileManager.CurrentProfile == null || ProfileManager.CurrentProfile.ReproduceSoundsInBackground)
            {
                return;
            }

            SoundEffect.MasterVolume = 0;
        }

        private void OnWindowActivated(object sender, EventArgs e)
        {
            if (!_canReproduceAudio || ProfileManager.CurrentProfile == null || ProfileManager.CurrentProfile.ReproduceSoundsInBackground)
            {
                return;
            }

            SoundEffect.MasterVolume = 1;
        }

        public void PlaySound(int index)
        {
            Profile currentProfile = ProfileManager.CurrentProfile;

            if (!_canReproduceAudio || currentProfile == null)
            {
                return;
            }

            float volume = currentProfile.SoundVolume / Constants.SOUND_DELTA;

            if (Client.Game.IsActive)
            {
                if (!currentProfile.ReproduceSoundsInBackground)
                {
                    volume = currentProfile.SoundVolume / Constants.SOUND_DELTA;
                }
            }
            else if (!currentProfile.ReproduceSoundsInBackground)
            {
                volume = 0;
            }

            if (volume < -1 || volume > 1f)
            {
                return;
            }

            if (!currentProfile.EnableSound || !Client.Game.IsActive && !currentProfile.ReproduceSoundsInBackground)
            {
                volume = 0;
            }

            UOSound sound = (UOSound) SoundsLoader.Instance.GetSound(index);

            if (sound != null && sound.Play(volume))
            {
                sound.X = -1;
                sound.Y = -1;
                sound.CalculateByDistance = false;

                _currentSounds.AddLast(sound);
            }
        }

        public void PlaySoundWithDistance(int index, int x, int y)
        {
            if (!_canReproduceAudio || !World.InGame)
            {
                return;
            }

            int distX = Math.Abs(x - World.Player.X);
            int distY = Math.Abs(y - World.Player.Y);
            int distance = Math.Max(distX, distY);

            Profile currentProfile = ProfileManager.CurrentProfile;
            float volume = currentProfile.SoundVolume / Constants.SOUND_DELTA;
            float distanceFactor = 0.0f;

            if (distance >= 1)
            {
                float volumeByDist = volume / (World.ClientViewRange + 1);
                distanceFactor = volumeByDist * distance;
            }

            if (distance > World.ClientViewRange)
            {
                volume = 0;
            }

            if (volume < -1 || volume > 1f)
            {
                return;
            }

            if (currentProfile == null || !currentProfile.EnableSound || !Client.Game.IsActive && !currentProfile.ReproduceSoundsInBackground)
            {
                volume = 0;
            }

            UOSound sound = (UOSound) SoundsLoader.Instance.GetSound(index);

            if (sound != null && sound.Play(volume, distanceFactor))
            {
                sound.X = x;
                sound.Y = y;
                sound.CalculateByDistance = true;

                _currentSounds.AddLast(sound);
            }
        }

        public void PlayMusic(int music, bool iswarmode = false, bool is_login = false)
        {
            if (!_canReproduceAudio)
            {
                return;
            }

            if (music >= Constants.MAX_MUSIC_DATA_INDEX_COUNT)
            {
                return;
            }

            float volume;

            if (is_login)
            {
                volume = Settings.GlobalSettings.LoginMusic ? Settings.GlobalSettings.LoginMusicVolume / Constants.SOUND_DELTA : 0;
            }
            else
            {
                Profile currentProfile = ProfileManager.CurrentProfile;

                if (currentProfile == null || !currentProfile.EnableMusic)
                {
                    volume = 0;
                }
                else
                {
                    volume = currentProfile.MusicVolume / Constants.SOUND_DELTA;
                }

                if (currentProfile != null && !currentProfile.EnableCombatMusic && iswarmode)
                {
                    return;
                }
            }


            if (volume < -1 || volume > 1f)
            {
                return;
            }

            Sound m = SoundsLoader.Instance.GetMusic(music);

            if (m == null && _currentMusic[0] != null)
            {
                StopMusic();
            }
            else if (m != null && (m != _currentMusic[0] || iswarmode))
            {
                StopMusic();

                int idx = iswarmode ? 1 : 0;
                _currentMusicIndices[idx] = music;
                _currentMusic[idx] = (UOMusic) m;

                _currentMusic[idx].Play(volume);
            }
        }

        public void UpdateCurrentMusicVolume(bool isLogin = false)
        {
            if (!_canReproduceAudio)
            {
                return;
            }

            for (int i = 0; i < 2; i++)
            {
                if (_currentMusic[i] != null)
                {
                    float volume;

                    if (isLogin)
                    {
                        volume = Settings.GlobalSettings.LoginMusic ? Settings.GlobalSettings.LoginMusicVolume / Constants.SOUND_DELTA : 0;
                    }
                    else
                    {
                        Profile currentProfile = ProfileManager.CurrentProfile;

                        volume = currentProfile == null || !currentProfile.EnableMusic ? 0 : currentProfile.MusicVolume / Constants.SOUND_DELTA;
                    }


                    if (volume < -1 || volume > 1f)
                    {
                        return;
                    }

                    _currentMusic[i].Volume = i == 0 && _currentMusic[1] != null ? 0 : volume;
                }
            }
        }

        public void UpdateCurrentSoundsVolume()
        {
            if (!_canReproduceAudio)
            {
                return;
            }

            Profile currentProfile = ProfileManager.CurrentProfile;

            float volume = currentProfile == null || !currentProfile.EnableSound ? 0 : currentProfile.SoundVolume / Constants.SOUND_DELTA;

            if (volume < -1 || volume > 1f)
            {
                return;
            }

            for (LinkedListNode<UOSound> soundNode = _currentSounds.First; soundNode != null; soundNode = soundNode.Next)
            {
                soundNode.Value.Volume = volume;
            }
        }

        public void StopMusic()
        {
            for (int i = 0; i < 2; i++)
            {
                if (_currentMusic[i] != null)
                {
                    _currentMusic[i].Stop();
                    _currentMusic[i].Dispose();
                    _currentMusic[i] = null;
                }
            }
        }

        public void StopWarMusic()
        {
            PlayMusic(_currentMusicIndices[0]);
        }

        public void StopSounds()
        {
            LinkedListNode<UOSound> first = _currentSounds.First;

            while (first != null)
            {
                LinkedListNode<UOSound> next = first.Next;

                first.Value.Stop();

                _currentSounds.Remove(first);

                first = next;
            }
        }

        public void Update()
        {
            if (!_canReproduceAudio)
            {
                return;
            }

            bool runninWarMusic = _currentMusic[1] != null;
            Profile currentProfile = ProfileManager.CurrentProfile;

            for (int i = 0; i < 2; i++)
            {
                if (_currentMusic[i] != null && currentProfile != null)
                {
                    if (Client.Game.IsActive)
                    {
                        if (!currentProfile.ReproduceSoundsInBackground)
                        {
                            _currentMusic[i].Volume = i == 0 && runninWarMusic || !currentProfile.EnableMusic ? 0 : currentProfile.MusicVolume / Constants.SOUND_DELTA;
                        }
                    }
                    else if (!currentProfile.ReproduceSoundsInBackground && _currentMusic[i].Volume != 0.0f)
                    {
                        _currentMusic[i].Volume = 0;
                    }
                }

                _currentMusic[i]?.Update();
            }


            LinkedListNode<UOSound> first = _currentSounds.First;

            while (first != null)
            {
                LinkedListNode<UOSound> next = first.Next;

                if (!first.Value.IsPlaying)
                {
                    first.Value.Stop();
                    _currentSounds.Remove(first);
                }

                first = next;
            }
        }

        public UOMusic GetCurrentMusic()
        {
            for (int i = 0; i < 2; i++)
            {
                if (_currentMusic[i] != null && _currentMusic[i].IsPlaying)
                {
                    return _currentMusic[i];
                }
            }
            return null;
        }
    }
}