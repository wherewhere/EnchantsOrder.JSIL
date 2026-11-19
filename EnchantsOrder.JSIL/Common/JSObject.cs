using JSIL.Meta;

namespace EnchantsOrder.JSIL.Common
{
    public sealed class JSObject
    {
        [JSReplacement("{}")]
        public extern JSObject();

        public extern JSObject this[string key]
        {
            [JSReplacement("$this[$key]")]
            get;
            [JSReplacement("$this[$key] = $value")]
            set;
        }

        [JSReplacement("!!$obj")]
        public static extern bool operator true(JSObject obj);

        [JSReplacement("!$obj")]
        public static extern bool operator false(JSObject obj);

        [JSReplacement("!!$obj")]
        public static extern implicit operator bool(JSObject obj);
    }
}
