using JSIL.Meta;

namespace EnchantsOrder.JSIL.Common
{
    /// <summary>
    /// The jQuery instance members.
    /// </summary>
    public sealed class JQuery
    {
        [JSReplacement("window.jQuery()")]
        public extern JQuery();

        /// <summary>
        /// Accepts a string containing a CSS selector which is then used to match a set of elements.
        /// </summary>
        /// <param name="element">A string containing a selector expression or a DOM element to wrap in a jQuery object.</param>
        [JSReplacement("window.jQuery($element)")]
        public static extern JQuery Invoke(object element);

        /// <summary>
        /// Binds a function to be executed when the DOM has finished loading.
        /// </summary>
        /// <param name="element">A function to execute after the DOM is ready.</param>
        [JSReplacement("window.jQuery($element)")]
        public static extern JQuery Invoke(Action element);

        public extern IHTMLElement this[int index]
        {
            [JSReplacement("$this[$index]")]
            get;
        }

        /// <summary>
        /// Set the HTML contents of each element in the set of matched elements.
        /// </summary>
        /// <param name="html">A string of HTML to set as the content of each matched element.</param>
        [JSReplacement("$this.html($html)")]
        public extern JQuery Html(string html);

        /// <summary>
        /// Remove an attribute from each element in the set of matched elements.
        /// </summary>
        /// <param name="attributeName">An attribute to remove; as of version 1.7, it can be a space-separated list of attributes.</param>
        [JSReplacement("$this.removeAttr($attributeName)")]
        public extern JQuery RemoveAttr(string attributeName);

        /// <summary>
        /// Get the current value of the first element in the set of matched elements.
        /// </summary>
        [JSReplacement("$this.val()")]
        public extern T Val<T>();

        /// <summary>
        /// Set the value of each element in the set of matched elements.
        /// </summary>
        /// <param name="value">A string of text, an array of strings or number corresponding to the value of each matched element to set as selected/checked.</param>
        [JSReplacement("$this.val($value)")]
        public extern JQuery Val<T>(T value);

        /// <summary>
        /// Hide the matched elements.
        /// </summary>
        /// <param name="options">A map of additional options to pass to the method.</param>
        [JSReplacement("$this.hide($options)")]
        public extern JQuery Hide(object? options = null);

        /// <summary>
        /// Display the matched elements.
        /// </summary>
        /// <param name="options">A map of additional options to pass to the method.</param>
        [JSReplacement("$this.show($options)")]
        public extern JQuery Show(object? options = null);

        /// <summary>
        /// Bind an event handler to the "change" JavaScript event
        /// </summary>
        /// <param name="eventObject">A function to execute each time the event is triggered.</param>
        [JSReplacement("$this.change($eventObject)")]
        public extern JQuery Change(Action<dynamic> eventObject);

        /// <summary>
        /// Bind an event handler to the "click" JavaScript event
        /// </summary>
        /// <param name="eventObject">An object containing data that will be passed to the event handler.</param>
        [JSReplacement("$this.click($eventObject)")]
        public extern JQuery Click(Action<dynamic> eventObject);

        /// <summary>
        /// Set the content of each element in the set of matched elements to the specified text.
        /// </summary>
        /// <param name="text">The text to set as the content of each matched element. When Number or Boolean is supplied, it will be converted to a String representation.</param>
        [JSReplacement("$this.text($text)")]
        public extern JQuery Text(string text);

        /// <summary>
        /// Get the ancestors of each element in the current set of matched elements, optionally filtered by a selector.
        /// </summary>
        /// <param name="selector">A string containing a selector expression to match elements against.</param>
        [JSReplacement("$this.parents($selector)")]
        public extern JQuery Parents(string selector);
    }
}
