using JSIL.Meta;
using System.Diagnostics.CodeAnalysis;

namespace EnchantsOrder.JSIL.Common
{
    public static class Unsafe
    {
        [DoesNotReturn]
        [JSReplacement("throw $error")]
        public static extern void Throw(object error);

        [JSReplacement("new $constructor()")]
        public static extern T New<T>(object constructor);

        [JSReplacement("$obj[$key]")]
        public static extern T GetItem<T>(this object obj, string key);

        [JSReplacement("Object.keys($obj)")]
        public static extern string[] Keys(this object obj);
    }
}
