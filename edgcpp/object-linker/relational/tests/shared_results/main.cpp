#include "program.h"

bool check(long long expected, int ctor_count, int copy_count, int dtor_count, int selector_count) {
    return trace == expected && constructions == ctor_count && copies == copy_count &&
           destructions == dtor_count && selections == selector_count && alive == 0;
}
int exercise_result(bool fresh, bool fail, bool fail_copy) {
    Value result = choose_result(fresh, fail, fail_copy);
    if (!result.stable() || result.self != &result || result.value != (fresh ? 9 : 5)) return 1;
    return 0;
}
int exercise_comma(bool fresh, bool fail, bool fail_copy) {
    Value result = choose_comma(fresh, fail, fail_copy);
    if (!result.stable() || result.self != &result || result.value != (fresh ? 9 : 5)) return 1;
    return 0;
}
int exercise_nested(bool outer, bool inner, bool fail) {
    Value result = choose_nested(outer, inner, fail);
    if (!result.stable() || result.self != &result || result.value != (outer && inner ? 9 : 5)) return 1;
    return 0;
}

int main() {
    reset();
    if (exercise_result(true, false, false) || !check(2612344, 2, 0, 2, 1)) return 1;
    reset();
    if (exercise_result(false, false, false) || !check(26544, 1, 1, 2, 1)) return 2;
    reset();
    try { exercise_result(true, true, false); return 3; }
    catch (int &failure) { if (failure != 9) return 4; mark(8); }
    if (!check(2612348, 2, 0, 1, 1)) return 5;
    reset();
    try { exercise_result(false, false, true); return 6; }
    catch (int &failure) { if (failure != 17) return 7; mark(8); }
    if (!check(26548, 1, 1, 1, 1)) return 8;

    reset();
    if (exercise_comma(true, false, false) || !check(27612344, 2, 0, 2, 1)) return 9;
    reset();
    if (exercise_comma(false, false, false) || !check(276544, 1, 1, 2, 1)) return 10;
    reset();
    try { exercise_comma(true, true, false); return 11; }
    catch (int &failure) { if (failure != 9) return 12; mark(8); }
    if (!check(27612348, 2, 0, 1, 1)) return 13;
    reset();
    try { exercise_comma(false, false, true); return 14; }
    catch (int &failure) { if (failure != 17) return 15; mark(8); }
    if (!check(276548, 1, 1, 1, 1)) return 16;

    reset();
    if (exercise_nested(true, true, false) || !check(26612344, 2, 0, 2, 2)) return 17;
    reset();
    if (exercise_nested(true, false, false) || !check(266544, 1, 1, 2, 2)) return 18;
    reset();
    if (exercise_nested(false, true, false) || !check(26544, 1, 1, 2, 1)) return 19;
    reset();
    try { exercise_nested(true, true, true); return 20; }
    catch (int &failure) { if (failure != 9) return 21; mark(8); }
    if (!check(26612348, 2, 0, 1, 2)) return 22;

    reset();
    if (local_result(true, false, false) || !check(26123744, 2, 0, 2, 1)) return 23;
    reset();
    if (local_result(false, false, false) || !check(265744, 1, 1, 2, 1)) return 24;
    reset();
    try { local_result(true, true, false); return 25; }
    catch (int &failure) { if (failure != 9) return 26; mark(8); }
    if (!check(2612348, 2, 0, 1, 1)) return 27;
    reset();
    try { local_result(false, false, true); return 28; }
    catch (int &failure) { if (failure != 17) return 29; mark(8); }
    if (!check(26548, 1, 1, 1, 1)) return 30;
    return 0;
}
