#if UNITY_EDITOR
using System;

namespace FlatBuffers.Attributes
{
    /// <summary>
    /// Enum to determine the type of hashing algorithm used by the Hash attribute
    /// </summary>
    public enum FlatBuffersHash
    {
        /// <summary>
        /// No hashing
        /// </summary>
        None,

        /// <summary>
        /// Use the 32-bit version of FNA1
        /// </summary>
        Fnv132,

        /// <summary>
        /// Use the 32-bit version of FNA1a
        /// </summary>
        Fnv1A32,

        /// <summary>
        /// Use the 64-bit version of FNA1
        /// </summary>
        Fnv164,

        /// <summary>
        /// Use the 64-bit version of FNA1a
        /// </summary>
        Fnv1A64,
    }

    internal static class FlatBuffersHashUtils
    {
        public static string HashName(this FlatBuffersHash hash)
        {
            switch (hash)
            {
                case FlatBuffersHash.None:
                {
                    return null;
                }
                case FlatBuffersHash.Fnv132:
                {
                    return "fnv1_32";
                }
                case FlatBuffersHash.Fnv1A32:
                {
                    return "fnv1a_32";
                }
                case FlatBuffersHash.Fnv164:
                {
                    return "fnv1_64";
                }
                case FlatBuffersHash.Fnv1A64:
                {
                    return "fnv1a_64";
                }
                default:
                    throw new ArgumentException();
            }
        }
    }
}
#endif