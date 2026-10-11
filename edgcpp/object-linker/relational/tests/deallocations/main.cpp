#include "program.h"
int main() {
    char *first = new char[next_count(5)];
    first[0] = 'a'; first[4] = 'z';
    if (first[0] != 'a' || first[4] != 'z' || allocation_calls != 1 || allocation_bytes != 5 || count_calls != 1) return 1;
    char *zero = new char[next_count(0)];
    if (!zero || allocation_calls != 2 || allocation_bytes != 0 || count_calls != 2) return 2;
    char *provider = allocate_bytes(7);
    provider[7] = 'x';
    if (provider[7] != 'x' || allocation_calls != 3 || allocation_bytes != 8) return 3;
    if (allocation_calls < 0 && new char[next_count(4)]) return 4;
    if (count_calls != 2 || allocation_calls != 3) return 5;
    try {
        char *bad = new char[next_count(-1)];
        return bad ? 6 : 7;
    } catch (int error) {
        if (error != 91 || count_calls != 3 || failure_calls != 1 || allocation_calls != 3) return 8;
    }
    try {
        char *failed = new char[next_count(13)];
        return failed ? 9 : 10;
    } catch (int error) {
        if (error != 47 || count_calls != 4 || allocation_calls != 4 || allocation_bytes != 13) return 11;
    }
    char *fixed = new char[4];
    fixed[3] = 'f';
    if (fixed[3] != 'f' || allocation_calls != 5 || allocation_bytes != 4) return 12;
    delete[] next_pointer(fixed);
    if (deallocation_calls != 1 || pointer_calls != 1) return 20;
    delete[] next_pointer(nullptr);
    if (deallocation_calls != 1 || pointer_calls != 2) return 21;
    if (allocation_calls < 0) delete[] next_pointer(first);
    if (deallocation_calls != 1 || pointer_calls != 2) return 22;
    release_bytes(provider);
    if (deallocation_calls != 2 || pointer_calls != 3) return 23;
    return 0;
}
