#pragma warning disable IDE1006
using JSIL.Meta;

namespace EnchantsOrder.JSIL.Common
{
    /// <summary>
    /// The <see cref="Location"/> interface represents the location (URL) of the object it is linked to.
    /// Changes done on it are reflected on the object it relates to. Both the Document and Window interface
    /// have such a linked Location, accessible via Document.location and Window.location respectively.
    /// </summary>
    public interface Location
    {
        /// <summary>
        /// The hash property of the <see cref="Location"/> interface is a string containing a <c>"#"</c>
        /// followed by the fragment identifier of the location URL. If the URL does not have a fragment identifier,
        /// this property contains an empty string, <c>""</c>.
        /// </summary>
        string Hash
        {
            [JSReplacement("$this.hash")]
            get;
            [JSReplacement("$this.hash = $value")]
            set;
        }
    }
}
