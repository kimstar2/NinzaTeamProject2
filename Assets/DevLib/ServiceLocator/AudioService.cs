using System;
using System.Collections.Generic;
using System.Linq;
using DevLib.SoundSystem.Runtime;
using UnityEngine;
using AudioType = DevLib.SoundSystem.Runtime.AudioType;

namespace DevLib.ServiceLocator
{
    
    public class AudioService : MonoBehaviour, IAudioService
    {
        [SerializeField] private GameObject soundPlayerPrefab;
        
        private Dictionary<int, SoundPlayer> _playerDict = new();

        private SoundPlayer _bgmPlayer;

        private const string ResourcePath = "Audio System";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureAudioService()
        {
            if (ServiceLocator.TryGet<IAudioService>(out var service)
                && service is AudioService current && current != null)
                return;

            AudioService existing = FindFirstObjectByType<AudioService>(FindObjectsInactive.Include);
            if (existing != null)
            {
                existing.transform.SetParent(null);
                existing.enabled = true;
                existing.gameObject.SetActive(true);
                existing.Initialize();
                return;
            }

            GameObject prefab = Resources.Load<GameObject>(ResourcePath);
            if (prefab == null || !prefab.TryGetComponent<AudioService>(out _))
            {
                Debug.LogError($"Missing AudioService prefab at Resources/{ResourcePath}.");
                return;
            }

            GameObject instance = Instantiate(prefab);
            instance.name = prefab.name;
        }
        
        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (ServiceLocator.TryGet<IAudioService>(out var service)
                && service is AudioService current && current != null && current != this)
            {
                Destroy(gameObject);
                return;
            }

            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            if (_bgmPlayer == null)
            {
                GameObject bgmObject = Instantiate(soundPlayerPrefab, transform);
                _bgmPlayer = bgmObject.GetComponent<SoundPlayer>();
            }

            if (!ReferenceEquals(service, this))
                ServiceLocator.Register<IAudioService>(this);
        }

        private void OnDestroy()
        {
            // Destroying a duplicate must not replace the surviving service.
            if (ServiceLocator.TryGet<IAudioService>(out var service)
                && ReferenceEquals(service, this))
                ServiceLocator.Register<IAudioService>(new NullAudioService());
        }

        public void Play(SoundClipSO clipData, int channel = 0)
        {
            if (clipData == null || clipData.clip == null)
            {
                Debug.LogWarning("Assign a SoundClipSO with an audio clip before playing.", this);
                return;
            }

            if (clipData.audioType == AudioType.Music)
            {
                _bgmPlayer.ForceStopSound();
                _bgmPlayer.PlaySound(clipData);
                return;
            }

            GameObject playerObj = Instantiate(soundPlayerPrefab, transform);
            SoundPlayer player = playerObj.GetComponent<SoundPlayer>();
            player.PlaySound(clipData);

            player.OnSoundFinished += HandleSoundFinish;

            if (channel > 0)
            {
                if (_playerDict.TryGetValue(channel, out SoundPlayer oldPlayer))
                {
                    oldPlayer.ForceStopSound();
                    SetDisableSoundPlayer(oldPlayer);
                    _playerDict.Remove(channel);
                }
                
                _playerDict[channel] = player;
            }
        }

        private void HandleSoundFinish(SoundPlayer player)
        {
            player.OnSoundFinished -= HandleSoundFinish;
            //나중에 풀매니저로 변경.
            SetDisableSoundPlayer(player);
        }

        private void SetDisableSoundPlayer(SoundPlayer player)
        {
            Destroy(player.gameObject);
        }

        public void StopSfx(int channel)
        {
            if (_playerDict.TryGetValue(channel, out SoundPlayer player))
            {
                player.ForceStopSound();
                SetDisableSoundPlayer(player);
            }
        }

        public void StopBgm()
        {
            _bgmPlayer.ForceStopSound();
        }
    }
}
