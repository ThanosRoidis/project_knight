using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


public enum VolumeType
{
    MASTER, MUSIC, SFX
}

namespace Shared
{
    public class AudioController : Singleton<AudioController>
    {
        [SerializeField] private AudioMixerGroup musicMixerGroup;
        [SerializeField] private AudioMixerGroup sfxMixerGroup;

        [SerializeField] private int nurof_SFX_Sources = 10;
        [SerializeField] private AudioClip musicClip;
        [SerializeField] private AudioClip gameOverClip;
        [SerializeField] private AudioClip scoreClip;
        [SerializeField] private AudioClip scoreDecreaseClip;
        [SerializeField] private AudioClip countdownClip;
        [SerializeField] private AudioClip bellRingStart;
        [SerializeField] private AudioClip bellRingEnd;
        [SerializeField] private AudioClip fallingClip;
        [SerializeField] private AudioClip landingClip;
        [SerializeField] private float SFXVolume = 0.05f;
        [SerializeField] private bool muteEditorMusic;

        private List<AudioSource> sources;
        private AudioSource musicSource;


        // Start is called before the first frame update
        protected override void Awake()
        {
            base.Awake();
            sources = new List<AudioSource>();
            for (int i = 0; i < nurof_SFX_Sources; i++) {
                GameObject sourceGO = new GameObject();
                sourceGO.transform.parent = transform;
                AudioSource source = sourceGO.AddComponent<AudioSource>();
                source.volume = SFXVolume;
                source.playOnAwake = false;
                sources.Add(source);
            }

            musicSource = transform.Find("MusicSource").GetComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.clip = musicClip;
        }

        protected void Start()
        {
            if (BaseGameOptions.Instance != null) {
                SetVolume(VolumeType.MASTER, BaseGameOptions.Instance.masterVolume);
                SetVolume(VolumeType.MUSIC, BaseGameOptions.Instance.musicVolume);
                SetVolume(VolumeType.SFX, BaseGameOptions.Instance.sfxVolume);
            }
        }

        public void PlaySFX(AudioClip clip, bool exclusive = true)
        {
            if (clip == null) return;
            bool isPlaying = false;
            AudioSource availableSource = sources[0];
            foreach (AudioSource source in sources) {
                if (exclusive) {
                    if (source.isPlaying && clip.Equals(source.clip)) {
                        isPlaying = true;
                    }
                }

                if (!source.isPlaying) {
                    availableSource = source;
                }
            }

            if (!isPlaying) {
                availableSource.clip = clip;
                availableSource.Play();
            }
        }

        public void StopSFX(AudioClip clip)
        {
            foreach (AudioSource source in sources) {
                if (source.clip == clip)
                    source.Stop();
            }

        }

        public void StopAllSFX()
        {
            foreach (AudioSource source in sources) {
                source.Stop();
                source.clip = null;
            }
        }


        public void PauseAllSFX()
        {
            foreach (AudioSource source in sources) {
                if (!source.isPlaying) {
                    source.clip = null;
                }
                source.Pause();
            }
        }

        public void ContinueAllSFX()
        {
            foreach (AudioSource source in sources) {
                source.Play();
            }
        }

        public void PlayStageMusic(AudioClip clip)
        {
            musicSource.clip = clip;
            PlayStageMusic();
        }

        public void PlayStageMusic()
        {
            musicSource.Play();
            if (muteEditorMusic) {
#if UNITY_EDITOR
                musicSource.volume = 0;
#endif
            }
        }

        public void PauseStageMusic()
        {
            musicSource.Pause();
        }

        public void StopStageMusic()
        {
            musicSource.Stop();
        }

        public void PlayScoreIncreaseSFX()
        {
            PlaySFX(scoreClip, exclusive: false);
        }
        public void PlayScoreDecreaseSFX()
        {
            PlaySFX(scoreDecreaseClip, exclusive: false);
        }
        public void PlayGameoverClip()
        {
            PlaySFX(gameOverClip);
        }

        public void PlayRoundStartSFX()
        {
            PlaySFX(bellRingStart);
        }

        public void PlayRoundEndSFX()
        {
            PlaySFX(bellRingEnd);
        }

        public void PlayCountdownSFX()
        {
            PlaySFX(countdownClip);
        }

        public void PlayLandingSFX()
        {
            PlaySFX(landingClip);
        }

        public void PlayFallingSFX()
        {
            PlaySFX(fallingClip);
        }


        private string GetVolumePrefString(VolumeType volType)
        {
            string mixerVolumeString = "Master";
            switch (volType) {
                case VolumeType.MASTER:
                    mixerVolumeString = "Master";
                    break;
                case VolumeType.MUSIC:
                    mixerVolumeString = "Music";
                    break;
                case VolumeType.SFX:
                    mixerVolumeString = "SFX";
                    break;
            }
            return string.Format("{0}{1}", mixerVolumeString, "Volume");

        }

        public void SetVolume(VolumeType volType, float value)
        {
            string volumePrefStr = GetVolumePrefString(volType);
            float vol = Mathf.Log10(value / 100) * 20f + 10;
            musicMixerGroup.audioMixer.SetFloat(volumePrefStr, vol);

            switch (volType) {
                case VolumeType.MASTER:
                    BaseGameOptions.Instance.masterVolume = value;
                    break;
                case VolumeType.MUSIC:
                    BaseGameOptions.Instance.musicVolume = value;
                    break;
                case VolumeType.SFX:
                    BaseGameOptions.Instance.sfxVolume = value;
                    break;
            }
        }

        public float GetVolume(VolumeType volType)
        {
            float volume = 0;
            switch (volType) {
                case VolumeType.MASTER:
                    volume = BaseGameOptions.Instance.masterVolume;
                    break;
                case VolumeType.MUSIC:
                    volume = BaseGameOptions.Instance.musicVolume;
                    break;
                case VolumeType.SFX:
                    volume = BaseGameOptions.Instance.sfxVolume;
                    break;
            }
            return volume;
        }
    }
}