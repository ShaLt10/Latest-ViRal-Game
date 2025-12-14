using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuestionController : MonoBehaviour
{
    [SerializeField] private Button fakta;
    [SerializeField] private Button opini;

    private InstagramQuestionData questionData;

    [SerializeField] private TMP_Text question;
    [SerializeField] private Image potraitImage;

    private int score = 0;
    private int count = 0;

    private void OnEnable()
    {
        fakta.onClick.AddListener(() => Answer(1));
        opini.onClick.AddListener(() => Answer(0));
        EventManager.Subscribe<QuestionData>(GetQuestion);
    }

    private void OnDisable()
    {
        fakta.onClick.RemoveAllListeners();
        opini.onClick.RemoveAllListeners();
        EventManager.Unsubscribe<QuestionData>(GetQuestion);
    }

    private void Answer(int i)
    {
        count++;

        // 0 = opini, 1 = fakta
        if (i == 0)
        {
            if (!questionData.Fact) score += 2000;
        }
        else // i == 1
        {
            if (questionData.Fact) score += 2000;
        }

        if (count < 6)
        {
            EventManager.Publish(new GetQuestion(count));
            return;
        }

        // Selesai 6 soal → tentukan dialog & lanjutannya
        var scene = SceneManager.GetActiveScene();
        string dialogKey = score >= 12000
            ? DialoguesNames.InstagramFeed_Win          // ganti ke DialogName.InstagramFeed_Win kalau enum/const kamu sudah diganti
            : DialoguesNames.SpotTheDifference_Lose;    // (key lose khusus scene ini; silakan ganti ke key yang sesuai)

        // Mainkan dialog dulu, lalu callback pindah scene
        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.PlaySequenceThen(dialogKey, () =>
            {
                if (score >= 12000)
                    SceneManager.LoadScene(scene.buildIndex + 1);
                else
                    SceneManager.LoadScene(scene.buildIndex); // retry/ulang
            });
        }
        else
        {
            // Fallback kalau DialogManager belum ada (biar nggak buntu)
            Debug.LogWarning("DialogManager.Instance null. Melanjutkan tanpa dialog.");
            if (score >= 12000)
                SceneManager.LoadScene(scene.buildIndex + 1);
            else
                SceneManager.LoadScene(scene.buildIndex);
        }
    }

    private void GetQuestion(QuestionData data)
    {
        questionData = new InstagramQuestionData(data.data);
        if (question != null) question.SetText(questionData.Question);
        if (potraitImage != null) potraitImage.sprite = questionData.image;
    }
}
