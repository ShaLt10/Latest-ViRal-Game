using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class OpeningAnimator : MonoBehaviour
{
    [Header("=== Roots ===")]
    [SerializeField] private Transform lampRoot;
    [SerializeField] private RectTransform flower;    
    [SerializeField] private RectTransform leaves;    
    [SerializeField] private Transform characterRoot;
    [SerializeField] private Transform tablet;        
    [SerializeField] private Image fadeOverlay;       
    [SerializeField] private Camera mainCamera;       

    [Header("=== Character Parts ===")]
    [SerializeField] private Transform head;
    [SerializeField] private Transform eyes;
    [SerializeField] private Transform frontHair;
    [SerializeField] private Transform backHair;
    [SerializeField] private Transform frontHand;
    [SerializeField] private Transform backHand;
    [SerializeField] private Transform leftLeg;
    [SerializeField] private Transform rightLeg;

    [Header("=== Play Settings ===")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private float globalSpeed = 1f;
    [SerializeField] private bool allowSkipByTouch = true;  // Skip dengan tap layar

    // ------------------ Lamps ------------------
    [Header("Lamp Blink")]
    [SerializeField] private Color lampTargetColor = new Color(0.95f, 0.78f, 0.78f, 1f);
    [SerializeField, Range(0f, 3f)] private float lampBlinkSpeed = 1.2f;
    [SerializeField, Range(0f, 1f)] private float lampBlinkAmount = 0.8f;

    // ------------------ Falling Elements -----------------
    [Header("Flower Fall")]
    [SerializeField] private Vector2 flowerFallDistance = new Vector2(-80f, -120f);
    [SerializeField] private float flowerFallDuration = 6f;
    [SerializeField] private AnimationCurve flowerFallCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float flowerSwayAmount = 15f;    
    [SerializeField] private float flowerSwaySpeed = 0.6f;
    [SerializeField] private float flowerRotateAmount = 6f;   
    [SerializeField] private bool flowerLoop = true;

    [Header("Leaves Fall")]
    [SerializeField] private Vector2 leavesFallDistance = new Vector2(-50f, -100f);
    [SerializeField] private float leavesFallDuration = 7f;
    [SerializeField] private AnimationCurve leavesFallCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float leavesSwayAmount = 10f;
    [SerializeField] private float leavesSwaySpeed = 0.5f;
    [SerializeField] private float leavesRotateAmount = 10f;
    [SerializeField] private bool leavesLoop = true;

    // ------------------ Camera Focus ------------------
    [Header("Camera Focus to Tablet")]
    [SerializeField] private float cameraFocusDelay = 2f;        
    [SerializeField] private float cameraFocusDuration = 1.5f;   
    [SerializeField] private float cameraZoomAmount = 0.7f;      
    [SerializeField] private Vector3 cameraTargetOffset = Vector3.zero; 
    [SerializeField] private AnimationCurve cameraFocusCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // ------------------ Tablet Highlight ------------------
    [Header("Tablet Highlight Effect")]
    [SerializeField] private float tabletHighlightDelay = 2.2f;  
    [SerializeField] private float tabletHighlightDuration = 1f;
    [SerializeField] private Vector3 tabletHighlightScale = new Vector3(1.1f, 1.1f, 1f);  
    [SerializeField] private AnimationCurve tabletHighlightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // ------------------ Fade Overlay ------------------
    [Header("Fade Overlay")]
    [SerializeField] private float fadeDelay = 4.5f;            
    [SerializeField] private float fadeDuration = 2f;           
    [SerializeField] private Color fadeTargetColor = new Color(0f, 0f, 0f, 1f); 
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // ------------------ Transition ------------------
    [Header("Character Select Transition")]
    [SerializeField] private float totalOpeningDuration = 7f;   // Total durasi opening
    
    // Events
    [Space]
    [Header("Events")]
    public UnityEvent OnOpeningComplete;                         
    public UnityEvent OnSkipTriggered;                           

    // ---------------- Character Idle -----------
    [Header("Character Idle")]
    [SerializeField] private float headBobAmp = 1.5f;
    [SerializeField] private float headBobFreq = 0.7f;
    [SerializeField] private float headTiltAmp = 2f;
    [SerializeField] private float headTiltFreq = 0.5f;
    [SerializeField] private float hairSwayAmp = 1.0f;
    [SerializeField] private float hairSwayFreq = 1.0f;
    [SerializeField] private float handMoveAmp = 2.0f;
    [SerializeField] private float handMoveFreq = 0.9f;
    [SerializeField] private float legBobAmp = 0.7f;
    [SerializeField] private float legBobFreq = 0.6f;

    // ====== Caches ======
    private readonly List<Image> _lampImages = new();
    private readonly List<SpriteRenderer> _lampSprites = new();
    private Color[] _lampImgStart, _lampSprStart;

    // Starting positions
    private Vector2 _flowerStartPos, _leavesStartPos;
    private Vector3 _tabletStartScale;
    private Vector3 _cameraStartPos;
    private float _cameraStartSize;    
    private Color _fadeStartColor;

    // Base character positions
    private Vector3 _head0, _eyes0, _fhair0, _bhair0, _fhand0, _bhand0, _lleg0, _rleg0;

    // Animation timers
    private float _flowerTimer, _leavesTimer;
    private Vector3 _cameraTargetPos;
    private float _cameraTargetSize;
    private float _animationStartTime;
    private bool _openingComplete = false;
    private bool _skipped = false;

    private void Reset() => AutoFind();

    private void OnValidate()
    {
        if (!Application.isPlaying) AutoFind();
    }

    private void Awake()
    {
        CacheLamps();

        // Store starting positions
        if (flower) _flowerStartPos = flower.anchoredPosition;
        if (leaves) _leavesStartPos = leaves.anchoredPosition;
        if (tablet) _tabletStartScale = tablet.localScale;
        if (fadeOverlay) 
        {
            _fadeStartColor = fadeOverlay.color;
            // Ensure fadeOverlay starts transparent
            if (fadeOverlay.color.a > 0.1f)
            {
                Color transparentStart = fadeOverlay.color;
                transparentStart.a = 0f;
                fadeOverlay.color = transparentStart;
                _fadeStartColor = transparentStart;
            }
        }

        // Store camera starting values
        if (!mainCamera) mainCamera = Camera.main;
        if (mainCamera)
        {
            _cameraStartPos = mainCamera.transform.position;
            _cameraStartSize = mainCamera.orthographic ? mainCamera.orthographicSize : mainCamera.fieldOfView;
            
            // Calculate camera target
            if (tablet)
            {
                if (tablet.GetComponent<RectTransform>()) 
                {
                    // UI Element - zoom only
                    _cameraTargetPos = _cameraStartPos; 
                    _cameraTargetSize = _cameraStartSize * (1f - cameraZoomAmount);
                }
                else
                {
                    // World Element - move + zoom
                    _cameraTargetPos = tablet.position + cameraTargetOffset;
                    _cameraTargetPos.z = _cameraStartPos.z; 
                    _cameraTargetSize = _cameraStartSize * (1f - cameraZoomAmount);
                }
            }
        }

        // Cache character base positions
        if (head) _head0 = head.localPosition;
        if (eyes) _eyes0 = eyes.localPosition;
        if (frontHair) _fhair0 = frontHair.localPosition;
        if (backHair) _bhair0 = backHair.localPosition;
        if (frontHand) _fhand0 = frontHand.localPosition;
        if (backHand) _bhand0 = backHand.localPosition;
        if (leftLeg) _lleg0 = leftLeg.localPosition;
        if (rightLeg) _rleg0 = rightLeg.localPosition;

        // Reset timers
        _flowerTimer = _leavesTimer = 0f;
        _openingComplete = false;
        _skipped = false;
    }

    private void Start()
    {
        enabled = playOnStart;
        _animationStartTime = Time.time;
    }

    private void Update()
    {
        if (_skipped) return;

        float globalTime = Time.time * globalSpeed;
        float deltaTime = Time.deltaTime * globalSpeed;
        float elapsedTime = Time.time - _animationStartTime;

        // Skip by touch/click
        if (allowSkipByTouch && Input.GetMouseButtonDown(0))
        {
            SkipOpening();
            return;
        }

        // Run animations
        AnimateLamps(globalTime);
        AnimateFlowerFall(deltaTime);
        AnimateLeavesFall(deltaTime);
        AnimateCameraFocus(elapsedTime);
        AnimateTabletHighlight(elapsedTime);
        AnimateFadeOverlay(elapsedTime);
        AnimateCharacterIdle(globalTime);

        // Check completion
        if (!_openingComplete && elapsedTime >= totalOpeningDuration)
        {
            CompleteOpening();
        }
    }

    // ------------------ Animation Methods ------------------
    private void AnimateLamps(float t)
    {
        if (_lampImages.Count == 0 && _lampSprites.Count == 0) return;

        float blinkValue = 0.5f + 0.5f * Mathf.Sin(t * Mathf.PI * 2f * lampBlinkSpeed);
        blinkValue *= lampBlinkAmount;

        for (int i = 0; i < _lampImages.Count; i++)
            if (_lampImages[i]) _lampImages[i].color = Color.Lerp(_lampImgStart[i], lampTargetColor, blinkValue);

        for (int i = 0; i < _lampSprites.Count; i++)
            if (_lampSprites[i]) _lampSprites[i].color = Color.Lerp(_lampSprStart[i], lampTargetColor, blinkValue);
    }

    private void AnimateFlowerFall(float deltaTime)
    {
        if (!flower) return;

        _flowerTimer += deltaTime;
        float fallProgress = Mathf.Clamp01(_flowerTimer / Mathf.Max(0.001f, flowerFallDuration));
        float curvedProgress = flowerFallCurve.Evaluate(fallProgress);

        Vector2 fallOffset = flowerFallDistance * curvedProgress;
        float sway = Mathf.Sin(Time.time * Mathf.PI * 2f * flowerSwaySpeed) * flowerSwayAmount;
        float swayOffset = sway * (1f - fallProgress * 0.5f); 

        Vector2 finalPos = _flowerStartPos + fallOffset + new Vector2(swayOffset, 0f);
        flower.anchoredPosition = finalPos;

        float rotation = swayOffset * 0.1f + Mathf.Sin(Time.time * 2f) * flowerRotateAmount * curvedProgress;
        flower.localRotation = Quaternion.Euler(0, 0, rotation);

        if (fallProgress >= 1f && flowerLoop)
        {
            _flowerTimer = 0f;
        }
    }

    private void AnimateLeavesFall(float deltaTime)
    {
        if (!leaves) return;

        _leavesTimer += deltaTime;
        float fallProgress = Mathf.Clamp01(_leavesTimer / Mathf.Max(0.001f, leavesFallDuration));
        float curvedProgress = leavesFallCurve.Evaluate(fallProgress);

        Vector2 fallOffset = leavesFallDistance * curvedProgress;
        float sway = Mathf.Sin(Time.time * Mathf.PI * 2f * leavesSwaySpeed) * leavesSwayAmount;
        float swayOffset = sway * (1f - fallProgress * 0.3f);

        Vector2 finalPos = _leavesStartPos + fallOffset + new Vector2(swayOffset, 0f);
        leaves.anchoredPosition = finalPos;

        float rotation = swayOffset * 0.12f + Mathf.Sin(Time.time * 1.7f) * leavesRotateAmount * curvedProgress;
        leaves.localRotation = Quaternion.Euler(0, 0, rotation);

        if (fallProgress >= 1f && leavesLoop)
        {
            _leavesTimer = 0f;
        }
    }

    private void AnimateCameraFocus(float elapsedTime)
    {
        if (!mainCamera || !tablet || elapsedTime < cameraFocusDelay) return;

        float focusTime = elapsedTime - cameraFocusDelay;
        float focusProgress = Mathf.Clamp01(focusTime / Mathf.Max(0.001f, cameraFocusDuration));
        float curvedProgress = cameraFocusCurve.Evaluate(focusProgress);

        // Animate camera position (only if not UI element)
        if (_cameraTargetPos != _cameraStartPos)
        {
            Vector3 currentPos = Vector3.Lerp(_cameraStartPos, _cameraTargetPos, curvedProgress);
            mainCamera.transform.position = currentPos;
        }

        // Animate camera zoom
        float currentSize = Mathf.Lerp(_cameraStartSize, _cameraTargetSize, curvedProgress);
        if (mainCamera.orthographic)
            mainCamera.orthographicSize = currentSize;
        else
            mainCamera.fieldOfView = currentSize;
    }

    private void AnimateTabletHighlight(float elapsedTime)
    {
        if (!tablet || elapsedTime < tabletHighlightDelay) return;

        float highlightTime = elapsedTime - tabletHighlightDelay;
        float highlightProgress = Mathf.Clamp01(highlightTime / Mathf.Max(0.001f, tabletHighlightDuration));
        float curvedProgress = tabletHighlightCurve.Evaluate(highlightProgress);

        Vector3 currentScale = Vector3.Lerp(_tabletStartScale, tabletHighlightScale, curvedProgress);
        tablet.localScale = currentScale;
    }

    private void AnimateFadeOverlay(float elapsedTime)
    {
        if (!fadeOverlay || elapsedTime < fadeDelay) return;

        float fadeTime = elapsedTime - fadeDelay;
        float fadeProgress = Mathf.Clamp01(fadeTime / Mathf.Max(0.001f, fadeDuration));
        float curvedProgress = fadeCurve.Evaluate(fadeProgress);

        Color currentColor = Color.Lerp(_fadeStartColor, fadeTargetColor, curvedProgress);
        fadeOverlay.color = currentColor;
    }

    private void AnimateCharacterIdle(float t)
    {
        if (!characterRoot) return;

        if (head)
        {
            float bob = Mathf.Sin(t * Mathf.PI * 2f * headBobFreq) * headBobAmp;
            float tilt = Mathf.Sin(t * Mathf.PI * 2f * headTiltFreq) * headTiltAmp;

            head.localPosition = _head0 + new Vector3(0f, bob, 0f);
            head.localRotation = Quaternion.Euler(0, 0, tilt);

            if (eyes)
            {
                float tremor = Mathf.Sin(t * 6f) * 0.1f;
                eyes.localPosition = _eyes0 + new Vector3(0f, bob * 0.1f + tremor, 0f);
            }
        }

        if (frontHair)
        {
            float sway = Mathf.Sin(t * Mathf.PI * 2f * (hairSwayFreq * 1.1f)) * hairSwayAmp;
            frontHair.localPosition = _fhair0 + new Vector3(sway * 0.5f, 0f, 0f);
        }
        if (backHair)
        {
            float sway = Mathf.Sin(t * Mathf.PI * 2f * (hairSwayFreq * 0.9f)) * (hairSwayAmp * 0.8f);
            backHair.localPosition = _bhair0 + new Vector3(sway * 0.3f, sway * 0.5f, 0f);
        }

        if (frontHand)
        {
            float move = Mathf.Sin(t * Mathf.PI * 2f * (handMoveFreq * 1.1f)) * handMoveAmp;
            frontHand.localPosition = _fhand0 + new Vector3(move * 0.5f, move * 0.3f, 0f);
        }
        if (backHand)
        {
            float move = Mathf.Sin(t * Mathf.PI * 2f * (handMoveFreq * 0.9f)) * (handMoveAmp * 0.9f);
            backHand.localPosition = _bhand0 + new Vector3(move * 0.4f, move * 0.5f, 0f);
        }

        if (leftLeg)
        {
            float bob = Mathf.Sin(t * Mathf.PI * 2f * (legBobFreq * 0.95f)) * legBobAmp;
            leftLeg.localPosition = _lleg0 + new Vector3(0f, bob, 0f);
        }
        if (rightLeg)
        {
            float bob = Mathf.Sin(t * Mathf.PI * 2f * (legBobFreq * 1.05f)) * legBobAmp * 0.9f;
            rightLeg.localPosition = _rleg0 + new Vector3(0f, bob, 0f);
        }
    }

    // ------------------ Control Methods ------------------
    private void CompleteOpening()
    {
        _openingComplete = true;
        OnOpeningComplete?.Invoke();
    }

    public void SkipOpening()
    {
        if (_skipped) return;
        
        _skipped = true;
        
        // Set final states instantly
        if (fadeOverlay) fadeOverlay.color = fadeTargetColor;
        if (mainCamera) 
        {
            if (mainCamera.orthographic) mainCamera.orthographicSize = _cameraTargetSize;
            else mainCamera.fieldOfView = _cameraTargetSize;
        }
        if (tablet) tablet.localScale = tabletHighlightScale;

        OnSkipTriggered?.Invoke();
        OnOpeningComplete?.Invoke();
    }

    // ------------------ Helper Methods ------------------
    private void AutoFind()
    {
        if (!lampRoot) lampRoot = FindDeep(transform, "Lamp") ?? FindDeep(transform, "WallLampBG");
        if (!flower) flower = FindDeep(transform, "FlowerBG")?.GetComponent<RectTransform>();
        if (!leaves) leaves = FindDeep(transform, "Leaves")?.GetComponent<RectTransform>();
        if (!characterRoot) characterRoot = FindDeep(transform, "CharacterAnimation");
        if (!tablet) tablet = FindDeep(transform, "Tablet");
        if (!fadeOverlay) fadeOverlay = FindDeep(transform, "FadeOverlay")?.GetComponent<Image>();
        if (!mainCamera) mainCamera = Camera.main;

        if (characterRoot)
        {
            if (!head) head = FindDeep(characterRoot, "Head");
            if (!eyes) eyes = FindDeep(characterRoot, "AnimationEyes");
            if (!frontHair) frontHair = FindDeep(characterRoot, "FrontHair");
            if (!backHair) backHair = FindDeep(characterRoot, "BackHair");
            if (!frontHand) frontHand = FindDeep(characterRoot, "FrontHand");
            if (!backHand) backHand = FindDeep(characterRoot, "BackHand");
            if (!leftLeg) leftLeg = FindDeep(characterRoot, "LeftLeg");
            if (!rightLeg) rightLeg = FindDeep(characterRoot, "RightLeg");
        }
    }

    private void CacheLamps()
    {
        _lampImages.Clear();
        _lampSprites.Clear();
        
        if (lampRoot)
        {
            _lampImages.AddRange(lampRoot.GetComponentsInChildren<Image>(true));
            _lampSprites.AddRange(lampRoot.GetComponentsInChildren<SpriteRenderer>(true));
        }

        _lampImgStart = new Color[_lampImages.Count];
        for (int i = 0; i < _lampImages.Count; i++)
            _lampImgStart[i] = _lampImages[i].color;

        _lampSprStart = new Color[_lampSprites.Count];
        for (int i = 0; i < _lampSprites.Count; i++)
            _lampSprStart[i] = _lampSprites[i].color;
    }

    private static Transform FindDeep(Transform root, string contains)
    {
        if (!root || string.IsNullOrEmpty(contains)) return null;
        if (root.name.IndexOf(contains, System.StringComparison.OrdinalIgnoreCase) >= 0) return root;
        
        for (int i = 0; i < root.childCount; i++)
        {
            var result = FindDeep(root.GetChild(i), contains);
            if (result) return result;
        }
        return null;
    }
}