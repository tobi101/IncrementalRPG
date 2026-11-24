#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || (!UNITY_EDITOR && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX))
using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Boot
{
    [AddComponentMenu("")]
    internal sealed class SteamworksClient : MonoBehaviour
    {
#if UNITY_EDITOR_OSX || (!UNITY_EDITOR && UNITY_STANDALONE_OSX)
        private const string Library = "steam_api";
#else
        private const string Library = "steam_api64";
#endif
        private bool _initialized;

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SteamAPI_InitFlat([Out] byte[] errorMessage);

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SteamAPI_RunCallbacks();

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SteamAPI_Shutdown();

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr SteamAPI_SteamUtils_v011();

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint SteamAPI_ISteamUtils_GetAppID(IntPtr utils);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Create()
        {
            if (IntPtr.Size != 8)
                return;

            var host = new GameObject("Steamworks");
            DontDestroyOnLoad(host);
            host.AddComponent<SteamworksClient>();
        }

        private void OnEnable()
        {
            // SteamErrMsg is a 1024-byte UTF-8 buffer in the Steamworks SDK.
            var error = new byte[1024];
            try
            {
                int result = SteamAPI_InitFlat(error);
                if (result != 0)
                {
                    string reason = Encoding.UTF8.GetString(error).TrimEnd('\0');
                    Debug.LogWarning($"[Steamworks] Playing offline. Init result {result}: {reason}");
                    return;
                }

                _initialized = true;
                var utils = SteamAPI_SteamUtils_v011();
                if (utils != IntPtr.Zero)
                    Debug.Log($"[Steamworks] Connected to Steam. App ID {SteamAPI_ISteamUtils_GetAppID(utils)}.");
            }
            catch (Exception ex) when (ex is DllNotFoundException || ex is EntryPointNotFoundException || ex is BadImageFormatException)
            {
                Debug.LogError($"[Steamworks] Native SDK could not be loaded: {ex.Message}");
            }
        }

        private void Update()
        {
            if (_initialized)
                SteamAPI_RunCallbacks();
        }

        private void OnDisable()
        {
            if (_initialized)
            {
                SteamAPI_Shutdown();
                _initialized = false;
            }
        }
    }
}
#endif
