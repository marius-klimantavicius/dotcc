//type: fp
//options:  --c++17 --exceptions --exceptions: --c++20 --exceptions -DUSE_CONSTEVAL --exceptions: --c++23 --exceptions -DUSE_CONSTEVAL -DPAREN_INIT --exceptions: --c++17 --exceptions --ms_extensions -DMS --exceptions --ms_compatibility: --c++20 --exceptions --ms_extensions -DMS -DUSE_CONSTEVAL --exceptions --ms_compatibility: --c++17 --exceptions --exceptions -DNEW_INTERP: --c++20 --exceptions -DUSE_CONSTEVAL --exceptions -DNEW_INTERP: --c++23 --exceptions -DUSE_CONSTEVAL -DPAREN_INIT --exceptions -DNEW_INTERP: --c++17 --exceptions --ms_extensions -DMS --exceptions -DNEW_INTERP --ms_compatibility: --c++20 --exceptions --ms_extensions -DMS -DUSE_CONSTEVAL --exceptions -DNEW_INTERP --ms_compatibility
# 1 "SemaCXX/source_location.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 479 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/source_location.cpp" 2
# 18 "SemaCXX/source_location.cpp"
template <unsigned>
struct Printer;







namespace std {
class source_location {
  struct __impl;

public:
  static constexpr source_location
    current(const __impl *__p = __builtin_source_location()) noexcept {
      source_location __loc;
      __loc.__m_impl = __p;
      return __loc;
  }
  constexpr source_location() = default;
  constexpr source_location(source_location const &) = default;
  constexpr unsigned int line() const noexcept { return __m_impl ? __m_impl->_M_line : 0; }
  constexpr unsigned int column() const noexcept { return __m_impl ? __m_impl->_M_column : 0; }
  constexpr const char *file() const noexcept { return __m_impl ? __m_impl->_M_file_name : ""; }
  constexpr const char *function() const noexcept { return __m_impl ? __m_impl->_M_function_name : ""; }

private:


  struct __impl {
    const char *_M_file_name;
    const char *_M_function_name;
    unsigned _M_line;
    unsigned _M_column;
  };
  const __impl *__m_impl = nullptr;

public:
  using public_impl_alias = __impl;
};
}

using SL = std::source_location;

# 1 "SemaCXX/Inputs/source-location-file.h" 1



namespace source_location_file {

constexpr const char *FILE = "SemaCXX/Inputs/source-location-file.h";
constexpr const char *FILE_NAME = "source-location-file.h";

constexpr SL global_info = SL::current();
constexpr const char *global_info_filename = __builtin_FILE_NAME();

constexpr SL test_function(SL v = SL::current()) {
  return v;
}

constexpr SL test_function_indirect() {
  return test_function();
}

constexpr const char *test_function_filename(
                      const char *file_name = __builtin_FILE_NAME()) {
  return file_name;
}

constexpr const char *test_function_filename_indirect() {
  return test_function_filename();
}

template <class T, class U = SL>
constexpr U test_function_template(T, U u = U::current()) {
  return u;
}

template <class T, class U = SL>
constexpr U test_function_template_indirect(T t) {
  return test_function_template(t);
}

template <class T, class U = const char *>
constexpr U test_function_filename_template(T, U u = __builtin_FILE_NAME()) {
  return u;
}

template <class T, class U = const char *>
constexpr U test_function_filename_template_indirect(T t) {
  return test_function_filename_template(t);
}

struct TestClass {
  SL info = SL::current();
  const char *info_file_name = __builtin_FILE_NAME();
  SL ctor_info;
  const char *ctor_info_file_name = nullptr;
  TestClass() = default;
  constexpr TestClass(int, SL cinfo = SL::current(),
                      const char *cfile_name = __builtin_FILE_NAME()) :
                      ctor_info(cinfo), ctor_info_file_name(cfile_name) {}
  template <class T, class U = SL>
  constexpr TestClass(int, T, U u = U::current(),
                      const char *cfile_name = __builtin_FILE_NAME()) :
                      ctor_info(u), ctor_info_file_name(cfile_name) {}
};

template <class T = SL>
struct AggrClass {
  int x;
  T info;
  T init_info = T::current();
  const char *init_info_file_name = __builtin_FILE_NAME();
};

}
# 64 "SemaCXX/source_location.cpp" 2
namespace SLF = source_location_file;

constexpr bool is_equal(const char *LHS, const char *RHS) {
  while (*LHS != 0 && *RHS != 0) {
    if (*LHS != *RHS)
      return false;
    ++LHS;
    ++RHS;
  }
  return *LHS == 0 && *RHS == 0;
}

template <class T>
constexpr T identity(T t) {
  return t;
}

template <class T, class U>
struct Pair {
  T first;
  U second;
};

template <class T, class U>
constexpr bool is_same = false;
template <class T>
constexpr bool is_same<T, T> = true;


static_assert(is_same<decltype(__builtin_LINE()), unsigned>);
static_assert(is_same<decltype(__builtin_COLUMN()), unsigned>);
static_assert(is_same<decltype(__builtin_FILE()), const char *>);
static_assert(is_same<decltype(__builtin_FILE_NAME()), const char *>);
static_assert(is_same<decltype(__builtin_FUNCTION()), const char *>);



static_assert(is_same<decltype(__builtin_source_location()), const std::source_location::public_impl_alias *>);


static_assert(noexcept(__builtin_LINE()));
static_assert(noexcept(__builtin_COLUMN()));
static_assert(noexcept(__builtin_FILE()));
static_assert(noexcept(__builtin_FILE_NAME()));
static_assert(noexcept(__builtin_FUNCTION()));



static_assert(noexcept(__builtin_source_location()));





namespace test_line {
static_assert(SL::current().line() == 119);
static_assert(SL::current().line() == SL::current().line());

static constexpr SL GlobalS = SL::current();

static_assert(GlobalS.line() == 124 - 2);


constexpr bool test_line_fn() {
  constexpr SL S = SL::current();
  static_assert(S.line() == (129 - 1), "");

  constexpr int ExpectLine = 131 + 3;
  constexpr SL S2
  =
  SL
  ::
  current
  (

  )
  ;
  static_assert(S2.line() == ExpectLine, "");

  static_assert(
          __builtin_LINE ( )




    == 149 - 1, "");
  static_assert( __builtin_LINE() == 154 - 2, "");




  static_assert( __builtin_LINE()


          == 158 - 2, "");

  return true;
}

static_assert(test_line_fn());

static_assert(__builtin_LINE() == 165, "");

constexpr int baz() { return 101; }

constexpr int test_line_fn_simple(int z = baz(), int x = __builtin_LINE()) {
  return x;
}
void bar() {
  static_assert(test_line_fn_simple() == 173, "");
  static_assert(test_line_fn_simple() == 174, "");
}

struct CallExpr {
  constexpr int operator()(int x = __builtin_LINE()) const { return x; }
};
constexpr CallExpr get_call() { return CallExpr{}; }
static_assert(get_call()() == 181, "");

template <class T>
constexpr bool test_line_fn_template(T Expect, int L = __builtin_LINE()) {
  return Expect == L;
}
static_assert(test_line_fn_template(187));

struct InMemInit {
  constexpr bool check(int expect) const {
    return info.line() == expect;
  }
  SL info = SL::current();
  InMemInit() = default;
  constexpr InMemInit(int) {}
};
static_assert(InMemInit{}.check(197 - 3), "");
static_assert(InMemInit{42}.check(198 - 3), "");

template <class T, class U = SL>
struct InMemInitTemplate {
  constexpr bool check(int expect) const {
    return info.line() == expect;
  }
  U info = U::current();
  InMemInitTemplate() = default;
  constexpr InMemInitTemplate(T) {}
  constexpr InMemInitTemplate(T, T) : info(U::current()) {}
  template <class V = U> constexpr InMemInitTemplate(T, T, T, V info = U::current())
      : info(info) {}
};
void test_mem_init_template() {
  constexpr int line_offset = 8;
  static_assert(InMemInitTemplate<int>{}.check(214 - line_offset), "");
  static_assert(InMemInitTemplate<unsigned>{42}.check(215 - line_offset), "");
  static_assert(InMemInitTemplate<unsigned>{42, 42}.check(216 - line_offset), "");
  static_assert(InMemInitTemplate<unsigned>{42, 42, 42}.check(217), "");
}

struct AggInit {
  int x;
  int y = __builtin_LINE();
  constexpr bool check(int expect) const {
    return y == expect;
  }
};
constexpr AggInit AI{42};
static_assert(AI.check(228 - 1), "");

template <class T, class U = SL>
struct AggInitTemplate {
  constexpr bool check(int expect) const {
    return expect == info.line();
  }
  T x;
  U info = U::current();
};

template <class T, class U = SL>
constexpr U test_fn_template(T, U u = U::current()) {
  return u;
}
void fn_template_tests() {
  static_assert(test_fn_template(42).line() == 244, "");
}

struct TestMethodTemplate {
  template <class T, class U = SL, class U2 = SL>
  constexpr U get(T, U u = U::current(), U2 u2 = identity(U2::current())) const {
    ((u.line() == u2.line()) ? ((void)0) : throw 42);
    return u;
  }
};
void method_template_tests() {
  static_assert(TestMethodTemplate{}.get(42).line() == 255, "");
}

struct InStaticInit {
  static constexpr int LINE = 259;
  static constexpr const int x1 = __builtin_LINE();
  static constexpr const int x2 = identity(__builtin_LINE());
  static const int x3;
  const int x4 = __builtin_LINE();
  int x5 = __builtin_LINE();
};
const int InStaticInit::x3 = __builtin_LINE();
static_assert(InStaticInit::x1 == InStaticInit::LINE + 1, "");
static_assert(InStaticInit::x2 == InStaticInit::LINE + 2, "");

template <class T, int N = __builtin_LINE(), int Expect = -1>
constexpr void check_fn_template_param(T) {
  constexpr int RealExpect = Expect == -1 ? 272 - 2 : Expect;
  static_assert(N == RealExpect);
}
template void check_fn_template_param(int);
template void check_fn_template_param<long, 42, 42>(long);
# 100 "SemaCXX/source_location.cpp"
struct AggBase {
# 200 "SemaCXX/source_location.cpp"
 int x = __builtin_LINE();
  int y = __builtin_LINE();
  int z = __builtin_LINE();
};
# 300 "SemaCXX/source_location.cpp"
struct AggDer : AggBase {
};
# 400 "SemaCXX/source_location.cpp"
static_assert(AggDer{}.x == 400, "");

struct ClassBase {
# 400 "SemaCXX/source_location.cpp"
 int x = __builtin_LINE();
  int y = 0;
  int z = 0;
# 500 "SemaCXX/source_location.cpp"
 ClassBase() = default;
  constexpr ClassBase(int yy, int zz = __builtin_LINE())
      : y(yy), z(zz) {}
};
struct ClassDer : ClassBase {
# 600 "SemaCXX/source_location.cpp"
 ClassDer() = default;
  constexpr ClassDer(int yy) : ClassBase(yy) {}
  constexpr ClassDer(int yy, int zz) : ClassBase(yy, zz) {}
};
# 700 "SemaCXX/source_location.cpp"
static_assert(ClassDer{}.x == 500, "");
static_assert(ClassDer{42}.x == 501, "");
static_assert(ClassDer{42}.z == 601, "");
static_assert(ClassDer{42, 42}.x == 501, "");

struct ClassAggDer : AggBase {
# 800 "SemaCXX/source_location.cpp"
 ClassAggDer() = default;
  constexpr ClassAggDer(int, int x = __builtin_LINE()) : AggBase{x} {}
};
static_assert(ClassAggDer{}.x == 100, "");

}





namespace test_file {
constexpr const char *test_file_simple(const char *__f = __builtin_FILE()) {
  return __f;
}
void test_function() {
# 900 "SemaCXX/source_location.cpp"
 static_assert(is_equal(test_file_simple(), "SemaCXX/source_location.cpp"));
  static_assert(is_equal(SLF::test_function().file(), "SemaCXX/source_location.cpp"), "");
  static_assert(is_equal(SLF::test_function_template(42).file(), "SemaCXX/source_location.cpp"), "");

  static_assert(is_equal(SLF::test_function_indirect().file(), SLF::global_info.file()), "");
  static_assert(is_equal(SLF::test_function_template_indirect(42).file(), SLF::global_info.file()), "");

  static_assert(test_file_simple() != nullptr);
  static_assert(!is_equal(test_file_simple(), "source_location.cpp"));
}

void test_class() {
# 315 "SemaCXX/source_location.cpp"
 using SLF::TestClass;
  constexpr TestClass Default;
  constexpr TestClass InParam{42};
  constexpr TestClass Template{42, 42};
  constexpr auto *F = Default.info.file();
  constexpr auto Char = F[0];
  static_assert(is_equal(Default.info.file(), SLF::FILE), "");
  static_assert(is_equal(InParam.info.file(), SLF::FILE), "");
  static_assert(is_equal(InParam.ctor_info.file(), "SemaCXX/source_location.cpp"), "");
}

void test_aggr_class() {
  using Agg = SLF::AggrClass<>;
  constexpr Agg Default{};
  constexpr Agg InitOne{42};
  static_assert(is_equal(Default.init_info.file(), "SemaCXX/source_location.cpp"), "");
  static_assert(is_equal(InitOne.init_info.file(), "SemaCXX/source_location.cpp"), "");
}

}





namespace test_file_name {
constexpr const char *test_file_name_simple(
  const char *__f = __builtin_FILE_NAME()) {
  return __f;
}
void test_function() {
# 900 "SemaCXX/source_location.cpp"
 static_assert(is_equal(test_file_name_simple(), "source_location.cpp"));
  static_assert(is_equal(SLF::test_function_filename(), "source_location.cpp"), "");
  static_assert(is_equal(SLF::test_function_filename_template(42),
                         "source_location.cpp"), "");

  static_assert(is_equal(SLF::test_function_filename_indirect(),
                         SLF::global_info_filename), "");
  static_assert(is_equal(SLF::test_function_filename_template_indirect(42),
                         SLF::global_info_filename), "");

  static_assert(test_file_name_simple() != nullptr);
  static_assert(is_equal(test_file_name_simple(), "source_location.cpp"));
}

void test_class() {
# 315 "SemaCXX/source_location.cpp"
 using SLF::TestClass;
  constexpr TestClass Default;
  constexpr TestClass InParam{42};
  constexpr TestClass Template{42, 42};
  constexpr auto *F = Default.info_file_name;
  constexpr auto Char = F[0];
  static_assert(is_equal(Default.info_file_name, SLF::FILE_NAME), "");
  static_assert(is_equal(InParam.info_file_name, SLF::FILE_NAME), "");
  static_assert(is_equal(InParam.ctor_info_file_name, "source_location.cpp"), "");
}

void test_aggr_class() {
  using Agg = SLF::AggrClass<>;
  constexpr Agg Default{};
  constexpr Agg InitOne{42};
  static_assert(is_equal(Default.init_info_file_name, "source_location.cpp"), "");
  static_assert(is_equal(InitOne.init_info_file_name, "source_location.cpp"), "");
}

}





namespace test_func {

constexpr const char *test_func_simple(const char *__f = __builtin_FUNCTION()) {
  return __f;
}
constexpr const char *get_function() {
  return __func__;
}
constexpr bool test_function() {
  return is_equal(__func__, test_func_simple()) &&
         !is_equal(get_function(), test_func_simple());
}
static_assert(test_function());

template <class T, class U = SL>
constexpr Pair<U, U> test_func_template(T, U u = U::current()) {
  static_assert(is_equal(__PRETTY_FUNCTION__, U::current().function()));
  return {u, U::current()};
}
template <class T>
void func_template_tests() {
  constexpr auto P = test_func_template(42);


}
template void func_template_tests<int>();

template <class = int, class T = SL>
struct TestCtor {
  T info = T::current();
  T ctor_info;
  TestCtor() = default;
  template <class U = SL>
  constexpr TestCtor(int, U u = U::current()) : ctor_info(u) {}
};
void ctor_tests() {
  constexpr TestCtor<> Default;
  constexpr TestCtor<> Template{42};
  static const char *XYZZY = Template.info.function();
  static_assert(is_equal(Default.info.function(), "test_func::TestCtor<>::TestCtor() [T = std::source_location]"));
  static_assert(is_equal(Default.ctor_info.function(), ""));
  static_assert(is_equal(Template.info.function(), "test_func::TestCtor<>::TestCtor(int, U) [T = std::source_location, U = std::source_location]"));
  static_assert(is_equal(Template.ctor_info.function(), __PRETTY_FUNCTION__));
}

constexpr SL global_sl = SL::current();
static_assert(is_equal(global_sl.function(), ""));

template <class T>
class TestBI {
public:
   TestBI() {



     static_assert(is_equal(__FUNCTION__, "TestBI"));

     static_assert(is_equal(__func__, "TestBI"));
   }
};

template <class T>
class TestClass {
public:
   TestClass() {



      static_assert(is_equal(__FUNCTION__, "TestClass"));

      static_assert(is_equal(__func__, "TestClass"));
   }
};

template <class T>
class TestStruct {
public:
   TestStruct() {



      static_assert(is_equal(__FUNCTION__, "TestStruct"));

      static_assert(is_equal(__func__, "TestStruct"));
   }
};

template <class T>
class TestEnum {
public:
   TestEnum() {



      static_assert(is_equal(__FUNCTION__, "TestEnum"));

      static_assert(is_equal(__func__, "TestEnum"));
   }
};

class C {};
struct S {};
enum E {};

TestBI<int> t1;
TestClass<test_func::C> t2;
TestStruct<test_func::S> t3;
TestEnum<test_func::E> t4;

class A { int b;};
namespace inner {
  template <class Ty>
  class C {
  public:
    template <class T>
    static void f(int i) {
      (void)i;



     static_assert(is_equal(__FUNCTION__, "f"));

    }
    template <class T>
    static constexpr void cf(int i) {
      (void)i;



     static_assert(is_equal(__FUNCTION__, "cf"));

    }
    template <class T>
    static void df(double f) {
      (void)f;



      static_assert(is_equal(__FUNCTION__, "df"));

    }
    template <class T>
    static constexpr void cdf(double f) {
      (void)f;



      static_assert(is_equal(__FUNCTION__, "cdf"));

    }
  };
}

  void foo() {
  test_func::inner::C<test_func::A>::f<char>(1);
  test_func::inner::C<test_func::A>::cf<char>(1);
  test_func::inner::C<test_func::A>::df<void>(1.0);
  test_func::inner::C<test_func::A>::cdf<void>(1.0);
}

}
# 558 "SemaCXX/source_location.cpp"
namespace test_column {


constexpr bool test_column_fn() {
  constexpr SL S = SL::current();
  static_assert(S.line() == (563 - 1), "");
  constexpr int Indent = 4;
  {

    constexpr int ExpectCol = Indent + 3;
    constexpr SL S2
     =
      SL
        ::
          current
                 (

                  )
                   ;
    static_assert(S2.column() == ExpectCol, "");
  }
  {
    constexpr int ExpectCol = 2;
    constexpr int C =
 __builtin_COLUMN
      ();
    static_assert(C == ExpectCol);
  }
  return true;
}
# 420 "SemaCXX/source_location.cpp"
static_assert(test_column_fn());


static_assert(SL::current().column() == __builtin_strlen("static_assert(S"));
struct TestClass {
  int x = __builtin_COLUMN();
   TestClass() = default;
  constexpr TestClass(int, int o = __builtin_COLUMN()) : x(o) {}
};
struct TestAggClass {
  int x = __builtin_COLUMN();
};
constexpr bool test_class() {

  auto check = [](int V, const char* S, int indent = 4) {
    ((V == (__builtin_strlen(S) + indent)) ? ((void)0) : throw 42);
  };
  {
    TestClass t{};
    check(t.x, "   T", 0);
  }
  {
    TestClass t1
            {42};
    check(t1.x, "TestClass t");
  }
  {
    TestAggClass t { };
    check(t.x, "TestAggClass t  { }");
  }
  {
    TestAggClass t = { };
    check(t.x, "TestAggClass t = { }");
  }
  return true;
}
static_assert(test_class());

}




namespace test_pragma_line {
constexpr int StartLine = 42;
# 42 "SemaCXX/source_location.cpp"
static_assert(__builtin_LINE() == StartLine);
static_assert(__builtin_LINE() == StartLine + 1);
static_assert(SL::current().line() == StartLine + 2);
# 44 "test_file.c"
static_assert(is_equal("test_file.c", "test_file.c"));
static_assert(is_equal("test_file.c", __builtin_FILE()));
static_assert(is_equal("test_file.c", __builtin_FILE_NAME()));
static_assert(is_equal("test_file.c", SL::current().file()));
static_assert(is_equal("test_file.c", SLF::test_function().file()));
static_assert(is_equal(SLF::FILE, SLF::test_function_indirect().file()));
}

namespace test_out_of_line_init {
# 4000 "test_out_of_line_init.cpp"
constexpr unsigned get_line(unsigned n = __builtin_LINE()) { return n; }
constexpr const char *get_file(const char *f = __builtin_FILE()) { return f; }
constexpr const char *get_func(const char *f = __builtin_FUNCTION()) { return f; }
# 4100 "A.cpp"
struct A {
  int n = __builtin_LINE();
  int n2 = get_line();
  const char *f = __builtin_FILE();
  const char *f2 = get_file();
  const char *func = __builtin_FUNCTION();
  const char *func2 = get_func();
  SL info = SL::current();
};
# 4200 "B.cpp"
struct B {
  A a = {};
};
# 4300 "test_passed.cpp"
constexpr B b = {};
static_assert(b.a.n == 4300, "");
static_assert(b.a.n2 == 4300, "");
static_assert(b.a.info.line() == 4300, "");
static_assert(is_equal(b.a.f, "test_passed.cpp"));
static_assert(is_equal(b.a.f2, "test_passed.cpp"));
static_assert(is_equal(b.a.info.file(), "test_passed.cpp"));
static_assert(is_equal(b.a.func, ""));
static_assert(is_equal(b.a.func2, ""));
static_assert(is_equal(b.a.info.function(), ""));

constexpr bool test_in_func() {
# 4400 "test_func_passed.cpp"
 constexpr B b = {};
  static_assert(b.a.n == 4400, "");
  static_assert(b.a.n2 == 4400, "");
  static_assert(b.a.info.line() == 4400, "");
  static_assert(is_equal(b.a.f, "test_func_passed.cpp"));
  static_assert(is_equal(b.a.f2, "test_func_passed.cpp"));
  static_assert(is_equal(b.a.info.file(), "test_func_passed.cpp"));
  static_assert(is_equal(b.a.func, "test_in_func"));
  static_assert(is_equal(b.a.func2, "test_in_func"));
  static_assert(is_equal(b.a.info.function(), "bool test_out_of_line_init::test_in_func()"));
  return true;
}
static_assert(test_in_func());

}

namespace test_global_scope {
# 5000 "test_global_scope.cpp"
constexpr unsigned get_line(unsigned n = __builtin_LINE()) { return n; }
constexpr const char *get_file(const char *f = __builtin_FILE()) { return f; }
constexpr const char *get_func(const char *f = __builtin_FUNCTION()) { return f; }
# 5100 "test_global_scope.cpp"
struct InInit {
  unsigned l = get_line();
  const char *f = get_file();
  const char *func = get_func();
# 5200 "in_init.cpp"
 constexpr InInit() {}
};
# 5300 "in_init.cpp"
constexpr InInit II;

static_assert(II.l == 5200, "");
static_assert(is_equal(II.f, "in_init.cpp"));
static_assert(is_equal(II.func, "InInit"));
# 5400 "in_init.cpp"
struct AggInit {
  unsigned l = get_line();
  const char *f = get_file();
  const char *func = get_func();
};
# 5500 "brace_init.cpp"
constexpr AggInit AI = {};
static_assert(AI.l == 5500);
static_assert(is_equal(AI.f, "brace_init.cpp"));
static_assert(is_equal(AI.func, ""));

}

namespace TestFuncInInit {
# 6000 "InitClass.cpp"
struct Init {
  SL info;
# 6100 "InitCtor.cpp"
 constexpr Init(SL info = SL::current()) : info(info) {}
};
# 6200 "InitGlobal.cpp"
constexpr Init I;
static_assert(I.info.line() == 6200);
static_assert(is_equal(I.info.file(), "InitGlobal.cpp"));

}

namespace TestConstexprContext {
# 7000 "TestConstexprContext.cpp"
 constexpr const char* foo() { return __builtin_FILE(); }
# 7100 "Bar.cpp"
 constexpr const char* bar(const char* x = foo()) { return x; }
  constexpr bool test() {
    static_assert(is_equal(bar(), "TestConstexprContext.cpp"));
    return true;
  }
  static_assert(test());
}

namespace Lambda {
# 8000 "TestLambda.cpp"
constexpr int nested_lambda(int l = []{
  return SL::current().line();
}()) {
  return l;
}
static_assert(nested_lambda() == 8005 - 4);

constexpr int lambda_param(int l = [](int l = SL::current().line()) {
  return l;
}()) {
  return l;
}
static_assert(lambda_param() == 8012);


}

constexpr int compound_literal_fun(int a =
                  (int){ SL::current().line() }
) { return a ;}
static_assert(compound_literal_fun() == 8020);

struct CompoundLiteral {
  int a = (int){ SL::current().line() };
};
static_assert(CompoundLiteral{}.a == 8025);
# 8036 "TestLambda.cpp"
constexpr int test_init_capture(int a =
                [b = SL::current().line()] { return b; }()) {
  return a;
}



static_assert(test_init_capture() == 8043 );


namespace check_immediate_invocations_in_templates {

template <typename T = int>
struct G {
    T line = __builtin_LINE();
};
template <typename T>
struct S {
    int i = G<T>{}.line;
};
static_assert(S<int>{}.i !=
              S<int>{}.i);

template <typename T>
constexpr int f(int i = G<T>{}.line) {
    return i;
}

static_assert(f<int>() !=
              f<int>());
}
# 8085 "TestLambda.cpp"
namespace GH78128 {

template<int N>
constexpr int f() {
  return N;
}

template<typename T>
void foo() {
  constexpr auto* F1 = std::source_location::current().function();
  static_assert(__builtin_strlen(F1) == f<__builtin_strlen(F1)>());

  constexpr auto* F2 = __builtin_FUNCTION();
  static_assert(__builtin_strlen(F2) == f<__builtin_strlen(F2)>());





}

void test() {
  foo<int>();
}

}

namespace GH80630 {







auto f( std::source_location const* loc = []( char const* fn ) { static constexpr std::source_location loc = std::source_location::current(); return &loc; }( std::source_location::current().function() ) ) {
    return loc;
}

auto g() {
    return f();
}

}

namespace GH92680 {

struct IntConstuctible {
  IntConstuctible(std::source_location = std::source_location::current());
};

template <typename>
auto construct_at(IntConstuctible) -> decltype(IntConstuctible()) {
  return {};
}

void test() {
  construct_at<IntConstuctible>({});
}

}

namespace GH106428 {

struct add_fn {
    template <typename T>
    constexpr auto operator()(T lhs, T rhs,
                              const std::source_location loc = std::source_location::current())
        const -> T
    {
        return lhs + rhs;
    }
};


template <class _Fp, class... _Args>
decltype(_Fp{}(0, 0))
__invoke(_Fp&& __f);

template<typename T>
struct type_identity { using type = T; };

template<class Fn>
struct invoke_result : type_identity<decltype(__invoke(Fn{}))> {};

using i = invoke_result<add_fn>::type;
static_assert(__is_same(i, int));

}
# 8208 "TestLambda.cpp"
namespace GH67134 {
template <int loc = std::source_location::current().line()>
constexpr auto f(std::source_location loc2 = std::source_location::current()) { return loc; }

int g = []() -> decltype(f()) { return 0; }();

int call() {



  return []() -> decltype(f()) { return 0; }();
}
# 8228 "TestLambda.cpp"
}

namespace GH119129 {
struct X{
  constexpr int foo(std::source_location loc = std::source_location::current()) {
    return loc.line();
  }
};
static_assert(X{}.foo() == 8236);
static_assert(X{}.
                foo() == 8238);
static_assert(X{}.


                foo() == 8242);
# 10000 "TestLambda.cpp"
static_assert(X{}.
                foo() == 10001);
}
