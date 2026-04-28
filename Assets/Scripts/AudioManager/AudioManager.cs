using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace IncrementalRPG.Scripts.AudioManager
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _hitAudioClip;
        [SerializeField] private AudioClip _waveAudioClip;
        [SerializeField] private AudioClip _uiHoverAudioClip;
        [SerializeField] private AudioClip _uiClickAudioClip;

        public void PlayHitAudio(float delay = 0f)
        {
            if (delay <= 0f)
                PlayImmediate();
            else
                StartCoroutine(PlayDelayed(delay));
        }

        public void PlayWaveAudio()
        {
            _audioSource.PlayOneShot(_waveAudioClip);
        }

        public void PlayUiHover()
        {
            PlayUiOneShot(_uiHoverAudioClip);
        }

        public void PlayUiClick()
        {
            PlayUiOneShot(_uiClickAudioClip);
        }

        private IEnumerator PlayDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            PlayImmediate();
        }

        private void PlayImmediate()
        {
            _audioSource.pitch = Random.Range(0.85f, 1.2f);
            _audioSource.PlayOneShot(_hitAudioClip);
        }

        private void PlayUiOneShot(AudioClip clip)
        {
            if (clip == null) return;

            _audioSource.pitch = 1f;
            _audioSource.PlayOneShot(clip);
        }
    }
}
