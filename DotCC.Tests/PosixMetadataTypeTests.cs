using System;
using System.IO;
using Xunit;

namespace DotCC.Tests;

public sealed class PosixMetadataTypeTests
{
    [Theory]
    [InlineData("sys/types.h")]
    [InlineData("sys/stat.h")]
    public void Metadata_header_exposes_Linux_LP64_types_without_include_order_dependencies(string header)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-metadata-types-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, "#include <" + header + ">\n" + """
            _Static_assert(sizeof(time_t) == 8, "time_t width");
            _Static_assert(sizeof(ino_t) == 8 && (ino_t)-1 > 0, "inode type");
            _Static_assert(sizeof(nlink_t) == 8 && (nlink_t)-1 > 0, "link count type");
            _Static_assert(sizeof(dev_t) == 8 && (dev_t)-1 > 0, "device type");
            _Static_assert(sizeof(blksize_t) == 8 && (blksize_t)-1 < 0, "block size type");
            _Static_assert(sizeof(blkcnt_t) == 8 && (blkcnt_t)-1 < 0, "block count type");
            #include <sys/types.h>
            #include <sys/stat.h>
            #include <time.h>
            void copy_metadata(struct stat *st, unsigned long inode, unsigned long links) {
                st->st_ino = (ino_t)inode;
                st->st_nlink = (nlink_t)links;
                st->st_mtime = (time_t)123;
            }
            int main(void) { return 0; }
            """);
        try { Compiler.EmitCSharp(new[] { path }); }
        finally { File.Delete(path); }
    }
}
