using BepInEx;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace JesterKnife
{
    [BepInPlugin(P_GUID, P_Name, P_Version)]
    public class JesterKnife : BaseUnityPlugin
    {
        public const string P_GUID = $"{P_Author}.{P_Name}";
        public const string P_Author = "RigsInRags";
        public const string P_Name = "JesterKnife";
        public const string P_Version = "1.0.0";

        public static AssetBundle MainAssets;

        public void Awake()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("JesterKnife.jesterassets"))
                MainAssets = AssetBundle.LoadFromStream(stream);

            Harmony harmony = new Harmony(P_GUID);

            MethodInfo targetMethod = AccessTools.Method(typeof(JesterAI), "Start");
            MethodInfo postfix = typeof(JesterKnife).GetMethod("OnJesterAIStart", BindingFlags.Public | BindingFlags.Static);

            harmony.Patch(targetMethod, postfix: new HarmonyMethod(postfix));


            targetMethod = AccessTools.Method(typeof(JesterAI), "Update");
            postfix = typeof(JesterKnife).GetMethod("OnJesterAIUpdate", BindingFlags.Public | BindingFlags.Static);

            harmony.Patch(targetMethod, postfix: new HarmonyMethod(postfix));
        }

        public static void OnJesterAIStart(JesterAI __instance)
        {
            // Jester theme: buildup music (audio file is 42 seconds long, actual duration is 35 to 40 seconds).
            __instance.popGoesTheWeaselTheme = MainAssets.LoadAsset<AudioClip>("BuildUp.wav");

            // Popup theme: knight roar
            __instance.popUpSFX = MainAssets.LoadAsset<AudioClip>("Scream.wav");

            // Screaming sfx: rest of music minus buildup
            __instance.screamingSFX = MainAssets.LoadAsset<AudioClip>("Chase.wav");

            // Kill player sfx: swoon sound
            __instance.killPlayerSFX = MainAssets.LoadAsset<AudioClip>("Kill.wav");
        }

        public static void OnJesterAIUpdate(JesterAI __instance)
        {
            // The crank volume is quite loud and can somewhat obstruct the modded pop goes the weasel theme, so its quietened quite a bit.
            // Because the scream sfx uses the same audio source, it needs to be reverted back to it's normal volume after the crank phase is finished.
            if (__instance.creatureAnimator.GetBool("poppedOut"))
                __instance.creatureSFX.volume = 1f;

            else
                __instance.creatureSFX.volume = 0.15f;
        }
    }
}