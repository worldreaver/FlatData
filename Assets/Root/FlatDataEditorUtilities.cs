#if UNITY_EDITOR
using UnityEditor;

namespace FlatBuffers
{
    [InitializeOnLoad]
    public partial class FlatDataEditorUtilities : AssetPostprocessor
    {
        public static bool initialized;

        static FlatDataEditorUtilities() { Initialize(); }

        /// <summary>
        /// 
        /// </summary>
        private static void Initialize()
        {
            FlatDataPreferences.Load();
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            initialized = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public static void ConfirmInitialization()
        {
            if (!initialized) Initialize();
        }
    }
}

#endif