using JSIL.Meta;

namespace EnchantsOrder.JSIL.Common
{
    /// <summary>
    /// The <see cref="URLSearchParams"/> interface defines utility methods to work with the query string of a URL.
    /// </summary>
    public sealed class URLSearchParams
    {
        [JSReplacement("new URLSearchParams($init)")]
        public extern URLSearchParams(object? init = null);

        /// <summary>
        /// The <see cref="Get"/> method of the <see cref="URLSearchParams"/> interface returns the first value associated to the given search parameter.
        /// </summary>
        [JSReplacement("$this.get($name)")]
        public extern string? Get(string name);

        /// <summary>
        /// The <see cref="Has"/> method of the <see cref="URLSearchParams"/> interface returns a boolean value that indicates whether the specified parameter is in the search parameters.
        /// </summary>
        [JSReplacement("$this.getAll($name)")]
        public extern bool Has(string name);
    }
}
