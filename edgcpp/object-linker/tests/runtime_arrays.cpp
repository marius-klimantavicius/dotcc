#include <cxxabi.h>
#include <cstdio>

static int *base, ctor_failure, dtor_failure;
static void construct(void* pointer) {
    int index = static_cast<int*>(pointer) - base;
    std::printf("c%d,", index);
    if (index == ctor_failure) throw 17;
    *static_cast<int*>(pointer) = index + 10;
}
static void destroy(void* pointer) {
    int index = static_cast<int*>(pointer) - base;
    if (*static_cast<int*>(pointer) != index + 10) throw 99;
    std::printf("d%d,", index);
    if (index == dtor_failure) { dtor_failure = -1; throw 23; }
}
int main() {
    int values[4]; base = values;
    for (int failure = -1; failure != 4; ++failure) {
        ctor_failure = failure; dtor_failure = -1;
        std::printf("construct%d:", failure);
        try {
            __cxxabiv1::__cxa_vec_ctor(values, 4, sizeof(int), construct, destroy);
            __cxxabiv1::__cxa_vec_dtor(values, 4, sizeof(int), destroy);
        } catch (int value) { if (value != 17) return 1; std::printf("caught,"); }
        std::puts("");
    }
    for (int failure = 0; failure != 4; ++failure) {
        for (int i = 0; i != 4; ++i) values[i] = i + 10;
        dtor_failure = failure;
        std::printf("destroy%d:", failure);
        try { __cxxabiv1::__cxa_vec_dtor(values, 4, sizeof(int), destroy); return 2; }
        catch (int value) { if (value != 23) return 3; std::puts("caught,"); }
    }
    __cxxabiv1::__cxa_vec_ctor(nullptr, 0, sizeof(int), construct, destroy);
    __cxxabiv1::__cxa_vec_dtor(nullptr, 0, sizeof(int), destroy);
    __cxxabiv1::__cxa_vec_ctor(nullptr, 4, sizeof(int), nullptr, nullptr);
    __cxxabiv1::__cxa_vec_dtor(nullptr, 4, sizeof(int), nullptr);
    std::puts("empty/null callbacks OK");
}
