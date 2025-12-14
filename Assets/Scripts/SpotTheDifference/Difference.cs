using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class Difference : MonoBehaviour, IPointerClickHandler
{
    [Header("Stage")]
    [SerializeField] private int stageIndex = 1;

    [Header("Visual")]
    [SerializeField] private CanvasGroup cg;      // dipakai buat fade
    [SerializeField] private float fadeDuration = 0.25f;

    [Header("Linked Spots (opsional)")]
    [SerializeField] private Difference[] linkedDifferences;

    private bool isClicked = false;

    private void Awake()
    {
        if (!cg) cg = GetComponent<CanvasGroup>();
        if (cg) cg.alpha = 1f;
    }

    private void OnEnable()
    {
        EventManager.Subscribe<ResetGameData>(OnReset);
        EventManager.Subscribe<SpotTheDifferenceStatusData>(OnStatus);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<ResetGameData>(OnReset);
        EventManager.Unsubscribe<SpotTheDifferenceStatusData>(OnStatus);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isClicked) return;

        isClicked = true;

        // Fade out spot
        if (cg) cg.DOFade(0f, fadeDuration);

        // Publish progress (+1)
        EventManager.Publish(new DifferenceSpottedData(1, stageIndex));

        // optional: auto-clear link yang ikut satu pasang
        if (linkedDifferences != null)
        {
            foreach (var d in linkedDifferences)
            {
                if (d && !d.isClicked) d.AutoResolveLinked();
            }
        }
    }

    private void AutoResolveLinked()
    {
        isClicked = true;
        if (cg) cg.DOFade(0f, fadeDuration);
        // tidak double-kirim event untuk linked (biar tetap +1 total dari pasangan)
    }

    private void OnReset(ResetGameData r)
    {
        if (r.stageIndex != stageIndex) return;
        isClicked = false;
        if (cg) cg.DOFade(1f, 0f);
    }

    private void OnStatus(SpotTheDifferenceStatusData s)
    {
        if (s.stageIndex != stageIndex) return;
        if (s.gameFinished && !s.started)
        {
            // stage selesai → kunci klik
            isClicked = true;
        }
        else if (s.started && !s.gameFinished)
        {
            // stage dimulai → buka klik
            isClicked = false;
            if (cg) cg.alpha = 1f;
        }
    }
}
