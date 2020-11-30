using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shared
{
    public static class Extensions
    {
        public static void Each<T>(this IEnumerable els, Action<T, int> a)
        {
            int i = 0;
            foreach (T e in els)
            {
                a(e, i++);
            }
        }

        public static Dictionary<TKey, TValue> Merge<TKey, TValue>(IEnumerable<TKey> keys, IEnumerable<TValue> values)
        {
            var dic = new Dictionary<TKey, TValue>();

            keys.Each<TKey>((x, i) =>
            {
                dic.Add(x, values.ElementAt(i));
            });

            return dic;
        }
    }
}
