using JSIL;

namespace EnchantsOrder.JSIL.Common
{
    /// <summary>
    /// <see cref="XMLHttpRequest"/> (XHR) objects are used to interact with servers. You can retrieve data from a URL without having to do a full page refresh.
    /// This enables a Web page to update just part of a page without disrupting what the user is doing.
    /// </summary>
    public sealed class XMLHttpRequest
    {
        private readonly dynamic request = Builtins.Eval("new XMLHttpRequest()");

        /// <summary>
        /// The read-only <see cref="XMLHttpRequest"/> property <see cref="ResponseText"/> returns the text received from a server following a request being sent.
        /// </summary>
        public string ResponseText => request.responseText;

        /// <summary>
        /// The <see cref="XMLHttpRequest"/> method <see cref="Open"/> initializes a newly-created request, or re-initializes an existing one.
        /// </summary>
        /// <param name="method">The HTTP request method to use, such as "GET", "POST", "PUT", "DELETE", etc. Ignored for non-HTTP(S) URLs.</param>
        /// <param name="url">A string or any other object with a stringifier — including a URL object — that provides the URL of the resource to send the request to.</param>
        /// <param name="async">An optional Boolean parameter, defaulting to <see langword="true"/>, indicating whether or not to perform the operation asynchronously.
        /// If this value is <see langword="false"/>, the <see cref="Send"/> method does not return until the response is received. If <see langword="true"/>, notification of a completed transaction is provided using event listeners.
        /// This must be <see langword="true"/> if the <c>multipart</c> attribute is <see langword="true"/>, or an exception will be thrown.</param>
        public void Open(string method, string url, bool async = true) => request.open(method, url, async);

        /// <summary>
        /// The XMLHttpRequest method <see cref="Send"/> sends the request to the server.
        /// </summary>
        /// <param name="body">A body of data to be sent in the XHR request.</param>
        public void Send(object? body = null) => request.send(body);

        /// <summary>
        /// The <see cref="AddEventListener"/> method of the EventTarget interface sets up a function that will be called whenever the specified event is delivered to the target.
        /// </summary>
        /// <param name="type">A case-sensitive string representing the event type to listen for.</param>
        /// <param name="listener">The object that receives a notification (an object that implements the Event interface) when an event of the specified type occurs.
        /// This must be <see langword="null"/>, an <see langword="object"/> with a handleEvent() method, or a JavaScript function. See The event listener callback for details on the callback itself.</param>
        public void AddEventListener(string type, Action<dynamic> listener) => request.addEventListener(type, listener);

        public static Task<string> FetchAsync(string url)
        {
            TaskCompletionSource<string> tcs = new();
            dynamic fetch = Builtins.Global["fetch"];
            if (Builtins.IsTruthy((object)fetch))
            {
                fetch(url)
                    .then(new Func<dynamic, object>(x => x.text()))
                    .then(
                        new Action<dynamic>(value => _ = tcs.TrySetResult(value)),
                        new Action<dynamic>(reason => _ = tcs.TrySetException(new Exception(reason.ToString()))));
            }
            else
            {
                XMLHttpRequest request = new();
                request.Open("GET", url);
                request.AddEventListener("load", e =>
                {
                    try
                    {
                        _ = tcs.TrySetResult(request.ResponseText);
                    }
                    catch (Exception ex)
                    {
                        _ = tcs.TrySetException(ex);
                    }
                });
                request.AddEventListener("error", e => _ = tcs.TrySetException(new Exception($"Failed to fetch {url}")));
                request.AddEventListener("timeout", e => _ = tcs.TrySetException(new TimeoutException($"Timeout while fetching {url}")));
                request.AddEventListener("abort", e => _ = tcs.TrySetCanceled());
                request.Send();
            }
            return tcs.Task;
        }

        public static async Task<dynamic> FetchJsonAsync(string url)
        {
            dynamic fetch = Builtins.Global["fetch"];
            if (Builtins.IsTruthy((object)fetch))
            {
                return await fetch(url).then(new Func<dynamic, object>(x => x.json()));
            }
            else
            {
                string responseText = await FetchAsync(url);
                dynamic JSON = Builtins.Global["JSON"];
                return JSON.parse(responseText);
            }
        }
    }
}
