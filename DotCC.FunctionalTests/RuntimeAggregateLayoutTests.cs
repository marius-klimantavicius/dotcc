using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using DotCC.Layout;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class RuntimeAggregateLayoutTests
{
    [Fact]
    public void Runtime_field_metadata_matches_actual_libc_storage()
    {
        var path = Path.Combine(Path.GetTempPath(), $"dotcc-runtime-layout-{Guid.NewGuid():N}.c");
        File.WriteAllText(path, "#include <time.h>\n#include <locale.h>\n#include <stddef.h>\nint main(void) { return offsetof(struct timespec, tv_nsec) + offsetof(struct tm, tm_zone) + offsetof(struct lconv, int_n_sign_posn); }");
        try
        {
            var aggregates = OffsetDocument.ReadSource(Compiler.EmitObject(path))
                .SelectMany(document => document.Aggregates).ToDictionary(pair => pair.Key, pair => pair.Value);
            var model = new OffsetLayoutModel(name => aggregates[name]);
            foreach (var type in new[] { typeof(Libc.Libc.timespec), typeof(Libc.Libc.tm), typeof(Libc.Libc.lconv) })
            {
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                var described = aggregates[type.Name].Fields;
                described.Select(field => field.Name).ShouldBe(fields.Select(field => field.Name));
                model.Aggregate(type.Name).Size.ShouldBe(Marshal.SizeOf(type));
                foreach (var field in fields)
                {
                    model.Offset(type.Name, new[] { field.Name })
                        .ShouldBe(Marshal.OffsetOf(type, field.Name).ToInt32());
                }
            }
        }
        finally { File.Delete(path); }
    }
}
