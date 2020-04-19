#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FlatBuffers
{
    public class GenerateFbsDefinition : Editor
    {
        private const string FAIL_DIALOG_TITLE = "Generate scripts form fbs failed!";

        [MenuItem("Assets/FlatData/Generate")]
        private static void FbsGenerateCsharp() { GenerateDefinition(EOption.Csharp); }

        [MenuItem("Assets/FlatData/Generate", true)]
        private static bool FbsGenerateCsharpValidation() { return CheckFbsSelected(); }
        
        [MenuItem("Assets/FlatData/Generate One File")]
        private static void FbsGenerateCsharpOneFile() { GenerateDefinition(EOption.Csharp, EOption.Onefile); }

        [MenuItem("Assets/FlatData/Generate One File", true)]
        private static bool FbsGenerateCsharpOneFileValidation() { return CheckFbsSelected(); }

        [MenuItem("Assets/FlatData/Generate With Mutable")]
        private static void FbsGenerateCsharpMutable() { GenerateDefinition(EOption.Csharp, EOption.Mutable); }

        [MenuItem("Assets/FlatData/Generate With Mutable", true)]
        private static bool FbsGenerateCsharpMutableValidation() { return CheckFbsSelected(); }
        
        [MenuItem("Assets/FlatData/Generate One File With Mutable")]
        private static void FbsGenerateCsharpMutableOneFile() { GenerateDefinition(EOption.Csharp, EOption.Mutable, EOption.Onefile); }

        [MenuItem("Assets/FlatData/Generate One File With Mutable", true)]
        private static bool FbsGenerateCsharpMutableOneFileValidation() { return CheckFbsSelected(); }
        
        [MenuItem("Assets/FlatData/Generate With Object API")]
        private static void FbsGenerateCsharpObject() { GenerateDefinition(EOption.Csharp, EOption.Object); }

        [MenuItem("Assets/FlatData/Generate With Object API", true)]
        private static bool FbsGenerateCsharpObjectValidation() { return CheckFbsSelected(); }
        
        [MenuItem("Assets/FlatData/Generate One File With Object API")]
        private static void FbsGenerateCsharpObjectOneFile() { GenerateDefinition(EOption.Csharp, EOption.Object, EOption.Onefile); }

        [MenuItem("Assets/FlatData/Generate One File With Object API", true)]
        private static bool FbsGenerateCsharpObjectOneFileValidation() { return CheckFbsSelected(); }
        
        /// <summary>
        /// generate definition
        /// </summary>
        /// <param name="options"></param>
        private static void GenerateDefinition(
            params EOption[] options)
        {
            var files = GetSelectedFbsFiles();
            var fileOrFiles = files.Count == 1 ? "file" : "files";
            Debug.Log($"Generating {Extensions.ToString(options)} {fileOrFiles} for {string.Join(", ", files)}");

            var exe = new ProcessRunner(FlatDataEditorUtilities.Preferences.flatcPath);
            var errors = exe.Run(files, options);
            if (errors.Length > 0)
            {
                EditorUtility.DisplayDialog(FAIL_DIALOG_TITLE, $"Generating {options} definition for [{string.Join(", ", files)}] failed:\n{errors}", "Ok");
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// indicates if any fbs files are selected
        /// </summary>
        /// <returns></returns>
        private static bool CheckFbsSelected()
        {
            var files = GetSelectedFbsFiles();
            return files.Count > 0;
        }

        /// <summary>
        /// get selected fbs file
        /// </summary>
        /// <returns></returns>
        private static List<string> GetSelectedFbsFiles()
        {
            return Selection.GetFiltered(typeof(Object), SelectionMode.Assets)
                .Select(AssetDatabase.GetAssetPath)
                .Where(path => !string.IsNullOrEmpty(path) && File.Exists(path) && path.EndsWith(".fbs"))
                .ToList();
        }
    }
}

#endif