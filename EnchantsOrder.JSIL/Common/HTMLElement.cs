using JSIL.Meta;

namespace EnchantsOrder.JSIL.Common
{
    public interface IHTMLElementBase
    {
        /// <summary>
        /// The <see cref="AddEventListener"/> method of the EventTarget interface sets up a function that will be called whenever the specified event is delivered to the target.
        /// </summary>
        /// <param name="type">A case-sensitive string representing the event type to listen for.</param>
        /// <param name="listener">The object that receives a notification (an object that implements the Event interface) when an event of the specified type occurs.
        /// This must be <see langword="null"/>, an <see langword="object"/> with a handleEvent() method, or a JavaScript function. See The event listener callback for details on the callback itself.</param>
        [JSReplacement("$this.addEventListener($type, $listener)")]
        void AddEventListener<T>(string type, Action<T> listener);
    }

    /// <summary>
    /// The <see cref="IHTMLElement"/> interface represents any HTML element. Some elements directly implement this interface, while others implement it via an interface that inherits it.
    /// </summary>
    public interface IHTMLElement : IHTMLElementBase
    {
        IWinJSElement WinControl
        {
            [JSReplacement("$this.winControl")]
            get;
        }
    }

    public interface ICustomEvent<T>
    {
        /// <summary>
        /// The read-only <see cref="Detail"/> property of the <see cref="ICustomEvent{T}"/> interface returns any data passed when initializing the event.
        /// </summary>
        T Detail
        {
            [JSReplacement("$this.detail")]
            get;
        }
    }
}
