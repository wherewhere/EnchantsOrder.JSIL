using EnchantsOrder.JSIL.Common;
using EnchantsOrder.JSIL.Models;
using EnchantsOrder.Models;
using JSIL;
using System.Collections;
using System.Text;
using Enchantment = EnchantsOrder.JSIL.Models.Enchantment;

namespace EnchantsOrder.JSIL
{
    public static class Program
    {
        private static object[] Items = [];
        private static readonly List<Enchantment> Enchantments = [];

        public static void Main(string[] _)
        {
            dynamic WinJS = Builtins.Global["WinJS"];
            WinJS.Application.start();

            dynamic jquery = Builtins.Global["$"];
            jquery((Action)OnLoaded);
        }

        private static async void OnLoaded()
        {
            await InitializeEnchantmentsAsync();
            InitializeEnchants();
            InitializeItems();
        }

        private static void InitializeEnchants()
        {
            dynamic jquery = Builtins.Global["$"];
            dynamic name = jquery("#enchants-enchantment-name-selector");
            dynamic selector = name[0].winControl;

            dynamic penalty = jquery("#enchants-object-penalty-input");
            dynamic level = jquery("#enchants-enchantment-level-input");
            dynamic weight = jquery("#enchants-enchantment-weight-input");

            selector.addEventListener("suggestionsrequested", new Action<dynamic>(eventInfo =>
            {
                string queryText = eventInfo.detail.queryText;
                dynamic collection = eventInfo.detail.searchSuggestionCollection;
                collection.appendQuerySuggestions(
                    (string.IsNullOrWhiteSpace(queryText) ? Enchantments
                        : Enchantments.Where(x => x.Name.Contains(queryText))).Select(x => x.Name).ToArray());
            }));

            selector.addEventListener("querysubmitted", new Action<dynamic>(eventInfo =>
            {
                string queryText = eventInfo.detail.queryText;
                Enchantment enchantment = Enchantments.FirstOrDefault(x => x.Name.Equals(queryText, StringComparison.OrdinalIgnoreCase));
                if (enchantment != null)
                {
                    level.val(enchantment.Level);
                    weight.val(enchantment.Weight);
                    dynamic input = name.find("input.win-autosuggestbox-input");
                    input.on("focusout", new Action(() =>
                    {
                        selector.queryText = enchantment.Name;
                        input.off("focusout");
                    }));
                }
            }));

            dynamic enchantments = Builtins.Eval("new WinJS.Binding.List([])");
            jquery("#enchants-wanted-list")[0].winControl.itemDataSource = enchantments.dataSource;

            dynamic wantedGroup = jquery("#enchants-wanted-group").removeAttr("style").hide();
            dynamic resultsGroup = jquery("#enchants-results-group").removeAttr("style").hide();

            jquery("#enchants-enchantment-add").click(new Action(() =>
            {
                string name = selector.queryText;
                int levelValue = int.Parse(level.val());
                int weightValue = int.Parse(weight.val());
                global::EnchantsOrder.Models.Enchantment enchantment = new(name, levelValue, weightValue);
                enchantments.push(enchantment);
                wantedGroup.show();
            }));

            jquery("#enchants-enchantment-start").click(new Action(() =>
            {
                int penaltyValue = int.Parse(penalty.val());
                jquery("#enchants-results-output").text(new ListReader<global::EnchantsOrder.Models.Enchantment>((object)enchantments).Ordering(penaltyValue));
                resultsGroup.show();
            }));
        }

        private static void InitializeItems()
        {
            dynamic jquery = Builtins.Global["$"];
            dynamic item = jquery("#items-object-select-selector");
            dynamic penalty = jquery("#items-object-penalty-input");

            item.html("<option style='display: none'>Choose Item</option>" + string.Join("\n", Items.Select(x => $"<option>{x}</option>")));

            dynamic resultsGroup = jquery("#items-results-group").removeAttr("style").hide();

            item.change(new Action(() =>
            {
                string itemName = item.val();
                int penaltyValue = int.Parse(penalty.val());
                jquery("#items-results-output").text(GetItemList(itemName, penaltyValue));
                resultsGroup.show();
            }));
        }

        private static async Task InitializeEnchantmentsAsync()
        {
            dynamic json = await XMLHttpRequest.FetchJsonAsync("https://cdn.jsdelivr.net/gh/wherewhere/Enchants-Order@main/EnchantsOrder/EnchantsOrder.Demo/Assets/Enchants/Enchants.en-US.json");
            dynamic jobject = Builtins.Global["Object"];
            object[] keys = jobject.keys(json);
            foreach (object key in keys)
            {
                Enchantments.Add(new Enchantment(key.ToString(), (object)json[key]));
            }
            Items = Enchantments.MaxByItems().Items;
        }

        private static string GetItemList(string text, int initialPenalty)
        {
            List<Enchantment> enchantments = [.. Enchantments.Where(x => !x.Hidden && x.Items.Any(x => text.Equals(x.ToString(), StringComparison.OrdinalIgnoreCase)))];
            if (enchantments.Count > 0)
            {
                StringBuilder builder = new();
                List<List<IEnchantment>> group = [];
                while (enchantments.Count > 0)
                {
                    Enchantment enchantment = enchantments[0];
                    List<IEnchantment> list = [enchantment];
                    enchantments.RemoveAt(0);
                    if (enchantment.Incompatible?.Length > 0)
                    {
                        for (int i = enchantments.Count; --i >= 0;)
                        {
                            Enchantment temp = enchantments[i];
                            if (temp.Incompatible?.Any(x => enchantment.Name.Equals(x.ToString(), StringComparison.OrdinalIgnoreCase)) == true)
                            {
                                list.Add(temp);
                                enchantments.RemoveAt(i);
                            }
                        }
                        if (list.Count > 0)
                        {
                            List<IEnchantment> tempList = [];
                            while (list.Count > 0)
                            {
                                IEnchantment temp = list[0];
                                list.RemoveAt(0);
                                IEnumerable<IEnchantment> temps = list.Where(x => x.Level == temp.Level && x.Weight == temp.Weight);
                                if (temps.Any())
                                {
                                    IEnchantment[] array = [.. temps.Append(temp).OrderBy(x => x.Name)];
                                    tempList.Add(new EnchantmentGroup(array));
                                    foreach (IEnchantment enchantmentTemp in array)
                                    {
                                        list.Remove(enchantmentTemp);
                                    }
                                }
                                else
                                {
                                    tempList.Add(temp);
                                }
                            }
                            list = tempList;
                        }
                    }
                    group.Add(list);
                }

                static List<List<IEnchantment>> GetAllEnchantmentPaths(List<List<IEnchantment>> group)
                {
                    List<List<IEnchantment>> result = [];
                    static void Growing(List<List<IEnchantment>> result, List<List<IEnchantment>> group, int depth = 0, params List<IEnchantment> path)
                    {
                        if (depth == group.Count)
                        {
                            result.Add([.. path]);
                            return;
                        }
                        int next = depth + 1;
                        foreach (IEnchantment enchantment in group[depth])
                        {
                            path.Add(enchantment);
                            Growing(result, group, next, path);
                            path.RemoveAt(path.Count - 1);
                        }
                    }
                    Growing(result, group);
                    return result;
                }

                _ = builder.AppendLine(text);
                foreach (List<IEnchantment> list in GetAllEnchantmentPaths(group))
                {
                    try
                    {
                        _ = builder.AppendLine("*****************");
                        OrderingResults results = list.Ordering(initialPenalty);
                        _ = builder.AppendLine(results.ToString());
                    }
                    catch (Exception ex)
                    {
                        _ = builder.AppendLine(ex.Message);
                    }
                }

                return builder.ToString();
            }
            return $"No enchantments found for {text}.";
        }

        internal static void Log(object message)
        {
            dynamic console = Builtins.Global["console"];
            console.log(message);
        }
    }

    file readonly struct ListReader<T>(dynamic list) : IEnumerable<T>
    {
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < list.length; i++)
            {
                yield return (T)list.getAt(i);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    file static class Extensions
    {
        public static Enchantment MaxByItems(this IEnumerable<Enchantment> source)
        {
            Enchantment value;

            using (IEnumerator<Enchantment> e = source.GetEnumerator())
            {
                if (!e.MoveNext())
                {
                    throw new Exception("NoElementsException");
                }

                value = e.Current;
                while (e.MoveNext())
                {
                    Enchantment x = e.Current;
                    if (x.Items.Length > value.Items.Length)
                    {
                        value = x;
                    }
                }
            }

            return value;
        }

        public static IEnumerable<IEnchantment> Append(this IEnumerable<IEnchantment> source, IEnchantment element)
        {
            ArgumentNullException.ThrowIfNull(source);
            foreach (IEnchantment item in source)
            {
                yield return item;
            }
            yield return element;
        }

        public static IEnumerable<IEnchantment> OrderBy(this IEnumerable<IEnchantment> source, Func<IEnchantment, string> keySelector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(keySelector);

            List<IEnchantment> list = [.. source];

            if (list.Count <= 1) { return list; }

            list.Sort((a, b) => string.Compare(keySelector(a), keySelector(b)));

            return list;
        }
    }
}