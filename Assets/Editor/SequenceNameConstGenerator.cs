// Assets/Editor/SequenceNameConstGenerator.cs
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class SequenceNameConstGenerator : EditorWindow
{
    string targetFolder = "Assets/ScriptableObject/DialogSequence";
    string outputPath = "Assets/Scripts/Utility/DialogNames.cs";
    string className = "DialogNames";

    [MenuItem("Tools/Generate Sequence Name Constants")]
    public static void ShowWindow()
    {
        GetWindow<SequenceNameConstGenerator>("Sequence Const Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Sequence Name Const Generator", EditorStyles.boldLabel);
        targetFolder = EditorGUILayout.TextField("Source Folder", targetFolder);
        outputPath   = EditorGUILayout.TextField("Output File", outputPath);
        className    = EditorGUILayout.TextField("Class Name", className);

        if (GUILayout.Button("Generate"))
        {
            GenerateConstants();
        }
    }

    void GenerateConstants()
    {
        string[] guids = AssetDatabase.FindAssets("t:DialogSequence", new[] { targetFolder });

        if (guids.Length == 0)
        {
            Debug.LogWarning("No DialogSequence assets found in folder: " + targetFolder);
            return;
        }

        var usedAssetKeys = new HashSet<string>();
        var usedAreaKeys  = new HashSet<string>();

        var sb = new StringBuilder();
        sb.AppendLine("// Auto-generated file. Do not edit manually.");
        sb.AppendLine($"public static class {className}");
        sb.AppendLine("{");

        // --- Asset name constants ---
        sb.AppendLine("\tpublic static class Asset");
        sb.AppendLine("\t{");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            DialogSequence so = AssetDatabase.LoadAssetAtPath<DialogSequence>(path);
            if (so == null) continue;

            string value = so.name;                    // pakai nama asset
            string constName = MakeValidIdentifier(value);

            if (string.IsNullOrEmpty(constName)) continue;
            if (usedAssetKeys.Contains(constName))     continue;
            usedAssetKeys.Add(constName);

            sb.AppendLine($"\t\tpublic const string {constName} = \"{value}\";");
        }
        sb.AppendLine("\t}");

        // --- Area name constants (optional) ---
        sb.AppendLine("\tpublic static class Area");
        sb.AppendLine("\t{");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            DialogSequence so = AssetDatabase.LoadAssetAtPath<DialogSequence>(path);
            if (so == null) continue;

            string value = so.areaName;                // pakai areaName
            if (string.IsNullOrEmpty(value)) continue;

            string constName = MakeValidIdentifier(value);
            if (string.IsNullOrEmpty(constName)) continue;
            if (usedAreaKeys.Contains(constName))  continue;
            usedAreaKeys.Add(constName);

            sb.AppendLine($"\t\tpublic const string {constName} = \"{value}\";");
        }
        sb.AppendLine("\t}");

        sb.AppendLine("}");

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
        File.WriteAllText(outputPath, sb.ToString());
        AssetDatabase.Refresh();

        Debug.Log($"Generated constants from {guids.Length} assets to: {outputPath}");
    }

    string MakeValidIdentifier(string input)
    {
        if (string.IsNullOrEmpty(input)) return null;

        var valid = new StringBuilder();
        // identifier harus diawali huruf atau underscore
        if (!char.IsLetter(input[0]) && input[0] != '_')
            valid.Append('_');

        foreach (char c in input)
        {
            if (char.IsLetterOrDigit(c) || c == '_')
                valid.Append(c);
            else
                valid.Append('_');
        }

        // hindari nama kosong (kalau semua karakter non-identifier)
        if (valid.Length == 0) return null;
        return valid.ToString();
    }
}
