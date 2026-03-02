using EnchantsOrder.Common;
using EnchantsOrder.JSIL.Common;
using EnchantsOrder.Models;

namespace EnchantsOrder.JSIL.Models
{
    internal sealed class EnchantmentGroup(IEnchantment[] enchantments) : IEnchantment
    {
        /// <inheritdoc/>
        public int Level => enchantments[0].Level;
        /// <inheritdoc/>
        public int Weight => enchantments[0].Weight;

        /// <inheritdoc/>
        public string Name => string.Join("/", enchantments.OfType<IEnchantment>().Select(x => x.Name));

        /// <inheritdoc/>
        public long Experience => enchantments[0].Experience;

        /// <inheritdoc/>
        public override string ToString() => $"{Name} {Level.GetRomanNumber()}";

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
