namespace FlatBuffers
{
    /// <summary>
    /// A collection of built-in field meta data attribute names
    /// </summary>
    public static class FieldTypeMetadata
    {
        /// <summary>
        /// The 'id' attribute from the fbs schema
        /// </summary>
        public const string INDEX = "id";

        /// <summary>
        /// The 'required' attribute from the fbs schema
        /// </summary>
        public const string REQUIRED = "required";

        /// <summary>
        /// The 'deprecated' attribute from the fbs schema
        /// </summary>
        public const string DEPRECATED = "deprecated";

        /// <summary>
        /// The 'key' attribute from the fbs schema
        /// </summary>
        public const string KEY = "key";

        /// <summary>
        /// The 'hash' attribute from the fbs schema
        /// </summary>
        public const string HASH = "hash";

        /// <summary>
        /// The 'nested_flatbuffer' attribute from the fbs schema
        /// </summary>
        public const string NESTED_FLAT_BUFFER = "nested_flatbuffer";
    }
}