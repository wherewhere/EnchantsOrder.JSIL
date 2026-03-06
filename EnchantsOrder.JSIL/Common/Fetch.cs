using JSIL;
using JSIL.Meta;

namespace EnchantsOrder.JSIL.Common
{
    public static class Fetch
    {
        public static bool IsSupported { get; } = Builtins.IsFalsy(Builtins.IsFalsy(Builtins.Global["fetch"]));

        /// <summary>
        /// <see href="https://developer.mozilla.org/docs/Web/API/Window/fetch">MDN Reference</see>
        /// </summary>
        [JSReplacement("fetch($url)")]
        public static extern IPromise<IResponse> Invoke(string url);
    }

    public interface IResponse
    {
        /// <summary>
        /// <see href="https://developer.mozilla.org/docs/Web/API/Request/json">MDN Reference</see>
        /// </summary>
        [JSReplacement("$this.json()")]
        IPromise<object> Json();

        /// <summary>
        /// <see href="https://developer.mozilla.org/docs/Web/API/Request/text">MDN Reference</see>
        /// </summary>
        [JSReplacement("$this.text()")]
        IPromise<string> Text();
    }
}
