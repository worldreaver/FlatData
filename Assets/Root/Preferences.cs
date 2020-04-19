#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FlatBuffers
{
    public partial class FlatDataEditorUtilities
    {
        private static class FlatDataSettingsProviderRegistration
        {
            [SettingsProvider]
            public static SettingsProvider CreateFlatDataSettingsProvider()
            {
                var provider = new SettingsProvider("FlatData", SettingsScope.User)
                {
                    label = "FlatData",
                    guiHandler = (
                        searchContext) =>
                    {
                        var settings = FlatDataPreferences.GetOrCreateSettings();
                        var serializedSettings = new SerializedObject(settings);
                        FlatDataPreferences.HandlePreferencesGui(serializedSettings);
                        if (serializedSettings.ApplyModifiedProperties())
                            OldPreferences.SaveToEditorPrefs(settings);
                    },

                    // Populate the search keywords to enable smart search filtering and label highlighting:
                    keywords = new HashSet<string>(new[] {"FlatData", "Preferences", "Settings"})
                };
                return provider;
            }
        }

        public static FlatDataPreferences Preferences => FlatDataPreferences.GetOrCreateSettings();

        /// <summary>
        /// old preferences
        /// </summary>
        public static class OldPreferences
        {
            private const string PACKAGE_PATH = "Packages/com.worldreaver.flatdata";

            private const string DEFAULT_FLATC_WINDOW_PATH_KEY = "DEFAULT_FLATC_WINDOW_PATH";
            public static string defaultFlatcWindowPath = FlatDataPreferences.DEFAULT_FLATC_WINDOW_PATH;

            private const string DEFAULT_OUTPUT_PATH_KEY = "DEFAULT_OUTPUT_PATH";
            public static string defaultOutputPath = FlatDataPreferences.DEFAULT_OUTPUT_PATH;
            private static bool preferencesLoaded = false;

            /// <summary>
            /// load
            /// </summary>
            public static void Load()
            {
                if (preferencesLoaded)
                    return;

                defaultFlatcWindowPath = EditorPrefs.GetString(DEFAULT_FLATC_WINDOW_PATH_KEY, FlatDataPreferences.DEFAULT_FLATC_WINDOW_PATH);
                defaultOutputPath = PlayerPrefs.GetString(DEFAULT_OUTPUT_PATH_KEY, FlatDataPreferences.DEFAULT_OUTPUT_PATH);

                preferencesLoaded = true;
            }

            /// <summary>
            /// copy old to new preferences
            /// </summary>
            /// <param name="newPreferences"></param>
            public static void CopyOldToNewPreferences(
                ref FlatDataPreferences newPreferences)
            {
                var flatcPath = EditorPrefs.GetString(DEFAULT_FLATC_WINDOW_PATH_KEY, FlatDataPreferences.DEFAULT_FLATC_WINDOW_PATH);
                var flatcToolPath = Path.GetFullPath(EditorHelper.PACKAGES_PATH);
                flatcToolPath = Directory.Exists(flatcToolPath) ? Path.Combine(flatcToolPath, "FlatBuffers/Windows/flatc.exe") : flatcPath;
                if (!flatcToolPath.Contains("flatc.exe")) flatcToolPath = DEFAULT_FLATC_WINDOW_PATH_KEY;
                newPreferences.flatcPath = flatcToolPath;
                newPreferences.outputPath = PlayerPrefs.GetString(DEFAULT_OUTPUT_PATH_KEY, FlatDataPreferences.DEFAULT_OUTPUT_PATH);
            }

            /// <summary>
            /// save
            /// </summary>
            /// <param name="preferences"></param>
            public static void SaveToEditorPrefs(
                FlatDataPreferences preferences)
            {
                var flatcToolPath = Path.GetFullPath(EditorHelper.PACKAGES_PATH);
                flatcToolPath = Directory.Exists(flatcToolPath) ? Path.Combine(flatcToolPath, "FlatBuffers/Windows/flatc.exe") : preferences.flatcPath;
                EditorPrefs.SetString(DEFAULT_FLATC_WINDOW_PATH_KEY, flatcToolPath);
                PlayerPrefs.SetString(DEFAULT_OUTPUT_PATH_KEY, preferences.outputPath);
            }
        }

        /// <summary>
        /// serialize bool prefs field
        /// </summary>
        /// <param name="currentValue"></param>
        /// <param name="editorPrefsKey"></param>
        /// <param name="label"></param>
        private static void BoolPrefsField(
            ref bool currentValue,
            string editorPrefsKey,
            GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            currentValue = EditorGUILayout.Toggle(label, currentValue);
            if (EditorGUI.EndChangeCheck())
                EditorPrefs.SetBool(editorPrefsKey, currentValue);
        }

        /// <summary>
        /// serialize float prefs field
        /// </summary>
        /// <param name="currentValue"></param>
        /// <param name="editorPrefsKey"></param>
        /// <param name="label"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        private static void FloatPrefsField(
            ref float currentValue,
            string editorPrefsKey,
            GUIContent label,
            float min = float.NegativeInfinity,
            float max = float.PositiveInfinity)
        {
            EditorGUI.BeginChangeCheck();
            currentValue = EditorGUILayout.DelayedFloatField(label, currentValue);
            if (EditorGUI.EndChangeCheck())
            {
                currentValue = Mathf.Clamp(currentValue, min, max);
                EditorPrefs.SetFloat(editorPrefsKey, currentValue);
            }
        }

        /// <summary>
        /// serialize texture2d prefs field
        /// </summary>
        /// <param name="currentValue"></param>
        /// <param name="editorPrefsKey"></param>
        /// <param name="label"></param>
        private static void Texture2DPrefsField(
            ref string currentValue,
            string editorPrefsKey,
            GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUIUtility.wideMode = true;
            var texture = (EditorGUILayout.ObjectField(label, AssetDatabase.LoadAssetAtPath<Texture2D>(currentValue), typeof(Object), false) as Texture2D);
            currentValue = texture != null ? AssetDatabase.GetAssetPath(texture) : "";
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetString(editorPrefsKey, currentValue);
            }
        }

        /// <summary>
        /// serialize float property field
        /// </summary>
        /// <param name="property"></param>
        /// <param name="label"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public static void FloatPropertyField(
            SerializedProperty property,
            GUIContent label,
            float min = float.NegativeInfinity,
            float max = float.PositiveInfinity)
        {
            EditorGUI.BeginChangeCheck();
            property.floatValue = EditorGUILayout.DelayedFloatField(label, property.floatValue);
            if (EditorGUI.EndChangeCheck())
            {
                property.floatValue = Mathf.Clamp(property.floatValue, min, max);
            }
        }

        /// <summary>
        /// serialize shader property field
        /// </summary>
        /// <param name="property"></param>
        /// <param name="label"></param>
        /// <param name="fallbackShaderName"></param>
        public static void ShaderPropertyField(
            SerializedProperty property,
            GUIContent label,
            string fallbackShaderName)
        {
            var shader = (EditorGUILayout.ObjectField(label, Shader.Find(property.stringValue), typeof(Shader), false) as Shader);
            property.stringValue = shader != null ? shader.name : fallbackShaderName;
        }

        /// <summary>
        /// serialize preset property field
        /// </summary>
        /// <param name="property"></param>
        /// <param name="label"></param>
        public static void PresetAssetPropertyField(
            SerializedProperty property,
            GUIContent label)
        {
            var texturePreset =
                (EditorGUILayout.ObjectField(label,
                    AssetDatabase.LoadAssetAtPath<UnityEditor.Presets.Preset>(property.stringValue),
                    typeof(UnityEditor.Presets.Preset),
                    false) as UnityEditor.Presets.Preset);
            bool isTexturePreset = texturePreset != null && texturePreset.GetTargetTypeName() == "TextureImporter";
            property.stringValue = isTexturePreset ? AssetDatabase.GetAssetPath(texturePreset) : "";
        }
    }
}

#endif