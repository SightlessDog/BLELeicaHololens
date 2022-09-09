using UnityEngine;

    public class SoundManager : Singleton<SoundManager>
    {
        private void Awake()
        {
            ButtonHandlers.onStateChanged += OnStateChanged;
        }
        
        void OnDisable()
        {
            ButtonHandlers.onStateChanged -= OnStateChanged;
        }
        
        private void OnStateChanged(State state)
        {
            if (state == State.STARTED || state == State.CONNECTED || state == State.ENUMERATED)
            {
                GetComponent<AudioSource>().Play();
            }
        }
    }
