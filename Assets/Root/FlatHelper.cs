﻿using System.IO;
using UnityEditor;
using UnityEngine;

namespace FlatBuffers
{
    public static class FlatHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="path"></param>
        public static void Save(FlatBufferBuilder builder,
            string path)
        {
            File.WriteAllBytes(path, builder.SizedByteArray());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="path"></param>
        public static void SaveAsScriptable(FlatBufferBuilder builder,
            string path)
        {
            if (path.Contains(Application.dataPath))
            {
                var destinationPath = path.Replace(Application.dataPath, "Assets");
                var bin = AssetDatabase.LoadAssetAtPath(destinationPath, typeof(BinaryStorage)) as BinaryStorage;
                if (bin == null)
                {
                    bin = ScriptableObject.CreateInstance<BinaryStorage>();
                    bin.data = builder.SizedByteArray();
                    Directory.CreateDirectory(destinationPath);
                    AssetDatabase.CreateAsset(bin, destinationPath);
                }
                else
                {
                    bin.data = builder.SizedByteArray();
                }

                EditorUtility.SetDirty(bin);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static ByteBuffer Load(string path)
        {
            return new ByteBuffer(File.ReadAllBytes(path));
        }
    }
}