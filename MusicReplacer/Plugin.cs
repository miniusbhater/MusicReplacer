using Assets.Scripts;
using Assets.Scripts.Audio;
using Assets.Scripts.Flight.Demo;
using Assets.Scripts.UI;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Jundroo.Common.Settings;
using Notification;
using Rewired;
using Steamworks;
using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UIElements;


namespace MusicReplacer
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        AudioSource source;
        string[] files;
        int index;
        bool showGui;
        int currentPage = 0;

        void Awake()
        {
            var harmony = new Harmony("com.miniusbhater.musicreplacer");
            harmony.PatchAll();
        }

        void Start()
        {           
            Notification.Notification.Show("MusicReplacer", "Press Y to show UI", 6f); //wows im the first one to use my own stupid notification library awesome!
            source = gameObject.AddComponent<AudioSource>(); 
            source.loop = false; 
            string musicPath = Path.Combine(Paths.PluginPath, "Music");
            if (!Directory.Exists(musicPath))
            {
                Directory.CreateDirectory(musicPath);
            }
            files = Directory.GetFiles(musicPath);
            Shuffle();
            if (files.Length > 0)
            {
                StartCoroutine(Play(files[0]));
            }
            else
            {
                Notification.Notification.Show("MusicReplacer", "No songs to play", 6f);
            }   
            
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Y))
            {
                showGui = !showGui;
            }
            if (!source.isPlaying && source.clip != null)
            {
                index++;
                if (index >= files.Length)
                    index = 0;
                StartCoroutine(Play(files[index]));
            }
            // sorry for killing 5 fps :(
        }

        void Shuffle()
        {
            for (int index = 0; index < files.Length; index++)
            {
                int ranIndex = UnityEngine.Random.Range(index, files.Length);
                (files[index], files[ranIndex]) = (files[ranIndex], files[index]);
            }
        }

        IEnumerator Play(string path)
        {
            if (!path.EndsWith(".mp3") && !path.EndsWith(".wav"))
            {
                Notification.Notification.Show("MusicReplacer", $"Unsupported file type: {path}", 3f);
                yield break;
            }

            using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip("file://" + path, path.EndsWith(".mp3") ? AudioType.MPEG : AudioType.WAV))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Notification.Notification.Show("MusicReplacer", $"Failed to load song: {request.error}", 3f);
                    Logger.LogError("[MusicReplacer] Failed to load song");
                    yield break;
                }
                // there was meant to be a notification here to show the song as it changes but it really didnt want to work (i'll add it later)
                source.clip = DownloadHandlerAudioClip.GetContent(request);               
                source.Play();
            }
        }

        void OnGUI()
        {
            if (!showGui)
                return;
            Rect guiRect = new Rect(10, 100, 300, 400);
            GUI.Box(guiRect, "Music Replacer");
            GUI.BeginGroup(guiRect);
            int maxOnPage = 8;
            int totalPages = Mathf.CeilToInt(files.Length / (float)maxOnPage);
            int startIndex = currentPage * maxOnPage;
            for (int counter = 0; counter < maxOnPage; counter++)
            {
                int file = startIndex + counter;
                if (file >= files.Length)
                    break;
                string fileName = Path.GetFileName(files[file]);
                if (GUI.Button(new Rect(10, 30 + counter * 35, 280, 30), fileName))
                {
                    index = file;
                    StartCoroutine(Play(files[index]));
                }
            }
            if (GUI.Button(new Rect(10, guiRect.height - 50, 130, 30), "< Prev"))
            {
                currentPage--;
                if (currentPage < 0) currentPage = totalPages - 1;
            }
            if (GUI.Button(new Rect(160, guiRect.height - 50, 130, 30), "Next >"))
            {
                currentPage++;
                if (currentPage >= totalPages) currentPage = 0;
            }
            GUI.EndGroup();
        }

       
    }
}
