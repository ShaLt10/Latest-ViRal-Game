using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the Fact vs Opinion game logic, UI, and gameplay flow.
/// </summary>
public class FactOpinionManager : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI viewerCountText;
    public TextMeshProUGUI currentCommentText;
    public TextMeshProUGUI alunaDialogText;
    public TextMeshProUGUI systemNarratorText;
    public Button faktaButton;
    public Button opiniButton;
    public Button restartButton;

    [Header("Popups")]
    public GameObject successPopup;
    public GameObject failurePopup;
    public TextMeshProUGUI successPopupText;
    public TextMeshProUGUI failurePopupText;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip successClip;
    public AudioClip failClip;
    public AudioClip celebrationClip;

    [Header("Animation")]
    public ViewerCountAnimator viewerCountAnimator;
    public ParticleSystem successParticles;

    private int currentViewerCount = 0;
    private int currentCommentIndex = 0;
    private int correctAnswers = 0;
    private bool gameActive = false;
    private bool gameStarted = false;

    [System.Serializable]
    public class Comment
    {
        public string text;
        public bool isFakta;
        public string explanation;
    }

    // Sample comment dataset
    private Comment[] comments = new Comment[]
    {
        new Comment { text = "Akun asli @genz.berdampak beda sama yang di-share Aluna", isFakta = true,
            explanation = "Ini adalah fakta karena dapat diverifikasi dengan membandingkan kedua akun" },

        new Comment { text = "Menurutku Aluna harusnya lebih hati-hati sebelum share", isFakta = false,
            explanation = "Ini opini karena menggunakan kata 'menurutku' dan berisi penilaian subjektif" },

        new Comment { text = "Form pendaftaran mereka minta data berlebihan beyond normal requirements", isFakta = true,
            explanation = "Ini fakta karena dapat diverifikasi dengan melihat form pendaftaran" },

        new Comment { text = "Influencer sekarang tidak bisa dipercaya lagi", isFakta = false,
            explanation = "Ini opini karena berisi generalisasi dan penilaian subjektif" },

        new Comment { text = "Akun resmi udah announce ke berita mainstream kalau segala informasi valid hanya di akun resmi", isFakta = true,
            explanation = "Ini fakta karena dapat diverifikasi melalui berita mainstream" },

        new Comment { text = "Mungkin ini cuma salah paham doang", isFakta = false,
            explanation = "Ini opini karena menggunakan kata 'mungkin' dan berisi spekulasi" }
    };

    private void Start()
    {
        InitializeGame();
        StartCoroutine(PlayOpeningSequence());
    }

    private void InitializeGame()
    {
        currentViewerCount = 0;
        currentCommentIndex = 0;
        correctAnswers = 0;
        gameActive = false;
        gameStarted = false;

        UpdateViewerCount();

        faktaButton.onClick.RemoveAllListeners();
        opiniButton.onClick.RemoveAllListeners();
        restartButton.onClick.RemoveAllListeners();
        faktaButton.onClick.AddListener(() => OnAnswerSelected(true));
        opiniButton.onClick.AddListener(() => OnAnswerSelected(false));
        restartButton.onClick.AddListener(RestartGame);

        faktaButton.gameObject.SetActive(false);
        opiniButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        successPopup.SetActive(false);
        failurePopup.SetActive(false);
        currentCommentText.text = "";
    }

    private IEnumerator PlayOpeningSequence()
    {
        alunaDialogText.text = "Takes deep breath, starts live stream \"Halo semua! Klarifikasi darurat tentang post volunteer yang aku share kemarin...\"";
        yield return new WaitForSeconds(3f);

        systemNarratorText.text = "LIVE CHALLENGE: Bantu Aluna merespons komentar viewer! Bedakan antara FAKTA dan OPINI!";
        yield return new WaitForSeconds(2f);

        StartGame();
    }

    private void StartGame()
    {
        gameActive = true;
        gameStarted = true;

        faktaButton.gameObject.SetActive(true);
        opiniButton.gameObject.SetActive(true);

        alunaDialogText.text = "";
        DisplayCurrentComment();
    }

    private void DisplayCurrentComment()
    {
        if (currentCommentIndex < comments.Length)
        {
            currentCommentText.text = $"\"{comments[currentCommentIndex].text}\"";
            systemNarratorText.text = "Pilih FAKTA atau OPINI untuk komentar di atas!";

            faktaButton.interactable = true;
            opiniButton.interactable = true;
        }
    }

    public void OnAnswerSelected(bool selectedFakta)
    {
        if (!gameActive || currentCommentIndex >= comments.Length) return;

        faktaButton.interactable = false;
        opiniButton.interactable = false;

        bool isCorrect = (selectedFakta == comments[currentCommentIndex].isFakta);

        if (isCorrect)
        {
            correctAnswers++;
            int oldCount = currentViewerCount;
            currentViewerCount += 2000;
            StartCoroutine(viewerCountAnimator.AnimateCountIncrease(oldCount, currentViewerCount));
            StartCoroutine(HandleCorrectAnswer());
        }
        else
        {
            StartCoroutine(HandleIncorrectAnswer());
        }
    }

    private IEnumerator HandleCorrectAnswer()
    {
        if (audioSource && successClip) audioSource.PlayOneShot(successClip);
        if (successParticles) successParticles.Play();
        systemNarratorText.text = "Benar! +2k viewers!";
        yield return new WaitForSeconds(1.5f);
        MoveToNextComment();
    }

    private IEnumerator HandleIncorrectAnswer()
    {
        if (audioSource && failClip) audioSource.PlayOneShot(failClip);
        systemNarratorText.text = $"Salah! {comments[currentCommentIndex].explanation}";
        yield return new WaitForSeconds(2f);
        MoveToNextComment();
    }

    private void MoveToNextComment()
    {
        currentCommentIndex++;
        if (currentCommentIndex >= comments.Length)
        {
            EndGame();
        }
        else
        {
            DisplayCurrentComment();
        }
    }

    private void UpdateViewerCount()
    {
        viewerCountText.text = $"{currentViewerCount:N0} viewers";
    }

    private void EndGame()
    {
        gameActive = false;
        currentCommentText.text = "";
        faktaButton.gameObject.SetActive(false);
        opiniButton.gameObject.SetActive(false);
        StartCoroutine(ShowGameResult());
    }

    private IEnumerator ShowGameResult()
    {
        yield return new WaitForSeconds(1f);

        if (correctAnswers == comments.Length)
        {
            if (audioSource && celebrationClip) audioSource.PlayOneShot(celebrationClip);
            systemNarratorText.text = "Wih, congrats! Kalian berhasil spreading awareness kepada masyarakat soal isu ini, keep it up!";
            yield return new WaitForSeconds(3f);

            successPopupText.text = $"PERFECT SCORE!\n{currentViewerCount:N0} viewers reached!\nSemua jawaban benar!";
            successPopup.SetActive(true);
        }
        else
        {
            if (audioSource && failClip) audioSource.PlayOneShot(failClip);
            systemNarratorText.text = "Aduhh.. Yang nonton masih dikit nihh, masih banyak yang belum tahu informasi penting ini. Yuk, coba lagi!";
            yield return new WaitForSeconds(3f);

            failurePopupText.text = $"Skor: {correctAnswers}/{comments.Length}\n{currentViewerCount:N0} viewers reached\nCoba lagi untuk hasil yang lebih baik!";
            failurePopup.SetActive(true);
            restartButton.gameObject.SetActive(true);
        }
    }

    public void RestartGame()
    {
        successPopup.SetActive(false);
        failurePopup.SetActive(false);
        restartButton.gameObject.SetActive(false);
        InitializeGame();
        StartCoroutine(PlayOpeningSequence());
    }

    public void ShowHint()
    {
        if (currentCommentIndex < comments.Length && gameActive)
        {
            string hint = comments[currentCommentIndex].isFakta ?
                "Hint: Apakah ini bisa diverifikasi?" :
                "Hint: Apakah ini berisi penilaian subjektif?";
            StartCoroutine(ShowHintText(hint));
        }
    }

    private IEnumerator ShowHintText(string hint)
    {
        string originalText = systemNarratorText.text;
        systemNarratorText.text = hint;
        yield return new WaitForSeconds(3f);
        systemNarratorText.text = originalText;
    }
}

[System.Serializable]
public class ViewerCountAnimator
{
    public TextMeshProUGUI viewerText;
    public float animationDuration = 0.5f;
    public float scalePunch = 1.2f;
    public float fadeDuration = 0.2f;

    public IEnumerator AnimateCountIncrease(int fromValue, int toValue)
    {
        if (viewerText == null) yield break;

        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / animationDuration);
            int currentValue = Mathf.RoundToInt(Mathf.Lerp(fromValue, toValue, progress));
            viewerText.text = $"{currentValue:N0} viewers";

            float scale = Mathf.Lerp(1f, scalePunch, Mathf.Sin(progress * Mathf.PI));
            viewerText.rectTransform.localScale = new Vector3(scale, scale, 1f);

            yield return null;
        }

        viewerText.text = $"{toValue:N0} viewers";
        viewerText.rectTransform.localScale = Vector3.one;

        Color originalColor = viewerText.color;
        viewerText.color = Color.yellow;
        yield return new WaitForSeconds(fadeDuration);
        viewerText.color = originalColor;
    }
}