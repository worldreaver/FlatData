#if UNITY_EDITOR
using System;

namespace FlatBuffers
{
    /// <summary>
    /// string argument
    /// </summary>
    public class StringArgument : Attribute
    {
        public readonly string data;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="data"></param>
        public StringArgument(
            string data)
        {
            this.data = data;
        }
    }
}
#endif