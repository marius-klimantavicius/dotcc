using System;
using System.IO;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("Console")]
public class StringHeaderTests
{
    [Fact]
    public void readonly_string_and_memory_sources_do_not_discard_const()
    {
        const string source = """
            #include <string.h>
            int main(void) {
                const char *s = "text", *set = "tx";
                const void *p = s;
                char dst[32]; char *save;
                strlen(s); strcmp(s, s); strncmp(s, s, 4); strcoll(s, s);
                strcasecmp(s, s); strncasecmp(s, s, 4);
                strcpy(dst, s); strncpy(dst, s, 4); strcat(dst, s); strncat(dst, s, 4);
                strchr(s, 't'); strrchr(s, 't'); strstr(s, set);
                strspn(s, set); strcspn(s, set); strpbrk(s, set);
                strtok(dst, set); strtok_r(dst, set, &save);
                memcpy(dst, p, 4); memmove(dst, p, 4); memcmp(p, p, 4); memchr(p, 't', 4);
                return 0;
            }
            """;
        string path = Path.Combine(Path.GetTempPath(), $"dotcc-string-const-{Guid.NewGuid():N}.c");
        File.WriteAllText(path, source);
        var prior = Console.Error;
        using var capture = new StringWriter();
        try
        {
            Console.SetError(capture);
            Compiler.EmitCSharp([path]);
            capture.ToString().ShouldNotContain("discards 'const'");
        }
        finally { Console.SetError(prior); File.Delete(path); }
    }
}
