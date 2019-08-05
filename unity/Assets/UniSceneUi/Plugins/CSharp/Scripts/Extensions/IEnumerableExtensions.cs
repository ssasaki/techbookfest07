using System;
using System.Collections.Generic;

namespace UniSceneUi
{
    public static class IEnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> callback)
        {
            foreach (var item in enumerable)
            {
                callback(item);
            }
        }
    }
}