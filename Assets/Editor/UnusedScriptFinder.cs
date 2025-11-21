using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

/// <summary>
/// Unity Editor tool to find unused scripts in the project
/// Place this script in an "Editor" folder in your project
/// Access via: Tools > Find Unused Scripts
/// </summary>
public class UnusedScriptFinder : EditorWindow
{
    private Vector2 scrollPosition;
    private List<ScriptInfo> unusedScripts = new List<ScriptInfo>();
    private List<ScriptInfo> usedScripts = new List<ScriptInfo>();
    private bool showUsedScripts = false;
    private bool isAnalyzing = false;
    private string searchFilter = "";

    [System.Serializable]
    private class ScriptInfo
    {
        public string name;
        public string path;
        public MonoScript script;
        public List<string> referencedBy = new List<string>();
    }

    [MenuItem("Tools/Find Unused Scripts")]
    public static void ShowWindow()
    {
        UnusedScriptFinder window = GetWindow<UnusedScriptFinder>("Unused Scripts");
        window.minSize = new Vector2(500, 400);
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Unity Unused Script Finder", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        EditorGUILayout.HelpBox(
            "This tool finds MonoScript files that are not referenced by any:\n" +
            "• Other scripts\n" +
            "• GameObjects in scenes\n" +
            "• Prefabs\n" +
            "• ScriptableObjects\n" +
            "• Other assets",
            MessageType.Info);

        EditorGUILayout.Space(10);

        EditorGUI.BeginDisabledGroup(isAnalyzing);
        if (GUILayout.Button("Analyze Project", GUILayout.Height(30)))
        {
            AnalyzeProject();
        }
        EditorGUI.EndDisabledGroup();

        if (isAnalyzing)
        {
            EditorGUILayout.LabelField("Analyzing... Please wait...", EditorStyles.centeredGreyMiniLabel);
            return;
        }

        if (unusedScripts.Count == 0 && usedScripts.Count == 0)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Click 'Analyze Project' to start", EditorStyles.centeredGreyMiniLabel);
            return;
        }

        EditorGUILayout.Space(10);

        // Summary
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Summary", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Total Scripts: {unusedScripts.Count + usedScripts.Count}");
        EditorGUILayout.LabelField($"Used Scripts: {usedScripts.Count}");
        EditorGUILayout.LabelField($"Unused Scripts: {unusedScripts.Count}", new GUIStyle(EditorStyles.label) { normal = { textColor = Color.yellow } });
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        // Search filter
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
        searchFilter = EditorGUILayout.TextField(searchFilter);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        // Toggle to show used scripts
        showUsedScripts = EditorGUILayout.Toggle("Show Used Scripts", showUsedScripts);

        EditorGUILayout.Space(10);

        // Display results
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // Unused Scripts
        EditorGUILayout.LabelField($"Potentially Unused Scripts ({unusedScripts.Count})", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        if (unusedScripts.Count == 0)
        {
            EditorGUILayout.LabelField("✓ No unused scripts found!", EditorStyles.centeredGreyMiniLabel);
        }
        else
        {
            var filteredUnused = string.IsNullOrEmpty(searchFilter)
                ? unusedScripts
                : unusedScripts.Where(s => s.name.ToLower().Contains(searchFilter.ToLower())).ToList();

            foreach (var scriptInfo in filteredUnused)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                // Script name and select button
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(scriptInfo.name, EditorStyles.boldLabel);
                EditorGUILayout.LabelField(scriptInfo.path, EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();

                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    Selection.activeObject = scriptInfo.script;
                    EditorGUIUtility.PingObject(scriptInfo.script);
                }

                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    if (EditorUtility.DisplayDialog("Delete Script",
                        $"Are you sure you want to delete '{scriptInfo.name}'?\n\nThis cannot be undone!",
                        "Delete", "Cancel"))
                    {
                        DeleteScript(scriptInfo);
                    }
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(2);
            }
        }

        // Used Scripts (optional)
        if (showUsedScripts)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField($"Used Scripts ({usedScripts.Count})", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            var filteredUsed = string.IsNullOrEmpty(searchFilter)
                ? usedScripts
                : usedScripts.Where(s => s.name.ToLower().Contains(searchFilter.ToLower())).ToList();

            foreach (var scriptInfo in filteredUsed.Take(50)) // Limit display to avoid slowdown
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(scriptInfo.name, EditorStyles.boldLabel);
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    Selection.activeObject = scriptInfo.script;
                    EditorGUIUtility.PingObject(scriptInfo.script);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField($"Referenced by {scriptInfo.referencedBy.Count} asset(s)", EditorStyles.miniLabel);

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }

            if (filteredUsed.Count > 50)
            {
                EditorGUILayout.LabelField($"... and {filteredUsed.Count - 50} more", EditorStyles.centeredGreyMiniLabel);
            }
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(10);

        // Warning
        EditorGUILayout.HelpBox(
            "IMPORTANT: Review scripts before deleting!\n" +
            "Some scripts might not be detected if they are:\n" +
            "• Loaded via Reflection or Resources.Load()\n" +
            "• Referenced by string name in code\n" +
            "• Editor scripts or tools\n" +
            "• Used by plugins or packages",
            MessageType.Warning);
    }

    private void AnalyzeProject()
    {
        isAnalyzing = true;
        unusedScripts.Clear();
        usedScripts.Clear();

        try
        {
            // Find all MonoScript assets
            string[] guids = AssetDatabase.FindAssets("t:MonoScript");
            Dictionary<string, ScriptInfo> allScripts = new Dictionary<string, ScriptInfo>();

            EditorUtility.DisplayProgressBar("Analyzing Scripts", "Finding all scripts...", 0f);

            // Build list of all scripts
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

                if (script != null)
                {
                    // Skip if it's not in Assets folder (e.g., packages)
                    if (!path.StartsWith("Assets/"))
                        continue;

                    ScriptInfo info = new ScriptInfo
                    {
                        name = script.name,
                        path = path,
                        script = script
                    };
                    allScripts[path] = info;
                }

                if (i % 10 == 0)
                {
                    EditorUtility.DisplayProgressBar("Analyzing Scripts",
                        $"Finding scripts... {i}/{guids.Length}", (float)i / guids.Length * 0.3f);
                }
            }

            // Check dependencies for each asset in the project
            string[] allAssetGuids = AssetDatabase.FindAssets("");

            for (int i = 0; i < allAssetGuids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(allAssetGuids[i]);

                if (i % 50 == 0)
                {
                    EditorUtility.DisplayProgressBar("Analyzing Scripts",
                        $"Checking references... {i}/{allAssetGuids.Length}",
                        0.3f + (float)i / allAssetGuids.Length * 0.7f);
                }

                // Get all dependencies of this asset
                string[] dependencies = AssetDatabase.GetDependencies(assetPath, false);

                foreach (string dependency in dependencies)
                {
                    if (allScripts.ContainsKey(dependency) && dependency != assetPath)
                    {
                        // This script is referenced by the current asset
                        allScripts[dependency].referencedBy.Add(assetPath);
                    }
                }
            }

            // Categorize scripts
            foreach (var scriptInfo in allScripts.Values)
            {
                if (scriptInfo.referencedBy.Count > 0)
                {
                    usedScripts.Add(scriptInfo);
                }
                else
                {
                    unusedScripts.Add(scriptInfo);
                }
            }

            // Sort by name
            unusedScripts = unusedScripts.OrderBy(s => s.name).ToList();
            usedScripts = usedScripts.OrderBy(s => s.name).ToList();

            Debug.Log($"<color=green>Analysis complete!</color>\n" +
                      $"Total scripts: {allScripts.Count}\n" +
                      $"Used scripts: {usedScripts.Count}\n" +
                      $"Unused scripts: {unusedScripts.Count}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error during analysis: {e.Message}");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            isAnalyzing = false;
        }
    }

    private void DeleteScript(ScriptInfo scriptInfo)
    {
        if (AssetDatabase.DeleteAsset(scriptInfo.path))
        {
            unusedScripts.Remove(scriptInfo);
            Debug.Log($"Deleted script: {scriptInfo.name}");
        }
        else
        {
            Debug.LogError($"Failed to delete script: {scriptInfo.name}");
        }
    }
}