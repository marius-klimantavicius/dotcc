using Xunit;

namespace DotCC.FunctionalTests;

public sealed partial class ManagedLibraryTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    public void Eager_global_addresses_preserve_forward_self_and_special_storage_relocations(bool objects, bool nested, bool reverse)
    {
        const string declarations = """
            struct Node { int value; struct Node *next; int *slot; };
            struct Flex { struct Node *node; struct Node **roots; int data[]; };
            extern int zero;
            extern struct Node first, second, aligned;
            extern struct Flex flex;
            """;
        RunGlobalStorageCase(objects, nested, reverse, "Api", declarations + """
            int zero;
            struct Node first = {7, &second, &zero};
            struct Node second = {11, &first, &zero};
            _Alignas(32) struct Node aligned = {13, &aligned, &zero};
            struct Flex flex = {&first, (struct Node *[]){&aligned, &first, &aligned}, {17, 19}};
            struct Node **roots = (struct Node *[]){&first, &second, &first};
            """, declarations + """
            extern struct Node **roots;
            _Thread_local int per_thread;
            int __dotcc_globalAddress_first(void) { return 23; }
            int *thread_address(void) { return &per_thread; }
            int check(void) {
                if (first.next != &second || second.next != &first) return 1;
                if (first.slot != &zero || second.slot != &zero) return 2;
                if (aligned.next != &aligned || aligned.slot != &zero) return 3;
                if (flex.node != &first || flex.data[0] != 17 || flex.data[1] != 19) return 4;
                if (roots[0] != &first || roots[1] != &second || roots[2] != &first) return 5;
                if (__dotcc_globalAddress_first() != 23) return 6;
                if (first.value != 7 || second.value != 11 || aligned.value != 13) return 7;
                if (flex.roots[0] != &aligned || flex.roots[1] != &first || flex.roots[2] != &aligned) return 8;
                return 0;
            }
            """, """
            if (Review.Api.check() != 0) return 1;
            int* original = Review.Api.thread_address();
            *original = 31;
            int error = 0;
            var worker = new System.Threading.Thread(() => {
                if (Review.Api.thread_address() == original || *Review.Api.thread_address() != 0) error = 2;
                if (Review.Api.check() != 0) error = 3;
            });
            worker.Start();
            if (!worker.Join(10000)) return 4;
            System.GC.Collect(2, System.GCCollectionMode.Forced, true, true);
            if (error != 0 || Review.Api.thread_address() != original || *original != 31) return 5;
            return Review.Api.check();
            """);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Eager_global_addresses_initialize_even_without_C_initializers(bool objects, bool aligned)
    {
        RunGlobalStorageCase(objects, nested: false, reverse: false, "Api",
            (aligned ? "_Alignas(32) " : "") + "int value;", """
            extern int value;
            int *address(void) {
                int *first = &value;
                return first == &value ? first : 0;
            }
            """, """
            int* value = Review.Api.address();
            if (value == null || *value != 0) return 1;
            *value = 37;
            System.GC.Collect(2, System.GCCollectionMode.Forced, true, true);
            return Review.Api.address() == value && *Review.Api.address() == 37 ? 0 : 2;
            """);
    }
}
