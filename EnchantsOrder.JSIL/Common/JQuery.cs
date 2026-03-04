using JSIL;

namespace EnchantsOrder.JSIL.Common
{
    /// <summary>
    /// Static members of jQuery (those on $ and jQuery themselves)
    /// </summary>
    public sealed class JQueryStatic(dynamic jquery)
    {
        public static JQueryStatic Instance => new(Builtins.Global["$"]);

        /// <summary>
        /// Accepts a string containing a CSS selector which is then used to match a set of elements.
        /// </summary>
        /// <param name="element">A string containing a selector expression or a DOM element to wrap in a jQuery object.</param>
        public JQuery Invoke(object element) => new((object)jquery(element));
    }

    /// <summary>
    /// The jQuery instance members.
    /// </summary>
    public sealed class JQuery(dynamic jquery)
    {
        public dynamic this[int index] => jquery[index];

        /// <summary>
        /// Set the HTML contents of each element in the set of matched elements.
        /// </summary>
        /// <param name="html">A string of HTML to set as the content of each matched element.</param>
        public JQuery Html(string html) => new((object)jquery.html(html));

        /// <summary>
        /// Remove an attribute from each element in the set of matched elements.
        /// </summary>
        /// <param name="attributeName">An attribute to remove; as of version 1.7, it can be a space-separated list of attributes.</param>
        public JQuery RemoveAttr(string attributeName) => new((object)jquery.removeAttr(attributeName));

        /// <summary>
        /// Get the current value of the first element in the set of matched elements.
        /// </summary>
        public dynamic Val() => jquery.val();

        /// <summary>
        /// Set the value of each element in the set of matched elements.
        /// </summary>
        /// <param name="value">A string of text, an array of strings or number corresponding to the value of each matched element to set as selected/checked.</param>
        public JQuery Val(object value) => new((object)jquery.val(value));

        /// <summary>
        /// Hide the matched elements.
        /// </summary>
        /// <param name="options">A map of additional options to pass to the method.</param>
        public JQuery Hide(object? options = null) => new((object)jquery.hide(options));

        /// <summary>
        /// Display the matched elements.
        /// </summary>
        /// <param name="options">A map of additional options to pass to the method.</param>
        public JQuery Show(object? options = null) => new((object)jquery.show(options));

        /// <summary>
        /// Bind an event handler to the "change" JavaScript event
        /// </summary>
        /// <param name="eventObject">A function to execute each time the event is triggered.</param>
        public JQuery Change(Action<dynamic> eventObject) => new((object)jquery.change(eventObject));

        /// <summary>
        /// Bind an event handler to the "click" JavaScript event
        /// </summary>
        /// <param name="eventObject">An object containing data that will be passed to the event handler.</param>
        public JQuery Click(Action<dynamic> eventObject) => new((object)jquery.click(eventObject));

        /// <summary>
        /// Set the content of each element in the set of matched elements to the specified text.
        /// </summary>
        /// <param name="text">The text to set as the content of each matched element. When Number or Boolean is supplied, it will be converted to a String representation.</param>
        public JQuery Text(string text) => new((object)jquery.text(text));

        /// <summary>
        /// Get the ancestors of each element in the current set of matched elements, optionally filtered by a selector.
        /// </summary>
        /// <param name="selector">A string containing a selector expression to match elements against.</param>
        public JQuery Parents(string selector) => new((object)jquery.parents(selector));
    }
}
