using EnchantsOrder.JSIL.Common;
using EnchantsOrder.Models;
using JSIL;
using JSIL.Meta;

namespace EnchantsOrder.JSIL.Models
{
    internal sealed class Enchantment(string name, object token) : IEnchantment
    {
        /// <inheritdoc/>
        public int Level
        {
            get
            {
                return levelMax(token);
                [JSReplacement("$token.levelMax")]
                static extern int levelMax(object token);
            }
        }

        /// <inheritdoc/>
        public int Weight
        {
            get
            {
                return weight(token);
                [JSReplacement("$token.weight")]
                static extern int weight(object token);
            }
        }

        public bool Hidden
        {
            get
            {
                return hidden(token);
                [JSReplacement("$token.hidden")]
                static extern bool hidden(object token);
            }
        }

        /// <inheritdoc/>
        public string Name => name;

        public object[] Items
        {
            get
            {
                return items(token);
                [JSReplacement("$token.items")]
                static extern object[] items(object token);
            }
        }

        public object[]? Incompatible
        {
            get
            {
                return incompatible(token);
                [JSReplacement("$token.incompatible || null")]
                static extern object[] incompatible(object token);
            }
        }

        /// <inheritdoc/>
        public long Experience => (long)Level * Weight;

        /// <inheritdoc/>
        public override string ToString() => Name + Level.GetRomanNumber();

        /// <inheritdoc/>
        public int CompareTo(IEnchantment? other)
        {
            if (other is null) { return -1; }
            int value = Experience.CompareTo(other.Experience);
            if (value == 0)
            {
                value = Level.CompareTo(other.Level);
                if (value == 0)
                {
                    value = Name.CompareTo(other.Name);
                }
            }
            return value;
        }
    }
}
