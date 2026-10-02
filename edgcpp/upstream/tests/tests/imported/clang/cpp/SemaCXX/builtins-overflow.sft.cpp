//type: fp
//options:  --c++17: --c++17
# 1 "SemaCXX/builtins-overflow.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 479 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/builtins-overflow.cpp" 2




# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/limits.h" 1
# 25 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/limits.h"
# 1 "/usr/include/limits.h" 1 3 4
# 26 "/usr/include/limits.h" 3 4
# 1 "/usr/include/features.h" 1 3 4
# 345 "/usr/include/features.h" 3 4
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 346 "/usr/include/features.h" 2 3 4
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
# 27 "/usr/include/limits.h" 2 3 4
# 144 "/usr/include/limits.h" 3 4
# 1 "/usr/include/bits/posix1_lim.h" 1 3 4
# 160 "/usr/include/bits/posix1_lim.h" 3 4
# 1 "/usr/include/bits/local_lim.h" 1 3 4
# 38 "/usr/include/bits/local_lim.h" 3 4
# 1 "/usr/include/linux/limits.h" 1 3 4
# 39 "/usr/include/bits/local_lim.h" 2 3 4
# 161 "/usr/include/bits/posix1_lim.h" 2 3 4
# 145 "/usr/include/limits.h" 2 3 4



# 1 "/usr/include/bits/posix2_lim.h" 1 3 4
# 149 "/usr/include/limits.h" 2 3 4



# 1 "/usr/include/bits/xopen_lim.h" 1 3 4
# 33 "/usr/include/bits/xopen_lim.h" 3 4
# 1 "/usr/include/bits/stdio_lim.h" 1 3 4
# 34 "/usr/include/bits/xopen_lim.h" 2 3 4
# 153 "/usr/include/limits.h" 2 3 4
# 26 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/limits.h" 2
# 6 "SemaCXX/builtins-overflow.cpp" 2
# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stdint.h" 1
# 56 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stdint.h"
# 1 "/usr/include/stdint.h" 1 3 4
# 26 "/usr/include/stdint.h" 3 4
# 1 "/usr/include/bits/wchar.h" 1 3 4
# 22 "/usr/include/bits/wchar.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 23 "/usr/include/bits/wchar.h" 2 3 4
# 27 "/usr/include/stdint.h" 2 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 28 "/usr/include/stdint.h" 2 3 4








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
# 57 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stdint.h" 2
# 7 "SemaCXX/builtins-overflow.cpp" 2

int a() {
  const int x = 3;
  static int z;
  constexpr int *y = &z;
  return []() { return __builtin_sub_overflow((int)x, (int)x, (int *)y); }();
}
int a2() {
  const int x = 3;
  static int z;
  constexpr int *y = &z;
  return []() { return __builtin_sub_overflow(x, x, y); }();
}

template<typename T>
struct Result {
  bool B;
  T Value;
  constexpr bool operator==(const Result<T> &Other) {
    return B == Other.B && Value == Other.Value;
  }
};


template <typename RET, typename LHS, typename RHS>
constexpr Result<RET> add(LHS &&lhs, RHS &&rhs) {
  RET sum{};
  return {__builtin_add_overflow(lhs, rhs, &sum), sum};
}

static_assert(add<short>(static_cast<char>(120), static_cast<char>(10)) == Result<short>{false, 130});
static_assert(add<char>(static_cast<short>(120), static_cast<short>(10)) == Result<char>{true, -126});
static_assert(add<unsigned int>(2147483647, 2147483647) == Result<unsigned int>{false, static_cast<unsigned int>(2147483647) * 2u});
static_assert(add<int>(static_cast<unsigned int>(2147483647), 1u) == Result<int>{true, (-2147483647 -1)});

static_assert(add<int>(17, 22) == Result<int>{false, 39});
static_assert(add<int>(2147483647 - 22, 24) == Result<int>{true, (-2147483647 -1) + 1});
static_assert(add<int>((-2147483647 -1) + 22, -23) == Result<int>{true, 2147483647});

template <typename RET, typename LHS, typename RHS>
constexpr Result<RET> sub(LHS &&lhs, RHS &&rhs) {
  RET sum{};
  return {__builtin_sub_overflow(lhs, rhs, &sum), sum};
}

static_assert(sub<unsigned char>(static_cast<char>(0),static_cast<char>(1)) == Result<unsigned char>{true, (127*2 +1)});
static_assert(sub<char>(static_cast<unsigned char>(0),static_cast<unsigned char>(1)) == Result<char>{false, -1});
static_assert(sub<unsigned short>(static_cast<short>(0),static_cast<short>(1)) == Result<unsigned short>{true, (32767 * 2 + 1)});
static_assert(sub<uint8_t>(static_cast<uint8_t>(255),static_cast<int>(100)) == Result<uint8_t>{false, 155});

static_assert(sub<int>(17,22) == Result<int>{false, -5});
static_assert(sub<int>(2147483647 - 22, -23) == Result<int>{true, (-2147483647 -1)});
static_assert(sub<int>((-2147483647 -1) + 22, 23) == Result<int>{true, 2147483647});

template <typename RET, typename LHS, typename RHS>
constexpr Result<RET> mul(LHS &&lhs, RHS &&rhs) {
  RET sum{};
  return {__builtin_mul_overflow(lhs, rhs, &sum), sum};
}

static_assert(mul<int>(17,22) == Result<int>{false, 374});
static_assert(mul<int>(2147483647 / 22, 23) == Result<int>{true, -2049870757});
static_assert(mul<int>((-2147483647 -1) / 22, -23) == Result<int>{true, -2049870757});

constexpr Result<int> sadd(int lhs, int rhs) {
  int sum{};
  return {__builtin_sadd_overflow(lhs, rhs, &sum), sum};
}

static_assert(sadd(17,22) == Result<int>{false, 39});
static_assert(sadd(2147483647 - 22, 23) == Result<int>{true, (-2147483647 -1)});
static_assert(sadd((-2147483647 -1) + 22, -23) == Result<int>{true, 2147483647});

constexpr Result<int> ssub(int lhs, int rhs) {
  int sum{};
  return {__builtin_ssub_overflow(lhs, rhs, &sum), sum};
}

static_assert(ssub(17,22) == Result<int>{false, -5});
static_assert(ssub(2147483647 - 22, -23) == Result<int>{true, (-2147483647 -1)});
static_assert(ssub((-2147483647 -1) + 22, 23) == Result<int>{true, 2147483647});

constexpr Result<int> smul(int lhs, int rhs) {
  int sum{};
  return {__builtin_smul_overflow(lhs, rhs, &sum), sum};
}

static_assert(smul(17,22) == Result<int>{false, 374});
static_assert(smul(2147483647 / 22, 23) == Result<int>{true, -2049870757});
static_assert(smul((-2147483647 -1) / 22, -23) == Result<int>{true, -2049870757});

template<typename T>
struct CarryResult {
  T CarryOut;
  T Value;
  constexpr bool operator==(const CarryResult<T> &Other) {
    return CarryOut == Other.CarryOut && Value == Other.Value;
  }
};

constexpr CarryResult<unsigned char> addcb(unsigned char lhs, unsigned char rhs, unsigned char carry) {
  unsigned char carry_out{};
  unsigned char sum{};
  sum = __builtin_addcb(lhs, rhs, carry, &carry_out);
  return {carry_out, sum};
}

static_assert(addcb(120, 10, 0) == CarryResult<unsigned char>{0, 130});
static_assert(addcb(250, 10, 0) == CarryResult<unsigned char>{1, 4});
static_assert(addcb(255, 255, 0) == CarryResult<unsigned char>{1, 254});
static_assert(addcb(255, 255, 1) == CarryResult<unsigned char>{1, 255});
static_assert(addcb(255, 0, 1) == CarryResult<unsigned char>{1, 0});
static_assert(addcb(255, 1, 0) == CarryResult<unsigned char>{1, 0});
static_assert(addcb(255, 1, 1) == CarryResult<unsigned char>{1, 1});



static_assert(addcb(255, 255, 2) == CarryResult<unsigned char>{1, 0});

constexpr CarryResult<unsigned char> subcb(unsigned char lhs, unsigned char rhs, unsigned char carry) {
  unsigned char carry_out{};
  unsigned char sum{};
  sum = __builtin_subcb(lhs, rhs, carry, &carry_out);
  return {carry_out, sum};
}

static_assert(subcb(20, 10, 0) == CarryResult<unsigned char>{0, 10});
static_assert(subcb(10, 10, 0) == CarryResult<unsigned char>{0, 0});
static_assert(subcb(10, 15, 0) == CarryResult<unsigned char>{1, 251});

static_assert(subcb(10, 15, 1) == CarryResult<unsigned char>{1, 250});
static_assert(subcb(0, 0, 1) == CarryResult<unsigned char>{1, 255});
static_assert(subcb(0, 1, 0) == CarryResult<unsigned char>{1, 255});
static_assert(subcb(0, 1, 1) == CarryResult<unsigned char>{1, 254});
static_assert(subcb(0, 255, 0) == CarryResult<unsigned char>{1, 1});
static_assert(subcb(0, 255, 1) == CarryResult<unsigned char>{1, 0});



static_assert(subcb(0, 255, 2) == CarryResult<unsigned char>{1, 255});
