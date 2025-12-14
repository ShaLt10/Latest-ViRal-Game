using UnityEngine;

public class InstagramFeedManager : SingletonDestroy<InstagramFeedManager>
{
    [SerializeField] private IntagramFeedQuestionData questionDatas; // (pakai nama tipe yg kamu pakai sekarang)

    private void Awake()
    {
        // Minigame ini portrait
        ScreenRotateControl.Instance.SetPortrait();
    }

    private void OnEnable()
    {
        EventManager.Subscribe<GetQuestion>(OnGetQuestion);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<GetQuestion>(OnGetQuestion);
    }

    private void Start()
    {
        // Mainkan opening dulu, lalu mulai soal pertama
        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.PlaySequenceThen(DialoguesNames.InstagramFeed_Opening, () =>
            {
                GetDataQuestion(0);
            });
        }
        else
        {
            Debug.LogWarning("DialogManager.Instance null di InstagramFeedManager. Mulai tanpa dialog.");
            GetDataQuestion(0);
        }
    }

    public void GetDataQuestion(int questionIndex)
    {
        if (questionDatas == null)
        {
            Debug.LogError("InstagramFeedManager: questionDatas belum di-assign!");
            return;
        }

        var q = questionDatas.GetQuestion(questionIndex);
        EventManager.Publish(new QuestionData(q));
    }

    private void OnGetQuestion(GetQuestion req)
    {
        GetDataQuestion(req.index);
    }
}
