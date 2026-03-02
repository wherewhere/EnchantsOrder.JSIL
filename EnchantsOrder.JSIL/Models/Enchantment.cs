using EnchantsOrder.JSIL.Common;
using EnchantsOrder.Models;
using JSIL;

namespace EnchantsOrder.JSIL.Models
{
    internal sealed class Enchantment(string name, dynamic token) : IEnchantment
    {
        /// <inheritdoc/>
        public int Level => token.levelMax;
        /// <inheritdoc/>
        public int Weight => token.weight;

        public bool Hidden => Builtins.IsTruthy((object)token.hidden);

        /// <inheritdoc/>
        public string Name => name;

        public object[] Items => token.items;
        public object[]? Incompatible
        {
            get
            {
                dynamic incompatible = token.incompatible;
                return Builtins.IsTruthy((object)incompatible) ? incompatible : null;
            }
        }

        /// <inheritdoc/>
        public long Experience => (long)Level * Weight;

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
