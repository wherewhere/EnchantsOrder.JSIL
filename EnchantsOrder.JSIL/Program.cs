using EnchantsOrder;
using EnchantsOrder.Models;
using JSIL;
using JSIL.Meta;
using System.Reflection;

namespace EnchantsOrder.JSIL
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            dynamic WinJS = Builtins.Global["WinJS"];
            WinJS.UI.processAll();
            WinJS.Application.start();

            dynamic jquery = Builtins.Global["$"];
            jquery((Action)OnLoaded);
        }

        private static void OnLoaded()
        {
            dynamic jquery = Builtins.Global["$"];
            dynamic selector = jquery("#enchantment-name")[0].winControl;
            selector.addEventListener("suggestionsrequested", (Action<dynamic>)(eventInfo =>
            {
                dynamic collection = eventInfo.detail.searchSuggestionCollection;
                collection.appendQuerySuggestions(new[] { "test", "test1", "test2" });
            }));

            dynamic penalty = jquery("#object-penalty");
            dynamic level = jquery("#enchantment-level");
            dynamic weight = jquery("#enchantment-weight");

            List<Enchantment> enchantments = [];
            jquery("#enchantment-add").click((Action)(() =>
            {
                string name = selector.queryText;
                int levelValue = int.Parse(level.val());
                int weightValue = int.Parse(weight.val());
                Enchantment enchantment = new Enchantment(name, levelValue, weightValue);
                enchantments.Add(enchantment);
            }));

            jquery("#enchantment-start").click((Action)(() =>
            {
                int penaltyValue = int.Parse(penalty.val());
                jquery("#results").text(enchantments.Ordering(penaltyValue));
            }));
        }
    }
}