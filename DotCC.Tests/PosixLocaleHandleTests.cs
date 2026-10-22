using System;
using System.Text;
using System.Threading;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class PosixLocaleHandleTests
{
    private static void* Create(string name, int mask = 0x1fbf, void* basis = null)
    {
        fixed (byte* text = Encoding.UTF8.GetBytes(name + "\0")) return newlocale(mask, text, basis);
    }
    private static void Global(string name)
    {
        fixed (byte* text = Encoding.UTF8.GetBytes(name + "\0")) Assert.True(setlocale(6, text) != null);
    }
    private static string Text(byte* value) => value == null ? "<null>" : Encoding.UTF8.GetString(value, strlen(value));
    private static void* GlobalLocale => (void*)(nint)(-1);

    [Fact]
    public void Handles_are_distinct_stable_and_freed_tokens_never_alias()
    {
        using var owner = new RuntimeContext(); using var binding = owner.Enter();
        void* a = Create("C"), b = Create("POSIX");
        Assert.True(a != null && b != null && a != b);
        Assert.Equal("ANSI_X3.4-1968", Text(nl_langinfo_l(14, a)));
        freelocale(a);
        Assert.True(duplocale(a) == null); Assert.Equal(EINVAL, errno);
        Assert.True(nl_langinfo_l(14, a) == null); Assert.Equal(EINVAL, errno);
        freelocale(a); Assert.Equal(EINVAL, errno);
        for (int i = 0; i < 1000; i++)
        {
            void* next = Create("C"); Assert.True(next != a && next != b); freelocale(next);
        }
        Assert.Equal('a', tolower_l('A', b)); freelocale(b);
    }

    [Fact]
    public void Foreign_handles_cannot_be_used_modified_or_freed()
    {
        using var a = new RuntimeContext(); using var b = new RuntimeContext();
        using var boundA = a.Enter(); void* handle = Create("C.UTF-8"); Assert.True(handle != null);
        Assert.Equal((nint)GlobalLocale, (nint)uselocale(handle));
        using (b.Enter())
        {
            Assert.True(uselocale(handle) == null); Assert.Equal(EINVAL, errno);
            Assert.True(duplocale(handle) == null); Assert.Equal(EINVAL, errno);
            Assert.True(Create("C", 1, handle) == null); Assert.Equal(EINVAL, errno);
            freelocale(handle); Assert.Equal(EINVAL, errno);
            Assert.Equal((nint)GlobalLocale, (nint)uselocale(null));
        }
        Assert.Equal((nint)handle, (nint)uselocale(null));
        Assert.Equal("UTF-8", Text(nl_langinfo(14)));
        uselocale(GlobalLocale); freelocale(handle);
    }

    [Fact]
    public void Invalid_names_masks_and_bases_fail_without_modifying_the_base()
    {
        using var owner = new RuntimeContext(); using var binding = owner.Enter();
        void* handle = Create("C.UTF-8"); Assert.True(handle != null);
        Assert.True(Create("not-an-installed-locale", 1, handle) == null); Assert.Equal(ENOENT, errno);
        Assert.Equal("UTF-8", Text(nl_langinfo_l(14, handle)));
        Assert.True(Create("C", 1 << 6, handle) == null); Assert.Equal(EINVAL, errno);
        Assert.True(newlocale(1, null, handle) == null); Assert.Equal(EINVAL, errno);
        Assert.True(Create("C", 1, GlobalLocale) == null); Assert.Equal(EINVAL, errno);
        Assert.Equal((nint)handle, (nint)Create("C", 2, handle));
        Assert.Equal("UTF-8", Text(nl_langinfo_l(14, handle)));
        Assert.Equal((nint)handle, (nint)Create("C", 1, handle));
        Assert.Equal("ANSI_X3.4-1968", Text(nl_langinfo_l(14, handle))); freelocale(handle);
    }

    [Fact]
    public void Duplicates_snapshot_global_and_explicit_state_independently()
    {
        using var owner = new RuntimeContext(); using var binding = owner.Enter(); Global("C");
        void* utf8 = Create("C.utf8"); Assert.True(utf8 != null);
        void* copy = duplocale(utf8); Assert.True(copy != null && copy != utf8);
        uselocale(utf8);
        void* globalCopy = duplocale(GlobalLocale);
        Assert.Equal("ANSI_X3.4-1968", Text(nl_langinfo_l(14, globalCopy)));
        Assert.Equal("C", Text(setlocale(0, null)));
        Global("C.UTF-8"); Assert.Equal("C.UTF-8", Text(setlocale(0, null)));
        Assert.Equal("ANSI_X3.4-1968", Text(nl_langinfo_l(14, globalCopy)));
        uselocale(GlobalLocale); freelocale(utf8);
        Assert.Equal("UTF-8", Text(nl_langinfo_l(14, copy)));
        freelocale(copy); freelocale(globalCopy);
    }

    [Fact]
    public void Selected_locale_cannot_be_freed_or_mutated_and_previous_selection_restores()
    {
        using var owner = new RuntimeContext(); using var binding = owner.Enter();
        void* a = Create("C"), b = Create("C.UTF-8"); Assert.True(a != null && b != null);
        Assert.Equal((nint)GlobalLocale, (nint)uselocale(a));
        errno = 0; freelocale(a); Assert.Equal(EBUSY, errno);
        Assert.True(Create("C.UTF-8", 1, a) == null); Assert.Equal(EBUSY, errno);
        Assert.Equal((nint)a, (nint)uselocale(b));
        Assert.Equal((nint)b, (nint)uselocale(a));
        Assert.Equal("ANSI_X3.4-1968", Text(nl_langinfo(14)));
        uselocale(GlobalLocale); freelocale(a); freelocale(b);
    }

    [Fact]
    public void Selection_is_per_thread_and_live_worker_prevents_free()
    {
        using var owner = new RuntimeContext(); using var binding = owner.Enter(); Global("C");
        nint locale = (nint)Create("C.UTF-8"); Assert.NotEqual(0, locale);
        using var ready = new ManualResetEventSlim(); using var release = new ManualResetEventSlim();
        Exception? error = null;
        var worker = new Thread(() =>
        {
            try
            {
                using var entered = owner.Enter(); Assert.Equal((nint)GlobalLocale, (nint)uselocale((void*)locale));
                ready.Set(); Assert.True(release.Wait(TimeSpan.FromSeconds(15)));
                Assert.Equal("UTF-8", Text(nl_langinfo(14)));
                // A completed physical thread no longer holds a live selection.
            }
            catch (Exception exception) { error = exception; ready.Set(); }
        });
        worker.Start();
        try
        {
            Assert.True(ready.Wait(TimeSpan.FromSeconds(15)));
            Assert.Equal("ANSI_X3.4-1968", Text(nl_langinfo(14)));
            freelocale((void*)locale); Assert.Equal(EBUSY, errno);
        }
        finally { release.Set(); Assert.True(worker.Join(TimeSpan.FromSeconds(15))); }
        Assert.Null(error); errno = 0; freelocale((void*)locale); Assert.Equal(0, errno);
    }

    [Fact]
    public void Explicit_locale_changes_multibyte_conversion_without_changing_global()
    {
        using var owner = new RuntimeContext(); using var binding = owner.Enter(); Global("C");
        void* utf8 = Create("C.UTF-8"); Assert.True(utf8 != null);
        fixed (byte* source = "\u00e9\U0001f680\0"u8)
        {
            char* output = stackalloc char[5]; output[4] = 'X';
            Assert.Equal(ulong.MaxValue, mbstowcs(output, source, 4)); Assert.Equal(EILSEQ, errno);
            void* previous = uselocale(utf8);
            Assert.Equal(3UL, mbstowcs(output, source, 4));
            Assert.Equal('\u00e9', output[0]); Assert.Equal('\ud83d', output[1]); Assert.Equal('\ude80', output[2]);
            Assert.Equal('\0', output[3]); Assert.Equal('X', output[4]);
            byte* bytes = stackalloc byte[8]; Assert.Equal(6UL, wcstombs(bytes, output, 8));
            Assert.Equal("\u00e9\U0001f680", Text(bytes));
            Assert.Equal("C", Text(setlocale(0, null))); uselocale(previous);
        }
        freelocale(utf8);
    }

    [Fact]
    public void Wide_unicode_scalars_use_real_ctype_including_supplementary_case_pairs()
    {
        using var owner = new RuntimeContext(); using var binding = owner.Enter();
        void* c = Create("C"), utf8 = Create("C.UTF-8"); Assert.True(utf8 != null);
        Assert.Equal(0, iswalpha_l(0x10400, c)); Assert.Equal(0x10400, towlower_l(0x10400, c));
        Assert.NotEqual(0, iswalpha_l(0x10400, utf8)); Assert.Equal(0x10428, towlower_l(0x10400, utf8));
        Assert.Equal(0x10400, towupper_l(0x10428, utf8));
        Assert.Equal(0x69, towlower_l(0x130, utf8)); // simple C.UTF-8 mapping, not invariant Rune casing
        Assert.Equal(0, isalpha_l(0xe9, utf8)); Assert.NotEqual(0, iswalpha_l(0xe9, utf8));
        Assert.Equal(0, iswalpha_l(-1, utf8)); Assert.Equal(-1, towupper_l(-1, utf8));
        Assert.Equal(0, iswalpha_l(0xd800, utf8)); Assert.Equal(0, iswalpha_l(0x110000, utf8));
        freelocale(c); freelocale(utf8);
    }

    [Fact]
    public void C_collation_transform_bounds_langinfo_and_time_are_real_operations()
    {
        using var owner = new RuntimeContext(); using var binding = owner.Enter();
        void* locale = Create("C.UTF-8"); Assert.True(locale != null);
        fixed (byte* a = "a\0"u8) fixed (byte* b = "b\0"u8) fixed (byte* source = "abc\0"u8)
        {
            Assert.True(strcoll_l(a, b, locale) < 0); Assert.Equal(3UL, strxfrm_l(null, source, 0, locale));
            byte* output = stackalloc byte[5]; for (int i = 0; i < 5; i++) output[i] = 42;
            Assert.Equal(3UL, strxfrm_l(output, source, 2, locale)); Assert.Equal(42, output[2]);
            Assert.Equal(3UL, strxfrm_l(output, source, 4, locale)); Assert.Equal("abc", Text(output)); Assert.Equal(42, output[4]);
            Assert.Equal(ulong.MaxValue, strxfrm_l(null, source, 1, locale)); Assert.Equal(EFAULT, errno);
        }
        Assert.Equal(".", Text(nl_langinfo_l(65536, locale))); Assert.Equal("Sunday", Text(nl_langinfo_l(131079, locale)));
        Assert.Equal((byte)255, *nl_langinfo_l(262151, locale));
        Assert.Equal("", Text(nl_langinfo_l(int.MaxValue, locale))); Assert.Equal(ENOTSUP, errno);
        byte* buffer = stackalloc byte[80]; tm time = new() { tm_year = 124, tm_mon = 0, tm_mday = 7, tm_wday = 0 };
        fixed (byte* format = "%A %B %Y\0"u8)
            Assert.Equal(19UL, strftime_l(buffer, 80, format, &time, locale));
        Assert.Equal("Sunday January 2024", Text(buffer));
        fixed (byte* unsupported = "%c\0"u8)
        {
            buffer[0] = 42;
            Assert.Equal(0UL, strftime_l(buffer, 80, unsupported, &time, locale));
            Assert.Equal(ENOTSUP, errno); Assert.Equal((byte)42, buffer[0]);
        }
        freelocale(locale);
    }

    [Fact]
    public void Owner_disposal_clears_tables_and_old_handles_fail_in_a_new_owner()
    {
        var owner = new RuntimeContext(); nint old;
        using (owner.Enter()) { old = (nint)Create("C"); uselocale((void*)old); }
        Assert.Single(owner.PosixLocales); owner.Dispose(); Assert.Empty(owner.PosixLocales); Assert.Empty(owner.PosixLocaleThreads);
        using var next = new RuntimeContext(); using var entered = next.Enter();
        Assert.True(uselocale((void*)old) == null); Assert.Equal(EINVAL, errno);
        Assert.Throws<ObjectDisposedException>(() => owner.Enter());
    }
}
