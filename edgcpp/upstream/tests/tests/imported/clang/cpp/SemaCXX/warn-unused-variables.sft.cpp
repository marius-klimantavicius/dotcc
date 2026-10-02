//type: fp
//options: 
# 1 "SemaCXX/warn-unused-variables.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 482 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/warn-unused-variables.cpp" 2
# 11 "SemaCXX/warn-unused-variables.cpp"
template<typename T> void f() {
  T t;
  t = 17;
}


struct A { A(); };
struct B { ~B(); };
void f() {
  A a;
  B b;
}


namespace PR5531 {
  struct A {
  };

  struct B {
    B(int);
  };

  struct C {
    ~C();
  };

  void test() {
    A();
    B(17);
    C();
  }
}

template<typename T>
struct X0 { };

template<typename T>
void test_dependent_init(T *p) {
  X0<int> i(p);
  (void)i;
}

void unused_local_static() {
  static int x = 0;
  static int y = 0;
#pragma unused(x)
  static __attribute__((used)) int z;
  static __attribute__((unused)) int w;
  [[maybe_unused]] static int v;
}


namespace PR10168 {


  template<typename T>
  struct S {
    void f() {
      int a;
      T b;
    }
  };

  template<typename T>
  void f() {
    int a;
    T b;
  }

  void g() {
    S<int>().f();
    S<char>().f();
    f<int>();
    f<char>();
  }
}

namespace PR11550 {
  struct S1 {
    S1();
  };
  S1 makeS1();
  void testS1(S1 a) {

    S1 x = makeS1();


    S1 y;


    S1 z = a;
  }


  void foo();
  struct S2 {
    S2() {
      foo();
    }
  };
  S2 makeS2();
  void testS2(S2 a) {
    S2 x = makeS2();
    S2 y;
    S2 z = a;
  }


  struct S3 {
    S1 m;
  };
  S3 makeS3();
  void testS3(S3 a) {
    S3 x = makeS3();
    S3 y;
    S3 z = a;
  }
}

namespace PR19305 {
  template<typename T> int n = 0;
  int a = n<int>;

  template<typename T> const int l = 0;
  int b = l<int>;


  template<typename T> const int o = 0;
  template<typename T> const int o<T*> = 0;
  int c = o<int*>;

  template<> int o<void> = 0;
  int d = o<void>;


  template<typename T> int m = 0;
  template<typename T> int m<T*> = 0;



  template<> const int m<void> = 0;
}

namespace ctor_with_cleanups {
  struct S1 {
    ~S1();
  };
  struct S2 {
    S2(const S1&);
  };
  void func() {
    S2 s((S1()));
  }
}

# 1 "SemaCXX/Inputs/warn-unused-variables.h" 1


namespace PR15558 {
namespace {
class A {};
}

class B {
  static A a;
  static A b;
  static const int x = sizeof(b);
};
}
# 167 "SemaCXX/warn-unused-variables.cpp" 2

class NonTriviallyDestructible {
public:
  ~NonTriviallyDestructible() {}
};

namespace arrayRecords {

struct Foo {
  int x;
  Foo(int x) : x(x) {}
};

struct Elidable {
  Elidable();
};

void foo(int size) {
  Elidable elidable;
  Elidable elidableArray[2];
  Elidable elidableDynArray[size];
  Elidable elidableNestedArray[1][2][3];

  NonTriviallyDestructible scalar;
  NonTriviallyDestructible array[2];
  NonTriviallyDestructible nestedArray[2][2];


  Foo fooScalar = 1;
  Foo fooArray[] = {1,2};
  Foo fooNested[2][2] = { {1,2}, {3,4} };
}

template<int N>
void bar() {
  NonTriviallyDestructible scaler;
  NonTriviallyDestructible array[N];
}

void test() {
  foo(10);
  bar<2>();
}

}


namespace with_constexpr {
template <typename T>
struct Literal {
  T i;
  Literal() = default;
  constexpr Literal(T i) : i(i) {}
};

struct NoLiteral {
  int i;
  NoLiteral() = default;
  constexpr NoLiteral(int i) : i(i) {}
  ~NoLiteral() {}
};

static Literal<int> gl1;
static Literal<int> gl2(1);
static const Literal<int> gl3(0);

template <typename T>
void test(int i) {
  Literal<int> l1;
  Literal<int> l2(42);
  Literal<int> l3(i);
  Literal<T> l4(0);
  NoLiteral nl1;
  NoLiteral nl2(42);
}
}

namespace crash {
struct a {
  a(const char *);
};
template <typename b>
void c() {
  a d(b::e ? "" : "");
}
}


namespace dependent_ctor {
struct S {
  S() = default;
  S(const S &) = default;
  S(int);
};

template <typename T>
void foo(T &t) {
  S s{t};
}
}



namespace gh54489 {

void f() {
  const auto &a = NonTriviallyDestructible();
  const auto &b = a;

  const auto &&c = NonTriviallyDestructible();
  auto &&d = c;

}

struct S {
  S() = default;
  S(const S &) = default;
  S(int);
};

template <typename T>
void foo(T &t) {
  const auto &extended = S{t};
}

void test_foo() {
  int i;
  foo(i);
}

struct RAIIWrapper {
  RAIIWrapper();
  ~RAIIWrapper();
};

void RAIIWrapperTest() {
  auto const guard = RAIIWrapper();
  auto const &guard2 = RAIIWrapper();
  auto &&guard3 = RAIIWrapper();
}

}




namespace gh79518 {

struct S {
    S(int);
};


struct A {
  int x;
  A(int x) : x(x) {}
};

void foo() {
    S s(0);
    S s2 = 0;
    S s3{0};

    A a = 1;
}

}
