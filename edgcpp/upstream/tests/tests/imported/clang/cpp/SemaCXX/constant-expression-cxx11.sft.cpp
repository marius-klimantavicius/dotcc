//type: fn
//options:  --c++23 --exceptions: --c++20 --exceptions: --c++11 --exceptions
# 1 "SemaCXX/constant-expression-cxx11.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 499 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/constant-expression-cxx11.cpp" 2








namespace StaticAssertFoldTest {

int x;
static_assert(++x, "test");

static_assert(false, "test");

}

int array[(long)(char *)0];



typedef decltype(sizeof(char)) size_t;

template<typename T> constexpr T id(const T &t) { return t; }
template<typename T> constexpr T min(const T &a, const T &b) {
  return a < b ? a : b;
}
template<typename T> constexpr T max(const T &a, const T &b) {
  return a < b ? b : a;
}
template<typename T, size_t N> constexpr T *begin(T (&xs)[N]) { return xs; }
template<typename T, size_t N> constexpr T *end(T (&xs)[N]) { return xs + N; }

struct MemberZero {
  constexpr int zero() const { return 0; }
};

constexpr int arr[];
constexpr int arr2[2];
constexpr int arr3[2] = {};

namespace DerivedToVBaseCast {

  struct U { int n; };
  struct V : U { int n; };
  struct A : virtual V { int n; };
  struct Aa { int n; };
  struct B : virtual A, Aa {};
  struct C : virtual A, Aa {};
  struct D : B, C {};

  D d;
  constexpr B *p = &d;
  constexpr C *q = &d;

  static_assert((void*)p != (void*)q, "");
  static_assert((A*)p == (A*)q, "");
  static_assert((Aa*)p != (Aa*)q, "");

  constexpr B &pp = d;
  constexpr C &qq = d;
  static_assert((void*)&pp != (void*)&qq, "");
  static_assert(&(A&)pp == &(A&)qq, "");
  static_assert(&(Aa&)pp != &(Aa&)qq, "");

  constexpr V *v = p;
  constexpr V *w = q;
  constexpr V *x = (A*)p;
  static_assert(v == w, "");
  static_assert(v == x, "");

  static_assert((U*)&d == p, "");
  static_assert((U*)&d == q, "");
  static_assert((U*)&d == v, "");
  static_assert((U*)&d == w, "");
  static_assert((U*)&d == x, "");

  struct X {};
  struct Y1 : virtual X {};
  struct Y2 : X {};
  struct Z : Y1, Y2 {};
  Z z;
  static_assert((X*)(Y1*)&z != (X*)(Y2*)&z, "");
}

namespace ConstCast {

constexpr int n1 = 0;
constexpr int n2 = const_cast<int&>(n1);
constexpr int *n3 = const_cast<int*>(&n1);
constexpr int n4 = *const_cast<int*>(&n1);
constexpr const int * const *n5 = const_cast<const int* const*>(&n3);
constexpr int **n6 = const_cast<int**>(&n3);
constexpr int n7 = **n5;
constexpr int n8 = **n6;


struct A { int n; };
constexpr int n9 = (const_cast<A&&>(A{123})).n;
static_assert(n9 == 123, "");

}

namespace TemplateArgumentConversion {
  template<int n> struct IntParam {};

  using IntParam0 = IntParam<0>;
  using IntParam0 = IntParam<id(0)>;
  using IntParam0 = IntParam<MemberZero().zero>;
}

namespace CaseStatements {
  int x;
  void f(int n) {
    switch (n) {
    case MemberZero().zero:
    case id(0):
      return;
    case __builtin_constant_p(true) ? (long unsigned int)&x : 0:;
    }
  }
}

extern int &Recurse1;
int &Recurse2 = Recurse1;
int &Recurse1 = Recurse2;
constexpr int &Recurse3 = Recurse2;

extern const int RecurseA;
const int RecurseB = RecurseA;
const int RecurseA = 10;
constexpr int RecurseC = RecurseB;

namespace MemberEnum {
  struct WithMemberEnum {
    enum E { A = 42 };
  } wme;

  static_assert(wme.A == 42, "");
}

namespace DefaultArguments {

const int z = int();
constexpr int Sum(int a = 0, const int &b = 0, const int *c = &z, char d = 0) {
  return a + b + *c + d;
}
const int four = 4;
constexpr int eight = 8;
constexpr const int twentyseven = 27;
static_assert(Sum() == 0, "");
static_assert(Sum(1) == 1, "");
static_assert(Sum(1, four) == 5, "");
static_assert(Sum(1, eight, &twentyseven) == 36, "");
static_assert(Sum(1, 2, &four, eight) == 15, "");

}

namespace Ellipsis {


constexpr int F(int a, ...) { return a; }
static_assert(F(0) == 0, "");
static_assert(F(1, 0) == 1, "");
static_assert(F(2, "test") == 2, "");
static_assert(F(3, &F) == 3, "");
int k = 0;
static_assert(F(4, k) == 3, "");

}

namespace Recursion {
  constexpr int fib(int n) { return n > 1 ? fib(n-1) + fib(n-2) : n; }
  static_assert(fib(11) == 89, "");

  constexpr int gcd_inner(int a, int b) {
    return b == 0 ? a : gcd_inner(b, a % b);
  }
  constexpr int gcd(int a, int b) {
    return gcd_inner(max(a, b), min(a, b));
  }

  static_assert(gcd(1749237, 5628959) == 7, "");
}

namespace FunctionCast {


  constexpr int f() { return 1; }
  typedef double (*DoubleFn)();
  typedef int (*IntFn)();
  int a[(int)DoubleFn(f)()];
  int b[(int)IntFn(f)()];
}

namespace StaticMemberFunction {
  struct S {
    static constexpr int k = 42;
    static constexpr int f(int n) { return n * k + 2; }
  } s;

  constexpr int n = s.f(19);
  static_assert(S::f(19) == 800, "");
  static_assert(s.f(19) == 800, "");
  static_assert(n == 800, "");

  constexpr int (*sf1)(int) = &S::f;
  constexpr int (*sf2)(int) = &s.f;
  constexpr const int *sk = &s.k;



  constexpr S *out_of_lifetime(S s) { return &s; }
  static_assert(out_of_lifetime({})->k == 42, "");
  static_assert(out_of_lifetime({})->f(3) == 128, "");


  union U {
    int n;
    S s;
  };
  constexpr U u = {0};
  static_assert(u.s.k == 42, "");
  static_assert(u.s.f(1) == 44, "");


  static_assert((&s)[1].k == 42, "");
  static_assert((&s)[1].f(1) == 44, "");
}

namespace ParameterScopes {

  const int k = 42;
  constexpr const int &ObscureTheTruth(const int &a) { return a; }
  constexpr const int &MaybeReturnJunk(bool b, const int a) {
    return ObscureTheTruth(b ? a : k);
  }
  static_assert(MaybeReturnJunk(false, 0) == 42, "");
  constexpr int a = MaybeReturnJunk(true, 0);

  constexpr const int MaybeReturnNonstaticRef(bool b, const int a) {
    return ObscureTheTruth(b ? a : k);
  }
  static_assert(MaybeReturnNonstaticRef(false, 0) == 42, "");
  constexpr int b = MaybeReturnNonstaticRef(true, 0);

  constexpr int InternalReturnJunk(int n) {
    return MaybeReturnJunk(true, n);
  }
  constexpr int n3 = InternalReturnJunk(0);

  constexpr int LToR(int &n) { return n; }
  constexpr int GrabCallersArgument(bool which, int a, int b) {
    return LToR(which ? b : a);
  }
  static_assert(GrabCallersArgument(false, 1, 2) == 1, "");
  static_assert(GrabCallersArgument(true, 4, 8) == 8, "");

}

namespace Pointers {

  constexpr int f(int n, const int *a, const int *b, const int *c) {
    return n == 0 ? 0 : *a + f(n-1, b, c, a);
  }

  const int x = 1, y = 10, z = 100;
  static_assert(f(23, &x, &y, &z) == 788, "");

  constexpr int g(int n, int a, int b, int c) {
    return f(n, &a, &b, &c);
  }
  static_assert(g(23, x, y, z) == 788, "");

}

namespace FunctionPointers {

  constexpr int Double(int n) { return 2 * n; }
  constexpr int Triple(int n) { return 3 * n; }
  constexpr int Twice(int (*F)(int), int n) { return F(F(n)); }
  constexpr int Quadruple(int n) { return Twice(Double, n); }
  constexpr auto Select(int n) -> int (*)(int) {
    return n == 2 ? &Double : n == 3 ? &Triple : n == 4 ? &Quadruple : 0;
  }
  constexpr int Apply(int (*F)(int), int n) { return F(n); }

  static_assert(1 + Apply(Select(4), 5) + Apply(Select(3), 7) == 42, "");

  constexpr int Invalid = Apply(Select(0), 0);

}

namespace PointerComparison {

int x, y;
static_assert(&x == &y, "false");
static_assert(&x != &y, "");
constexpr bool g1 = &x == &y;
constexpr bool g2 = &x != &y;
constexpr bool g3 = &x <= &y;
constexpr bool g4 = &x >= &y;
constexpr bool g5 = &x < &y;
constexpr bool g6 = &x > &y;

struct S { int x, y; } s;
static_assert(&s.x == &s.y, "false");
static_assert(&s.x != &s.y, "");
static_assert(&s.x <= &s.y, "");
static_assert(&s.x >= &s.y, "false");
static_assert(&s.x < &s.y, "");
static_assert(&s.x > &s.y, "false");

static_assert(0 == &y, "false");
static_assert(0 != &y, "");
constexpr bool n3 = (int*)0 <= &y;
constexpr bool n4 = (int*)0 >= &y;
constexpr bool n5 = (int*)0 < &y;
constexpr bool n6 = (int*)0 > &y;

static_assert(&x == 0, "false");
static_assert(&x != 0, "");
constexpr bool n9 = &x <= (int*)0;
constexpr bool n10 = &x >= (int*)0;
constexpr bool n11 = &x < (int*)0;
constexpr bool n12 = &x > (int*)0;

static_assert(&x == &x, "");
static_assert(&x != &x, "false");
static_assert(&x <= &x, "");
static_assert(&x >= &x, "");
static_assert(&x < &x, "false");
static_assert(&x > &x, "false");

constexpr S* sptr = &s;
constexpr bool dyncast = sptr == dynamic_cast<S*>(sptr);

struct U {};
struct Str {
  int a : dynamic_cast<S*>(sptr) == dynamic_cast<S*>(sptr);


  int b : reinterpret_cast<S*>(sptr) == reinterpret_cast<S*>(sptr);


  int c : (S*)(long)(sptr) == (S*)(long)(sptr);


  int d : (S*)(42) == (S*)(42);


  int e : (Str*)(sptr) == (Str*)(sptr);


  int f : &(U&)(*sptr) == &(U&)(*sptr);


  int g : (S*)(void*)(sptr) == sptr;


};

extern char externalvar[];
constexpr bool constaddress = (void *)externalvar == (void *)0x4000UL;
static_assert(0 != "foo", "");


static_assert(+"foo" != +"bar", "");
static_assert("xfoo" + 1 != "yfoo" + 1, "");
static_assert(+"foot" != +"foo", "");
static_assert(+"foo\0bar" != +"foo\0baz", "");


static_assert((__builtin_constant_p((const char*)u"A" != (const char*)"\0A\0x") ? ((const char*)u"A" != (const char*)"\0A\0x") : ((const char*)u"A" != (const char*)"\0A\0x")), "");
static_assert((__builtin_constant_p((const char*)u"A" != (const char*)"A\0\0x") ? ((const char*)u"A" != (const char*)"A\0\0x") : ((const char*)u"A" != (const char*)"A\0\0x")), "");

constexpr const char *string = "hello";
constexpr const char *also_string = string;
static_assert(string == string, "");
static_assert(string == also_string, "");


constexpr bool may_overlap_1 = +"foo" == +"foo";
constexpr bool may_overlap_2 = +"foo" == +"foo\0bar";
constexpr bool may_overlap_3 = +"foo" == "bar\0foo" + 4;
constexpr bool may_overlap_4 = "xfoo" + 1 == "xfoo" + 1;




constexpr bool may_overlap_different_encoding[] =
  {(__builtin_constant_p((const char*)u"A" != (const char*)"xA\0\0\0x" + 1) ? ((const char*)u"A" != (const char*)"xA\0\0\0x" + 1) : ((const char*)u"A" != (const char*)"xA\0\0\0x" + 1)), (__builtin_constant_p((const char*)u"A" != (const char*)"x\0A\0\0x" + 1) ? ((const char*)u"A" != (const char*)"x\0A\0\0x" + 1) : ((const char*)u"A" != (const char*)"x\0A\0\0x" + 1))};


}

constexpr const char *getStr() {
  return "abc";
}
constexpr int strMinus() {
  (void)(getStr() - getStr());

  return 0;
}
static_assert(strMinus() == 0, "");


constexpr int a = 0;
constexpr int b = 1;
constexpr int n = &b - &a;

constexpr static int arrk[2] = {1,2};
constexpr static int arrk2[2] = {3,4};
constexpr int k2 = &arrk[1] - &arrk2[0];


namespace MaterializeTemporary {

constexpr int f(const int &r) { return r; }
constexpr int n = f(1);

constexpr bool same(const int &a, const int &b) { return &a == &b; }
constexpr bool sameTemporary(const int &n) { return same(n, n); }

static_assert(n, "");
static_assert(!same(4, 4), "");
static_assert(same(n, n), "");
static_assert(sameTemporary(9), "");

struct A { int &&r; };
struct B { A &&a1; A &&a2; };

constexpr B b1 { { 1 }, { 2 } };
static_assert(&b1.a1 != &b1.a2, "");
static_assert(&b1.a1.r != &b1.a2.r, "");

constexpr B &&b2 { { 3 }, { 4 } };
static_assert(&b1 != &b2, "");
static_assert(&b1.a1 != &b2.a1, "");

constexpr thread_local B b3 { { 1 }, { 2 } };
void foo() {
  constexpr static B b1 { { 1 }, { 2 } };
  constexpr thread_local B b2 { { 1 }, { 2 } };
  constexpr B b3 { { 1 }, { 2 } };
}

constexpr B &&b4 = ((1, 2), 3, 4, B { {10}, {{20}} });
static_assert(&b4 != &b2, "");



constexpr B b5 = B{ {0}, {0} };

namespace NestedNonStatic {



  struct A { int &&r; };
  struct B { A &&a; };
  constexpr B a = { A{0} };

  constexpr B b = { A(A{0}) };
}

namespace FakeInitList {
  struct init_list_3_ints { const int (&x)[3]; };
  struct init_list_2_init_list_3_ints { const init_list_3_ints (&x)[2]; };
  constexpr init_list_2_init_list_3_ints ils = { { { { 1, 2, 3 } }, { { 4, 5, 6 } } } };
}

namespace ConstAddedByReference {
  const int &r = (0);
  constexpr int n = r;

  int &&r2 = 0;
  constexpr int n2 = r2;

  struct A { constexpr operator int() const { return 0; }};
  struct B { constexpr operator const int() const { return 0; }};
  const int &ra = A();
  const int &rb = B();
  constexpr int na = ra;
  constexpr int nb = rb;

  struct C { int &&r; };
  constexpr C c1 = {1};
  constexpr int &c1r = c1.r;
  constexpr const C &c2 = {2};
  constexpr int &c2r = c2.r;
  constexpr C &&c3 = {3};
  constexpr int &c3r = c3.r;
}

}

constexpr int strcmp_ce(const char *p, const char *q) {
  return (!*p || *p != *q) ? *p - *q : strcmp_ce(p+1, q+1);
}

namespace StringLiteral {

template<typename Char>
constexpr int MangleChars(const Char *p) {
  return *p + 3 * (*p ? MangleChars(p+1) : 0);
}

static_assert(MangleChars("constexpr!") == 1768383, "");
static_assert(MangleChars(u8"constexpr!") == 1768383, "");
static_assert(MangleChars(L"constexpr!") == 1768383, "");
static_assert(MangleChars(u"constexpr!") == 1768383, "");
static_assert(MangleChars(U"constexpr!") == 1768383, "");

constexpr char c0 = "nought index"[0];
constexpr char c1 = "nice index"[10];
constexpr char c2 = "nasty index"[12];
constexpr char c3 = "negative index"[-1];
constexpr char c4 = ((char*)(int*)"no reinterpret_casts allowed")[14];

constexpr const char *p = "test" + 2;
static_assert(*p == 's', "");

constexpr const char *max_iter(const char *a, const char *b) {
  return *a < *b ? b : a;
}
constexpr const char *max_element(const char *a, const char *b) {
  return (a+1 >= b) ? a : max_iter(a, max_element(a+1, b));
}

constexpr char str[] = "the quick brown fox jumped over the lazy dog";
constexpr const char *max = max_element(begin(str), end(str));
static_assert(*max == 'z', "");
static_assert(max == str + 38, "");

static_assert(strcmp_ce("hello world", "hello world") == 0, "");
static_assert(strcmp_ce("hello world", "hello clang") > 0, "");
static_assert(strcmp_ce("constexpr", "test") < 0, "");
static_assert(strcmp_ce("", " ") < 0, "");

struct S {
  int n : "foo"[4];
};

struct T {
  char c[6];
  constexpr T() : c{"foo"} {}
};
constexpr T t;

static_assert(t.c[0] == 'f', "");
static_assert(t.c[1] == 'o', "");
static_assert(t.c[2] == 'o', "");
static_assert(t.c[3] == 0, "");
static_assert(t.c[4] == 0, "");
static_assert(t.c[5] == 0, "");
static_assert(t.c[6] == 0, "");

struct U {
  wchar_t chars[6];
  int n;
} constexpr u = { { L"test" }, 0 };
static_assert(u.chars[2] == L's', "");

struct V {
  char c[4];
  constexpr V() : c("hi!") {}
};
static_assert(V().c[1] == "i"[0], "");

namespace Parens {
  constexpr unsigned char a[] = ("foo"), b[] = {"foo"}, c[] = {("foo")},
                          d[4] = ("foo"), e[5] = {"foo"}, f[6] = {("foo")};
  static_assert(a[0] == 'f', "");
  static_assert(b[1] == 'o', "");
  static_assert(c[2] == 'o', "");
  static_assert(d[0] == 'f', "");
  static_assert(e[1] == 'o', "");
  static_assert(f[2] == 'o', "");
  static_assert(f[5] == 0, "");
  static_assert(f[6] == 0, "");
}

}

namespace Array {

template<typename Iter>
constexpr auto Sum(Iter begin, Iter end) -> decltype(+*begin) {
  return begin == end ? 0 : *begin + Sum(begin+1, end);
}

constexpr int xs[] = { 1, 2, 3, 4, 5 };
constexpr int ys[] = { 5, 4, 3, 2, 1 };
constexpr int sum_xs = Sum(begin(xs), end(xs));
static_assert(sum_xs == 15, "");

constexpr int ZipFoldR(int (*F)(int x, int y, int c), int n,
                       const int *xs, const int *ys, int c) {
  return n ? F(
               *xs,
               *ys,
               ZipFoldR(F, n-1, xs+1, ys+1, c))


           : c;
}
constexpr int MulAdd(int x, int y, int c) { return x * y + c; }
constexpr int InnerProduct = ZipFoldR(MulAdd, 5, xs, ys, 0);
static_assert(InnerProduct == 35, "");

constexpr int SubMul(int x, int y, int c) { return (x - y) * c; }
constexpr int DiffProd = ZipFoldR(SubMul, 2, xs+3, ys+3, 1);
static_assert(DiffProd == 8, "");
static_assert(ZipFoldR(SubMul, 3, xs+3, ys+3, 1), "");



constexpr const int *p = xs + 3;
constexpr int xs4 = p[1];
constexpr int xs5 = p[2];
constexpr int xs6 = p[3];
constexpr int xs0 = p[-3];
constexpr int xs_1 = p[-4];

constexpr int zs[2][2][2][2] = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
static_assert(zs[0][0][0][0] == 1, "");
static_assert(zs[1][1][1][1] == 16, "");
static_assert(zs[0][0][0][2] == 3, "");
static_assert((&zs[0][0][0][2])[-1] == 2, "");
static_assert(**(**(zs + 1) + 1) == 11, "");
static_assert(*(&(&(*(*&(&zs[2] - 1)[0] + 2 - 2))[2])[-1][-1] + 1) == 11, "");
static_assert(*(&(&(*(*&(&zs[2] - 1)[0] + 2 - 2))[2])[-1][2] - 2) == 11, "");
constexpr int err_zs_1_2_0_0 = zs[1][2][0][0];



constexpr int fail(const int &p) {
  return (&p)[64];
}
static_assert(fail(*(&(&(*(*&(&zs[2] - 1)[0] + 2 - 2))[2])[-1][2] - 2)) == 11, "");



constexpr int arr[40] = { 1, 2, 3, [8] = 4 };
constexpr int SumNonzero(const int *p) {
  return *p + (*p ? SumNonzero(p+1) : 0);
}
constexpr int CountZero(const int *p, const int *q) {
  return p == q ? 0 : (*p == 0) + CountZero(p+1, q);
}
static_assert(SumNonzero(arr) == 6, "");
static_assert(CountZero(arr, arr + 40) == 36, "");

struct ArrayElem {
  constexpr ArrayElem() : n(0) {}
  int n;
  constexpr int f() const { return n; }
};
struct ArrayRVal {
  constexpr ArrayRVal() {}
  ArrayElem elems[10];
};
static_assert(ArrayRVal().elems[3].f() == 0, "");

namespace CopyCtor {
  struct A {
    constexpr A() {}
    constexpr A(const A &) {}
  };
  struct B {
    A a;
    int arr[10];
  };
  constexpr B b{{}, {1, 2, 3, 4, 5}};
  constexpr B c = b;
  static_assert(c.arr[2] == 3, "");
  static_assert(c.arr[7] == 0, "");


  struct X { struct Y {} y; } x1;
  constexpr X x2 = x1;
}

constexpr int selfref[2][2][2] = {
  1, selfref[0][0][0] + 1,
  1, selfref[0][1][0] + 1,
  1, selfref[0][1][1] + 1 };
static_assert(selfref[0][0][0] == 1, "");
static_assert(selfref[0][0][1] == 2, "");
static_assert(selfref[0][1][0] == 1, "");
static_assert(selfref[0][1][1] == 2, "");
static_assert(selfref[1][0][0] == 1, "");
static_assert(selfref[1][0][1] == 3, "");
static_assert(selfref[1][1][0] == 0, "");
static_assert(selfref[1][1][1] == 0, "");

constexpr int badselfref[2][2][2] = {
  badselfref[1][0][0]
};

struct TrivialDefCtor { int n; };
typedef TrivialDefCtor TDCArray[2][2];
static_assert(TDCArray{}[1][1].n == 0, "");

struct NonAggregateTDC : TrivialDefCtor {};
typedef NonAggregateTDC NATDCArray[2][2];
static_assert(NATDCArray{}[1][1].n == 0, "");

}



namespace ArrayOfUnknownBound {
  extern int arr[];
  constexpr int *a = arr;
  constexpr int *b = &arr[0];
  static_assert(a == b, "");
  constexpr int *c = &arr[1];
  constexpr int *d = &a[1];
  constexpr int *e = a + 1;

  struct X {
    int a;
    int b[];
  };
  extern X x;
  constexpr int *xb = x.b;

  struct Y { int a; };
  extern Y yarr[];
  constexpr Y *p = yarr;
  constexpr int *q = &p->a;

  extern const int carr[];
  constexpr int n = carr[0];

  constexpr int local_extern[] = {1, 2, 3};
  void f() { extern const int local_extern[]; }
  static_assert(local_extern[1] == 2, "");
}

namespace DependentValues {

struct I { int n; typedef I V[10]; };
I::V x, y;
int g();
template<bool B, typename T> struct S : T {
  int k;
  void f() {
    I::V &cells = B ? x : y;
    I &i = cells[k];
    switch (i.n) {}

    constexpr int n = g();

    constexpr int m = this->g();
  }
};

extern const int n;
template<typename T> void f() {



  constexpr int k = n;
}

constexpr int n = 4;
template void f<int>();

}

namespace Class {

struct A { constexpr A(int a, int b) : k(a + b) {} int k; };
constexpr int fn(const A &a) { return a.k; }
static_assert(fn(A(4,5)) == 9, "");

struct B { int n; int m; } constexpr b = { 0, b.n };
struct C {
  constexpr C(C *this_) : m(42), n(this_->m) {}
  int m, n;
};
struct D {
  C c;
  constexpr D() : c(&c) {}
};
static_assert(D().c.n == 42, "");

struct E {
  constexpr E() : p(&p) {}
  void *p;
};
constexpr const E &e1 = E();


constexpr E e2 = E();
static_assert(e2.p == &e2.p, "");
constexpr E e3;
static_assert(e3.p == &e3.p, "");

extern const class F f;
struct F {
  constexpr F() : p(&f.p) {}
  const void *p;
};
constexpr F f;

struct G {
  struct T {
    constexpr T(T *p) : u1(), u2(p) {}
    union U1 {
      constexpr U1() {}
      int a, b = 42;
    } u1;
    union U2 {
      constexpr U2(T *p) : c(p->u1.b) {}
      int c, d;
    } u2;
  } t;
  constexpr G() : t(&t) {}
} constexpr g;

static_assert(g.t.u1.a == 42, "");
static_assert(g.t.u1.b == 42, "");
static_assert(g.t.u2.c == 42, "");
static_assert(g.t.u2.d == 42, "");

struct S {
  int a, b;
  const S *p;
  double d;
  const char *q;

  constexpr S(int n, const S *p) : a(5), b(n), p(p), d(n), q("hello") {}
};

S global(43, &global);

static_assert(S(15, &global).b == 15, "");

constexpr bool CheckS(const S &s) {
  return s.a == 5 && s.b == 27 && s.p == &global && s.d == 27. && s.q[3] == 'l';
}
static_assert(CheckS(S(27, &global)), "");

struct Arr {
  char arr[3];
  constexpr Arr() : arr{'x', 'y', 'z'} {}
};
constexpr int hash(Arr &&a) {
  return a.arr[0] + a.arr[1] * 0x100 + a.arr[2] * 0x10000;
}
constexpr int k = hash(Arr());
static_assert(k == 0x007a7978, "");


struct AggregateInit {
  const char &c;
  int n;
  double d;
  int arr[5];
  void *p;
};

constexpr AggregateInit agg1 = { "hello"[0] };

static_assert(strcmp_ce(&agg1.c, "hello") == 0, "");
static_assert(agg1.n == 0, "");
static_assert(agg1.d == 0.0, "");
static_assert(agg1.arr[-1] == 0, "");
static_assert(agg1.arr[0] == 0, "");
static_assert(agg1.arr[4] == 0, "");
static_assert(agg1.arr[5] == 0, "");
static_assert(agg1.p == nullptr, "");

static constexpr const unsigned char uc[] = { "foo" };
static_assert(uc[0] == 'f', "");
static_assert(uc[3] == 0, "");

namespace SimpleDerivedClass {

struct B {
  constexpr B(int n) : a(n) {}
  int a;
};
struct D : B {
  constexpr D(int n) : B(n) {}
};
constexpr D d(3);
static_assert(d.a == 3, "");

}

struct Bottom { constexpr Bottom() {} };
struct Base : Bottom {
  constexpr Base(int a = 42, const char *b = "test") : a(a), b(b) {}
  int a;
  const char *b;
};
struct Base2 : Bottom {
  constexpr Base2(const int &r) : r(r) {}
  int q = 123;
  const int &r;
};
struct Derived : Base, Base2 {
  constexpr Derived() : Base(76), Base2(a) {}
  int c = r + b[1];
};

constexpr bool operator==(const Base &a, const Base &b) {
  return a.a == b.a && strcmp_ce(a.b, b.b) == 0;
}

constexpr Base base;
constexpr Base base2(76);
constexpr Derived derived;
static_assert(derived.a == 76, "");
static_assert(derived.b[2] == 's', "");
static_assert(derived.c == 76 + 'e', "");
static_assert(derived.q == 123, "");
static_assert(derived.r == 76, "");
static_assert(&derived.r == &derived.a, "");

static_assert(!(derived == base), "");
static_assert(derived == base2, "");

constexpr Bottom &bot1 = (Base&)derived;
constexpr Bottom &bot2 = (Base2&)derived;
static_assert(&bot1 != &bot2, "");

constexpr Bottom *pb1 = (Base*)&derived;
constexpr Bottom *pb2 = (Base2*)&derived;
static_assert(&pb1 != &pb2, "");
static_assert(pb1 == &bot1, "");
static_assert(pb2 == &bot2, "");

constexpr Base2 &fail = (Base2&)bot1;
constexpr Base &fail2 = (Base&)*pb2;
constexpr Base2 &ok2 = (Base2&)bot2;
static_assert(&ok2 == &derived, "");

constexpr Base2 *pfail = (Base2*)pb1;
constexpr Base *pfail2 = (Base*)&bot2;
constexpr Base2 *pok2 = (Base2*)pb2;
static_assert(pok2 == &derived, "");
static_assert(&ok2 == pok2, "");
static_assert((Base2*)(Derived*)(Base*)pb1 == pok2, "");
static_assert((Derived*)(Base*)pb1 == (Derived*)pok2, "");



constexpr Base *nullB = 42 - 6 * 7;
constexpr Base *nullB1 = 0;
static_assert((Bottom*)nullB == 0, "");
static_assert((Derived*)nullB1 == 0, "");
static_assert((void*)(Bottom*)nullB1 == (void*)(Derived*)nullB1, "");
Base *nullB2 = '\0';
Base *nullB3 = (0);
Base *nullB4 = false;
Base *nullB5 = ((0ULL));
Base *nullB6 = 0.;
enum Null { kNull };
Base *nullB7 = kNull;
static_assert(nullB1 == (1 - 1), "");



namespace ConversionOperators {

struct T {
  constexpr T(int n) : k(5*n - 3) {}
  constexpr operator int() const { return k; }
  int k;
};

struct S {
  constexpr S(int n) : k(2*n + 1) {}
  constexpr operator int() const { return k; }
  constexpr operator T() const { return T(k); }
  int k;
};

constexpr bool check(T a, T b) { return a == b.k; }

static_assert(S(5) == 11, "");
static_assert(check(S(5), 11), "");

namespace PR14171 {

struct X {
  constexpr (operator int)() const { return 0; }
};
static_assert(X() == 0, "");

}

}

struct This {
  constexpr int f() const { return 0; }
  static constexpr int g() { return 0; }
  void h() {
    constexpr int x = f();

    constexpr int y = this->f();

    constexpr int z = g();
    static_assert(z == 0, "");
  }
};

}

namespace Temporaries {

struct S {
  constexpr S() {}
  constexpr int f() const;
  constexpr int g() const;
};
struct T : S {
  constexpr T(int n) : S(), n(n) {}
  int n;
};
constexpr int S::f() const {
  return static_cast<const T*>(this)->n;
}
constexpr int S::g() const {

  return this->*(int(S::*))&T::n;
}



static_assert(S().f(), "");
static_assert(S().g(), "");
constexpr S sobj;
constexpr const S& slref = sobj;
constexpr const S&& srref = S();
constexpr const S *sptr = &sobj;
static_assert(sobj.f(), "");

static_assert(sptr->f(), "");

static_assert(slref.f(), "");

static_assert(srref.f(), "");

static_assert(T(3).f() == 3, "");
static_assert(T(4).g() == 4, "");

constexpr int f(const S &s) {
  return static_cast<const T&>(s).n;
}
constexpr int n = f(T(5));
static_assert(f(T(5)) == 5, "");

constexpr bool b(int n) { return &n; }
static_assert(b(0), "");

struct NonLiteral {
  NonLiteral();
  int f();
};
constexpr int k = NonLiteral().f();



}

namespace Union {

union U {
  int a;
  int b;
};

constexpr U u[4] = { { .a = 0 }, { .b = 1 }, { .a = 2 }, { .b = 3 } };
static_assert(u[0].a == 0, "");
static_assert(u[0].b, "");
static_assert(u[1].b == 1, "");
static_assert((&u[1].b)[1] == 2, "");
static_assert(*(&(u[1].b) + 1 + 1) == 3, "");
static_assert((&(u[1]) + 1 + 1)->b == 3, "");

constexpr U v = {};
static_assert(v.a == 0, "");

union Empty {};
constexpr Empty e = {};


constexpr U x = {42};
constexpr U y = x;
static_assert(y.a == 42, "");
static_assert(y.b == 42, "");

}

namespace MemberPointer {
  struct A {
    constexpr A(int n) : n(n) {}
    int n;
    constexpr int f() const { return n + 3; }
  };
  constexpr A a(7);
  static_assert(A(5).*&A::n == 5, "");
  static_assert((&a)->*&A::n == 7, "");
  static_assert((A(8).*&A::f)() == 11, "");
  static_assert(((&a)->*&A::f)() == 10, "");

  struct B : A {
    constexpr B(int n, int m) : A(n), m(m) {}
    int m;
    constexpr int g() const { return n + m + 1; }
  };
  constexpr B b(9, 13);
  static_assert(B(4, 11).*&A::n == 4, "");
  static_assert(B(4, 11).*&B::m == 11, "");
  static_assert(B(4, 11).*(int(A::*))&B::m == 11, "");
  static_assert((&b)->*&A::n == 9, "");
  static_assert((&b)->*&B::m == 13, "");
  static_assert((&b)->*(int(A::*))&B::m == 13, "");
  static_assert((B(4, 11).*&A::f)() == 7, "");
  static_assert((B(4, 11).*&B::g)() == 16, "");
  static_assert((B(4, 11).*(int(A::*)()const)&B::g)() == 16, "");
  static_assert(((&b)->*&A::f)() == 12, "");
  static_assert(((&b)->*&B::g)() == 23, "");
  static_assert(((&b)->*(int(A::*)()const)&B::g)() == 23, "");

  struct S {
    constexpr S(int m, int n, int (S::*pf)() const, int S::*pn) :
      m(m), n(n), pf(pf), pn(pn) {}
    constexpr S() : m(), n(), pf(&S::f), pn(&S::n) {}

    constexpr int f() const { return this->*pn; }
    virtual int g() const;

    int m, n;
    int (S::*pf)() const;
    int S::*pn;
  };

  constexpr int S::*pm = &S::m;
  constexpr int S::*pn = &S::n;
  constexpr int (S::*pf)() const = &S::f;
  constexpr int (S::*pg)() const = &S::g;

  constexpr S s(2, 5, &S::f, &S::m);

  static_assert((s.*&S::f)() == 2, "");
  static_assert((s.*s.pf)() == 2, "");

  static_assert(pf == &S::f, "");
  static_assert(pf == s.*&S::pf, "");
  static_assert(pm == &S::m, "");
  static_assert(pm != pn, "");
  static_assert(s.pn != pn, "");
  static_assert(s.pn == pm, "");
  static_assert(pg != nullptr, "");
  static_assert(pf != nullptr, "");
  static_assert((int S::*)nullptr == nullptr, "");
  static_assert(pg == pg, "");
  static_assert(pf != pg, "");

  template<int n> struct T : T<n-1> {};
  template<> struct T<0> { int n; };
  template<> struct T<30> : T<29> { int m; };

  T<17> t17;
  T<30> t30;

  constexpr int (T<10>::*deepn) = &T<0>::n;
  static_assert(&(t17.*deepn) == &t17.n, "");
  static_assert(deepn == &T<2>::n, "");

  constexpr int (T<15>::*deepm) = (int(T<10>::*))&T<30>::m;
  constexpr int *pbad = &(t17.*deepm);
  static_assert(&(t30.*deepm) == &t30.m, "");
  static_assert(deepm == &T<50>::m, "");
  static_assert(deepm != deepn, "");

  constexpr T<5> *p17_5 = &t17;
  constexpr T<13> *p17_13 = (T<13>*)p17_5;
  constexpr T<23> *p17_23 = (T<23>*)p17_13;
  static_assert(&(p17_5->*(int(T<3>::*))deepn) == &t17.n, "");
  static_assert(&(p17_13->*deepn) == &t17.n, "");
  constexpr int *pbad2 = &(p17_13->*(int(T<9>::*))deepm);

  constexpr T<5> *p30_5 = &t30;
  constexpr T<23> *p30_23 = (T<23>*)p30_5;
  constexpr T<13> *p30_13 = p30_23;
  static_assert(&(p30_5->*(int(T<3>::*))deepn) == &t30.n, "");
  static_assert(&(p30_13->*deepn) == &t30.n, "");
  static_assert(&(p30_23->*deepn) == &t30.n, "");
  static_assert(&(p30_5->*(int(T<2>::*))deepm) == &t30.m, "");
  static_assert(&(((T<17>*)p30_13)->*deepm) == &t30.m, "");
  static_assert(&(p30_23->*deepm) == &t30.m, "");

  struct Base { int n; };
  template<int N> struct Mid : Base {};
  struct Derived : Mid<0>, Mid<1> {};
  static_assert(&Mid<0>::n == &Mid<1>::n, "");
  static_assert((int Derived::*)(int Mid<0>::*)&Mid<0>::n !=
                (int Derived::*)(int Mid<1>::*)&Mid<1>::n, "");
  static_assert(&Mid<0>::n == (int Mid<0>::*)&Base::n, "");

  constexpr int apply(const A &a, int (A::*f)() const) {
    return (a.*f)();
  }
  static_assert(apply(A(2), &A::f) == 5, "");
}

namespace ArrayBaseDerived {

  struct Base {
    constexpr Base() {}
    int n = 0;
  };
  struct Derived : Base {
    constexpr Derived() {}
    constexpr const int *f() const { return &n; }
  };

  constexpr Derived a[10];
  constexpr Derived *pd3 = const_cast<Derived*>(&a[3]);
  constexpr Base *pb3 = const_cast<Derived*>(&a[3]);
  static_assert(pb3 == pd3, "");


  constexpr Base *pb4 = pb3 + 1;
  constexpr int pb4n = pb4->n;
  constexpr Base *err_pb5 = pb3 + 2;
  constexpr int err_pb5n = err_pb5->n;
  constexpr Base *err_pb2 = pb3 - 1;
  constexpr int err_pb2n = err_pb2->n;
  constexpr Base *pb3a = pb4 - 1;


  constexpr Derived *err_pd4 = (Derived*)pb4;
  constexpr Derived *pd3a = (Derived*)pb3a;
  constexpr int pd3n = pd3a->n;


  constexpr Derived *pd6 = pd3a + 3;
  static_assert(pd6 == &a[6], "");
  constexpr Derived *pd9 = pd6 + 3;
  constexpr Derived *pd10 = pd6 + 4;
  constexpr int pd9n = pd9->n;
  constexpr int err_pd10n = pd10->n;
  constexpr int pd0n = pd10[-10].n;
  constexpr int err_pdminus1n = pd10[-11].n;

  constexpr Base *pb9 = pd9;
  constexpr const int *(Base::*pfb)() const =
      static_cast<const int *(Base::*)() const>(&Derived::f);
  static_assert((pb9->*pfb)() == &a[9].n, "");
}

namespace Complex {

class complex {
  int re, im;
public:
  constexpr complex(int re = 0, int im = 0) : re(re), im(im) {}
  constexpr complex(const complex &o) : re(o.re), im(o.im) {}
  constexpr complex operator-() const { return complex(-re, -im); }
  friend constexpr complex operator+(const complex &l, const complex &r) {
    return complex(l.re + r.re, l.im + r.im);
  }
  friend constexpr complex operator-(const complex &l, const complex &r) {
    return l + -r;
  }
  friend constexpr complex operator*(const complex &l, const complex &r) {
    return complex(l.re * r.re - l.im * r.im, l.re * r.im + l.im * r.re);
  }
  friend constexpr bool operator==(const complex &l, const complex &r) {
    return l.re == r.re && l.im == r.im;
  }
  constexpr bool operator!=(const complex &r) const {
    return re != r.re || im != r.im;
  }
  constexpr int real() const { return re; }
  constexpr int imag() const { return im; }
};

constexpr complex i = complex(0, 1);
constexpr complex k = (3 + 4*i) * (6 - 4*i);
static_assert(complex(1,0).real() == 1, "");
static_assert(complex(1,0).imag() == 0, "");
static_assert(((complex)1).imag() == 0, "");
static_assert(k.real() == 34, "");
static_assert(k.imag() == 12, "");
static_assert(k - 34 == 12*i, "");
static_assert((complex)1 == complex(1), "");
static_assert((complex)1 != complex(0, 1), "");
static_assert(complex(1) == complex(1), "");
static_assert(complex(1) != complex(0, 1), "");
constexpr complex makeComplex(int re, int im) { return complex(re, im); }
static_assert(makeComplex(1,0) == complex(1), "");
static_assert(makeComplex(1,0) != complex(0, 1), "");

class complex_wrap : public complex {
public:
  constexpr complex_wrap(int re, int im = 0) : complex(re, im) {}
  constexpr complex_wrap(const complex_wrap &o) : complex(o) {}
};

static_assert((complex_wrap)1 == complex(1), "");
static_assert((complex)1 != complex_wrap(0, 1), "");
static_assert(complex(1) == complex_wrap(1), "");
static_assert(complex_wrap(1) != complex(0, 1), "");
constexpr complex_wrap makeComplexWrap(int re, int im) {
  return complex_wrap(re, im);
}
static_assert(makeComplexWrap(1,0) == complex(1), "");
static_assert(makeComplexWrap(1,0) != complex(0, 1), "");

constexpr auto GH55390 = 1 / 65536j;


}

namespace PR11595 {
  struct A { constexpr bool operator==(int x) const { return true; } };
  struct B { B(); A& x; };
  static_assert(B().x == 3, "");



  constexpr bool f(int k) {
    return B().x == k;
  }
}

namespace ExprWithCleanups {
  struct A { A(); ~A(); int get(); };
  constexpr int get(bool FromA) { return FromA ? A().get() : 1; }
  constexpr int n = get(false);
}

namespace Volatile {

volatile constexpr int n1 = 0;
volatile const int n2 = 0;
int n3 = 37;

constexpr int m1 = n1;
constexpr int m2 = n2;
constexpr int m1b = const_cast<const int&>(n1);
constexpr int m2b = const_cast<const int&>(n2);

struct T { int n; };
const T t = { 42 };

constexpr int f(volatile int &&r) {
  return r;
}
constexpr int g(volatile int &&r) {
  return const_cast<int&>(r);
}
struct S {
  int j : f(0);
  int k : g(0);
  int l : n3;
  int m : t.n;
};

}

namespace ExternConstexpr {
  extern constexpr int n = 0;
  extern constexpr int m;
  void f() {
    extern constexpr int i;
    constexpr int j = 0;
    constexpr int k;
  }

  extern const int q;
  constexpr int g() { return q; }
  constexpr int q = g();

  extern int r;
  constexpr int h() { return r; }

  struct S { int n; };
  extern const S s;
  constexpr int x() { return s.n; }
  constexpr S s = {x()};
}

namespace ComplexConstexpr {
  constexpr _Complex float test1 = {};
  constexpr _Complex float test2 = {1};
  constexpr _Complex double test3 = {1,2};
  constexpr _Complex int test4 = {4};
  constexpr _Complex int test5 = 4;
  constexpr _Complex int test6 = {5,6};
  typedef _Complex float fcomplex;
  constexpr fcomplex test7 = fcomplex();

  constexpr const double &t2r = __real test3;
  constexpr const double &t2i = __imag test3;
  static_assert(&t2r + 1 == &t2i, "");
  static_assert(t2r == 1.0, "");
  static_assert(t2i == 2.0, "");
  constexpr const double *t2p = &t2r;
  static_assert(t2p[-1] == 0.0, "");
  static_assert(t2p[0] == 1.0, "");
  static_assert(t2p[1] == 2.0, "");
  static_assert(t2p[2] == 0.0, "");
  static_assert(t2p[3] == 0.0, "");
  constexpr _Complex float *p = 0;
  constexpr float pr = __real *p;
  constexpr float pi = __imag *p;
  constexpr const _Complex double *q = &test3 + 1;
  constexpr double qr = __real *q;
  constexpr double qi = __imag *q;

  static_assert(__real test6 == 5, "");
  static_assert(__imag test6 == 6, "");
  static_assert(&__imag test6 == &__real test6 + 1, "");
}



namespace Atomic {
  constexpr _Atomic int n = 3;

  struct S { _Atomic(double) d; };
  constexpr S s = { 0.5 };
  constexpr double d1 = s.d;
  constexpr double d2 = n;
  constexpr _Atomic double d3 = n;

  constexpr _Atomic(int) n2 = d3;
  static_assert(d1 == 0.5, "");
  static_assert(d3 == 3.0, "");

  namespace PR16056 {
    struct TestVar {
      _Atomic(int) value;
      constexpr TestVar(int value) : value(value) {}
    };
    constexpr TestVar testVar{-1};
    static_assert(testVar.value == -1, "");
  }

  namespace PR32034 {
    struct A {};
    struct B { _Atomic(A) a; };
    constexpr int n = (B(), B(), 0);

    struct C { constexpr C() {} void *self = this; };
    constexpr _Atomic(C) c = C();
  }
}

namespace InstantiateCaseStmt {
  template<int x> constexpr int f() { return x; }
  template<int x> int g(int c) { switch(c) { case f<x>(): return 1; } return 0; }
  int gg(int c) { return g<4>(c); }
}

namespace ConvertedConstantExpr {
  extern int &m;
  extern int &n;

  constexpr int k = 4;
  int &m = const_cast<int&>(k);



  enum class E {
    em = m,
    en = n,
    eo = (m +
          n
          ),
    eq = reinterpret_cast<long>((int*)0)
  };
}

namespace IndirectField {
  struct S {
    struct {
      union {
        struct {
          int a;
          int b;
        };
        int c;
      };
      int d;
    };
    union {
      int e;
      int f;
    };
    constexpr S(int a, int b, int d, int e) : a(a), b(b), d(d), e(e) {}
    constexpr S(int c, int d, int f) : c(c), d(d), f(f) {}
  };

  constexpr S s1(1, 2, 3, 4);
  constexpr S s2(5, 6, 7);



  static_assert(s1.a == 1, "");
  static_assert(s1.b == 2, "");
  static_assert(s1.c == 0, "");
  static_assert(s1.d == 3, "");
  static_assert(s1.e == 4, "");
  static_assert(s1.f == 0, "");

  static_assert(s2.a == 0, "");
  static_assert(s2.b == 0, "");
  static_assert(s2.c == 5, "");
  static_assert(s2.d == 6, "");
  static_assert(s2.e == 0, "");
  static_assert(s2.f == 7, "");
}


namespace MutableMembers {
  struct MM {
    mutable int n;
  } constexpr mm = { 4 };
  constexpr int mmn = mm.n;
  int x = (mm.n = 1, 3);
  constexpr int mmn2 = mm.n;


  template<int n> struct Id { int k = n; };
  int f() {
    constexpr MM m = { 0 };
    ++m.n;
    return Id<m.n>().k;
  }

  struct A { int n; };
  struct B { mutable A a; };
  struct C { B b; };
  constexpr C c[3] = {};
  constexpr int k = c[1].b.a.n;

  struct D { int x; mutable int y; };
  constexpr D d1 = { 1, 2 };
  int l = ++d1.y;
  constexpr D d2 = d1;

  struct E {
    union {
      int a;
      mutable int b;
    };
  };
  constexpr E e1 = {{1}};
  constexpr E e2 = e1;

  struct F {
    union U { };
    mutable U u;
    struct X { };
    mutable X x;
    struct Y : X { X x; U u; };
    mutable Y y;
    int n;
  };

  constexpr F f1 = {};
  constexpr F f2 = f1;

  struct G {
    struct X {};
    union U { X a; };
    mutable U u;
  };
  constexpr G g1 = {};
  constexpr G g2 = g1;
  constexpr G::U gu1 = {};
  constexpr G::U gu2 = gu1;

  union H {
    mutable G::X gx;
  };
  constexpr H h1 = {};
  constexpr H h2 = h1;
}

namespace Fold {

  constexpr int n = (long)(char*)123;
  constexpr int m = (__builtin_constant_p((long)(char*)123) ? ((long)(char*)123) : ((long)(char*)123));
  static_assert(m == 123, "");

}

namespace DR1454 {

constexpr const int &f(const int &n) { return n; }
constexpr int k1 = f(0);

struct Wrap {
  const int &value;
};
constexpr const Wrap &g(const Wrap &w) { return w; }
constexpr int k2 = g({0}).value;



constexpr const int &i = 1;
constexpr const int j = i;
static_assert(j == 1, "");




constexpr int &&k = 1;
constexpr const int l = k;

void f() {


  constexpr const int &i = 1;
}

}

namespace RecursiveOpaqueExpr {
  template<typename Iter>
  constexpr auto LastNonzero(Iter p, Iter q) -> decltype(+*p) {
    return p != q ? (LastNonzero(p+1, q) ?: *p) : 0;
  }

  constexpr int arr1[] = { 1, 0, 0, 3, 0, 2, 0, 4, 0, 0 };
  static_assert(LastNonzero(begin(arr1), end(arr1)) == 4, "");

  constexpr int arr2[] = { 1, 0, 0, 3, 0, 2, 0, 4, 0, 5 };
  static_assert(LastNonzero(begin(arr2), end(arr2)) == 5, "");

  constexpr int arr3[] = {
    1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0,
    1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0,
    1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0,
    1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0,
    1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0,
    1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0,
    1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0,
    2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
  static_assert(LastNonzero(begin(arr3), end(arr3)) == 2, "");
}

namespace VLASizeof {

  void f(int k) {
    int arr[k];
    constexpr int n = 1 +
        sizeof(arr)
        * 3;
  }
}

namespace CompoundLiteral {


  constexpr int *p = (int*)(int[1]){3};
  static_assert(*p == 3, "");


  static_assert((int[2]){1, 2}[1] == 2, "");





  struct X { int a[2]; };
  constexpr int *n = (X){1, 2}.a;




  void f() {
    static constexpr int *p = (int*)(int[1]){3};



    static_assert((int[2]){1, 2}[1] == 2, "");
  }
}

namespace Vector {
  typedef int __attribute__((vector_size(16))) VI4;
  constexpr VI4 f(int n) {
    return VI4 { n * 3, n + 4, n - 5, n / 6 };
  }
  constexpr auto v1 = f(10);

  typedef double __attribute__((vector_size(32))) VD4;
  constexpr VD4 g(int n) {
    return (VD4) { n / 2.0, n + 1.5, n - 5.4, n * 0.9 };
  }
  constexpr auto v2 = g(4);
}


namespace InvalidClasses {
  void test0() {
    struct X;
    struct Y { bool b; X x; };
    Y y;
    auto& b = y.b;
  }
}

namespace NamespaceAlias {
  constexpr int f() {
    namespace NS = NamespaceAlias;
    return &NS::f != nullptr;
  }
}


namespace ImplicitConstexpr {
  struct Q { Q() = default; Q(const Q&) = default; Q(Q&&) = default; ~Q(); };
  struct R { constexpr R() noexcept; constexpr R(const R&) noexcept; constexpr R(R&&) noexcept; ~R() noexcept; };
  struct S { R r; };
  struct T { T(const T&) noexcept; T(T &&) noexcept; ~T() noexcept; };
  struct U { T t; };
  static_assert(!__is_literal_type(Q), "");
  static_assert(!__is_literal_type(R), "");
  static_assert(!__is_literal_type(S), "");
  static_assert(!__is_literal_type(T), "");
  static_assert(!__is_literal_type(U), "");
  struct Test {
    friend Q::Q() noexcept;
    friend Q::Q(Q&&) noexcept;
    friend Q::Q(const Q&) noexcept;
    friend S::S() noexcept;
    friend S::S(S&&) noexcept;
    friend S::S(const S&) noexcept;
    friend constexpr U::U() noexcept;
    friend constexpr U::U(U&&) noexcept;
    friend constexpr U::U(const U&) noexcept;
  };
}



namespace PR12826 {
  struct Foo {};
  constexpr Foo id(Foo x) { return x; }
  constexpr Foo res(id(Foo()));
}

namespace PR13273 {
  struct U {
    int t;
    U() = default;
  };

  struct S : U {
    S() = default;
  };




  static_assert(S{}.t == 0, "");
}

namespace PR12670 {
  struct S {
    constexpr S(int a0) : m(a0) {}
    constexpr S() : m(6) {}
    int m;
  };
  constexpr S x[3] = { {4}, 5 };
  static_assert(x[0].m == 4, "");
  static_assert(x[1].m == 5, "");
  static_assert(x[2].m == 6, "");
}




namespace ConditionalLValToRVal {
  struct A {
    constexpr A(int a) : v(a) {}
    int v;
  };

  constexpr A f(const A &a) {
    return a.v == 0 ? throw a : a;
  }

  constexpr A a(4);
  static_assert(f(a).v == 4, "");
}

namespace TLS {
  __thread int n;
  int m;

  constexpr bool b = &n == &n;

  constexpr int *p = &n;

  constexpr int *f() { return &n; }
  constexpr int *q = f();
  constexpr bool c = f() == f();

  constexpr int *g() { return &m; }
  constexpr int *r = g();
}

namespace Void {
  constexpr void f() { return; }

  void assert_failed(const char *msg, const char *file, int line);

  template<typename T, size_t S>
  constexpr T get(T (&a)[S], size_t k) {
    return ((k > 0 && k < S) ? static_cast<void>(0) : assert_failed("k > 0 && k < S", "SemaCXX/constant-expression-cxx11.cpp", 1826)), a[k];
  }

  template int get(int (&a)[4], size_t);
  constexpr int arr[] = { 4, 1, 2, 3, 4 };
  static_assert(get(arr, 1) == 1, "");
  static_assert(get(arr, 4) == 4, "");
  static_assert(get(arr, 0) == 4, "");

}

namespace std { struct type_info; }

namespace TypeId {
  struct A { virtual ~A(); };
  A f();
  A &g();
  constexpr auto &x = typeid(f());
  constexpr auto &y = typeid(g());



}

namespace PR14203 {
  struct duration {
    constexpr duration() {}
    constexpr operator int() const { return 0; }
  };

  template<typename T> void f() {
    constexpr duration d = duration();
  }
  int n = sizeof(short{duration(duration())});
}

namespace ArrayEltInit {
  struct A {
    constexpr A() : p(&p) {}
    void *p;
  };
  constexpr A a[10];
  static_assert(a[0].p == &a[0].p, "");
  static_assert(a[9].p == &a[9].p, "");
  static_assert(a[0].p != &a[9].p, "");
  static_assert(a[9].p != &a[0].p, "");

  constexpr A b[10] = {};
  static_assert(b[0].p == &b[0].p, "");
  static_assert(b[9].p == &b[9].p, "");
  static_assert(b[0].p != &b[9].p, "");
  static_assert(b[9].p != &b[0].p, "");
}

namespace PR15884 {
  struct S {};
  constexpr S f() { return {}; }
  constexpr S *p = &f();




}

namespace AfterError {
  constexpr int error() {
    return foobar;
  }
  constexpr int k = error();

}

namespace std {
  typedef decltype(sizeof(int)) size_t;

  template <class _E>
  class initializer_list
  {
    const _E* __begin_;
    size_t __size_;

    constexpr initializer_list(const _E* __b, size_t __s)
      : __begin_(__b),
        __size_(__s)
    {}

  public:
    typedef _E value_type;
    typedef const _E& reference;
    typedef const _E& const_reference;
    typedef size_t size_type;

    typedef const _E* iterator;
    typedef const _E* const_iterator;

    constexpr initializer_list() : __begin_(nullptr), __size_(0) {}

    constexpr size_t size() const {return __size_;}
    constexpr const _E* begin() const {return __begin_;}
    constexpr const _E* end() const {return __begin_ + __size_;}
  };
}

namespace InitializerList {
  constexpr int sum(const int *b, const int *e) {
    return b != e ? *b + sum(b+1, e) : 0;
  }
  constexpr int sum(std::initializer_list<int> ints) {
    return sum(ints.begin(), ints.end());
  }
  static_assert(sum({1, 2, 3, 4, 5}) == 15, "");

  static_assert(*std::initializer_list<int>{1, 2, 3}.begin() == 1, "");
  static_assert(std::initializer_list<int>{1, 2, 3}.begin()[2] == 3, "");

  namespace DR2126 {
    constexpr std::initializer_list<float> il = {1.0, 2.0, 3.0};
    static_assert(il.begin()[1] == 2.0, "");
  }
}

namespace StmtExpr {
  struct A { int k; };
  void f() {
    static_assert(({ const int x = 5; x * 3; }) == 15, "");
    constexpr auto a = ({ A(); });
  }
  constexpr int g(int k) {
    return ({
      const int x = k;
      x * x;
    });
  }
  static_assert(g(123) == 15129, "");
  constexpr int h() {
    return ({
      return 0;
      1;
    });
  }
}

namespace VirtualFromBase {
  struct S1 {
    virtual int f() const;
  };
  struct S2 {
    virtual int f();
  };
  template <typename T> struct X : T {
    constexpr X() {}
    double d = 0.0;
    constexpr int f() { return sizeof(T); }
  };


  constexpr X<X<S1>> xxs1;
  constexpr X<S1> *p = const_cast<X<X<S1>>*>(&xxs1);
  static_assert(p->f() == sizeof(X<S1>), "");






  constexpr X<X<S2>> xxs2;
  constexpr X<S2> *q = const_cast<X<X<S2>>*>(&xxs2);
  static_assert(q->f() == sizeof(S2), "");

}

namespace ConstexprConstructorRecovery {
  class X {
  public:
      enum E : short {
          headers = 0x1,
          middlefile = 0x2,
          choices = 0x4
      };
      constexpr X() noexcept {};
  protected:
      E val{0};
  };

  constexpr X x{};
}

namespace Lifetime {
  void f() {
    constexpr int &n = n;

    constexpr int m = m;
  }

  constexpr int &get(int &&n) { return n; }

  constexpr int &&get_rv(int &&n) { return static_cast<int&&>(n); }
  struct S {
    int &&r;
    int &s;
    int t;
    constexpr S() : r(get_rv(0)), s(get(0)), t(r) {}
    constexpr S(int) : r(get_rv(0)), s(get(0)), t(s) {}
  };
  constexpr int k1 = S().t;
  constexpr int k2 = S(0).t;

  struct Q {
    int n = 0;
    constexpr int f() const { return 0; }
  };
  constexpr Q *out_of_lifetime(Q q) { return &q; }
  constexpr int k3 = out_of_lifetime({})->n;
  constexpr int k4 = out_of_lifetime({})->f();

  constexpr int null = ((Q*)nullptr)->f();

  Q q;
  Q qa[3];
  constexpr int pte0 = (&q)[0].f();
  constexpr int pte1 = (&q)[1].f();
  constexpr int pte2 = qa[2].f();
  constexpr int pte3 = qa[3].f();

  constexpr Q cq;
  constexpr Q cqa[3];
  constexpr int cpte0 = (&cq)[0].f();
  constexpr int cpte1 = (&cq)[1].f();
  constexpr int cpte2 = cqa[2].f();
  constexpr int cpte3 = cqa[3].f();



  union U {
    int n;
    Q q;
  };
  U u1 = {0};
  constexpr U u2 = {0};
  constexpr int union_member1 = u1.q.f();
  constexpr int union_member2 = u2.q.f();

  struct R {
    struct Inner { constexpr int f() const { return 0; } };
    int a = b.f();
    Inner b;
  };
  constexpr R r;
  void rf() {
    constexpr R r;
  }
}

namespace Bitfields {
  struct A {
    bool b : 1;
    unsigned u : 5;
    int n : 5;
    bool b2 : 3;
    unsigned u2 : 74;
    int n2 : 81;
  };

  constexpr A a = { false, 33, 31, false, 0xffffffff, 0x7fffffff };
  static_assert(a.b == 0 && a.u == 1 && a.n == -1 && a.b2 == 0 &&
                a.u2 + 1 == 0 && a.n2 == 0x7fffffff,
                "bad truncation of bitfield values");

  struct B {
    int n : 3;
    constexpr B(int k) : n(k) {}
  };
  static_assert(B(3).n == 3, "");
  static_assert(B(4).n == -4, "");
  static_assert(B(7).n == -1, "");
  static_assert(B(8).n == 0, "");
  static_assert(B(-1).n == -1, "");
  static_assert(B(-8889).n == -1, "");

  namespace PR16755 {
    struct X {
      int x : 1;
      constexpr static int f(int x) {
        return X{x}.x;
      }
    };
    static_assert(X::f(3) == -1, "3 should truncate to -1");
    static_assert(X::f(1) == -1, "1 should truncate to -1");
  }

  struct HasUnnamedBitfield {
    unsigned a;
    unsigned : 20;
    unsigned b;

    constexpr HasUnnamedBitfield() : a(), b() {}
    constexpr HasUnnamedBitfield(unsigned a, unsigned b) : a(a), b(b) {}
  };

  void testUnnamedBitfield() {
    const HasUnnamedBitfield zero{};
    int a = 1 / zero.b;
    const HasUnnamedBitfield oneZero{1, 0};
    int b = 1 / oneZero.b;
  }

  union UnionWithUnnamedBitfield {
    int : 3;
    int n;
  };
  static_assert(UnionWithUnnamedBitfield().n == 0, "");
  static_assert(UnionWithUnnamedBitfield{}.n == 0, "");
  static_assert(UnionWithUnnamedBitfield{1}.n == 1, "");
}

namespace ZeroSizeTypes {
  constexpr int (*p1)[0] = 0, (*p2)[0] = 0;
  constexpr int k = p2 - p1;



  int arr[5][0];
  constexpr int f() {
    return &arr[3] - &arr[0];
  }
}

namespace BadDefaultInit {
  template<int N> struct X { static const int n = N; };

  struct A {
    int k =
        X<A().k>::n;
  };

  struct B {
    constexpr B(
        int k = X<B().k>::n) :
      k(k) {}
    int k;
  };
}

namespace NeverConstantTwoWays {



  constexpr int f(int n) {
    return (int *)(long)&n == &n ?
        1 / 0 :
        0;
  }

  constexpr int n =
      (int *)(long)&n == &n ?
        1 / 0 :
        0;
}

namespace PR17800 {
  struct A {
    constexpr int operator()() const { return 0; }
  };
  template <typename ...T> constexpr int sink(T ...) {
    return 0;
  }
  template <int ...N> constexpr int run() {
    return sink(A()() + N ...);
  }
  constexpr int k = run<1, 2, 3>();
}

namespace BuiltinStrlen {
  constexpr const char *a = "foo\0quux";
  constexpr char b[] = "foo\0quux";
  constexpr int f() { return 'u'; }
  constexpr char c[] = { 'f', 'o', 'o', 0, 'q', f(), 'u', 'x', 0 };

  static_assert(__builtin_strlen("foo") == 3, "");
  static_assert(__builtin_strlen("foo\0quux") == 3, "");
  static_assert(__builtin_strlen("foo\0quux" + 4) == 4, "");
  static_assert(__builtin_strlen("foo") + 1 + "foo" == "foo", "");


  constexpr bool check(const char *p) {
    return __builtin_strlen(p) == 3 &&
           __builtin_strlen(p + 1) == 2 &&
           __builtin_strlen(p + 2) == 1 &&
           __builtin_strlen(p + 3) == 0 &&
           __builtin_strlen(p + 4) == 4 &&
           __builtin_strlen(p + 5) == 3 &&
           __builtin_strlen(p + 6) == 2 &&
           __builtin_strlen(p + 7) == 1 &&
           __builtin_strlen(p + 8) == 0;
  }

  static_assert(check(a), "");
  static_assert(check(b), "");
  static_assert(check(c), "");

  constexpr int over1 = __builtin_strlen(a + 9);
  constexpr int over2 = __builtin_strlen(b + 9);
  constexpr int over3 = __builtin_strlen(c + 9);

  constexpr int under1 = __builtin_strlen(a - 1);
  constexpr int under2 = __builtin_strlen(b - 1);
  constexpr int under3 = __builtin_strlen(c - 1);


  constexpr char d[] = { 'f', 'o', 'o' };
  constexpr int bad = __builtin_strlen(d);
}

namespace PR19010 {
  struct Empty {};
  struct Empty2 : Empty {};
  struct Test : Empty2 {
    constexpr Test() {}
    Empty2 array[2];
  };
  void test() { constexpr Test t; }
}

void PR21327(int a, int b) {
  static_assert(&a + 1 != &b, "");

}

namespace EmptyClass {
  struct E1 {} e1;
  union E2 {} e2;
  struct E3 : E1 {} e3;




  constexpr E1 e1b(e1);
  constexpr E2 e2b(e2);
  constexpr E3 e3b(e3);
}

namespace PR21786 {
  extern void (*start[])();
  extern void (*end[])();
  static_assert(&start != &end, "");

  static_assert(&start != nullptr, "");

  struct Foo;
  struct Bar {
    static const Foo x;
    static const Foo y;
  };
  static_assert(&Bar::x != nullptr, "");
  static_assert(&Bar::x != &Bar::y, "");
}

namespace PR21859 {
  constexpr int Fun() { return; }
  constexpr int Var = Fun();

  template <typename T> constexpr int FunT1() { return; }
  template <typename T> constexpr int FunT2() { return 0; }
  template <> constexpr int FunT2<double>() { return 0; }
  template <> constexpr int FunT2<int>() { return; }
}

struct InvalidRedef {
  int f;
  constexpr int f(void);
};

namespace PR17938 {
  template <typename T> constexpr T const &f(T const &x) { return x; }

  struct X {};
  struct Y : X {};
  struct Z : Y { constexpr Z() {} };

  static constexpr auto z = f(Z());
}

namespace PR24597 {
  struct A {
    int x, *p;
    constexpr A() : x(0), p(&x) {}
    constexpr A(const A &a) : x(a.x), p(&x) {}
  };
  constexpr A f() { return A(); }
  constexpr A g() { return f(); }
  constexpr int a = *f().p;
  constexpr int b = *g().p;
}

namespace IncompleteClass {
  struct XX {
    static constexpr int f(XX*) { return 1; }
    friend constexpr int g(XX*) { return 2; }

    static constexpr int i = f(static_cast<XX*>(nullptr));
    static constexpr int j = g(static_cast<XX*>(nullptr));
  };
}

namespace InheritedCtor {
  struct A { constexpr A(int) {} };

  struct B : A { int n; using A::A; };
  constexpr B b(0);


  struct C : A { using A::A; struct { union { int n, m = 0; }; union { int a = 0; }; int k = 0; }; struct {}; union {}; };
  constexpr C c(0);

  struct D : A {
    using A::A;
    struct {
      union {
        int n;
      };
    };
  };
  constexpr D d(0);

  struct E : virtual A { using A::A; };




  void f() {
    constexpr E e(0);

  }



  struct W { constexpr W(int n) : w(n) {} int w; };
  struct X : W { using W::W; int x = 2; };
  struct Y : X { using X::X; int y = 3; };
  struct Z : Y { using Y::Y; int z = 4; };
  constexpr Z z(1);
  static_assert(z.w == 1 && z.x == 2 && z.y == 3 && z.z == 4, "");
}


namespace PR28366 {
namespace ns1 {

void f(char c) {

  struct X {
    static constexpr char f() {
      return c;
    }
  };
  int I = X::f();
}

void g() {
  const int c = 'c';
  static const int d = 'd';
  struct X {
    static constexpr int f() {
      return c + d;
    }
  };
  static_assert(X::f() == 'c' + 'd',"");
}


}

}

namespace PointerArithmeticOverflow {
  int n;
  int a[1];
  constexpr int *b = &n + 1 + (long)-1;
  constexpr int *c = &n + 1 + (unsigned long)-1;
  constexpr int *d = &n + 1 - (unsigned long)1;
  constexpr int *e = a + 1 + (long)-1;
  constexpr int *f = a + 1 + (unsigned long)-1;
  constexpr int *g = a + 1 - (unsigned long)1;

  constexpr int *p = (&n + 1) + (unsigned __int128)-1;
  constexpr int *q = (&n + 1) - (unsigned __int128)-1;
  constexpr int *r = &(&n + 1)[(unsigned __int128)-1];
}

namespace PR40430 {
  struct S {
    char c[10] = "asdf";
    constexpr char foo() const { return c[3]; }
  };
  static_assert(S().foo() == 'f', "");
}

namespace PR41854 {
  struct e { operator int(); };
  struct f { e c; };
  int a;
  f &d = reinterpret_cast<f&>(a);
  unsigned b = d.c;
}

namespace array_size {
  template<int N> struct array {
    static constexpr int size() { return N; }
  };
  template<typename T> void f1(T t) {
    constexpr int k = t.size();
  }
  template<typename T> void f2(const T &t) {
    constexpr int k = t.size();
  }
  template<typename T> void f3(const T &t) {
    constexpr int k = T::size();
  }
  void g(array<3> a) {
    f1(a);
    f2(a);
    f3(a);
  }

  template<int N> struct array_nonstatic {
    constexpr int size() const { return N; }
  };
  void h(array_nonstatic<3> a) {
    f1(a);
    f2(a);
  }

}

namespace flexible_array {
  struct A { int x; char arr[]; };
  constexpr A a = {1};
  static_assert(a.x == 1, "");
  static_assert(&a.arr != nullptr, "");
  static_assert(a.arr[0], "");
  static_assert(a.arr[1], "");

  constexpr A b[] = {{1}, {2}, {3}};
  static_assert(b[0].x == 1, "");
  static_assert(b[1].x == 2, "");
  static_assert(b[2].x == 3, "");
  static_assert(b[2].arr[0], "");



  constexpr A c = {1, 2, 3};


}

void local_constexpr_var() {
  constexpr int a = 0;
  constexpr const int *p = &a;
}

namespace GH50055 {

enum E1 {e11=-4, e12=4};
enum E2 {e21=0, e22=4};
enum E3 {e31=-4, e32=1024};
enum E4 {e41=0};

enum EEmpty {};


enum EFixed : int {efixed1=-4, efixed2=4};

enum class EScoped {escoped1=-4, escoped2=4};

enum EMaxInt {emaxint1=-1, emaxint2=2147483647};

enum NumberType {};

E2 testDefaultArgForParam(E2 e2Param = (E2)-1) {
  E2 e2LocalInit = e2Param;
  return e2LocalInit;
}

# 1 "SemaCXX/Inputs/enum-constexpr-conversion-system-header.h" 1



enum SystemEnum
{
    a = 0,
    b = 1,
};

void testValueInRangeOfEnumerationValuesInSystemHeader()
{
    constexpr SystemEnum x1 = static_cast<SystemEnum>(123);



    const SystemEnum x2 = static_cast<SystemEnum>(123);
}
# 2510 "SemaCXX/constant-expression-cxx11.cpp" 2

void testValueInRangeOfEnumerationValues() {
  constexpr E1 x1 = static_cast<E1>(-8);
  constexpr E1 x2 = static_cast<E1>(8);


  E1 x2b = static_cast<E1>(8);
  static_assert(static_cast<E1>(8), "");



  constexpr E2 x3 = static_cast<E2>(-8);


  constexpr E2 x4 = static_cast<E2>(0);
  constexpr E2 x5 = static_cast<E2>(8);



  constexpr E3 x6 = static_cast<E3>(-2048);
  constexpr E3 x7 = static_cast<E3>(-8);
  constexpr E3 x8 = static_cast<E3>(0);
  constexpr E3 x9 = static_cast<E3>(8);
  constexpr E3 x10 = static_cast<E3>(2048);



  constexpr E4 x11 = static_cast<E4>(0);
  constexpr E4 x12 = static_cast<E4>(1);
  constexpr E4 x13 = static_cast<E4>(2);



  constexpr EEmpty x14 = static_cast<EEmpty>(0);
  constexpr EEmpty x15 = static_cast<EEmpty>(1);
  constexpr EEmpty x16 = static_cast<EEmpty>(2);



  constexpr EFixed x17 = static_cast<EFixed>(100);
  constexpr EScoped x18 = static_cast<EScoped>(100);

  constexpr EMaxInt x19 = static_cast<EMaxInt>(2147483647 -1);
  constexpr EMaxInt x20 = static_cast<EMaxInt>((long)2147483647 +1);



  const NumberType neg_one = (NumberType) ((NumberType) 0 - (NumberType) 1);
  constexpr NumberType neg_one_constexpr = neg_one;




  constexpr SystemEnum system_enum = static_cast<SystemEnum>(123);


}

template<class T, unsigned size> struct Bitfield {
  static constexpr T max = static_cast<T>((1 << size) - 1);


};

void testValueInRangeOfEnumerationValuesViaTemplate() {
  Bitfield<E2, 3> good;
  Bitfield<E2, 4> bad;
}

enum SortOrder {
  AscendingOrder,
  DescendingOrder
};

class A {
  static void f(SortOrder order);
};

void A::f(SortOrder order) {
  if (order == SortOrder(-1))
    return;
}
}

GH50055::E2 GlobalInitNotCE1 = (GH50055::E2)-1;
GH50055::E2 GlobalInitNotCE2 = GH50055::testDefaultArgForParam();
constexpr GH50055::E2 GlobalInitCE = (GH50055::E2)-1;



namespace GH112140 {
struct S {
  constexpr S(const int &a = ) { }
};

void foo() {
  constexpr S s[2] = { };
}
}

namespace DoubleCapture {
  int DC() {
  int a = 1000;
    static auto f =
      [a, &a] {
    };
  }
}

namespace GH150709 {
  struct C { };
  struct D : C {
    constexpr int f() const { return 1; };
  };
  struct E : C { };
  struct F : D { };
  struct G : E { };

  constexpr C c1, c2[2];
  constexpr D d1, d2[2];
  constexpr E e1, e2[2];
  constexpr F f;
  constexpr G g;

  constexpr auto mp = static_cast<int (C::*)() const>(&D::f);


  static_assert((c1.*mp)() == 1, "");
  static_assert((d1.*mp)() == 1, "");
  static_assert((f.*mp)() == 1, "");
  static_assert((c2[0].*mp)() == 1, "");
  static_assert((d2[0].*mp)() == 1, "");


  static_assert((e1.*mp)() == 1, "");
  static_assert((e2[0].*mp)() == 1, "");
  static_assert((g.*mp)() == 1, "");
}

namespace GH154567 {
  struct T {
    int i;
  };

  struct S {
    struct {
      T val;
    };
    constexpr S() : val() {}
  };

  constexpr S s{};
  static_assert(s.val.i == 0, "");
}
