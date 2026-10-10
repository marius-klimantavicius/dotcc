#include <cstdio>
#include <cstdlib>
#include <initializer_list>
extern "C" unsigned long long array_cleanup_case(int, int);
extern "C" unsigned long long vtt_cleanup_case(int, int);
static int frees;
extern "C" void dotobj_test_free(void *pointer) { ++frees; std::free(pointer); }
extern "C" void array_delete_adapter_probe(void *pointer);
int main() {
    for (int scenario = 0; scenario != 5; ++scenario)
        for (int fail : {0, 1, 2, 3, 4, 6, 7, 9, 10, 11, 12})
            std::printf("a %d %d %llu\n", scenario, fail, array_cleanup_case(scenario, fail));
    for (int scenario = 0; scenario != 3; ++scenario)
        for (int fail : {0, 1, 2, 5, 8, 10, 12, 13, 14})
            std::printf("v %d %d %llu\n", scenario, fail, vtt_cleanup_case(scenario, fail));
    array_delete_adapter_probe(std::malloc(13));
    if (frees != 1) return 1;
    std::printf("d %d\n", frees);
}
