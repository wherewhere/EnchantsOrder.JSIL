using JSIL;
using JSIL.Meta;

namespace EnchantsOrder.JSIL.Common
{
    public sealed class Fetch
    {
        public static bool IsSupported { get; } = Builtins.IsTruthy(Builtins.Global["fetch"]);

        [JSReplacement("fetch($url)")]
        public static extern Promise<Response> Invoke(string url);
    }

    public sealed class Response;

    public static class ResponseImplement
    {
        [JSReplacement("$response.json()")]
        public static extern Promise<dynamic> Json(this Response response);

        [JSReplacement("$response.json()")]
        public static extern Promise<string> Text(this Response response);
    }
}
