#if UNITY_EDITOR
using System;
using System.Collections.Generic;

namespace FlatBuffers
{
    internal sealed class TypeDefaultValueProvider : IDefaultValueProvider
    {
        private static readonly TypeDefaultValueProvider _instance = new TypeDefaultValueProvider();
        
        private static readonly Dictionary<Type, object> DefaultValues;

        static TypeDefaultValueProvider()
        {
            DefaultValues = new Dictionary<Type, object>
            {
                {typeof(sbyte), (sbyte) 0},
                {typeof(byte), (byte) 0},
                {typeof(short), (short) 0},
                {typeof(ushort), (ushort) 0},
                {typeof(int), (int) 0},
                {typeof(uint), (uint) 0},
                {typeof(long), (long) 0},
                {typeof(ulong), (ulong) 0},
                {typeof(float), (float) 0},
                {typeof(double), (double) 0},
                {typeof(bool), false},
            };
        }

        public static TypeDefaultValueProvider Instance => _instance;

        public object GetDefaultValue(Type valueType)
        {
            if (DefaultValues.TryGetValue(valueType, out var defaultValue))
            {
                return defaultValue;
            }

            return null;
        }

        public bool IsDefaultValue(object value)
        {
            if (value == null)
            {
                return true;
            }

            if (DefaultValues.TryGetValue(value.GetType(), out var defaultValue))
            {
                return defaultValue.Equals(value);
            }

            return value == null;
        }

        public bool IsDefaultValueSetExplicity => false;
    }
}
#endif