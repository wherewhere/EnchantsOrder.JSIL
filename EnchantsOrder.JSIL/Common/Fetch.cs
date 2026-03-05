using JSIL;
using JSIL.Meta;

namespace EnchantsOrder.JSIL.Common
{
    public static class Fetch
    {
        public static bool IsSupported { get; } = Builtins.IsFalsy(Builtins.IsFalsy(Builtins.Global["fetch"]));

        [JSReplacement("fetch($url)")]
        public static extern IPromise<IResponse> Invoke(string url);
    }

    public interface IResponse
    {
        [JSReplacement("$this.json()")]
        IPromise<object> Json();

        [JSReplacement("$this.text()")]
        IPromise<string> Text();
    }
}
