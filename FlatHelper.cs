using System.IO;

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
        /// <param name="path"></param>
        /// <returns></returns>
        public static ByteBuffer Load(string path)
        {
            return new ByteBuffer(File.ReadAllBytes(path));
        }
    }
}