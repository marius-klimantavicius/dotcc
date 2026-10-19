using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class CPythonR5HostServicesTests
{
    private static string Text(byte* p) => Marshal.PtrToStringUTF8((nint)p)!;

    [Fact]
    public void Ieee_values_preserve_signed_zero_nan_and_fused_rounding()
    {
        signbit(copysign(0.0, -1)).ShouldBe(1);
        signbit(nextafter(0.0, -0.0)).ShouldBe(1);
        nextafter(double.PositiveInfinity, 0).ShouldBe(double.MaxValue);
        nextafter(0, 1).ShouldBe(double.Epsilon);
        errno = EIO;
        nextafter(1, 2).ShouldBe(Math.BitIncrement(1));
        errno.ShouldBe(EIO);
        nextafter(double.MaxValue, double.PositiveInfinity).ShouldBe(double.PositiveInfinity);
        errno.ShouldBe(ERANGE);
        fma(Math.BitIncrement(1), 1 - Math.ScaleB(1, -52), -1).ShouldBe(-Math.ScaleB(1, -104));
        fma(double.MaxValue, 2, -double.MaxValue).ShouldBe(double.MaxValue);
        double.IsNaN(fma(double.PositiveInfinity, 0, 1)).ShouldBeTrue();
        errno.ShouldBe(EDOM);
        exp2(-1074).ShouldBe(double.Epsilon);
        exp2(1024).ShouldBe(double.PositiveInfinity); errno.ShouldBe(ERANGE);
    }

    [Fact]
    public void Encoding_is_owner_scoped_and_length_bearing_without_native_wchar_calls()
    {
        using var first = new RuntimeContext();
        using var second = new RuntimeContext();
        byte* utf8 = stackalloc byte[] { 65, 0xce, 0xbb, 0xf0, 0x9f, 0x90, 0x8d, 0, 0xff };
        char* wide = stackalloc char[8];
        byte* bytes = stackalloc byte[12];
        using (first.Enter())
        {
            fixed (byte* locale = "C.UTF-8\0"u8) Text(setlocale(0, locale)).ShouldBe("C.UTF-8");
            Text(nl_langinfo(14)).ShouldBe("UTF-8");
            mbstowcs(null, utf8, 0).ShouldBe(4UL);
            wide[2] = 'x';
            mbstowcs(wide, utf8, 3).ShouldBe(2UL); wide[2].ShouldBe('x');
            mbstowcs(wide, utf8, 4).ShouldBe(4UL);
            wide[2].ShouldBe('\ud83d'); wide[3].ShouldBe('\udc0d');
            wide[4] = '\0';
            wcstombs(null, wide, 0).ShouldBe(7UL);
            bytes[1] = 0x77;
            wcstombs(bytes, wide, 2).ShouldBe(1UL); bytes[1].ShouldBe((byte)0x77);
            wcstombs(bytes, wide, 12).ShouldBe(7UL); bytes[7].ShouldBe((byte)0);
            using (second.Enter())
            {
                Text(nl_langinfo(14)).ShouldBe("ANSI_X3.4-1968");
                mbstowcs(wide, utf8, 8).ShouldBe(ulong.MaxValue); errno.ShouldBe(EILSEQ);
            }
            Text(nl_langinfo(14)).ShouldBe("UTF-8");
        }
    }

    [Fact]
    public void Langinfo_keeps_c_conventions_and_stable_borrowed_storage_when_ctype_changes()
    {
        using var owner = new RuntimeContext();
        using var entry = owner.Enter();
        byte* sunday = nl_langinfo(131072);
        Text(sunday).ShouldBe("Sun");
        Text(nl_langinfo(131109)).ShouldBe("December");
        Text(nl_langinfo(65536)).ShouldBe(".");
        Text(nl_langinfo(262159)).ShouldBe("-");
        Text(nl_langinfo(131112)).ShouldBe("%a %b %e %H:%M:%S %Y");
        fixed (byte* locale = "C.UTF-8\0"u8) setlocale(0, locale);
        ((nint)nl_langinfo(131072)).ShouldBe((nint)sunday);
        Text(nl_langinfo(14)).ShouldBe("UTF-8");
        errno = EIO;
        Text(nl_langinfo(-123)).ShouldBe(""); errno.ShouldBe(ENOTSUP);
    }

    [Theory]
    [InlineData("c080")]
    [InlineData("eda080")]
    [InlineData("f4908080")]
    [InlineData("e282")]
    [InlineData("80")]
    public void Invalid_multibyte_sequences_fail_instead_of_replacing(string hex)
    {
        using var context = new RuntimeContext();
        using var entry = context.Enter();
        fixed (byte* locale = "C.UTF-8\0"u8) setlocale(0, locale);
        byte[] input = new byte[hex.Length / 2 + 1];
        Convert.FromHexString(hex).CopyTo(input, 0);
        char* output = stackalloc char[8];
        fixed (byte* source = input)
        {
            errno = EIO;
            mbstowcs(output, source, 0).ShouldBe(0UL); errno.ShouldBe(EIO);
            mbstowcs(output, source, 8).ShouldBe(ulong.MaxValue); errno.ShouldBe(EILSEQ);
        }
    }

    [Theory]
    [InlineData('\ud800')]
    [InlineData('\udfff')]
    public void Lone_surrogates_reject_without_overwriting_the_next_byte(char invalid)
    {
        using var context = new RuntimeContext();
        using var entry = context.Enter();
        fixed (byte* locale = "C.UTF-8\0"u8) setlocale(0, locale);
        char* input = stackalloc char[] { 'a', invalid, '\0' };
        byte* output = stackalloc byte[] { 0x66, 0x77, 0x88 };
        wcstombs(output, input, 3).ShouldBe(ulong.MaxValue); errno.ShouldBe(EILSEQ);
        output[0].ShouldBe((byte)'a'); output[1].ShouldBe((byte)0x77);
    }

    [Fact]
    public void Full_line_and_unbuffered_output_reach_the_file_at_the_correct_boundary()
    {
        using var context = new RuntimeContext();
        using var entry = context.Enter();
        string path = Path.Combine(Path.GetTempPath(), "dotcc-r5-" + Guid.NewGuid());
        byte[] name = Encoding.UTF8.GetBytes(path + "\0");
        fixed (byte* p = name)
        fixed (byte* mode = "w+\0"u8)
        {
            FILE* file = fopen(p, mode);
            ((nint)file).ShouldNotBe(0);
            byte* buffer = stackalloc byte[8192];
            setbuf(file, buffer);
            fputc('a', file).ShouldBe('a');
            File.ReadAllText(path).ShouldBe("");
            ftell(file).ShouldBe(1);
            fflush(file).ShouldBe(0); File.ReadAllText(path).ShouldBe("a");
            setvbuf(file, buffer, 1, 8).ShouldBe(0);
            fputc('b', file); File.ReadAllText(path).ShouldBe("a");
            fputc('\n', file); File.ReadAllText(path).ShouldBe("ab\n");
            setbuf(file, null);
            fputc('c', file); File.ReadAllText(path).ShouldBe("ab\nc");
            int fd = fileno(file);
            fclose(file).ShouldBe(0);
            byte value;
            read(fd, &value, 1).ShouldBe(-1); errno.ShouldBe(EBADF);
            // Descriptor reuse must never resurrect an old FILE* after close(fd).
            int reopened = open(p, 2);
            FILE* adopted = fdopen(reopened, mode);
            close(reopened).ShouldBe(0);
            int again = open(p, 2);
            fputc('x', adopted).ShouldBe(-1);
            close(again).ShouldBe(0);
        }
        File.Delete(path);
    }

    [Fact]
    public void Buffered_close_reports_full_device_error_and_still_releases_the_descriptor()
    {
        using var context = new RuntimeContext();
        using var entry = context.Enter();
        fixed (byte* path = "/dev/full\0"u8)
        fixed (byte* mode = "w\0"u8)
        {
            FILE* file = fopen(path, mode);
            ((nint)file).ShouldNotBe(0);
            setvbuf(file, null, 0, 8).ShouldBe(0);
            fputc('x', file).ShouldBe('x');
            int descriptor = fileno(file);
            fclose(file).ShouldBe(-1); errno.ShouldBe(ENOSPC);
            close(descriptor).ShouldBe(-1); errno.ShouldBe(EBADF);
        }
    }

    [Fact]
    public void Unbuffered_formatted_console_output_keeps_utf8_across_individual_bytes()
    {
        using var context = new RuntimeContext();
        using var entry = context.Enter();
        var previous = Console.Out;
        using var capture = new StringWriter();
        Console.SetOut(capture);
        try
        {
            setbuf(stdout, null);
            fixed (byte* text = "λ🐍\0"u8) fputs(text, stdout).ShouldBeGreaterThanOrEqualTo(0);
            capture.ToString().ShouldBe("λ🐍");
        }
        finally { Console.SetOut(previous); }
    }

    [Fact]
    public void Real_mapping_partial_unmap_foreign_owner_and_disposal_are_distinct()
    {
        if (!OperatingSystem.IsLinux() || IntPtr.Size != 8) return;
        using var first = new RuntimeContext();
        using var second = new RuntimeContext();
        ulong page = (ulong)Environment.SystemPageSize;
        string path = Path.GetTempFileName();
        try
        {
            using (var backing = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
                backing.SetLength((long)page);
            using (first.Enter())
            {
                byte* memory = (byte*)mmap(null, page * 3, 3, 0x22, -1, 0);
                ((nint)memory).ShouldNotBe(-1); memory[0].ShouldBe((byte)0);
                memory[0] = 42; memory[page * 2] = 21;
                using (second.Enter()) { munmap(memory, page).ShouldBe(-1); errno.ShouldBe(EINVAL); }
                munmap(memory + page, page).ShouldBe(0);
                memory[0].ShouldBe((byte)42); memory[page * 2].ShouldBe((byte)21);
                munmap(memory, page).ShouldBe(0);
                munmap(memory + page * 2, page).ShouldBe(0);
                // Identify the retained storage in the kernel, not just its address:
                // JIT/PInvoke work may reuse an unmapped address before mincore.
                int descriptor;
                fixed (byte* name = Encoding.UTF8.GetBytes(path + "\0"))
                    descriptor = open(name, 2, 0);
                descriptor.ShouldBeGreaterThanOrEqualTo(0);
                byte* retained = (byte*)mmap(null, page, 3, 1, descriptor, 0);
                ((nint)retained).ShouldNotBe(-1);
                close(descriptor).ShouldBe(0);
                retained[0] = 73;
                byte vector;
                NativeMincore(retained, page, &vector).ShouldBe(0);
                File.ReadAllLines("/proc/self/maps").ShouldContain(line => line.EndsWith(" " + path, StringComparison.Ordinal));
            }
            first.Dispose();
            File.ReadAllLines("/proc/self/maps").ShouldNotContain(line => line.EndsWith(" " + path, StringComparison.Ordinal));
        }
        finally { File.Delete(path); }
    }

    [DllImport("libc", EntryPoint = "mincore", SetLastError = true)]
    private static extern int NativeMincore(void* address, ulong length, byte* vector);

    [Fact]
    public void Passwd_records_and_errno_are_separate_on_two_actual_workers()
    {
        if (!OperatingSystem.IsLinux() || IntPtr.Size != 8) return;
        using var context = new RuntimeContext();
        using var barrier = new Barrier(2);
        nint[] addresses = new nint[2];
        Exception?[] failures = new Exception?[2];
        Thread Worker(int index) => new(() =>
        {
            try
            {
                using var entry = context.Enter();
                errno = EIO + index;
                void* record = getpwuid(getuid());
                ((nint)record).ShouldNotBe(0); addresses[index] = (nint)record;
                string name = Text(*(byte**)record);
                barrier.SignalAndWait(TimeSpan.FromSeconds(5)).ShouldBeTrue();
                Text(*(byte**)record).ShouldBe(name); errno.ShouldBe(EIO + index);
                // Aliasing the previous borrowed string as a lookup key is valid.
                void* byName = getpwnam(*(byte**)record);
                Text(*(byte**)byName).ShouldBe(name);
                fixed (byte* missing = "dotcc-r5-no-such-user\0"u8) ((nint)getpwnam(missing)).ShouldBe(0);
                // NSS backends may report ENOENT directly, or return success
                // with a NULL result. The shared wrapper preserves that real
                // distinction rather than inventing a missing account.
                (errno == EIO + index || errno == ENOENT).ShouldBeTrue();
            }
            catch (Exception failure) { failures[index] = failure; }
        });
        var one = Worker(0); var two = Worker(1);
        one.Start(); two.Start();
        one.Join(10000).ShouldBeTrue(); two.Join(10000).ShouldBeTrue();
        failures[0].ShouldBeNull(); failures[1].ShouldBeNull(); addresses[0].ShouldNotBe(addresses[1]);
    }
}
