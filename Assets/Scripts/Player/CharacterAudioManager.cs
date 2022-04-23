using UnityEngine;
using System.Collections;
using System;

namespace TPS_Redux
{
    public class CharacterAudioManager : MonoBehaviour
    {
        public AudioSource gunSounds;
        public AudioSource runFoley;

        public float footStepTimer;
        public float walkThresHold;
        public float runThreshold;
        public AudioSource footStep1;
        public AudioSource footStep2;
        public AudioClip[] footStepClips;
        public AudioSource effectsSource;
        public AudioClipList[] effectsList;
        StatesManager statesManager;

        float startingVolumeRun;
        float characterMovement;

        void Start()
        {
            statesManager = GetComponent<StatesManager>();
            startingVolumeRun = runFoley.volume;

            runFoley.volume = 0;
        }

        private void Update()
        {
            characterMovement = Mathf.Abs(statesManager.horizontal) + Math.Abs(statesManager.vertical);
            characterMovement = Mathf.Clamp01(characterMovement);

            float targetThreshold = 0;

            if(!statesManager.isWalking && !statesManager.isAiming && !statesManager.isReloading)
            {
                runFoley.volume = startingVolumeRun * characterMovement;
                targetThreshold = runThreshold;
            }
            else
            {
                targetThreshold = walkThresHold;

                runFoley.volume = Mathf.Lerp(runFoley.volume, 0, Time.deltaTime * 2);
            }

            if(characterMovement > 0)
            {
                footStepTimer += Time.deltaTime;

                if(footStepTimer > targetThreshold)
                {
                    PlayFootStep();
                    footStepTimer = 0;
                }
            }
            else
            {
                footStep1.Stop();
                footStep2.Stop();
            }
        }

        private void PlayFootStep()
        {
            throw new NotImplementedException();
        }

        internal void PlayGunSound()
        {
            throw new NotImplementedException();
        }

        internal void PlayEffect(string v)
        {
            throw new NotImplementedException();
        }

        
    }
    [System.Serializable]
    public class AudioClipList
    {
        public string name;
        public AudioClip clip;
    }
}