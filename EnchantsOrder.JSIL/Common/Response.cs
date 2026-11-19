#pragma warning disable IDE1006
using JSIL.Meta;

namespace EnchantsOrder.JSIL.Common
{
    public interface Response
    {
        /// <summary>
        /// <see href="https://developer.mozilla.org/docs/Web/API/Request/json">MDN Reference</see>
        /// </summary>
        [JSReplacement("$this.json()")]
        Promise<object> Json();

        /// <summary>
        /// <see href="https://developer.mozilla.org/docs/Web/API/Request/text">MDN Reference</see>
        /// </summary>
        [JSReplacement("$this.text()")]
        Promise<string> Text();
    }
}
