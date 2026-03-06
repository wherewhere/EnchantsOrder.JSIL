using JSIL.Meta;
using System.Diagnostics.CodeAnalysis;

namespace EnchantsOrder.JSIL.Common
{
    public static class Unsafe
    {
        [DoesNotReturn]
        [JSReplacement("throw $error")]
        public static extern void Throw<T>(T error);

        [JSReplacement("$obj[$key]")]
        public static extern TResult GetItem<TSelf, TResult>(this TSelf obj, string key);

        [JSReplacement("Object.keys($obj)")]
        public static extern string[] Keys<T>(this T obj);

        [JSReplacement("$obj")]
        public static extern TTo As<TFrom, TTo>(this TFrom obj);
    }
}
