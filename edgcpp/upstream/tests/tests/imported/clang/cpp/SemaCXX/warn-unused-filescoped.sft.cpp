//type: fp
//options:  --c++14
# 1 "SemaCXX/warn-unused-filescoped.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 461 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/warn-unused-filescoped.cpp" 2
# 45 "SemaCXX/warn-unused-filescoped.cpp"
# 1 "SemaCXX/warn-unused-filescoped.cpp" 1






static void headerstatic() {}
static inline void headerstaticinline() {}

namespace {
void headeranon() {}
inline void headerinlineanon() {}
}

namespace test7
{
  template<typename T>
  static inline void foo(T) { }



  template<>
  inline void foo(int) { }


  template<typename T, typename U>
  static inline void bar(T, U) { }

  template<typename U>
  inline void bar(int, U) { }

  template<>
  inline void bar(int, int) { }
};

namespace pr19713 {

  static constexpr int constexpr1() { return 1; }
  constexpr int constexpr2() { return 2; }

}
# 46 "SemaCXX/warn-unused-filescoped.cpp" 2

static void f1();

namespace {
void f2();

void f3() {}

struct S {
  void m1() {}
  void m2();
  void m3();
  S(const S &);
  void operator=(const S &);
};

  template <typename T>
  struct TS {
    void m();
  };
  template <> void TS<int>::m() {}

  template <typename T>
  void tf() {}
  template <> void tf<int>() {}

  struct VS {
    virtual void vm() { }
  };

  struct SVS : public VS {
    void vm() { }
  };
}

void S::m3() {}

static inline void f4() {}
const unsigned int cx = 0;
const unsigned int cy = 0;
int f5() { return cy; }

static int x1;

namespace {
int x2;

struct S2 {
  static int x;
};

  template <typename T>
  struct TS2 {
    static int x;
  };
  template <> int TS2<int>::x;

  template <typename T, typename U> int vt = 0;
  template <typename T> int vt<T, void> = 0;
  template <> int vt<void, void> = 0;
}

namespace PR8841 {


  namespace {
    template <typename T> struct X {
      friend bool operator==(const X&, const X&) { return false; }
    };
  }
  template <typename T> void template_test(X<T> x) {
    (void)(x == x);
  }
  void test() {
    X<int> x;
    template_test(x);
  }
}

namespace test4 {
  namespace { struct A {}; }

  void test(A a);
  extern "C" void test4(A a);
}

namespace rdar8733476 {
static void foo() {}
template <typename T> static void foo_t() {}
template <> void foo_t<int>() {}

template <int>
void bar() {
  foo();
  foo_t<int>();
  foo_t<void>();
}
}

namespace test5 {
  static int n = 0;
  static int &r = n;
  int f(int &);
  int k = f(r);


  static const int m = n;
  int x = sizeof(m);
  static const double d = 0.0;
  int y = sizeof(d);

  namespace {
  template <typename T> const double var_t = 0;
  template <> const double var_t<int> = 0;
  int z = sizeof(var_t<int>);
  }
}

namespace unused_nested {
  class outer {
    void func1();
    struct {
      void func2() {
      }
    } x;
  };
}

namespace unused {
  struct {
    void func() {
    }
  } x;
}

namespace test6 {
  typedef struct {
    void bar();
  } A;

  typedef struct {
    void bar();
  } *B;

  struct C {
    void bar();
  };
}

namespace pr14776 {
  namespace {
    struct X {};
  }
  X a = X();
  auto b = X();
}

namespace UndefinedInternalStaticMember {
  namespace {
    struct X {
      static const unsigned x = 3;
      int y[x];
    };
  }
}

namespace test8 {
static void func();
void bar() { void func() __attribute__((used)); }
static void func() {}
}

namespace test9 {
template <typename T>
static void completeRedeclChainForTemplateSpecialization() {}
}

namespace test10 {


template<class T>
constexpr T pi = T(3.14);

}

namespace pr19713 {


static constexpr int constexpr3() { return 1; }
constexpr int constexpr4() { return 2; }

}
