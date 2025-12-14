// NPCNameDrawer.cs (OPTIONAL - Advanced Enhancement)
// PELETAKAN: Assets/Scripts/Editor/NPCNameDrawer.cs
// FUNGSI: Custom property drawer untuk NPC name dengan dropdown
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Game.Utility;
using System.Reflection;
using System.Linq;

/// <summary>
/// Attribute untuk mark field sebagai NPC name selector
/// Usage: [NPCName] public string npcName;
/// </summary>
public class NPCNameAttribute : PropertyAttribute { }

[CustomPropertyDrawer(typeof(NPCNameAttribute))]
public class NPCNameDrawer : PropertyDrawer
{
    private static string[] npcNames;
    
    static NPCNameDrawer()
    {
        // Get all NPC constants dari StringContainer
        var fields = typeof(StringContainer)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string));
        
        // Filter yang NPC saja (atau ambil semua)
        npcNames = fields
            .Select(f => (string)f.GetValue(null))
            .Where(name => !string.IsNullOrEmpty(name))
            .OrderBy(name => name)
            .ToArray();
    }
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }
        
        EditorGUI.BeginProperty(position, label, property);
        
        // Get current value
        string currentValue = property.stringValue;
        int currentIndex = System.Array.IndexOf(npcNames, currentValue);
        if (currentIndex < 0) currentIndex = 0;
        
        // Show dropdown
        int newIndex = EditorGUI.Popup(position, label.text, currentIndex, npcNames);
        
        // Update value if changed
        if (newIndex >= 0 && newIndex < npcNames.Length)
        {
            property.stringValue = npcNames[newIndex];
        }
        
        EditorGUI.EndProperty();
    }
}

/// <summary>
/// Similar drawer untuk DialogSequence names
/// Usage: [DialogueName] public string dialogSequenceName;
/// </summary>
public class DialogueNameAttribute : PropertyAttribute { }

[CustomPropertyDrawer(typeof(DialogueNameAttribute))]
public class DialogueNameDrawer : PropertyDrawer
{
    private static string[] dialogueNames;
    
    static DialogueNameDrawer()
    {
        // Get all dialogue constants dari DialoguesNames
        var fields = typeof(DialoguesNames)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string));
        
        dialogueNames = fields
            .Select(f => (string)f.GetValue(null))
            .Where(name => !string.IsNullOrEmpty(name))
            .OrderBy(name => name)
            .ToArray();
    }
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }
        
        EditorGUI.BeginProperty(position, label, property);
        
        string currentValue = property.stringValue;
        int currentIndex = System.Array.IndexOf(dialogueNames, currentValue);
        if (currentIndex < 0) currentIndex = 0;
        
        int newIndex = EditorGUI.Popup(position, label.text, currentIndex, dialogueNames);
        
        if (newIndex >= 0 && newIndex < dialogueNames.Length)
        {
            property.stringValue = dialogueNames[newIndex];
        }
        
        EditorGUI.EndProperty();
    }
}
#endif