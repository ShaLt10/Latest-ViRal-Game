using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.IO;

public class SpriteSheetAnimationGenerator : EditorWindow
{
    private Texture2D spriteSheet;
    private string outputFolder = "Assets/Animations";
    private string animationBaseName = "Anim";
    private string animatorControllerName = "MyAnimatorController";
    private GameObject targetGameObject;

    private List<AnimationRange> animationRanges = new List<AnimationRange>();

    [System.Serializable]
    public class AnimationRange
    {
        public string animationName;
        public int startIndex;
        public int endIndex;
        public float frameRate = 12f;
    }

    [MenuItem("Tools/Generate Sprite Animations + Animator")]
    public static void ShowWindow()
    {
        GetWindow<SpriteSheetAnimationGenerator>("Sprite Sheet Animation Generator");
    }

    private void OnGUI()
    {
        spriteSheet = (Texture2D)EditorGUILayout.ObjectField("Sprite Sheet", spriteSheet, typeof(Texture2D), false);
        targetGameObject = (GameObject)EditorGUILayout.ObjectField("Target GameObject", targetGameObject, typeof(GameObject), true);
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);
        animationBaseName = EditorGUILayout.TextField("Base Animation Name", animationBaseName);
        animatorControllerName = EditorGUILayout.TextField("Animator Controller Name", animatorControllerName);

        if (GUILayout.Button("Add Animation Range"))
        {
            animationRanges.Add(new AnimationRange { animationName = "NewAnim", startIndex = 0, endIndex = 0 });
        }

        for (int i = 0; i < animationRanges.Count; i++)
        {
            GUILayout.BeginHorizontal();
            animationRanges[i].animationName = EditorGUILayout.TextField("Name", animationRanges[i].animationName);
            animationRanges[i].startIndex = EditorGUILayout.IntField("Start", animationRanges[i].startIndex);
            animationRanges[i].endIndex = EditorGUILayout.IntField("End", animationRanges[i].endIndex);
            animationRanges[i].frameRate = EditorGUILayout.FloatField("Frame Rate", animationRanges[i].frameRate);

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                animationRanges.RemoveAt(i);
                break;
            }
            GUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Generate Animations + Animator"))
        {
            GenerateAnimationsAndAnimator();
        }
    }

    private void GenerateAnimationsAndAnimator()
    {
        if (spriteSheet == null)
        {
            Debug.LogError("Sprite sheet not assigned!");
            return;
        }

        if (!Directory.Exists(outputFolder))
            Directory.CreateDirectory(outputFolder);

        string spriteSheetPath = AssetDatabase.GetAssetPath(spriteSheet);
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(spriteSheetPath);

        List<Sprite> sprites = new List<Sprite>();
        foreach (var asset in assets)
        {
            if (asset is Sprite sprite)
                sprites.Add(sprite);
        }

        sprites.Sort((a, b) =>
        {
            string aNum = System.Text.RegularExpressions.Regex.Match(a.name, @"\d+").Value;
            string bNum = System.Text.RegularExpressions.Regex.Match(b.name, @"\d+").Value;

            int aIndex = int.TryParse(aNum, out var aVal) ? aVal : 0;
            int bIndex = int.TryParse(bNum, out var bVal) ? bVal : 0;

            return aIndex.CompareTo(bIndex);
        });

        List<AnimationClip> createdClips = new List<AnimationClip>();
        Dictionary<string, AnimationClip> clipDict = new Dictionary<string, AnimationClip>();

        foreach (var range in animationRanges)
        {
            AnimationClip clip = new AnimationClip
            {
                frameRate = range.frameRate
            };

            EditorCurveBinding spriteBinding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            int frameCount = range.endIndex - range.startIndex + 1;

            List<ObjectReferenceKeyframe> keyframeList = new List<ObjectReferenceKeyframe>();

            // Forward frames
            for (int i = range.startIndex; i <= range.endIndex; i++)
            {
                keyframeList.Add(new ObjectReferenceKeyframe
                {
                    time = keyframeList.Count / range.frameRate,
                    value = sprites[i]
                });
            }

            // Backward frame (excluding the last one)
            for (int i = range.endIndex - 1; i > range.startIndex; i--)
            {
                keyframeList.Add(new ObjectReferenceKeyframe
                {
                    time = keyframeList.Count / range.frameRate,
                    value = sprites[i]
                });
            }

            ObjectReferenceKeyframe[] keyframes = keyframeList.ToArray();
            AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);


            AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);

            string clipPath = Path.Combine(outputFolder, $"{animationBaseName}_{range.animationName}.anim");
            AssetDatabase.CreateAsset(clip, clipPath);
            createdClips.Add(clip);
            clipDict[range.animationName] = clip;

            Debug.Log($"Created Animation: {clipPath}");
        }

        // Create Animator Controller
        string controllerPath = Path.Combine(outputFolder, $"{animatorControllerName}.controller");
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;

        bool isFirst = true;
        foreach (var kvp in clipDict)
        {
            AnimatorState state = stateMachine.AddState(kvp.Key);
            state.motion = kvp.Value;

            if (isFirst)
            {
                stateMachine.defaultState = state;
                isFirst = false;
            }
        }

        // Assign Animator to GameObject
        if (targetGameObject != null)
        {
            Animator animator = targetGameObject.GetComponent<Animator>();
            if (animator == null)
                animator = targetGameObject.AddComponent<Animator>();

            animator.runtimeAnimatorController = controller;
            Debug.Log("Animator Controller assigned to GameObject.");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Animation and Animator generation complete.");
    }
}
