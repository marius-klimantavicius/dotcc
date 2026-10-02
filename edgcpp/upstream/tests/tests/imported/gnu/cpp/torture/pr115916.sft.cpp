//type: rp
//options: 
# 0 "./torture/pr115916.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/pr115916.C"


# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 1 3 4
# 145 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4

# 145 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef long int ptrdiff_t;
# 214 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 425 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef struct {
  long long __max_align_ll __attribute__((__aligned__(__alignof__(long long))));
  long double __max_align_ld __attribute__((__aligned__(__alignof__(long double))));
# 436 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
} max_align_t;






  typedef decltype(nullptr) nullptr_t;
# 4 "./torture/pr115916.C" 2
# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdint.h" 1 3 4
# 9 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdint.h" 3 4
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wpedantic"
# 1 "/usr/include/stdint.h" 1 3 4
# 25 "/usr/include/stdint.h" 3 4
# 1 "/usr/include/features.h" 1 3 4
# 375 "/usr/include/features.h" 3 4
# 1 "/usr/include/sys/cdefs.h" 1 3 4
# 392 "/usr/include/sys/cdefs.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 393 "/usr/include/sys/cdefs.h" 2 3 4
# 376 "/usr/include/features.h" 2 3 4
# 399 "/usr/include/features.h" 3 4
# 1 "/usr/include/gnu/stubs.h" 1 3 4
# 10 "/usr/include/gnu/stubs.h" 3 4
# 1 "/usr/include/gnu/stubs-64.h" 1 3 4
# 11 "/usr/include/gnu/stubs.h" 2 3 4
# 400 "/usr/include/features.h" 2 3 4
# 26 "/usr/include/stdint.h" 2 3 4
# 1 "/usr/include/bits/wchar.h" 1 3 4
# 22 "/usr/include/bits/wchar.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 23 "/usr/include/bits/wchar.h" 2 3 4
# 27 "/usr/include/stdint.h" 2 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 28 "/usr/include/stdint.h" 2 3 4
# 36 "/usr/include/stdint.h" 3 4
typedef signed char int8_t;
typedef short int int16_t;
typedef int int32_t;

typedef long int int64_t;







typedef unsigned char uint8_t;
typedef unsigned short int uint16_t;

typedef unsigned int uint32_t;



typedef unsigned long int uint64_t;
# 65 "/usr/include/stdint.h" 3 4
typedef signed char int_least8_t;
typedef short int int_least16_t;
typedef int int_least32_t;

typedef long int int_least64_t;






typedef unsigned char uint_least8_t;
typedef unsigned short int uint_least16_t;
typedef unsigned int uint_least32_t;

typedef unsigned long int uint_least64_t;
# 90 "/usr/include/stdint.h" 3 4
typedef signed char int_fast8_t;

typedef long int int_fast16_t;
typedef long int int_fast32_t;
typedef long int int_fast64_t;
# 103 "/usr/include/stdint.h" 3 4
typedef unsigned char uint_fast8_t;

typedef unsigned long int uint_fast16_t;
typedef unsigned long int uint_fast32_t;
typedef unsigned long int uint_fast64_t;
# 119 "/usr/include/stdint.h" 3 4
typedef long int intptr_t;


typedef unsigned long int uintptr_t;
# 134 "/usr/include/stdint.h" 3 4
typedef long int intmax_t;
typedef unsigned long int uintmax_t;
# 12 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdint.h" 2 3 4
#pragma GCC diagnostic pop
# 5 "./torture/pr115916.C" 2


# 6 "./torture/pr115916.C"
struct ve {
    ve() = default;
    ve(const ve&) = default;
    ve& operator=(const ve&) = default;


    uint8_t raw[16] = {};
};

static ve First8_(void) {
    ve m;
    __builtin_memset(m.raw, 0xff, 8);
    return m;
}

static ve And_(ve a, ve b) {
    ve au;
    __builtin_memcpy(au.raw, a.raw, 16);
    for (size_t i = 0; i < 8; ++i) {
        au.raw[i] &= b.raw[i];
    }
    return au;
}

__attribute__((noipa, optimize(0)))
static void vec_assert(ve a) {
    if (a.raw[6] != 0x06 && a.raw[6] != 0x07)
        __builtin_trap();
}

static ve Reverse4_(ve v) {
    ve ret;
    for (size_t i = 0; i < 8; i += 4) {
        ret.raw[i + 0] = v.raw[i + 3];
        ret.raw[i + 1] = v.raw[i + 2];
        ret.raw[i + 2] = v.raw[i + 1];
        ret.raw[i + 3] = v.raw[i + 0];
    }
    return ret;
}

static ve DupEven_(ve v) {
    for (size_t i = 0; i < 8; i += 2) {
        v.raw[i + 1] = v.raw[i];
    }
    return v;
}

template <bool b>
ve Per4LaneBlockShuffle_(ve v) {
    if (b) {
        return Reverse4_(v);
    } else {
        return DupEven_(v);
    }
}

template <bool b>
static inline __attribute__((always_inline)) void DoTestPer4LaneBlkShuffle(const ve v) {
    ve actual = Per4LaneBlockShuffle_<b>(v);
    const auto valid_lanes_mask = First8_();
    ve actual_masked = And_(valid_lanes_mask, actual);
    vec_assert(actual_masked);
}

static void DoTestPer4LaneBlkShuffles(const ve v) {
    alignas(128) uint8_t src_lanes[8];
    __builtin_memcpy(src_lanes, v.raw, 8);

    DoTestPer4LaneBlkShuffle<true >(v);
    DoTestPer4LaneBlkShuffle<false>(v);
}

__attribute__((noipa, optimize(0)))
static void bug(void) {
   uint8_t iv[8] = {1,2,3,4,5,6,7,8};
   ve v;
   __builtin_memcpy(v.raw, iv, 8);
   DoTestPer4LaneBlkShuffles(v);
}

int main(void) {
    bug();
}
