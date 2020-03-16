#if UNITY_EDITOR
namespace FlatBuffers
{
    /// <summary>
    /// A collection of built-in struct/table meta data attribute names
    /// </summary>
    public static class StructTypeMetadata
    {
        /// <summary>
        /// The 'force_align' attribute
        /// </summary>
        public const string FORCE_ALIGN = "force_align";

        /// <summary>
        /// The 'original_order' attribute
        /// </summary>
        public const string ORIGINAL_ORDER = "original_order";
    }
}
#endif