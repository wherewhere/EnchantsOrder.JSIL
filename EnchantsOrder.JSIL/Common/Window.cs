#pragma warning disable IDE1006
using JSIL.Meta;

namespace EnchantsOrder.JSIL.Common
{
    public interface Window
    {
        /// <summary>
        /// A <see cref="Common.Location"/> object.
        /// </summary>
        static extern Location Location
        {
            [JSReplacement("window.location")]
            get;
        }

        /// <summary>
        /// <see href="https://developer.mozilla.org/docs/Web/API/Window/fetch">MDN Reference</see>
        /// </summary>
        [JSReplacement("window.fetch($url)")]
        static extern Promise<Response> Fetch(string url);

        [JSReplacement("window.jQuery()")]
        static extern JQuery JQuery();

        /// <summary>
        /// Accepts a string containing a CSS selector which is then used to match a set of elements.
        /// </summary>
        /// <param name="element">A string containing a selector expression or a DOM element to wrap in a jQuery object.</param>
        [JSReplacement("window.jQuery($element)")]
        static extern JQuery JQuery(object element);

        /// <summary>
        /// Binds a function to be executed when the DOM has finished loading.
        /// </summary>
        /// <param name="element">A function to execute after the DOM is ready.</param>
        [JSReplacement("window.jQuery($element)")]
        static extern JQuery JQuery(Action element);

        static extern Action<HTMLElement> DeleteEnchantment
        {
            [JSReplacement("window.deleteEnchantment = $value")]
            set;
        }
    }
}
