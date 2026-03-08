using EnchantsOrder.JSIL.Common;
using EnchantsOrder.JSIL.Models;
using EnchantsOrder.JSIL.Properties;
using EnchantsOrder.Models;
using JSIL.Meta;
using System.Globalization;
using System.Text;
using Enchantment = EnchantsOrder.JSIL.Models.Enchantment;

namespace EnchantsOrder.JSIL
{
    public static class Program
    {
        private static bool hashChanged = false;
        [JSImmutable]
        private static readonly BindingList<global::EnchantsOrder.Models.Enchantment> wantedList = new([]);
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
            IAutoSuggestBox selector = name[0].WinControl.As<IAutoSuggestBox>();

            selector.AddEventListener<ICustomEvent<ISearchBoxSuggestionsRequestedEventArgs>>(
                "suggestionsrequested",
                eventInfo =>
                {
                    string queryText = eventInfo.Detail.QueryText;
                    ISearchSuggestionCollection collection = eventInfo.Detail.SearchSuggestionCollection;
                    collection.AppendQuerySuggestions(
                        [.. (string.IsNullOrWhiteSpace(queryText) ? Enchantments
                            : Enchantments.Where(x => x.Name.Contains(queryText))).Select(x => x.Name)]);
                });

            JQuery penalty = JQuery.Invoke("#enchants-object-penalty-input");
            JQuery level = JQuery.Invoke("#enchants-enchantment-level-input");
            JQuery weight = JQuery.Invoke("#enchants-enchantment-weight-input");

            selector.AddEventListener<ICustomEvent<ISearchBoxEventArgs>>(
                "querysubmitted",
                eventInfo =>
                {
                    string queryText = eventInfo.Detail.QueryText;
                    Enchantment enchantment = Enchantments.FirstOrDefault(x => x.Name.Equals(queryText, StringComparison.OrdinalIgnoreCase));
                    if (enchantment != null)
                    {
                        _ = level.Val(enchantment.Level);
                        _ = weight.Val(enchantment.Weight);
                        selector.PrevQueryText = enchantment.Name;
                    }
                });

            LoadSettings();
            [JSReplacement("addEventListener($type, $listener)")]
            static extern void addEventListener(string type, Action listener);
            addEventListener("hashchange", LoadSettings);

            JQuery wantedGroup = JQuery.Invoke("#enchants-wanted-group").RemoveAttr("style");
            if (wantedList.Length == 0)
            {
                _ = wantedGroup.Hide();
            }

            JQuery.Invoke("#enchants-enchantment-add").Click(_ =>
            {
                string name = selector.QueryText;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    _ = wantedList.Push(
                        new global::EnchantsOrder.Models.Enchantment(
                            name,
                            int.Parse(level.Val<string>()),
                            int.Parse(weight.Val<string>())));
                    _ = wantedGroup.Show();
                }
            });

            IListView<global::EnchantsOrder.Models.Enchantment> listView =
                JQuery.Invoke("#enchants-wanted-list")[0].WinControl.As<IListView<global::EnchantsOrder.Models.Enchantment>>();
            listView.ItemDataSource = wantedList.dataSource;

            JQuery resultsGroup = JQuery.Invoke("#enchants-results-group").RemoveAttr("style").Hide();
            [JSReplacement("window.deleteEnchantment = $action")]
            static extern void SetDeleteEnchantment(Action<IHTMLElement> action);
            SetDeleteEnchantment(e =>
            {
                try
                {
                    JQuery template = JQuery.Invoke(e).Parents(".win-template");
                    int index = listView.IndexOfElement(template[0]);
                    wantedList.Splice(index, 1);
                }
                finally
                {
                    if (wantedList.Length == 0)
                    {
                        _ = wantedGroup.Hide();
                        _ = resultsGroup.Hide();
                    }
                }
            });

            _ = JQuery.Invoke("#enchants-enchantment-start").Click(_ =>
            {
                BindingListImplement.ListReader<global::EnchantsOrder.Models.Enchantment> reader =
                    wantedList.GetEnumerable();
                if (reader.Count > 0)
                {
                    int penaltyValue = int.Parse(penalty.Val<string>());
                    OrderingResults results = reader.Ordering(penaltyValue);
                    string str = results.ToCultureString();
                    _ = results.IsTooExpensive
                        ? JQuery.Invoke("#enchants-results-output").Html(str + "\n<span class='accent-text'>" + Resource.TooExpensive + "</span>")
                        : JQuery.Invoke("#enchants-results-output").Text(str);
                    _ = resultsGroup.Show();
                    SetSettings(reader);
                }
            });
        }

        private static void InitializeItems()
        {
            JQuery item = JQuery.Invoke("#items-object-select-selector").Html(
                $"""
                <option style='display: none' value disabled selected>{Resource.ChooseItem}</option>
                {string.Join("\n", Items.Select(x => "<option>" + x + "</option>"))}
                """);
            JQuery penalty = JQuery.Invoke("#items-object-penalty-input");

            JQuery resultsGroup = JQuery.Invoke("#items-results-group").RemoveAttr("style").Hide();
            _ = item.Change(OnChange);
            _ = penalty.Change(OnChange);

            void OnChange(object _)
            {
                string itemName = item.Val<string>();
                if (!string.IsNullOrWhiteSpace(itemName))
                {
                    int penaltyValue = int.Parse(penalty.Val<string>());
                    _ = JQuery.Invoke("#items-results-output").Html(GetItemList(itemName, penaltyValue));
                    _ = resultsGroup.Show();
                }
            }
        }

        private static async Task InitializeEnchantmentsAsync()
        {
            JQuery progress = JQuery.Invoke("#progress-bar").Show();
            try
            {
                [JSReplacement("getCurrentLanguage()")]
                static extern string getCurrentLanguage();
                object json = await XMLHttpRequest.FetchJsonAsync("https://cdn.jsdelivr.net/gh/wherewhere/Enchants-Order@main/EnchantsOrder/EnchantsOrder.Demo/Assets/Enchants/Enchants." + getCurrentLanguage() + ".json");
                string[] keys = json.Keys();
                foreach (string key in keys)
                {
                    Enchantments.Add(new Enchantment(key.ToString(), json.GetItem<object, string>(key)));
                }
                Items = Enchantments.MaxByItems().Items;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.As<object, string>());
            }
            finally
            {
                _ = progress.Hide();
            }
        }

        private static void LoadSettings()
        {
            if (hashChanged)
            {
                hashChanged = false;
                return;
            }
            string hash = locationHash()[1..];
            [JSReplacement("location.hash")]
            static extern string locationHash();
            if (!string.IsNullOrWhiteSpace(hash))
            {
                URLSearchParams @params = new(hash);
                if (@params.Has("wanted"))
                {
                    string? value = @params.Get("wanted");
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        string? wanted = decompressFromEncodedURIComponent(value!);
                        [JSReplacement("LZString.decompressFromEncodedURIComponent($compressed)")]
                        static extern string? decompressFromEncodedURIComponent(string compressed);
                        if (!string.IsNullOrWhiteSpace(wanted))
                        {
                            object list = parse(wanted!);
                            if (list is object[] array)
                            {
                                _ = wantedList.Splice(0, wantedList.Length);
                                foreach (object item in array)
                                {
                                    _ = wantedList.Push(
                                        new global::EnchantsOrder.Models.Enchantment(
                                            item.GetItem<object, string>("name"),
                                            item.GetItem<object, int>("level"),
                                            item.GetItem<object, int>("weight")));
                                }
                            }
                            [JSReplacement("JSON.parse($wanted)")]
                            static extern object parse(string wanted);
                        }
                    }
                }
            }
        }

        private static void SetSettings(BindingListImplement.ListReader<global::EnchantsOrder.Models.Enchantment> wantedList)
        {
            JSObject settings = new();
            if (wantedList.Count > 0)
            {
                settings["wanted"] = compressToEncodedURIComponent(
                    stringify([
                        .. wantedList.Select(x => new JSObject() {
                            ["name"] = x.Name.As<string, JSObject>(),
                            ["level"] = x.Level.As<int, JSObject>(),
                            ["weight"] = x.Weight.As<int, JSObject>()
                        })])).As<string, JSObject>();
                [JSReplacement("JSON.stringify($array)")]
                static extern string stringify(object[] array);
                [JSReplacement("LZString.compressToEncodedURIComponent($input)")]
                static extern string compressToEncodedURIComponent(string input);
            }
            locationHash(new URLSearchParams(settings).ToString());
            [JSReplacement("location.hash = $hash")]
            static extern void locationHash(string hash);
            hashChanged = true;
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

                _ = builder.Append("<code class='win-code win-type-base'>")
                           .Append(text)
                           .AppendLine("</code>");
                foreach (List<IEnchantment> list in GetAllEnchantmentPaths(group))
                {
                    try
                    {
                        _ = builder.AppendLine("*****************");
                        OrderingResults results = list.Ordering(initialPenalty);
                        _ = builder.AppendLine(results.ToCultureString());
                        if (results.IsTooExpensive)
                        {
                            _ = builder.Append("<span class='accent-text'>")
                                       .Append(Resource.TooExpensive)
                                       .AppendLine("</span>");
                        }
                    }
                    catch (Exception ex)
                    {
                        _ = builder.AppendLine(ex.Message);
                    }
                }

                return builder.ToString();
            }
            return string.Format(Resource.NoEnchantments, text);
        }

        [JSReplacement("console.log($message)")]
        internal static extern void Log<T>(T message);
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