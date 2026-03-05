using JSIL.Meta;
using System.Collections;

namespace EnchantsOrder.JSIL.Common
{
    /// <summary>
    /// Represents a list of objects that can be accessed by index or by a string key. Provides methods to search, sort, filter, and manipulate the data.
    /// </summary>
    public sealed class BindingList<T>
    {
        /// <summary>
        /// Creates a List object.
        /// </summary>
        /// <param name="list">The array containing the elements to initalize the list.</param>
        [JSReplacement("new WinJS.Binding.List($list)")]
        public extern BindingList(T[]? list);

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="index">The index of the value to get.</param>
        /// <returns> The value at the specified index.</returns>
        [JSReplacement("$this.getAt($index)")]
        public extern T GetAt(int index);

        /// <summary>
        /// Appends new element(s) to a list, and returns the new length of the list.
        /// </summary>
        /// <param name="value">The element to insert at the end of the list.</param>
        /// <returns>The new length of the list.</returns>
        [JSReplacement("$this.push($value)")]
        public extern int Push(T value);

        /// <inheritdoc cref="Push"/>
        [JSReplacement("$this.push($value)")]
        public extern int Push(params T[] value);

        /// <summary>
        /// Removes elements from a list and, if necessary, inserts new elements in their place, returning the deleted elements.
        /// </summary>
        /// <param name="start">The zero-based location in the list from which to start removing elements.</param>
        /// <param name="howMany">The number of elements to remove.</param>
        /// <returns>The deleted elements.</returns>
        [JSReplacement("$this.splice($start, $howMany)")]
        public extern T[] Splice(int start, int howMany);

        /// <summary>
        /// Gets the <see cref="IListDataSource{T}"/> for the list. The only purpose of this property is to adapt a List to the data model that is used by <see cref="ListView"/> and FlipView.
        /// </summary>
        public extern IListDataSource<T> dataSource
        {
            [JSReplacement("$this.dataSource")]
            get;
        }

        /// <summary>
        /// Indicates that the object is compatibile with declarative processing.
        /// </summary>
        public extern int Length
        {
            [JSReplacement("$this.length")]
            get;
        }
    }

    public static class BindingListImplement
    {
        public static ListReader<T> GetEnumerable<T>(this BindingList<T> list) => new(list);

        [JSImmutable]
        public readonly struct ListReader<T>(BindingList<T> list) : IEnumerable<T>
        {
            public int Count => list.Length;

            public IEnumerator<T> GetEnumerator()
            {
                for (int i = 0; i < list.Length; i++)
                {
                    yield return list.GetAt(i);
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }

    public interface IListDataSource<T>;

    public interface IWinJSElement : IHTMLElementBase
    {
        [JSReplacement("$this")]
        T As<T>() where T : IWinJSElement;
    }

    /// <summary>
    /// A rich input box that provides suggestions as the user types.
    /// </summary>
    public interface IAutoSuggestBox : IWinJSElement
    {
        /// <summary>
        /// Gets or sets the query text for the AutoSuggestBox.
        /// </summary>
        string QueryText
        {
            [JSReplacement("$this.queryText")]
            get;
        }

        internal string PrevQueryText
        {
            [JSReplacement("$this._prevQueryText = $value")]
            set;
        }
    }

    public interface ISearchBoxEventArgs
    {
        /// <summary>
        /// The query text entered into the <see cref="IAutoSuggestBox"/> control.
        /// </summary>
        string QueryText
        {
            [JSReplacement("$this.queryText")]
            get;
        }
    }

    /// <summary>
    /// Provides event data for the SearchBox.SuggestionsRequested event.
    /// </summary>
    public interface ISearchBoxSuggestionsRequestedEventArgs : ISearchBoxEventArgs
    {
        /// <summary>
        /// The collection of suggested options for the search query. The maximum length of all of the textual fields in a suggestion (such as text, detail text, and image alt text) is 512 characters.
        /// </summary>
        ISearchSuggestionCollection SearchSuggestionCollection
        {
            [JSReplacement("$this.searchSuggestionCollection")]
            get;
        }
    }

    /// <summary>
    /// Appends a list of query suggestions to the list of search suggestions for the search pane.
    /// </summary>
    public interface ISearchSuggestionCollection
    {
        /// <summary>
        /// Appends a list of query suggestions to the list of search suggestions for the search pane.
        /// </summary>
        /// <param name="suggestions">The list of query suggestions.</param>
        [JSReplacement("$this.appendQuerySuggestions($suggestions)")]
        void AppendQuerySuggestions(params string[] suggestions);
    }

    public interface IListView<T> : IWinJSElement
    {
        /// <summary>
        /// Returns the index of the item that the specified DOM element displays.
        /// </summary>
        /// <param name="element">The DOM element that displays the item.</param>
        /// <returns>The index of the item displayed by element.</returns>
        [JSReplacement("$this.indexOfElement($element)")]
        int IndexOfElement(IHTMLElement element);

        /// <summary>
        /// Gets or sets the data source for the ListView.
        /// </summary>
        IListDataSource<T> ItemDataSource
        {
            [JSReplacement("$this.itemDataSource = $value")]
            set;
        }
    }
}
