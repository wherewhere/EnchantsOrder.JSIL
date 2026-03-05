using EnchantsOrder.JSIL.Common;
using EnchantsOrder.JSIL.Models;
using EnchantsOrder.JSIL.Properties;
using EnchantsOrder.Models;
using JSIL;
using JSIL.Meta;
using System.Collections;
using System.Globalization;
using System.Text;
using Enchantment = EnchantsOrder.JSIL.Models.Enchantment;

namespace EnchantsOrder.JSIL
{
    public static class Program
    {
        private static object[] Items = [];
        [JSImmutable]
        private static readonly List<Enchantment> Enchantments = [];

        public static void Main(string[] _)
        {
            start();
            [JSReplacement("WinJS.Application.start()")]
            static extern void start();

            JQuery.Invoke(OnLoaded);
        }

        private static async void OnLoaded()
        {
            _ = JQuery.Invoke("#progress-bar").RemoveAttr("style");
            await InitializeEnchantmentsAsync();
            InitializeEnchants();
            InitializeItems();
        }

        private static void InitializeEnchants()
        {
            JQuery name = JQuery.Invoke("#enchants-enchantment-name-selector");
            dynamic selector = name.GetItem(0).winControl;

            JQuery penalty = JQuery.Invoke("#enchants-object-penalty-input");
            JQuery level = JQuery.Invoke("#enchants-enchantment-level-input");
            JQuery weight = JQuery.Invoke("#enchants-enchantment-weight-input");

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
                    _ = level.Val(enchantment.Level);
                    _ = weight.Val(enchantment.Weight);
                    selector._prevQueryText = enchantment.Name;
                }
            }));

            dynamic enchantments = Builtins.Eval("new WinJS.Binding.List([])");
            dynamic listView = JQuery.Invoke("#enchants-wanted-list").GetItem(0).winControl;
            listView.itemDataSource = enchantments.dataSource;

            JQuery wantedGroup = JQuery.Invoke("#enchants-wanted-group").RemoveAttr("style").Hide();
            JQuery resultsGroup = JQuery.Invoke("#enchants-results-group").RemoveAttr("style").Hide();

            JQuery.Invoke("#enchants-enchantment-add").Click(_ =>
            {
                string name = selector.queryText;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    int levelValue = int.Parse(level.Val<string>());
                    int weightValue = int.Parse(weight.Val<string>());
                    global::EnchantsOrder.Models.Enchantment enchantment = new(name, levelValue, weightValue);
                    enchantments.push(enchantment);
                    _ = wantedGroup.Show();
                }
            });

            dynamic window = Builtins.Global["window"];
            window.deleteEnchantment = new Action<object>(e =>
            {
                try
                {
                    JQuery template = JQuery.Invoke(e).Parents(".win-template");
                    object index = listView.indexOfElement(template.GetItem(0));
                    enchantments.splice(index, 1);
                }
                finally
                {
                    if (enchantments.length == 0)
                    {
                        _ = wantedGroup.Hide();
                        _ = resultsGroup.Hide();
                    }
                }
            });

            _ = JQuery.Invoke("#enchants-enchantment-start").Click(_ =>
            {
                ListReader<global::EnchantsOrder.Models.Enchantment> reader = new((object)enchantments);
                if (reader.Count > 0)
                {
                    int penaltyValue = int.Parse(penalty.Val<string>());
                    _ = JQuery.Invoke("#enchants-results-output").Text(reader.Ordering(penaltyValue).ToCultureString());
                    _ = resultsGroup.Show();
                }
            });
        }

        private static void InitializeItems()
        {
            JQuery item = JQuery.Invoke("#items-object-select-selector");
            JQuery penalty = JQuery.Invoke("#items-object-penalty-input");

            _ = item.Html(
                $"""
                <option style='display: none' value disabled selected>{Resource.ChooseItem}</option>
                {string.Join("\n", Items.Select(x => $"<option>{x}</option>"))}
                """);

            JQuery resultsGroup = JQuery.Invoke("#items-results-group").RemoveAttr("style").Hide();

            item.Change(OnChange);
            penalty.Change(OnChange);

            void OnChange(object _)
            {
                string itemName = item.Val<string>();
                if (!string.IsNullOrWhiteSpace(itemName))
                {
                    int penaltyValue = int.Parse(penalty.Val<string>());
                    _ = JQuery.Invoke("#items-results-output").Text(GetItemList(itemName, penaltyValue));
                    resultsGroup.Show();
                }
            }
        }

        private static async Task InitializeEnchantmentsAsync()
        {
            JQuery progress = JQuery.Invoke("#progress-bar").Show();
            try
            {
                dynamic getCurrentLanguage = Builtins.Global["getCurrentLanguage"];
                dynamic json = await XMLHttpRequest.FetchJsonAsync($"https://cdn.jsdelivr.net/gh/wherewhere/Enchants-Order@main/EnchantsOrder/EnchantsOrder.Demo/Assets/Enchants/Enchants.{(string)getCurrentLanguage()}.json");
                dynamic Object = Builtins.Global["Object"];
                object[] keys = Object.keys(json);
                foreach (object key in keys)
                {
                    Enchantments.Add(new Enchantment(key.ToString(), (object)json[key]));
                }
                Items = Enchantments.MaxByItems().Items;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                _ = progress.Hide();
            }
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
                        _ = builder.AppendLine(results.ToCultureString());
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

        [JSReplacement("console.log($message)")]
        internal static extern void Log(object message);
    }

    [JSImmutable]
    file readonly struct ListReader<T>(object list) : IEnumerable<T>
    {
        public int Count
        {
            get
            {
                return length(list);
                [JSReplacement("$list.length")]
                extern static int length(object list);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return getAt(list, i);
                [JSReplacement("$list.getAt($index)")]
                extern static T getAt(object list, int index);
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

        public static string ToCultureString(this OrderingResults results)
        {
            CultureInfo culture = Resource.Culture ?? CultureInfo.CurrentUICulture ?? CultureInfo.CurrentCulture;
            if (culture.TwoLetterISOLanguageName.Equals("en", StringComparison.OrdinalIgnoreCase))
            {
                return results.ToString();
            }
            else
            {
                StringBuilder builder = new();
                foreach (EnchantmentStep step in results.Steps)
                {
                    _ = builder.AppendLine(step.ToCultureString());
                }
                return builder.Append(Resource.PenaltyLevel).Append(": ").AppendLine(results.Penalty.ToString())
                              .Append(Resource.MaxExperienceLevel).Append(": ").AppendLine(results.MaxExperience.ToString())
                              .Append(Resource.TotalExperienceLevel).Append(": ").Append(results.TotalExperience.ToString())
                              .ToString();
            }
        }

        public static string ToCultureString(this EnchantmentStep step)
        {
            CultureInfo culture = Resource.Culture ?? CultureInfo.CurrentUICulture ?? CultureInfo.CurrentCulture;
            if (culture.TwoLetterISOLanguageName.Equals("en", StringComparison.OrdinalIgnoreCase))
            {
                return step.ToString();
            }
            else
            {
                StringBuilder builder = new();
                _ = builder.Append(Resource.Step).Append(' ').Append(step.Step).Append(':');
                int half = step.Count / 2;
                for (int i = half; i > 0; i--)
                {
                    bool flag = step.Count == 2;
                    int index = (half * 2) - (i * 2);
                    _ = builder.Append(' ');
                    if (!flag)
                    {
                        _ = builder.Append('(');
                    }
                    _ = builder.Append(step[index])
                               .Append(" + ")
                               .Append(step[index + 1]);
                    if (!flag)
                    {
                        _ = builder.Append(')');
                    }
                    if (index + 2 != step.Count)
                    {
                        _ = builder.Append(" +");
                    }
                }
                if ((step.Count & 1) == 1)
                {
                    _ = builder.Append(' ').Append(step[^1]);
                }
                return builder.ToString();
            }
        }
    }
}