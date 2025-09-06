using System.Collections;
using UnityEngine;

namespace DigitalForensicsQuiz
{
    public class MinigameAudioManager : MonoBehaviour
    {
        private static MinigameAudioManager _instance;
        public static MinigameAudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<MinigameAudioManager>();
                    if (_instance == null)
                    {
                        GameObject audioManagerGO = new GameObject("MinigameAudioManager");
                        _instance = audioManagerGO.AddComponent<MinigameAudioManager>();
                        DontDestroyOnLoad(audioManagerGO);
                    }
                }
                return _instance;
            }
        }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource typewriterSource;

        [Header("Music")]
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private AudioClip gameStartMusic;
        [SerializeField] private AudioClip gameEndMusic;
        [SerializeField] private AudioClip victoryMusic;
        [SerializeField] private AudioClip defeatMusic;

        [Header("UI Sound Effects")]
        [SerializeField] private AudioClip buttonClickSFX;
        [SerializeField] private AudioClip correctAnswerSFX;
        [SerializeField] private AudioClip incorrectAnswerSFX;
        [SerializeField] private AudioClip glitchSFX;
        [SerializeField] private AudioClip typewriterSFX;
        [SerializeField] private AudioClip menuReturnSFX;

        [Header("Drag and Drop SFX")]
        [SerializeField] private AudioClip dragStartSFX;
        [SerializeField] private AudioClip dragMoveSFX;
        [SerializeField] private AudioClip dropSuccessSFX;
        [SerializeField] private AudioClip dropFailSFX;
        [SerializeField] private AudioClip dropZoneEnterSFX;
        [SerializeField] private AudioClip dropZoneExitSFX;
        [SerializeField] private AudioClip envelopeCloseSFX;

        [Header("Volume Settings")]
        [Range(0f, 1f)]
        [SerializeField] private float musicVolume = 0.7f;
        [Range(0f, 1f)]
        [SerializeField] private float sfxVolume = 0.8f;
        [Range(0f, 1f)]
        [SerializeField] private float typewriterVolume = 0.5f;

        [Header("Audio Fade Settings")]
        [SerializeField] private float fadeInDuration = 1f;
        [SerializeField] private float fadeOutDuration = 0.5f;

        private Coroutine typewriterCoroutine;
        private bool isInitialized = false;

        #region Initialization

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioSources();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (!isInitialized)
            {
                InitializeAudioManager();
            }
        }

        private void InitializeAudioSources()
        {
            // Create audio sources if they don't exist
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
                musicSource.volume = musicVolume;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
                sfxSource.volume = sfxVolume;
            }

            if (typewriterSource == null)
            {
                typewriterSource = gameObject.AddComponent<AudioSource>();
                typewriterSource.loop = true;
                typewriterSource.playOnAwake = false;
                typewriterSource.volume = typewriterVolume;
            }
        }

        private void InitializeAudioManager()
        {
            // Generate fallback audio clips if needed
            GenerateFallbackAudioClips();
            
            // Start background music
            PlayBackgroundMusic();
            
            isInitialized = true;
            Debug.Log("MinigameAudioManager initialized successfully");
        }

        private void GenerateFallbackAudioClips()
        {
            // Generate simple beep sounds for missing clips
            if (buttonClickSFX == null)
                buttonClickSFX = GenerateBeep(440f, 0.1f);
            
            if (correctAnswerSFX == null)
                correctAnswerSFX = GenerateBeep(880f, 0.3f);
                
            if (incorrectAnswerSFX == null)
                incorrectAnswerSFX = GenerateBeep(220f, 0.5f);
                
            if (dragStartSFX == null)
                dragStartSFX = GenerateBeep(330f, 0.15f);
                
            if (dropSuccessSFX == null)
                dropSuccessSFX = GenerateBeep(660f, 0.2f);
                
            if (dropFailSFX == null)
                dropFailSFX = GenerateBeep(165f, 0.3f);
                
            if (typewriterSFX == null)
                typewriterSFX = GenerateBeep(1000f, 0.05f);
        }

        private AudioClip GenerateBeep(float frequency, float duration)
        {
            int sampleRate = 44100;
            int sampleLength = Mathf.RoundToInt(sampleRate * duration);
            
            AudioClip beep = AudioClip.Create("GeneratedBeep", sampleLength, 1, sampleRate, false);
            float[] samples = new float[sampleLength];
            
            for (int i = 0; i < sampleLength; i++)
            {
                float time = (float)i / sampleRate;
                samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * time) * 0.1f;
            }
            
            beep.SetData(samples, 0);
            return beep;
        }

        #endregion

        #region Music Control

        public void PlayBackgroundMusic()
        {
            if (backgroundMusic != null && musicSource != null)
            {
                musicSource.clip = backgroundMusic;
                musicSource.Play();
                StartCoroutine(FadeIn(musicSource, fadeInDuration));
            }
        }

        public void OnGameStart()
        {
            if (gameStartMusic != null)
            {
                PlayMusic(gameStartMusic, false);
            }
            else
            {
                PlaySFX(buttonClickSFX);
            }
        }

        public void OnGameEnd(bool victory)
        {
            AudioClip clipToPlay = victory ? victoryMusic : defeatMusic;
            if (clipToPlay == null)
                clipToPlay = gameEndMusic;
                
            if (clipToPlay != null)
            {
                PlayMusic(clipToPlay, false);
            }
        }

        public void OnMenuReturn()
        {
            PlaySFX(menuReturnSFX ?? buttonClickSFX);
            StartCoroutine(ReturnToBackgroundMusic());
        }

        private void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip != null && musicSource != null)
            {
                StartCoroutine(FadeOut(musicSource, fadeOutDuration, () =>
                {
                    musicSource.clip = clip;
                    musicSource.loop = loop;
                    musicSource.Play();
                    StartCoroutine(FadeIn(musicSource, fadeInDuration));
                }));
            }
        }

        private IEnumerator ReturnToBackgroundMusic()
        {
            yield return new WaitForSeconds(1f);
            PlayBackgroundMusic();
        }

        #endregion

        #region SFX Control

        public void PlayButtonClickSFX()
        {
            PlaySFX(buttonClickSFX);
        }

        public void OnQuestionCorrect()
        {
            PlaySFX(correctAnswerSFX);
        }

        public void OnQuestionIncorrect()
        {
            PlaySFX(incorrectAnswerSFX);
        }

        public void PlayGlitchSFX()
        {
            PlaySFX(glitchSFX);
        }

        private void PlaySFX(AudioClip clip)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip);
            }
        }

        #endregion

        #region Drag and Drop SFX

        public void OnDragStart()
        {
            PlaySFX(dragStartSFX);
        }

        public void OnDragMove()
        {
            // Play subtle drag move sound (optional, can be disabled for performance)
            if (dragMoveSFX != null && Time.time % 0.1f < 0.05f)
            {
                PlaySFX(dragMoveSFX);
            }
        }

        public void OnDropSuccess()
        {
            PlaySFX(dropSuccessSFX);
        }

        public void OnDropFail()
        {
            PlaySFX(dropFailSFX);
        }

        public void OnDropZoneEnter()
        {
            PlaySFX(dropZoneEnterSFX);
        }

        public void OnDropZoneExit()
        {
            PlaySFX(dropZoneExitSFX);
        }

        public void OnEnvelopeClose()
        {
            PlaySFX(envelopeCloseSFX);
        }

        public void PlayDragDropSFX()
        {
            OnDropSuccess();
        }

        #endregion

        #region Typewriter Effects

        public void StartTypewriterSFX(float duration, float interval)
        {
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
            }
            
            typewriterCoroutine = StartCoroutine(TypewriterSoundCoroutine(duration, interval));
        }

        public void StopTypewriterSFX()
        {
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = null;
            }
            
            if (typewriterSource != null && typewriterSource.isPlaying)
            {
                typewriterSource.Stop();
            }
        }

        private IEnumerator TypewriterSoundCoroutine(float duration, float interval)
        {
            if (typewriterSFX != null && typewriterSource != null)
            {
                typewriterSource.clip = typewriterSFX;
                typewriterSource.Play();
                
                yield return new WaitForSeconds(duration);
                
                typewriterSource.Stop();
            }
        }

        #endregion

        #region Volume Control

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null)
            {
                musicSource.volume = musicVolume;
            }
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            if (sfxSource != null)
            {
                sfxSource.volume = sfxVolume;
            }
        }

        public void SetTypewriterVolume(float volume)
        {
            typewriterVolume = Mathf.Clamp01(volume);
            if (typewriterSource != null)
            {
                typewriterSource.volume = typewriterVolume;
            }
        }

        public void MuteAll()
        {
            SetMusicVolume(0f);
            SetSFXVolume(0f);
            SetTypewriterVolume(0f);
        }

        public void UnmuteAll()
        {
            SetMusicVolume(0.7f);
            SetSFXVolume(0.8f);
            SetTypewriterVolume(0.5f);
        }

        #endregion

        #region Audio Fade Effects

        private IEnumerator FadeIn(AudioSource source, float duration)
        {
            if (source == null) yield break;
            
            float startVolume = 0f;
            float targetVolume = source == musicSource ? musicVolume : 
                               source == sfxSource ? sfxVolume : typewriterVolume;
            
            source.volume = startVolume;
            
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / duration);
                yield return null;
            }
            
            source.volume = targetVolume;
        }

        private IEnumerator FadeOut(AudioSource source, float duration, System.Action onComplete = null)
        {
            if (source == null)
            {
                onComplete?.Invoke();
                yield break;
            }
            
            float startVolume = source.volume;
            float targetVolume = 0f;
            
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / duration);
                yield return null;
            }
            
            source.volume = targetVolume;
            source.Stop();
            onComplete?.Invoke();
        }

        #endregion

        #region Debug and Validation

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void OnValidate()
        {
            if (musicSource != null)
                musicSource.volume = musicVolume;
            if (sfxSource != null)
                sfxSource.volume = sfxVolume;
            if (typewriterSource != null)
                typewriterSource.volume = typewriterVolume;
        }

        public void TestAllSounds()
        {
            StartCoroutine(TestSoundsSequence());
        }

        private IEnumerator TestSoundsSequence()
        {
            Debug.Log("Testing all audio clips...");
            
            PlayButtonClickSFX();
            yield return new WaitForSeconds(0.5f);
            
            OnQuestionCorrect();
            yield return new WaitForSeconds(0.5f);
            
            OnQuestionIncorrect();
            yield return new WaitForSeconds(0.5f);
            
            OnDragStart();
            yield return new WaitForSeconds(0.5f);
            
            OnDropSuccess();
            yield return new WaitForSeconds(0.5f);
            
            OnDropFail();
            yield return new WaitForSeconds(0.5f);
            
            Debug.Log("Audio test completed!");
        }

        #endregion
    }
}