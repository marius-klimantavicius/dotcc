//type:fp
//options:--target linux_aarch64;fn:--target win64
//options_all:-w --ms_c++20

typedef unsigned long long uint64_t;
typedef int int32_t;

struct alignas(16) Aligned16 {
    uint64_t data[2];
};

struct Base {
    Aligned16 a;
    int32_t b;
};

struct Derived : public Base {
    int32_t c;
};

#define offsetof __builtin_offsetof
static_assert(sizeof(Derived) == 48, "");
static_assert(offsetof(Derived, c) == 32, "");
