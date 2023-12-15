using Stupid.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace Stupid.stujam01 {
    public class SoundUtilitiy : SingletonMonoBehaviour<SoundUtilitiy> {
        private Queue<AudioSource> availableSources = new Queue<AudioSource>();
        private List<AudioSource> activeSources = new List<AudioSource>();

        public AudioSource GetSource() {
            if (availableSources.Count <= 0) {
                availableSources.Enqueue(CreateAudioSource());
            }

            AudioSource selectedSource = availableSources.Dequeue();
            selectedSource.enabled = true;
            activeSources.Add(selectedSource);
            return selectedSource;
        }

        private AudioSource CreateAudioSource() {
            GameObject sourceGameObject = new GameObject();
            sourceGameObject.transform.parent = transform;
            return sourceGameObject.AddComponent<AudioSource>();
        }


        private void Update() {
            int s = 0;
            while (s < activeSources.Count) {
                if (activeSources[s].isPlaying) {
                    s++;
                    continue;
                }

                activeSources[s].enabled = false;
                availableSources.Enqueue(activeSources[s]);
                activeSources.RemoveAt(s);
            }
        }
    }

}