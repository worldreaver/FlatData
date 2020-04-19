#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FlatBuffers
{
    public class FbsCreator : ScriptableWizard
    {
        [MenuItem("Assets/Create/FlatBuffers Schema")]
        [MenuItem("Assets/FlatData/Create Flatbuffers Schema Here")]
        private static void CreateFbs() { CreateFbsFile(); }

        [MenuItem("Assets/FlatData/Create Flatbuffers Schema Here", true)]
        private static bool CreateFbsValidation() { return GetSelectedDirectory() != null; }

        [MenuItem("Assets/FlatData/Set As Output Folder")]
        private static void SetAsOutputFolder()
        {
            var dir = GetSelectedDirectory();
            if (dir != null)
            {
                FlatDataEditorUtilities.Preferences.SetAsOutputFolder(dir);
                FlatDataEditorUtilities.OldPreferences.SaveToEditorPrefs(FlatDataEditorUtilities.Preferences);
            }
        }

        [MenuItem("Assets/FlatData/Set As Output Folder", true)]
        private static bool SetAsOutputFolderValidation() { return GetSelectedDirectory() != null; }

        /// <summary>
        /// create fbs schema file
        /// </summary>
        public static void CreateFbsFile()
        {
            var path = GetSelectedDirectory();
            if (path == null)
            {
                EditorUtility.DisplayDialog("Please select folder first", "You have not chosen a folder to create a flatbuffer schema", "Ok");
                return;
            }

            var fullPath = CreateFullFilename(path, "Schema");
            CreatFbsFile(fullPath);
        }

        /// <summary>
        /// create schema file with path
        /// </summary>
        /// <param name="fullPath"></param>
        private static void CreatFbsFile(
            string fullPath)
        {
            Debug.Log("Creating file: " + fullPath);
            ProjectWindowUtil.CreateAssetWithContent(fullPath, FBS_EXAMPLE_CONTENT);
        }

        /// <summary>
        /// create full file name
        /// </summary>
        /// <param name="path"></param>
        /// <param name="baseName"></param>
        /// <returns></returns>
        private static string CreateFullFilename(
            string path,
            string baseName)
        {
            if (!File.Exists(Path.Combine(path, baseName + ".fbs")))
            {
                return Path.Combine(path, baseName + ".fbs");
            }

            int i;
            for (i = 1; File.Exists(Path.Combine(path, $"{baseName}_{i}.fbs")); ++i)
            {
            }

            return Path.Combine(path, $"{baseName}_{i}.fbs");
        }

        /// <summary>
        /// get selected directory
        /// </summary>
        /// <returns></returns>
        public static string GetSelectedDirectory()
        {
            foreach (var obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
            {
                var path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        /// <summary>
        /// get selected files
        /// </summary>
        /// <returns></returns>
        public static List<string> GetSelectedFiles()
        {
            List<string> files = new List<string>();
            foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    files.Add(path);
                }
            }

            return files;
        }

        private const string FBS_EXAMPLE_CONTENT = @"// Taken from here: https://google.github.io/flatbuffers/flatbuffers_guide_using_schema_compiler.html
// schema
namespace FlatBufferGenerated.Worldreaver.NameDatabase;
";
    }
}

#endif