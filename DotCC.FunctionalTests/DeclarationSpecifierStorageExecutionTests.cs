using System;
using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class DeclarationSpecifierStorageExecutionTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Header_private_objects_keep_distinct_addresses_and_mutable_state(bool objects, bool reverse)
    {
        const string header = """
            static int counter;
            static int *early_counter(void) { return &counter; }
            static int counter = 10;
            _Alignas(128) static volatile int ordered;
            static _Alignas(128) volatile int ordered = 20;
            static const int table[2] = {3, 7};
            static int (*callback)(int);
            static int (*callbacks[2])(int);
            """;
        const string first = """
            #include "storage.h"
            static long collision = 31;
            static int block_only = 43;
            extern int shared;
            int *first_counter(void) { return &counter; }
            const int *first_table(void) { return table; }
            volatile int *first_ordered(void) { return &ordered; }
            long *first_collision(void) { return &collision; }
            int increment(int value) { return value + 1; }
            int first_next(void) {
                extern int counter;
                _Alignas(128) extern volatile int ordered;
                return ++counter + ++ordered;
            }
            int *second_counter(void);
            const int *second_table(void);
            volatile int *second_ordered(void);
            int *second_collision(void);
            int second_next(void);
            int second_callbacks_empty(void);
            int second_block_external(void);
            void *second_callback_address(void);
            void *second_callbacks_address(void);
            int main(void) {
                if (first_counter() == second_counter() || first_table() == second_table()) return 1;
                if (first_ordered() == second_ordered() || early_counter() != first_counter()) return 2;
                if (first_next() != 32 || second_next() != 32 || first_next() != 34) return 3;
                if (*second_counter() != 11 || *second_ordered() != 21) return 4;
                if (first_table()[1] != 7 || second_table()[0] != 3) return 5;
                if ((void *)first_collision() == (void *)second_collision()) return 6;
                if (*first_collision() != 31 || *second_collision() != 37 || shared != 42
                    || second_block_external() != 47 || block_only != 43) return 7;
                *second_collision() = 99;
                if (*first_collision() != 31 || *second_collision() != 99) return 8;
                if ((void *)&callback == second_callback_address() || (void *)callbacks == second_callbacks_address()) return 9;
                callback = increment;
                callbacks[0] = callback;
                if (callback(5) != 6 || callbacks[0](7) != 8 || !second_callbacks_empty()) return 10;
                return 0;
            }
            """;
        const string second = """
            #include "storage.h"
            int collision = 37;
            int shared = 42;
            int second_block_external(void) { extern long block_only; return (int)block_only; }
            long block_only = 47;
            int *second_counter(void) { return early_counter(); }
            const int *second_table(void) { return table; }
            volatile int *second_ordered(void) { return &ordered; }
            int *second_collision(void) { return &collision; }
            int second_next(void) { return ++counter + ++ordered; }
            int second_callbacks_empty(void) { return callback == 0 && callbacks[0] == 0; }
            void *second_callback_address(void) { return &callback; }
            void *second_callbacks_address(void) { return callbacks; }
            """;
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-storage-header-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            File.WriteAllText(Path.Combine(directory, "storage.h"), header);
            var sources = new[] { Path.Combine(directory, "first.c"), Path.Combine(directory, "second.c") };
            File.WriteAllText(sources[0], first);
            File.WriteAllText(sources[1], second);
            if (reverse) Array.Reverse(sources);
            var emitted = objects ? Compiler.LinkObjects(sources.Select(path =>
            {
                var fragment = Path.ChangeExtension(path, ".cs");
                File.WriteAllText(fragment, Compiler.EmitObject(path));
                return fragment;
            }).ToArray()) : Compiler.EmitCSharp(sources);
            FixtureRunner.CompileAndRunCapturingExit(emitted, []).exit.ShouldBe(0);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Ordered_alignment_preserves_addresses_linkage_static_lifetime_and_values(bool objects)
    {
        // GCC C11 oracle: object alignment is 128, while Slice's natural
        // alignment/size remain 8/16 and each int array still strides by four.
        const string first = """
            #include <stddef.h>
            #include <stdint.h>
            #include <stdio.h>
            struct Slice { unsigned short const *ptr; unsigned long len; };
            struct Pair { int lead; int value; };
            static unsigned short const data[3] = {5, 7, 11};
            _Alignas(128) static struct Slice const table;
            void const *early(void) { return &table; }
            static _Alignas(128) struct Slice const table = {data, 3};
            _Alignas(128) static int rows[2];
            static _Alignas(128) int rows[2] = {19, 23};
            _Alignas(128) extern volatile int shared;
            static __attribute__((aligned(128))) int gnu = 29;
            int other_value(void);
            int bump(void) {
                _Alignas(128) static volatile int counter = 40;
                _Alignas(128) static int local_rows[2] = {3, 5};
                _Alignas(128) static struct Pair local = {2, 9};
                _Alignas(128) extern int volatile shared;
                if (((uintptr_t)&counter & 127) || ((uintptr_t)local_rows & 127) || ((uintptr_t)&local & 127)) return -100;
                return ++counter + ++local_rows[0] + ++local.value + shared;
            }
            int main(void) {
                _Alignas(128) auto int automatic = 13;
                int first = bump();
                int second = bump();
                printf("layout %lu %lu %lu %lu %lu\n", sizeof(struct Slice), _Alignof(struct Slice), offsetof(struct Slice, len), sizeof(rows), sizeof(rows[0]));
                printf("alignment %d %d %d %d %d\n", (int)((uintptr_t)&table & 127), (int)((uintptr_t)rows & 127), (int)((uintptr_t)&shared & 127), (int)((uintptr_t)&gnu & 127), (int)((uintptr_t)&automatic & 127));
                printf("values %u %lu %d %d %d %d %d %d\n", (unsigned int)table.ptr[1], table.len, rows[0], other_value(), first, second, early() == &table, automatic + gnu);
                return first != 72 || second != 75 || early() != &table;
            }
            """;
        const string second = """
            #include <stdint.h>
            struct Slice { long value; };
            _Alignas(128) static struct Slice const table;
            static _Alignas(128) struct Slice const table = {91};
            _Alignas(128) volatile int shared = 17;
            int other_value(void) { return ((uintptr_t)&table & 127) ? -100 : (int)table.value; }
            """;
        const string expected = """
            layout 16 8 8 8 4
            alignment 0 0 0 0 0
            values 7 3 19 91 72 75 1 42
            """ + "\n";
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-declaration-storage-run-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var sources = new[] { Path.Combine(directory, "first.c"), Path.Combine(directory, "second.c") };
            File.WriteAllText(sources[0], first);
            File.WriteAllText(sources[1], second);
            string emitted;
            if (objects)
            {
                var fragments = sources.Select(path =>
                {
                    var fragment = Path.ChangeExtension(path, ".cs");
                    File.WriteAllText(fragment, Compiler.EmitObject(path));
                    return fragment;
                }).ToArray();
                emitted = Compiler.LinkObjects(fragments);
            }
            else emitted = Compiler.EmitCSharp(sources);
            var result = FixtureRunner.CompileAndRunCapturingExit(emitted, Array.Empty<string>());
            result.exit.ShouldBe(0);
            result.stdout.ReplaceLineEndings("\n").ShouldBe(expected);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Reordered_tls_callbacks_keep_per_thread_storage_addresses_and_assignments(bool objects)
    {
        const string source = """
            static int increment(int value) { return value + 1; }
            static int decrement(int value) { return value - 1; }
            _Thread_local static int (*file_callback)(int);
            _Thread_local static int (*file_callbacks[2])(int);
            long probe(int operation, int choice, int value) {
                _Thread_local static int (*local_callback)(int);
                _Thread_local static int (*local_callbacks[2])(int);
                if (operation == 0) return (long)&file_callback;
                if (operation == 1) return (long)file_callbacks;
                if (operation == 2) return (long)&local_callback;
                if (operation == 3) return (long)local_callbacks;
                if (operation == 4) return file_callback == 0 && file_callbacks[0] == 0
                    && local_callback == 0 && local_callbacks[0] == 0;
                if (operation == 5) {
                    file_callback = choice ? increment : decrement;
                    file_callbacks[0] = file_callback;
                    local_callback = file_callback;
                    local_callbacks[0] = local_callback;
                    return 0;
                }
                return file_callback(value) + file_callbacks[0](value)
                    + local_callback(value) + local_callbacks[0](value);
            }
            """;
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-storage-callback-tls-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "callbacks.c");
            File.WriteAllText(path, source);
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "callbacks.cs");
                File.WriteAllText(fragment, Compiler.EmitObject(path));
                emitted = Compiler.LinkObjects([fragment], emit: EmitMode.ManagedLib, className: "StorageTlsApi");
            }
            else emitted = Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib, className: "StorageTlsApi");
            emitted += """

                public static class StorageTlsProbe {
                    private static long[] Addresses() {
                        var result = new long[4];
                        for (int i = 0; i < result.Length; ++i) result[i] = StorageTlsApi.probe(i, 0, 0);
                        return result;
                    }
                    private static bool Aligned(long[] addresses) {
                        for (int i = 0; i < addresses.Length; ++i) {
                            if (addresses[i] == 0 || (addresses[i] & 7) != 0) return false;
                            for (int j = 0; j < i; ++j) if (addresses[i] == addresses[j]) return false;
                        }
                        return true;
                    }
                    public static int Main() {
                        if (StorageTlsApi.probe(4, 0, 0) != 1) return 1;
                        var main = Addresses();
                        if (!Aligned(main)) return 2;
                        StorageTlsApi.probe(5, 1, 0);
                        var addresses = new long[2][];
                        var errors = new int[2];
                        using var barrier = new System.Threading.Barrier(2);
                        var threads = new System.Threading.Thread[2];
                        for (int i = 0; i < threads.Length; ++i) {
                            int slot = i;
                            threads[i] = new System.Threading.Thread(() => {
                                try {
                                    if (StorageTlsApi.probe(4, 0, 0) != 1) errors[slot] = 3;
                                    addresses[slot] = Addresses();
                                    if (!Aligned(addresses[slot])) errors[slot] = 4;
                                    StorageTlsApi.probe(5, slot, 0);
                                    if (!barrier.SignalAndWait(System.TimeSpan.FromSeconds(10))) { errors[slot] = 5; return; }
                                    System.GC.Collect(2, System.GCCollectionMode.Forced, true, true);
                                    var after = Addresses();
                                    for (int j = 0; j < after.Length; ++j)
                                        if (after[j] != addresses[slot][j]) errors[slot] = 6;
                                    if (StorageTlsApi.probe(6, 0, 20 + slot) != 4 * (20 + slot) + (slot == 0 ? -4 : 4)) errors[slot] = 7;
                                    if (!barrier.SignalAndWait(System.TimeSpan.FromSeconds(10))) errors[slot] = 8;
                                } catch { errors[slot] = 9; }
                            }) { IsBackground = true };
                            threads[i].Start();
                        }
                        foreach (var thread in threads) if (!thread.Join(15000)) return 10;
                        if (errors[0] != 0 || errors[1] != 0) return 11;
                        for (int slot = 0; slot < 2; ++slot)
                            foreach (long address in addresses[slot]) {
                                foreach (long other in main) if (address == other) return 12;
                                foreach (long other in addresses[1 - slot]) if (address == other) return 13;
                            }
                        var final = Addresses();
                        for (int i = 0; i < final.Length; ++i) if (final[i] != main[i]) return 14;
                        return StorageTlsApi.probe(6, 0, 10) == 44 ? 0 : 15;
                    }
                }
                """;
            FixtureRunner.CompileAndRunCapturingExit(emitted, []).exit.ShouldBe(0);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
