using System.Collections.Generic;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    // Bundled headers leave these bodies to Libc so generated functions use the
    // same types as runtime imports. The binder still needs their actual LP64
    // fields for initialization, member coercion, sizeof/alignof and offsetof.
    // Register only after a synthetic header declaration identifies the runtime
    // contract; an unrelated user-defined tag must keep its own C definition.
    // Keep this metadata in sync with TimeLib, CalendarLib and LocaleLib.
    private void RegisterRuntimeAggregate(string name)
    {
        if (!_runtimeAggregateTags.Add(name)) return;
        var fields = new List<StructField>();
        void Add(CType type, params string[] names)
        {
            foreach (var field in names) fields.Add(new StructField(field, type));
        }
        switch (name)
        {
            case "timespec":
                Add(CType.Long, "tv_sec", "tv_nsec");
                break;
            case "tm":
                Add(CType.Int, "tm_sec", "tm_min", "tm_hour", "tm_mday", "tm_mon",
                    "tm_year", "tm_wday", "tm_yday", "tm_isdst");
                Add(CType.Long, "tm_gmtoff");
                Add(new CType.Pointer(CType.Char), "tm_zone");
                break;
            case "lconv":
                Add(new CType.Pointer(CType.Char), "decimal_point", "thousands_sep", "grouping",
                    "int_curr_symbol", "currency_symbol", "mon_decimal_point", "mon_thousands_sep",
                    "mon_grouping", "positive_sign", "negative_sign");
                Add(CType.Char, "int_frac_digits", "frac_digits", "p_cs_precedes", "p_sep_by_space",
                    "n_cs_precedes", "n_sep_by_space", "p_sign_posn", "n_sign_posn", "int_p_cs_precedes",
                    "int_n_cs_precedes", "int_p_sep_by_space", "int_n_sep_by_space", "int_p_sign_posn",
                    "int_n_sign_posn");
                break;
        }
        if (!_structFields.TryAdd(name, fields))
            throw new IrUnsupportedException($"runtime aggregate '{name}' conflicts with an existing C definition");
        _structIsUnion[name] = false;
    }
}
