using JSIL.Meta;

namespace EnchantsOrder.JSIL.Common
{
    public static class Unsafe
    {
        [JSReplacement("throw $error")]
        public static extern void Throw(object error);

        [JSReplacement("new $constructor()")]
        public static extern T New<T>(object constructor);
    }
}
