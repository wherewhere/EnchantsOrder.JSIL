using JSIL.Meta;

namespace EnchantsOrder.JSIL.Common
{
    public interface IHTMLElementBase
    {
        [JSReplacement("$this.addEventListener($type, $listener)")]
        void AddEventListener<T>(string type, Action<T> listener);
    }

    public interface IHTMLElement : IHTMLElementBase
    {
        IWinJSElement WinControl
        {
            [JSReplacement("$this.winControl")]
            get;
        }
    }

    public interface IEvent<T>
    {
        T Detail
        {
            [JSReplacement("$this.detail")]
            get;
        }
    }
}
