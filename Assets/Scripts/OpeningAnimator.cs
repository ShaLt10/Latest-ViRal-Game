using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class OpeningAnimator : MonoBehaviour
{
    // ================== ASSIGN DI INSPECTOR ==================
    [Header("Root References")]
    [SerializeField] private Transform lampRoot;           // parent dari lamp images/sprites (optional)
    [SerializeField] private RectTransform flower;         // bunga (UI RectTransform)
    [SerializeField] private RectTransform leaves;         // daun  (UI RectTransform)
    [SerializeField] private Transform tablet;             // tablet (RectTransform untuk UI, Transform untuk world)
    [SerializeField] private Image fadeOverlay;            // Image hitam untuk fade
    [SerializeField] private Camera mainCamera;            // Camera utama (optional; auto Camera.main)

    [Header("Play Settings")]
    [SerializeField] private bool playOnStart = false;     // kalau true, anim mulai otomatis pada Start
    [SerializeField] private bool allowSkipByTouch = true; // klik/tap utk skip
    [SerializeField] private float globalSpeed = 1f;

    // ================== LAMPS ==================
    [Header("Lamp Blink (optional)")]
    [SerializeField] private Color lampTargetColor = new Color(0.95f, 0.78f, 0.78f, 1f);
    [SerializeField, Range(0f, 5f)]  private float lampBlinkSpeed = 1.2f;
    [SerializeField, Range(0f, 1f)]  private float lampBlinkAmount = 0.8f;

    // ================== FLOWER ==================
    [Header("Flower Fall (UI)")]
    [SerializeField] private Vector2 flowerFallDistance = new Vector2(-80f, -120f);
    [SerializeField] private float   flowerFallDuration = 6f;
    [SerializeField] private AnimationCurve flowerFallCurve = AnimationCurve.EaseInOut(0,0,1,1);
    [SerializeField] private float   flowerSwayAmount = 15f;
    [SerializeField] private float   flowerSwaySpeed  = 0.6f;
    [SerializeField] private float   flowerRotateAmount = 6f;
    [SerializeField] private bool    flowerLoop = true;

    // ================== LEAVES ==================
    [Header("Leaves Fall (UI)")]
    [SerializeField] private Vector2 leavesFallDistance = new Vector2(-50f, -100f);
    [SerializeField] private float   leavesFallDuration = 7f;
    [SerializeField] private AnimationCurve leavesFallCurve = AnimationCurve.EaseInOut(0,0,1,1);
    [SerializeField] private float   leavesSwayAmount = 10f;
    [SerializeField] private float   leavesSwaySpeed  = 0.5f;
    [SerializeField] private float   leavesRotateAmount = 10f;
    [SerializeField] private bool    leavesLoop = true;

    // ================== FOCUS / “TERSEDOT KE TABLET” ==================
    [Header("Focus to Tablet")]
    [Tooltip("Delay sebelum fokus/zoom ke tablet dimulai")]
    [SerializeField] private float focusDelay = 1.5f;
    [Tooltip("Durasi fokus/zoom ke tablet")]
    [SerializeField] private float focusDuration = 1.2f;
    [Tooltip("Seberapa besar zoom camera (0..1). Hanya berlaku bila tablet BUKAN UI Screen Space Overlay")]
    [SerializeField, Range(0f, 0.95f)] private float cameraZoomAmount = 0.5f;
    [SerializeField] private AnimationCurve focusCurve = AnimationCurve.EaseInOut(0,0,1,1);

    [Header("UI Tablet Zoom Sim (jika tablet di Canvas Overlay)")]
    [Tooltip("Offset target posisi (anchoredPosition) saat mensimulasikan zoom pada UI Overlay")]
    [SerializeField] private Vector2 uiFocusOffset = Vector2.zero;
    [Tooltip("Skala target saat mensimulasikan zoom pada UI Overlay")]
    [SerializeField] private Vector3 uiFocusScale = new Vector3(1.2f, 1.2f, 1f);

    // ================== TABLET HIGHLIGHT ==================
    [Header("Tablet Highlight")]
    [SerializeField] private float highlightDelay = 2.6f;
    [SerializeField] private float highlightDuration = 0.9f;
    [SerializeField] private Vector3 highlightScale = new Vector3(1.15f, 1.15f, 1f);
    [SerializeField] private AnimationCurve highlightCurve = AnimationCurve.EaseInOut(0,0,1,1);

    // ================== FADE ==================
    [Header("Fade Overlay")]
    [SerializeField] private float fadeDelay = 3.6f;
    [SerializeField] private float fadeDuration = 0.9f;
    [SerializeField] private Color fadeTargetColor = new Color(0f, 0f, 0f, 1f);
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0,0,1,1);

    // ================== TRANSITION ==================
    [Header("Transition")]
    [SerializeField] private float totalOpeningDuration = 5.5f; // total hingga Complete

    [Header("Events")]
    public UnityEvent OnOpeningComplete; // dipanggil saat selesai

    [Header("Debug")]
    [SerializeField] private bool enableLogs = false;

    // ====== INTERNAL CACHE ======
    private readonly List<Image> _lampImages = new();
    private readonly List<SpriteRenderer> _lampSprites = new();
    private Color[] _lampImgStart, _lampSprStart;

    private Vector2 _flowerStartPos, _leavesStartPos;
    private Vector3 _tabletStartScale;
    private Vector2 _tabletStartAnchoredPos;  // untuk UI Overlay
    private Color _fadeStartColor;

    private Vector3 _camStartPos;
    private float  _camStartSize;             // orthographicSize atau fieldOfView
    private Vector3 _camTargetPos;
    private float  _camTargetSize;

    private float _t0;
    private float _flowerTimer, _leavesTimer;
    private bool _playing = false;
    private bool _uiOverlayMode = false;      // true jika tablet berada di Canvas Screen Space Overlay

    // ================== UNITY LIFE ==================
    private void Awake()
    {
        AutoFind();

        if (!mainCamera) mainCamera = Camera.main;

        // cache lamp colors
        CacheLamps();

        // cache start states
        if (flower) _flowerStartPos = flower.anchoredPosition;
        if (leaves) _leavesStartPos = leaves.anchoredPosition;

        if (tablet)
        {
            _tabletStartScale = tablet.localScale;
            var rt = tablet as RectTransform;
            if (rt) _tabletStartAnchoredPos = rt.anchoredPosition;

            _uiOverlayMode = IsRectOnOverlayCanvas(rt);
            Log($"UI Overlay Mode = {_uiOverlayMode}");
        }

        if (fadeOverlay)
        {
            _fadeStartColor = fadeOverlay.color;
            var c = _fadeStartColor; c.a = 0f;
            fadeOverlay.color = c; // mulai transparan
        }

        if (mainCamera)
        {
            _camStartPos = mainCamera.transform.position;
            _camStartSize = mainCamera.orthographic ? mainCamera.orthographicSize : mainCamera.fieldOfView;

            // target camera jika BUKAN overlay UI
            if (!_uiOverlayMode && tablet)
            {
                _camTargetPos = tablet.position; _camTargetPos.z = _camStartPos.z;
                _camTargetSize = _camStartSize * (1f - cameraZoomAmount);
            }
            else
            {
                _camTargetPos = _camStartPos;
                _camTargetSize = _camStartSize;
            }
        }
    }

    private void Start()
    {
        if (playOnStart) BeginOpening();
    }

    private void Update()
    {
        if (!_playing) return;

        float now = Time.time * globalSpeed;
        float dt  = Time.deltaTime * globalSpeed;
        float el  = (Time.time - _t0) * globalSpeed;

        if (allowSkipByTouch && Input.GetMouseButtonDown(0))
        {
            SkipOpening();
            return;
        }

        // animasi
        AnimateLamps(now);
        AnimateFlower(dt);
        AnimateLeaves(dt);
        AnimateFocus(el);
        AnimateHighlight(el);
        AnimateFade(el);

        // selesai?
        if (el >= totalOpeningDuration)
        {
            FinishOpening();
        }
    }

    // ================== PUBLIC API ==================
    public void BeginOpening()
    {
        _flowerTimer = _leavesTimer = 0f;
        _t0 = Time.time;

        // reset states
        if (flower) flower.anchoredPosition = _flowerStartPos;
        if (leaves) leaves.anchoredPosition = _leavesStartPos;

        if (tablet)
        {
            tablet.localScale = _tabletStartScale;
            if (_uiOverlayMode)
            {
                var rt = tablet as RectTransform;
                if (rt) rt.anchoredPosition = _tabletStartAnchoredPos;
            }
        }

        if (fadeOverlay)
        {
            var c = _fadeStartColor; c.a = 0f; fadeOverlay.color = c;
        }

        if (mainCamera)
        {
            mainCamera.transform.position = _camStartPos;
            if (mainCamera.orthographic) mainCamera.orthographicSize = _camStartSize;
            else mainCamera.fieldOfView = _camStartSize;
        }

        _playing = true;
        Log("Opening started");
    }

    public void SkipOpening()
    {
        if (!_playing) return;

        // set semua ke final
        if (fadeOverlay) fadeOverlay.color = fadeTargetColor;

        if (_uiOverlayMode && tablet)
        {
            var rt = tablet as RectTransform;
            if (rt) rt.anchoredPosition = _tabletStartAnchoredPos + uiFocusOffset;
            tablet.localScale = highlightScale;
        }
        else
        {
            if (mainCamera)
            {
                mainCamera.transform.position = _camTargetPos;
                if (mainCamera.orthographic) mainCamera.orthographicSize = _camTargetSize;
                else mainCamera.fieldOfView = _camTargetSize;
            }
            if (tablet) tablet.localScale = highlightScale;
        }

        FinishOpening();
    }

    // ================== PRIVATE ANIMS ==================
    private void AnimateLamps(float t)
    {
        if (_lampImages.Count == 0 && _lampSprites.Count == 0) return;

        float blink = (Mathf.Sin(t * Mathf.PI * 2f * lampBlinkSpeed) + 1f) * 0.5f; // 0..1
        blink *= lampBlinkAmount;

        for (int i = 0; i < _lampImages.Count; i++)
            if (_lampImages[i])
                _lampImages[i].color = Color.Lerp(_lampImgStart[i], lampTargetColor, blink);

        for (int i = 0; i < _lampSprites.Count; i++)
            if (_lampSprites[i])
                _lampSprites[i].color = Color.Lerp(_lampSprStart[i], lampTargetColor, blink);
    }

    private void AnimateFlower(float dt)
    {
        if (!flower) return;

        _flowerTimer += dt;
        float p = Mathf.Clamp01(_flowerTimer / Mathf.Max(0.0001f, flowerFallDuration));
        float c = flowerFallCurve.Evaluate(p);

        Vector2 fall = flowerFallDistance * c;
        float sway = Mathf.Sin(Time.time * Mathf.PI * 2f * flowerSwaySpeed) * flowerSwayAmount * (1f - p * 0.3f);
        Vector2 pos = _flowerStartPos + fall + new Vector2(sway, 0f);
        flower.anchoredPosition = pos;

        float rot = Mathf.Sin(Time.time * 2f) * flowerRotateAmount * c + sway * 0.1f;
        flower.localRotation = Quaternion.Euler(0, 0, rot);

        if (p >= 1f && flowerLoop) _flowerTimer = 0f;
    }

    private void AnimateLeaves(float dt)
    {
        if (!leaves) return;

        _leavesTimer += dt;
        float p = Mathf.Clamp01(_leavesTimer / Mathf.Max(0.0001f, leavesFallDuration));
        float c = leavesFallCurve.Evaluate(p);

        Vector2 fall = leavesFallDistance * c;
        float sway = Mathf.Sin(Time.time * Mathf.PI * 2f * leavesSwaySpeed) * leavesSwayAmount * (1f - p * 0.25f);
        Vector2 pos = _leavesStartPos + fall + new Vector2(sway, 0f);
        leaves.anchoredPosition = pos;

        float rot = Mathf.Sin(Time.time * 1.7f) * leavesRotateAmount * c + sway * 0.12f;
        leaves.localRotation = Quaternion.Euler(0, 0, rot);

        if (p >= 1f && leavesLoop) _leavesTimer = 0f;
    }

    private void AnimateFocus(float elapsed)
    {
        if (elapsed < focusDelay) return;

        float t = Mathf.Clamp01((elapsed - focusDelay) / Mathf.Max(0.0001f, focusDuration));
        float k = focusCurve.Evaluate(t);

        if (_uiOverlayMode)
        {
            // tablet UI di Canvas Overlay → simulasikan zoom dengan scale + offset posisi
            if (!tablet) return;
            var rt = tablet as RectTransform;
            if (rt) rt.anchoredPosition = Vector2.Lerp(_tabletStartAnchoredPos, _tabletStartAnchoredPos + uiFocusOffset, k);
            tablet.localScale = Vector3.Lerp(_tabletStartScale, uiFocusScale, k);
        }
        else
        {
            // objek dunia → pakai kamera (pos + size/FOV)
            if (!mainCamera || !tablet) return;

            Vector3 pos = Vector3.Lerp(_camStartPos, _camTargetPos, k);
            mainCamera.transform.position = pos;

            float size = Mathf.Lerp(_camStartSize, _camTargetSize, k);
            if (mainCamera.orthographic) mainCamera.orthographicSize = size;
            else mainCamera.fieldOfView = size;
        }
    }

    private void AnimateHighlight(float elapsed)
    {
        if (!tablet || elapsed < highlightDelay) return;

        float t = Mathf.Clamp01((elapsed - highlightDelay) / Mathf.Max(0.0001f, highlightDuration));
        float k = highlightCurve.Evaluate(t);

        tablet.localScale = Vector3.Lerp(_tabletStartScale, highlightScale, k);
    }

    private void AnimateFade(float elapsed)
    {
        if (!fadeOverlay || elapsed < fadeDelay) return;

        float t = Mathf.Clamp01((elapsed - fadeDelay) / Mathf.Max(0.0001f, fadeDuration));
        float k = fadeCurve.Evaluate(t);

        fadeOverlay.color = Color.Lerp(_fadeStartColor, fadeTargetColor, k);
    }

    private void FinishOpening()
    {
        if (!_playing) return;
        _playing = false;

        Log("Opening complete");
        OnOpeningComplete?.Invoke();
    }

    // ================== HELPERS ==================
    private void AutoFind()
    {
        if (!mainCamera) mainCamera = Camera.main;

        // cari lampu
        _lampImages.Clear(); _lampSprites.Clear();
        if (lampRoot)
        {
            _lampImages.AddRange(lampRoot.GetComponentsInChildren<Image>(true));
            _lampSprites.AddRange(lampRoot.GetComponentsInChildren<SpriteRenderer>(true));
        }
    }

    private void CacheLamps()
    {
        _lampImgStart = new Color[_lampImages.Count];
        for (int i = 0; i < _lampImages.Count; i++) _lampImgStart[i] = _lampImages[i].color;

        _lampSprStart = new Color[_lampSprites.Count];
        for (int i = 0; i < _lampSprites.Count; i++) _lampSprStart[i] = _lampSprites[i].color;

        Log($"Lamp caches: img={_lampImages.Count}, spr={_lampSprites.Count}");
    }

    private bool IsRectOnOverlayCanvas(RectTransform rt)
    {
        if (!rt) return false;
        var cv = rt.GetComponentInParent<Canvas>();
        return cv && cv.renderMode == RenderMode.ScreenSpaceOverlay;
    }

    private void Log(string s)
    {
        if (enableLogs) Debug.Log($"[OpeningAnimator] {s}");
    }
}
