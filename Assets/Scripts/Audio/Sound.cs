using Stupid.Audio;
using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Stupid.stujam01 {
    [Serializable]
    public class Sound {
        public AudioMixerGroup Group;
        public AudioClip[] Clips;
        public Vector2 VolumeRange;
        public float BasePitch;
        public Vector2 PitchRange;
        public float Range;

        private Random random = new Random();
        private SoundUtilitiy utility => SoundUtilitiy.Instance;

        public void Play(Vector3? position = null, AudioSource source = null) {
            source ??= utility.GetSource();

            source.outputAudioMixerGroup = Group;
            source.clip = GetAudioClip();
            source.volume = GetVolume();
            source.pitch = GetPitch();
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 0f;
            source.maxDistance = Range;

            source.spatialBlend = position == null ? 0f : 1f;
            source.transform.position = position ?? Vector3.zero;

            source.Play();
        }

        public void PlayOneShot(AudioSource source) {
            source.PlayOneShot(GetAudioClip(), GetVolume());
        }

        public AudioClip GetAudioClip() => Clips[random.Range(0, Clips.Length - 1)];
        public float GetPitch() => Pitch.SemitoneToPlaybackSpeed(BasePitch + random.Range(PitchRange.x, PitchRange.y));
        public float GetVolume() => random.Range(VolumeRange.x, VolumeRange.y);
    }
}