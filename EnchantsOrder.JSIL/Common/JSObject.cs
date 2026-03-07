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
    }
}
