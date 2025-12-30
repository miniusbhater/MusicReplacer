using Assets.Scripts.Audio;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicReplacer
{
    public static class Patch
    {
        // gahhhh this took forever to find but it workie
        // in game music is cool and stuff but maybe your music is cooler?
        // i also do not know which one of these actually removes the music as i just added both at the same time but whatever works i guess :3
        [HarmonyPatch(typeof(MusicPlayerScript), "Awake")]
        public static class PatchOutSongs
        {
            static bool Prefix()
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(MusicPlayerScript), "PlayNextSong")]
        public static class PatchOutSongs2
        {
            static bool Prefix()
            {
                return false;
            }
        }
    }
}
