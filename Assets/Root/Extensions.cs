#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;

namespace FlatBuffers
{
    public static class Extensions
    {
        /// <summary>
        /// get argument data of enum
        /// if an enum value has argument then return argument data otherwise return name enum value
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public static string ArgumentData(
            this EOption option)
        {
            var type = option.GetType();
            var memInfo = type.GetMember(option.ToString());

            if (memInfo.Length <= 0) return option.ToString();

            var attrs = memInfo[0]
                .GetCustomAttributes(typeof(StringArgument), false);

            return attrs.Length > 0 ? ((StringArgument) attrs[0]).data : option.ToString();
        }

        /// <summary>
        /// to string
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string ToString(
            this IEnumerable<EOption> options)
        {
            return options.Aggregate("",
                (
                    current,
                    option) => current + $"{option.ToString()} ");
        }
    }
}
#endif