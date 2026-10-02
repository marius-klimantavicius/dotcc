//type: fn
//options:  --exceptions --c++20 --exceptions: --c++20 --exceptions: --c++20
# 1 "SemaCXX/compare-cxx2a.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 493 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/compare-cxx2a.cpp" 2








# 1 "SemaCXX/Inputs/std-compare.h" 1



namespace std {
inline namespace __1 {


enum class _EqResult : unsigned char {
  __equal = 0,
  __equiv = __equal,
};

enum class _OrdResult : signed char {
  __less = -1,
  __greater = 1
};

enum class _NCmpResult : signed char {
  __unordered = -127
};

struct _CmpUnspecifiedType;
using _CmpUnspecifiedParam = void (_CmpUnspecifiedType::*)();

class partial_ordering {
  using _ValueT = signed char;
  explicit constexpr partial_ordering(_EqResult __v) noexcept
      : __value_(_ValueT(__v)) {}
  explicit constexpr partial_ordering(_OrdResult __v) noexcept
      : __value_(_ValueT(__v)) {}
  explicit constexpr partial_ordering(_NCmpResult __v) noexcept
      : __value_(_ValueT(__v)) {}

  constexpr bool __is_ordered() const noexcept {
    return __value_ != _ValueT(_NCmpResult::__unordered);
  }

public:

  static const partial_ordering less;
  static const partial_ordering equivalent;
  static const partial_ordering greater;
  static const partial_ordering unordered;


  friend constexpr bool operator==(partial_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator!=(partial_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator<(partial_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator<=(partial_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator>(partial_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator>=(partial_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator==(_CmpUnspecifiedParam, partial_ordering __v) noexcept;
  friend constexpr bool operator!=(_CmpUnspecifiedParam, partial_ordering __v) noexcept;
  friend constexpr bool operator<(_CmpUnspecifiedParam, partial_ordering __v) noexcept;
  friend constexpr bool operator<=(_CmpUnspecifiedParam, partial_ordering __v) noexcept;
  friend constexpr bool operator>(_CmpUnspecifiedParam, partial_ordering __v) noexcept;
  friend constexpr bool operator>=(_CmpUnspecifiedParam, partial_ordering __v) noexcept;

  friend constexpr partial_ordering operator<=>(partial_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr partial_ordering operator<=>(_CmpUnspecifiedParam, partial_ordering __v) noexcept;


  constexpr bool test_eq(partial_ordering const &other) const noexcept {
    return __value_ == other.__value_;
  }

private:
  _ValueT __value_;
};

inline constexpr partial_ordering partial_ordering::less(_OrdResult::__less);
inline constexpr partial_ordering partial_ordering::equivalent(_EqResult::__equiv);
inline constexpr partial_ordering partial_ordering::greater(_OrdResult::__greater);
inline constexpr partial_ordering partial_ordering::unordered(_NCmpResult ::__unordered);
constexpr bool operator==(partial_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__is_ordered() && __v.__value_ == 0;
}
constexpr bool operator<(partial_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__is_ordered() && __v.__value_ < 0;
}
constexpr bool operator<=(partial_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__is_ordered() && __v.__value_ <= 0;
}
constexpr bool operator>(partial_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__is_ordered() && __v.__value_ > 0;
}
constexpr bool operator>=(partial_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__is_ordered() && __v.__value_ >= 0;
}
constexpr bool operator==(_CmpUnspecifiedParam, partial_ordering __v) noexcept {
  return __v.__is_ordered() && 0 == __v.__value_;
}
constexpr bool operator<(_CmpUnspecifiedParam, partial_ordering __v) noexcept {
  return __v.__is_ordered() && 0 < __v.__value_;
}
constexpr bool operator<=(_CmpUnspecifiedParam, partial_ordering __v) noexcept {
  return __v.__is_ordered() && 0 <= __v.__value_;
}
constexpr bool operator>(_CmpUnspecifiedParam, partial_ordering __v) noexcept {
  return __v.__is_ordered() && 0 > __v.__value_;
}
constexpr bool operator>=(_CmpUnspecifiedParam, partial_ordering __v) noexcept {
  return __v.__is_ordered() && 0 >= __v.__value_;
}
constexpr bool operator!=(partial_ordering __v, _CmpUnspecifiedParam) noexcept {
  return !__v.__is_ordered() || __v.__value_ != 0;
}
constexpr bool operator!=(_CmpUnspecifiedParam, partial_ordering __v) noexcept {
  return !__v.__is_ordered() || __v.__value_ != 0;
}

constexpr partial_ordering operator<=>(partial_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v;
}
constexpr partial_ordering operator<=>(_CmpUnspecifiedParam, partial_ordering __v) noexcept {
  return __v < 0 ? partial_ordering::greater : (__v > 0 ? partial_ordering::less : __v);
}

class weak_ordering {
  using _ValueT = signed char;
  explicit constexpr weak_ordering(_EqResult __v) noexcept : __value_(_ValueT(__v)) {}
  explicit constexpr weak_ordering(_OrdResult __v) noexcept : __value_(_ValueT(__v)) {}

public:
  static const weak_ordering less;
  static const weak_ordering equivalent;
  static const weak_ordering greater;


  constexpr operator partial_ordering() const noexcept {
    return __value_ == 0 ? partial_ordering::equivalent
                         : (__value_ < 0 ? partial_ordering::less : partial_ordering::greater);
  }


  friend constexpr bool operator==(weak_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator!=(weak_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator<(weak_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator<=(weak_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator>(weak_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator>=(weak_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator==(_CmpUnspecifiedParam, weak_ordering __v) noexcept;
  friend constexpr bool operator!=(_CmpUnspecifiedParam, weak_ordering __v) noexcept;
  friend constexpr bool operator<(_CmpUnspecifiedParam, weak_ordering __v) noexcept;
  friend constexpr bool operator<=(_CmpUnspecifiedParam, weak_ordering __v) noexcept;
  friend constexpr bool operator>(_CmpUnspecifiedParam, weak_ordering __v) noexcept;
  friend constexpr bool operator>=(_CmpUnspecifiedParam, weak_ordering __v) noexcept;

  friend constexpr weak_ordering operator<=>(weak_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr weak_ordering operator<=>(_CmpUnspecifiedParam, weak_ordering __v) noexcept;


  constexpr bool test_eq(weak_ordering const &other) const noexcept {
    return __value_ == other.__value_;
  }

private:
  _ValueT __value_;
};

inline constexpr weak_ordering weak_ordering::less(_OrdResult::__less);
inline constexpr weak_ordering weak_ordering::equivalent(_EqResult::__equiv);
inline constexpr weak_ordering weak_ordering::greater(_OrdResult::__greater);
constexpr bool operator==(weak_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__value_ == 0;
}
constexpr bool operator!=(weak_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__value_ != 0;
}
constexpr bool operator<(weak_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__value_ < 0;
}
constexpr bool operator<=(weak_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__value_ <= 0;
}
constexpr bool operator>(weak_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__value_ > 0;
}
constexpr bool operator>=(weak_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__value_ >= 0;
}
constexpr bool operator==(_CmpUnspecifiedParam, weak_ordering __v) noexcept {
  return 0 == __v.__value_;
}
constexpr bool operator!=(_CmpUnspecifiedParam, weak_ordering __v) noexcept {
  return 0 != __v.__value_;
}
constexpr bool operator<(_CmpUnspecifiedParam, weak_ordering __v) noexcept {
  return 0 < __v.__value_;
}
constexpr bool operator<=(_CmpUnspecifiedParam, weak_ordering __v) noexcept {
  return 0 <= __v.__value_;
}
constexpr bool operator>(_CmpUnspecifiedParam, weak_ordering __v) noexcept {
  return 0 > __v.__value_;
}
constexpr bool operator>=(_CmpUnspecifiedParam, weak_ordering __v) noexcept {
  return 0 >= __v.__value_;
}

constexpr weak_ordering operator<=>(weak_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v;
}
constexpr weak_ordering operator<=>(_CmpUnspecifiedParam, weak_ordering __v) noexcept {
  return __v < 0 ? weak_ordering::greater : (__v > 0 ? weak_ordering::less : __v);
}

class strong_ordering {
  using _ValueT = signed char;
  explicit constexpr strong_ordering(_EqResult __v) noexcept : __value_(static_cast<signed char>(__v)) {}
  explicit constexpr strong_ordering(_OrdResult __v) noexcept : __value_(static_cast<signed char>(__v)) {}

public:
  static const strong_ordering less;
  static const strong_ordering equal;
  static const strong_ordering equivalent;
  static const strong_ordering greater;


  constexpr operator partial_ordering() const noexcept {
    return __value_ == 0 ? partial_ordering::equivalent
                         : (__value_ < 0 ? partial_ordering::less : partial_ordering::greater);
  }
  constexpr operator weak_ordering() const noexcept {
    return __value_ == 0 ? weak_ordering::equivalent
                         : (__value_ < 0 ? weak_ordering::less : weak_ordering::greater);
  }


  friend constexpr bool operator==(strong_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator!=(strong_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator<(strong_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator<=(strong_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator>(strong_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator>=(strong_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr bool operator==(_CmpUnspecifiedParam, strong_ordering __v) noexcept;
  friend constexpr bool operator!=(_CmpUnspecifiedParam, strong_ordering __v) noexcept;
  friend constexpr bool operator<(_CmpUnspecifiedParam, strong_ordering __v) noexcept;
  friend constexpr bool operator<=(_CmpUnspecifiedParam, strong_ordering __v) noexcept;
  friend constexpr bool operator>(_CmpUnspecifiedParam, strong_ordering __v) noexcept;
  friend constexpr bool operator>=(_CmpUnspecifiedParam, strong_ordering __v) noexcept;

  friend constexpr strong_ordering operator<=>(strong_ordering __v, _CmpUnspecifiedParam) noexcept;
  friend constexpr strong_ordering operator<=>(_CmpUnspecifiedParam, strong_ordering __v) noexcept;


  constexpr bool test_eq(strong_ordering const &other) const noexcept {
    return __value_ == other.__value_;
  }

private:
  _ValueT __value_;
};

inline constexpr strong_ordering strong_ordering::less(_OrdResult::__less);
inline constexpr strong_ordering strong_ordering::equal(_EqResult::__equal);
inline constexpr strong_ordering strong_ordering::equivalent(_EqResult::__equiv);
inline constexpr strong_ordering strong_ordering::greater(_OrdResult::__greater);

constexpr bool operator==(strong_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__value_ == 0;
}
constexpr bool operator!=(strong_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__value_ != 0;
}
constexpr bool operator<(strong_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__value_ < 0;
}
constexpr bool operator<=(strong_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__value_ <= 0;
}
constexpr bool operator>(strong_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__value_ > 0;
}
constexpr bool operator>=(strong_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v.__value_ >= 0;
}
constexpr bool operator==(_CmpUnspecifiedParam, strong_ordering __v) noexcept {
  return 0 == __v.__value_;
}
constexpr bool operator!=(_CmpUnspecifiedParam, strong_ordering __v) noexcept {
  return 0 != __v.__value_;
}
constexpr bool operator<(_CmpUnspecifiedParam, strong_ordering __v) noexcept {
  return 0 < __v.__value_;
}
constexpr bool operator<=(_CmpUnspecifiedParam, strong_ordering __v) noexcept {
  return 0 <= __v.__value_;
}
constexpr bool operator>(_CmpUnspecifiedParam, strong_ordering __v) noexcept {
  return 0 > __v.__value_;
}
constexpr bool operator>=(_CmpUnspecifiedParam, strong_ordering __v) noexcept {
  return 0 >= __v.__value_;
}

constexpr strong_ordering operator<=>(strong_ordering __v, _CmpUnspecifiedParam) noexcept {
  return __v;
}
constexpr strong_ordering operator<=>(_CmpUnspecifiedParam, strong_ordering __v) noexcept {
  return __v < 0 ? strong_ordering::greater : (__v > 0 ? strong_ordering::less : __v);
}

}
}
# 10 "SemaCXX/compare-cxx2a.cpp" 2




struct S {
  static int x[5];
};

void self_compare() {
  int a;
  int *b = nullptr;
  S s;

  (void)(a <=> a);
  (void)(b <=> b);
  (void)(s.x[a] <=> S::x[a]);
}

void test0(long a, unsigned long b) {
  enum EnumA : int {A};
  enum EnumB {B};
  enum EnumC {C = 0x10000};

  (void)((short)a <=> (unsigned short)b);


  (void)(a <=> (unsigned long)b);
  (void)(a <=> (unsigned int) b);
  (void)(a <=> (unsigned short) b);
  (void)(a <=> (unsigned char) b);
  (void)((long)a <=> b);
  (void)((int)a <=> b);
  (void)((short)a <=> b);
  (void)((signed char)a <=> b);
  (void)((long)a <=> (unsigned long)b);
  (void)((int)a <=> (unsigned int)b);
  (void)((short) a <=> (unsigned short) b);
  (void)((signed char) a <=> (unsigned char) b);


  (void)(A <=> (unsigned long) b);
  (void)(A <=> (unsigned int) b);
  (void)(A <=> (unsigned short) b);
  (void)(A <=> (unsigned char) b);
  (void)((long) A <=> b);
  (void)((int) A <=> b);
  (void)((short) A <=> b);
  (void)((signed char) A <=> b);
  (void)((long) A <=> (unsigned long) b);
  (void)((int) A <=> (unsigned int) b);
  (void)((short) A <=> (unsigned short) b);
  (void)((signed char) A <=> (unsigned char) b);


  (void)(a <=> (unsigned long) B);
  (void)(a <=> (unsigned int) B);
  (void)(a <=> (unsigned short) B);
  (void)(a <=> (unsigned char) B);
  (void)((long) a <=> B);
  (void)((int) a <=> B);
  (void)((short) a <=> B);
  (void)((signed char) a <=> B);
  (void)((long) a <=> (unsigned long) B);
  (void)((int) a <=> (unsigned int) B);
  (void)((short) a <=> (unsigned short) B);
  (void)((signed char) a <=> (unsigned char) B);


  (void)(C <=> (unsigned long) b);
  (void)(C <=> (unsigned int) b);
  (void)(C <=> (unsigned short) b);
  (void)(C <=> (unsigned char) b);
  (void)((long) C <=> b);
  (void)((int) C <=> b);
  (void)((short) C <=> b);
  (void)((signed char) C <=> b);
  (void)((long) C <=> (unsigned long) b);
  (void)((int) C <=> (unsigned int) b);
  (void)((short) C <=> (unsigned short) b);
  (void)((signed char) C <=> (unsigned char) b);


  (void)(a <=> (unsigned long) C);
  (void)(a <=> (unsigned int) C);
  (void)(a <=> (unsigned short) C);
  (void)(a <=> (unsigned char) C);
  (void)((long) a <=> C);
  (void)((int) a <=> C);
  (void)((short) a <=> C);
  (void)((signed char) a <=> C);
  (void)((long) a <=> (unsigned long) C);
  (void)((int) a <=> (unsigned int) C);
  (void)((short) a <=> (unsigned short) C);
  (void)((signed char) a <=> (unsigned char) C);


  (void)(0x80000 <=> (unsigned long) b);
  (void)(0x80000 <=> (unsigned int) b);
  (void)(0x80000 <=> (unsigned short) b);
  (void)(0x80000 <=> (unsigned char) b);
  (void)((long) 0x80000 <=> b);
  (void)((int) 0x80000 <=> b);
  (void)((short) 0x80000 <=> b);
  (void)((signed char) 0x80000 <=> b);
  (void)((long) 0x80000 <=> (unsigned long) b);
  (void)((int) 0x80000 <=> (unsigned int) b);
  (void)((short) 0x80000 <=> (unsigned short) b);
  (void)((signed char) 0x80000 <=> (unsigned char) b);


  (void)(a <=> (unsigned long)0x80000);
  (void)(a <=> (unsigned int) 0x80000);
  (void)(a <=> (unsigned short) 0x80000);
  (void)(a <=> (unsigned char) 0x80000);
  (void)((long) a <=> 0x80000);
  (void)((int) a <=> 0x80000);
  (void)((short) a <=> 0x80000);
  (void)((signed char) a <=> 0x80000);
  (void)((long)a <=> (unsigned long)0x80000);
  (void)((int)a <=> (unsigned int)0x80000);
  (void)((short) a <=> (unsigned short) 0x80000);
  (void)((signed char) a <=> (unsigned char) 0x80000);
}

void test5(bool b, bool b2) {
  enum EnumA { A };
  (void)(b <=> b2);
  (void)(true <=> b);
  (void)(b <=> -10);
  (void)(b <=> char(1));
  (void)(b <=> A);


  (void)(b <=> 0);
  (void)(b <=> 1);
}

void test6(signed char sc) {
  (void)(sc <=> 200);
  (void)(200 <=> sc);
}


void test7(unsigned long other) {

  (void)((unsigned)other <=> (unsigned long)(0x1'ffff'ffff));
  (void)((unsigned)other <=> (unsigned long)(0xffff'ffff));
  (void)((unsigned long)other <=> (unsigned)(0x1'ffff'ffff));
  (void)((unsigned long)other <=> (unsigned)(0xffff'ffff));


  (void)((int)other <=> (unsigned long)(0xffff'ffff'ffff'ffff));
  (void)((int)other <=> (unsigned long)(0x0000'0000'ffff'ffff));
  (void)((int)other <=> (unsigned long)(0x0000'0000'0fff'ffff));
  (void)((int)other <=> (unsigned)(0x8000'0000));


  (void)((unsigned long)other <=> (int)(0xffff'ffff));





  (void)((int)other <=> (long)(0xffff'ffff));
  (void)((int)other <=> (long)(0xffff'ffff'0000'0000));
  (void)((int)other <=> (long)(0x0fff'ffff));
  (void)((int)other <=> (long)(0xffff'ffff'f000'0000));


  (void)((int)other <=> (unsigned char)(0xffff));
  (void)((int)other <=> (unsigned char)(0xff));


  (void)((unsigned char)other <=> (int)(0xff));
  (void)((unsigned char)other <=> (int)(0xffff));


  (void)((unsigned char)other <=> (unsigned short)(0xff));
  (void)((unsigned char)other <=> (unsigned short)(0x100));
  (void)((unsigned short)other <=> (unsigned char)(0xff));
}

void test8(void *vp, const void *cvp, int *ip) {
  (void)(vp <=> cvp);
  (void)(vp <=> ip);
  (void)(ip <=> cvp);
}

void test9(long double ld, double d, float f, int i, long long ll) {
  (void)(f <=> ll);
  (void)(d <=> ld);
  (void)(i <=> f);
}

typedef int *INTPTR;
void test_typedef_bug(int *x, INTPTR y) {
  (void)(x <=> y);
}

using nullptr_t = decltype(nullptr);

struct Class {};
struct ClassB : Class {};
struct Class2 {};
using FnTy = void(int);
using MemFnTy = void (Class::*)() const;
using MemDataTy = long(Class::*);

void test_nullptr(int *x, FnTy *fp, MemFnTy memp, MemDataTy memdp) {
  auto r1 = (nullptr <=> nullptr);
  auto r2 = (nullptr <=> x);
  auto r3 = (fp <=> nullptr);
  auto r4 = (0 <=> fp);
  auto r5 = (nullptr <=> memp);
  auto r6 = (0 <=> memdp);
  auto r7 = (0 <=> nullptr);
}

void test_memptr(MemFnTy mf, MemDataTy md) {
  (void)(mf <=> mf);
  (void)(md <=> md);
}


template <int Val>
auto test_template_overflow() {

  return (Val <=> (unsigned long)0);
}
template auto test_template_overflow<0>();
template auto test_template_overflow<-1>();

void test_enum_integral_compare() {
  enum EnumA : int {A, ANeg = -1, AMax = 2147483647};
  enum EnumB : unsigned {B, BMax = 4294967295U };
  enum EnumC : int {C = -1, C0 = 0};

  (void)(A <=> C);

  (void)(A <=> (unsigned)0);
  (void)((unsigned)0 <=> A);
  (void)(ANeg <=> (unsigned)0);
  (void)((unsigned)0 <=> ANeg);

  (void)(B <=> 42);
  (void)(42 <=> B);
  (void)(B <=> (unsigned long long)42);
  (void)(B <=> -1);
  (void)(BMax <=> (unsigned long)-1);

  (void)(C0 <=> (unsigned)42);
  (void)(C <=> (unsigned)42);
}

namespace EnumCompareTests {

enum class EnumA { A, A2 };
enum class EnumB { B };
enum class EnumC : unsigned { C };

void test_enum_enum_compare_no_builtin() {
  auto r1 = (EnumA::A <=> EnumA::A2);
  static_assert(__is_same(decltype(r1), std::strong_ordering));;
  (void)(EnumA::A <=> EnumA::A);
  (void)(EnumA::A <=> EnumB::B);
  (void)(EnumB::B <=> EnumA::A);
}

template <int>
struct Tag {};
Tag<0> operator<=>(EnumA, EnumA) {
  return {};
}

Tag<1> operator<=>(EnumA, EnumB) {
  return {};
}

void test_enum_ovl_provided() {
  auto r1 = (EnumA::A <=> EnumA::A);
  static_assert(__is_same(decltype(r1), Tag<0>));;
  auto r2 = (EnumA::A <=> EnumB::B);
  static_assert(__is_same(decltype(r2), Tag<1>));;
  (void)(EnumB::B <=> EnumA::A);
}

void enum_float_test() {
  enum EnumA { A };
  (void)(A <=> (float)0);
  (void)((double)0 <=> A);
  (void)((long double)0 <=> A);
}

enum class Bool1 : bool { Zero,
                          One };
enum Bool2 : bool { B2_Zero,
                    B2_One };

void test_bool_enum(Bool1 A1, Bool1 A2, Bool2 B1, Bool2 B2) {
  (void)(A1 <=> A2);
  (void)(B1 <=> B2);
}

}

namespace TestUserDefinedConvSeq {

template <class T, T Val>
struct Conv {
  constexpr operator T() const { return Val; }
  operator T() { return Val; }
};

void test_user_conv() {
  {
    using C = Conv<int, 0>;
    C c;
    const C cc;
    (void)(0 <=> c);
    (void)(c <=> -1);
    (void)((unsigned)0 <=> cc);
    (void)((unsigned)0 <=> c);
  }
  {
    using C = Conv<int, -1>;
    C c;
    const C cc;
    (void)(c <=> 0);
    (void)(cc <=> (unsigned)0);
    (void)(c <=> (unsigned)0);
  }
}

struct X {
  constexpr const Conv<int, -1> operator<=>(X) { return {}; }
};
static_assert(X() < X());

}

void test_array_conv() {
  int arr[5];
  int *ap = arr + 2;
  int arr2[3];
  (void)(arr <=> arr);
  (void)(+arr <=> arr);
}

void test_mixed_float_int(float f, double d, long double ld) {
  extern int i;
  extern unsigned u;
  extern long l;
  extern short s;
  extern unsigned short us;
  auto r1 = (f <=> i);
  static_assert(__is_same(decltype(r1), std::partial_ordering));;

  auto r2 = (us <=> ld);
  static_assert(__is_same(decltype(r2), std::partial_ordering));;

  auto r3 = (s <=> f);
  static_assert(__is_same(decltype(r3), std::partial_ordering));;

  auto r4 = (0.0 <=> i);
  static_assert(__is_same(decltype(r4), std::partial_ordering));;
}

namespace NullptrTest {
using nullptr_t = decltype(nullptr);
void foo(nullptr_t x, nullptr_t y) {
  auto r = x <=> y;
}
}

namespace ComplexTest {

enum class StrongE {};
enum WeakE { E_One,
             E_Two };

void test_diag(_Complex int ci, _Complex float cf, _Complex double cd, int i, float f, StrongE E1, WeakE E2, int *p) {
  (void)(ci <=> (_Complex int &)ci);
  (void)(ci <=> cf);
  (void)(ci <=> i);
  (void)(ci <=> f);
  (void)(cf <=> i);
  (void)(cf <=> f);
  (void)(ci <=> p);
  (void)(ci <=> E1);
  (void)(E2 <=> cf);
}

void test_int(_Complex int x, _Complex int y) {
  auto r = x <=> y;
}

void test_double(_Complex double x, _Complex double y) {
  auto r = x <=> y;
}

}

namespace Vector {
  typedef __attribute__((ext_vector_type(4))) int V;
  void f(V v1, V v2) {




    (void)(v1 <=> v2);
  }
}

namespace PR44992 {
  extern "C++" struct s {
    friend auto operator<=>(s const &, s const &) = default;
  };
}

namespace PR52537 {
  template<typename T> struct X {};
  template<typename T> bool operator==(const X<T> &, int) { return T::error; }
  template<typename T> int operator<=>(const X<T> &, int) { return T::error; }

  const X<int[1]> x1;
  template<typename T> bool f1() { return x1 != 0; }
  void g1() { f1<int>(); }

  const X<int[2]> x2;
  template<typename T> bool f2() { return 0 == x2; }
  void g2() { f2<int>(); }

  const X<int[3]> x3;
  template<typename T> bool f3() { return x3 < 0; }
  void g3() { f3<int>(); }

  const X<int[4]> x4;
  template<typename T> bool f4() { return 0 >= x4; }
  void g4() { f4<int>(); }

  template<typename T> struct Y {};
  template<typename T> struct Z { Z(int) { T::error; } using nondeduced = Z; };
  template<typename T> Z<T> operator<=>(const Y<T>&, int);
  template<typename T> bool operator<(const Z<T>&, const typename Z<T>::nondeduced&);
  template<typename T> bool operator<(const typename Z<T>::nondeduced&, const Z<T>&);

  const Y<int[5]> y5;
  template<typename T> bool f5() { return y5 < 0; }
  void g5() { f5<int>(); }

  const Y<int[6]> y6;
  template<typename T> bool f6() { return 0 < y6; }
  void g6() { f6<int>(); }
}


namespace GH64923 {
using nullptr_t = decltype(nullptr);
struct MyTask{};
constexpr MyTask DoAnotherThing() {
    return {};
}

constexpr nullptr_t operator++(MyTask &&T);
void DoSomething() {
  if constexpr (++DoAnotherThing() != nullptr) {}


  if constexpr (nullptr == ++DoAnotherThing()) {}

}
}

namespace GH64162 {
struct S {
    const std::strong_ordering& operator<=>(const S&) const = default;
};
bool test(S s) {
    return s < s;
}


struct A {};
bool operator<(A, int);

struct B {
    operator A();
};

struct C {
    B operator<=>(C);
};

bool f(C c) { return c < c; }
}
