// UNITY_2018_3_OR_NEWER

#if UNITY_EDITOR

using System.Threading;
using UnityEditor;
using UnityEngine;

namespace FlatBuffers
{
    public class FlatDataPreferences : ScriptableObject
    {
        public const string SETTINGS_ASSET_PATH = "Assets/Editor/FlatDataSettings.asset";
        private static int wasPreferencesDirCreated = 0;
        private static int wasPreferencesAssetCreated = 0;

        internal const string DEFAULT_FLATC_WINDOW_PATH = "Assets/Root/FlatBuffers/Windows/flatc.exe";
        [HideInInspector] public string flatcPath = DEFAULT_FLATC_WINDOW_PATH;

        internal const string DEFAULT_OUTPUT_PATH = "";
        public string outputPath = DEFAULT_OUTPUT_PATH;

        /// <summary>
        /// load
        /// </summary>
        public static void Load() { GetOrCreateSettings(); }

        /// <summary>
        /// get or create new setting
        /// </summary>
        /// <returns></returns>
        internal static FlatDataPreferences GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<FlatDataPreferences>(SETTINGS_ASSET_PATH);
            if (settings == null)
            {
                settings = CreateInstance<FlatDataPreferences>();
                FlatDataEditorUtilities.OldPreferences.CopyOldToNewPreferences(ref settings);
                if (!AssetDatabase.IsValidFolder("Assets/Editor") && Interlocked.Exchange(ref wasPreferencesDirCreated, 1) == 0)
                    AssetDatabase.CreateFolder("Assets", "Editor");
                if (Interlocked.Exchange(ref wasPreferencesAssetCreated, 1) == 0)
                    AssetDatabase.CreateAsset(settings, SETTINGS_ASSET_PATH);
            }

            return settings;
        }

        /// <summary>
        /// handle gui
        /// </summary>
        /// <param name="settings"></param>
        public static void HandlePreferencesGui(
            SerializedObject settings)
        {
            float prevLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 250;

            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
                {
                    var flatcPath = settings.FindProperty("flatcPath");
                    GUI.enabled = false;
                    flatcPath.stringValue = EditorGUILayout.TextField("Flatc Path", flatcPath.stringValue);
                    GUI.enabled = true;

                    var outputPath = settings.FindProperty("outputPath");
                    outputPath.stringValue = EditorGUILayout.TextField("Output Generate Path", outputPath.stringValue);
                }
            }

            EditorGUIUtility.labelWidth = prevLabelWidth;
        }

        /// <summary>
        /// set folder as output generate
        /// </summary>
        /// <param name="dir"></param>
        public void SetAsOutputFolder(
            string dir)
        {
            outputPath = dir;
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// get serialize setting
        /// </summary>
        /// <returns></returns>
        internal static SerializedObject GetSerializedSettings() { return new SerializedObject(GetOrCreateSettings()); }
    }
}

#endif