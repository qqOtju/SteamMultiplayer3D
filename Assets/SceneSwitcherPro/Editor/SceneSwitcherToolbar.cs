#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

[InitializeOnLoad]
public static class SceneSwitcherToolbar
{
    private static string[] sceneNames = new string[0];
    private static int selectedIndex;
    private static string lastActiveScene = "";
    private static VisualElement toolbarUI;

    private static readonly float positionOffset = 180f; // Move closer to Play button
    private static readonly float dropdownBoxHeight = 20f; // Dropdown button height

    static SceneSwitcherToolbar()
    {
        RefreshSceneList();
        SelectCurrentScene(); // Automatically select the open scene

        // Hook into scene change events
        EditorSceneManager.activeSceneChangedInEditMode += (prev, current) => UpdateSceneSelection();
        EditorApplication.playModeStateChanged += OnPlayModeChanged;

        EditorApplication.delayCall += AddToolbarUI;
    }

    private static bool fetchAllScenes
    {
        get => EditorPrefs.GetBool("SceneSwitcher_FetchAllScenes", false);
        set => EditorPrefs.SetBool("SceneSwitcher_FetchAllScenes", value);
    }

    private static void AddToolbarUI()
    {
        var toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
        if (toolbarType == null) return;

        var toolbars = Resources.FindObjectsOfTypeAll(toolbarType);
        if (toolbars.Length == 0) return;

        var toolbar = toolbars[0];
        var rootField = toolbarType.GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
        if (rootField == null) return;

        var root = rootField.GetValue(toolbar) as VisualElement;
        if (root == null) return;

        var leftContainer = root.Q("ToolbarZoneLeftAlign");
        if (leftContainer == null) return;

        // Remove old UI if it exists to prevent duplication
        if (toolbarUI != null) leftContainer.Remove(toolbarUI);

        toolbarUI = new IMGUIContainer(OnGUI);
        toolbarUI.style.marginLeft = positionOffset;

        leftContainer.Add(toolbarUI);
    }

    private static void OnGUI()
    {
        CheckAndRefreshScenes();

        if (selectedIndex >= sceneNames.Length)
            selectedIndex = 0;

        var isPlaying = EditorApplication.isPlaying; // Check if in Play Mode

        GUILayout.BeginHorizontal();

        // Fetch all scenes toggle button (Disabled in Play Mode)
        EditorGUI.BeginDisabledGroup(isPlaying);
        var newFetchAllScenes =
            GUILayout.Toggle(fetchAllScenes, "All Scenes", "Button", GUILayout.Height(dropdownBoxHeight));
        if (newFetchAllScenes != fetchAllScenes)
        {
            fetchAllScenes = newFetchAllScenes;
            RefreshSceneList();
            SelectCurrentScene();
        }

        EditorGUI.EndDisabledGroup();

        // Scene dropdown with the currently selected scene displayed (Disabled in Play Mode)
        EditorGUI.BeginDisabledGroup(isPlaying);
        var popupStyle = new GUIStyle(EditorStyles.popup)
        {
            fixedHeight = dropdownBoxHeight
        };

        var newIndex = EditorGUILayout.Popup(selectedIndex, sceneNames, popupStyle, GUILayout.Width(150),
            GUILayout.Height(dropdownBoxHeight));

        if (newIndex != selectedIndex)
        {
            selectedIndex = newIndex;
            LoadScene(sceneNames[selectedIndex]);
        }

        EditorGUI.EndDisabledGroup();

        GUILayout.EndHorizontal();
    }

    private static void RefreshSceneList()
    {
        if (fetchAllScenes)
            sceneNames = Directory.GetFiles("Assets", "*.unity", SearchOption.AllDirectories)
                .Select(path => Path.GetFileNameWithoutExtension(path))
                .ToArray();
        else
            sceneNames = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => Path.GetFileNameWithoutExtension(scene.path))
                .ToArray();
    }

    private static void CheckAndRefreshScenes()
    {
        string[] currentScenes;
        if (fetchAllScenes)
            currentScenes = Directory.GetFiles("Assets", "*.unity", SearchOption.AllDirectories)
                .Select(path => Path.GetFileNameWithoutExtension(path))
                .ToArray();
        else
            currentScenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => Path.GetFileNameWithoutExtension(scene.path))
                .ToArray();

        if (!currentScenes.SequenceEqual(sceneNames))
        {
            sceneNames = currentScenes;
            SelectCurrentScene();
        }
    }

    private static void SelectCurrentScene()
    {
        var currentScene = Path.GetFileNameWithoutExtension(EditorSceneManager.GetActiveScene().path);
        var index = Array.IndexOf(sceneNames, currentScene);
        if (index != -1)
        {
            selectedIndex = index;
            lastActiveScene = currentScene;
        }
    }

    private static void UpdateSceneSelection()
    {
        var currentScene = Path.GetFileNameWithoutExtension(EditorSceneManager.GetActiveScene().path);
        if (currentScene != lastActiveScene)
        {
            lastActiveScene = currentScene;
            SelectCurrentScene();
        }
    }

    private static void LoadScene(string sceneName)
    {
        string scenePath;
        Debug.Log("<b><color=green>Thank you for using the package  -Ajay</color></b>");

        if (fetchAllScenes)
            scenePath = Directory.GetFiles("Assets", "*.unity", SearchOption.AllDirectories)
                .FirstOrDefault(path => Path.GetFileNameWithoutExtension(path) == sceneName);
        else
            scenePath = EditorBuildSettings.scenes
                .FirstOrDefault(scene => scene.enabled && scene.path.Contains(sceneName))?.path;

        if (!string.IsNullOrEmpty(scenePath))
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(scenePath);
        }
        else
        {
            Debug.LogError("Scene not found: " + sceneName);
        }
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.ExitingPlayMode)
            EditorApplication.delayCall += () => AddToolbarUI();
    }
}
#endif