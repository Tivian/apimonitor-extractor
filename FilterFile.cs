using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace APIMonitor {
    // Field=      "API Module Name", "API Name", "Call #", "Call Depth", "Call Failed", "Call Stack Address", "Call Stack Function Name", "Call Stack Function Offset", "Call Stack Function Ordinal", "Call Stack Module Handle", "Call Stack Module Name", "Call Stack Module Path", "Call Stack Offset", "Calling Module Handle", "Calling Module Name", "Calling Module Path", "Category", "COM API", "COM Interface Name", "COM Method Name", "Duration", "Error Code", "Error Message", "External API", "Ordinal Number", "Thread #", "Thread ID (TID)", "Unicode API", "VTable Index"
    // Relation=   "is", "is not", "begins with", "ends with", "contains", "excludes"
    // Value=
    // Action=     "Show", "Hide"
    // Enabled=    "True", "False"
    // IgnoreCase= "True" [optional]
    public class Filter {
        public string Field;
        public string Relation;
        public string Value;
        public string Action;
        public string Enabled;
        public string IgnoreCase;

        public Filter() { }

        public Filter(string field, string relation, string value, string action)
            : this(field, relation, value, action, "True", null) { }

        public Filter(string field, string relation, string value, string action, string enabled)
            : this(field, relation, value, action, enabled, null) { }

        public Filter(string field, string relation, string value, string action, string enabled, string ignoreCase) {
            Field = field;
            Relation = relation;
            Value = value;
            Action = action;
            Enabled = enabled;
            IgnoreCase = ignoreCase;
        }

        public Filter(XmlElement element) {
            foreach (var field in GetType().GetFields())
                field.SetValue(this, element.GetAttribute(field.Name));
        }

        public override string ToString()
            => $"<Filter "
                + $"Field=\"{Field}\" "
                + $"Relation=\"{Relation}\" "
                + $"Value=\"{Value}\" "
                + $"Action=\"{Action}\" "
                + $"Enabled=\"{Enabled}\""
                + ((IgnoreCase == "True") ? $" IgnoreCase=\"{IgnoreCase}\"" : "")
                + "/>";
    }

    public class FilterFile : IEnumerable<Filter> {
        public const string PREAMBLE = "<?xml version=\"1.0\"?>\n\t<!--\n"
            + "\tAPI Monitor Filter\n"
            + "\t(c) 2010-2013, Rohitab Batra <rohitab@rohitab.com>\n"
            + "\thttp://www.rohitab.com/apimonitor/\n"
            + "\t-->\n<ApiMonitor>\n\t<DisplayFilter>\n\t\t";
        public const string POSTAMBLE = "\n\t</DisplayFilter>\n</ApiMonitor>\n";

        private List<Filter> Filters;

        public Filter this[int i]
            => (i < 0 || i >= Filters.Count) ? null : Filters[i];

        public FilterFile() {
            Filters = new List<Filter>();
        }

        public FilterFile(IEnumerable<Filter> filters) {
            Filters = new List<Filter>(filters);
        }

        public FilterFile(string path) {
            var doc = new XmlDocument();
            doc.Load(path);
            Filters = doc.SelectNodes("//Filter").OfType<XmlElement>().Select(x => new Filter(x)).ToList();
        }

        public void Add(Filter filter) {
            if (filter != null)
                Filters.Add(filter);
        }

        public void AddRange(IEnumerable<Filter> filters) {
            Filters.AddRange(filters);
        }

        public void Save(string path)
            => File.WriteAllText(path, ToString());

        public IEnumerator<Filter> GetEnumerator()
            => Filters.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public override string ToString()
            => PREAMBLE + string.Join("\n\t\t", Filters) + POSTAMBLE;
    }
}