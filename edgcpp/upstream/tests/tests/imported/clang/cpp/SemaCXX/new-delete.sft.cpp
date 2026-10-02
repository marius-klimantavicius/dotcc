//type: fn
//options:  --c++03: --c++11: --c++14: --c++17: --c++20: --c++23: --c++26 --c++03 -DNEW_INTERP: --c++11 -DNEW_INTERP: --c++14 -DNEW_INTERP: --c++17 -DNEW_INTERP: --c++20 -DNEW_INTERP: --c++23 -DNEW_INTERP: --c++26 -DNEW_INTERP
# 1 "SemaCXX/new-delete.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 431 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/new-delete.cpp" 2
# 20 "SemaCXX/new-delete.cpp"
# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 1
# 84 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h"
# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_header_macro.h" 1
# 85 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 2



# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_ptrdiff_t.h" 1
# 18 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_ptrdiff_t.h"
typedef long int ptrdiff_t;
# 89 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 2




# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_size_t.h" 1
# 18 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_size_t.h"
typedef long unsigned int size_t;
# 94 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 2
# 103 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h"
# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_wchar_t.h" 1
# 104 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 2




# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_null.h" 1
# 109 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 2




# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_nullptr_t.h" 1
# 114 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 2
# 128 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h"
# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_offsetof.h" 1
# 129 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 2
# 21 "SemaCXX/new-delete.cpp" 2




struct S
{
  S(int, int, double);
  S(double, int);
  S(float, int);
};
struct T;
struct U
{

  void* operator new(size_t, S*);
};
struct V : U
{
};
struct W
{
  int a;
  int b;
};

inline void operator delete(void *);

__attribute__((used))
inline void *operator new(size_t) {
  return 0;
}


void* operator new(const size_t);
void* operator new(size_t, int*);
void* operator new(size_t, float*);
void* operator new(size_t, S);

struct foo { };

void good_news()
{
  int *pi = new int;
  float *pf = new (pi) float();
  pi = new int(1);
  pi = new int('c');
  const int *pci = new const int();
  S *ps = new S(1, 2, 3.4);
  ps = new (pf) (S)(1, 2, 3.4);
  S *(*paps)[2] = new S*[*pi][2];
  typedef int ia4[4];
  ia4 *pai = new (int[3][4]);
  pi = ::new int;
  U *pu = new (ps) U;
  V *pv = new (ps) V;

  pi = new (S(1.0f, 2)) int;

  (void)new int[true];


  typedef int a[2];
  foo* f1 = new foo;
  foo* f2 = new foo[2];
  typedef foo x[2];
  typedef foo y[2][2];
  x* f3 = new y;






}

struct abstract {
  virtual ~abstract() = 0;
};

void bad_news(int *ip)
{
  int i = 1;
  (void)new;
  (void)new 4;
  (void)new () int;
  (void)new int[1.1];
# 115 "SemaCXX/new-delete.cpp"
  (void)new int[1][i];
  (void)new (int[1][i]);







  (void)new (int[i]);
  (void)new int(*(S*)0);
  (void)new int(1, 2);
  (void)new S(1);
  (void)new S(1, 1);
  (void)new const int;
  (void)new float*(ip);

  (void)new int[-1];





  (void)new int[2000000000];
  (void)new int[*(S*)0];
# 148 "SemaCXX/new-delete.cpp"
  (void)::S::new int;
  (void)new (0, 0) int;
  (void)new (0L) int;

  (void)::new ((S*)0) U;

  (void)new U;
  (void)new (int[]);
  (void)new int&;
  (void)new int[];
  (void)new int[]();


  (void)new int[]{};

}

void no_matching_placement_new() {
  struct X { int n; };
  __attribute__((aligned(__alignof(X)))) unsigned char buffer[sizeof(X)];
  (void)new(buffer) X;
  (void)new(+buffer) X;
  (void)new(&buffer) X;
}

void const_placement_new() {
  const int value = 42;
  (void)new(&value) int;
  struct X { int n; };
  const X cx = {5};
  (void)new(&cx) X{10};
  const X* const cx2 = 0;
  (void)new(cx2) X{10};
  const int arr[1] = {1};
  (void)new(&arr[0]) int(10);
  const void* ptr = 0;
  (void)new(ptr) int;
  const int complex_arr[5][3] = {};
  (void)new(&complex_arr[0][0]) int;
  (void)new(complex_arr[0]) int;
  const char str[] = "test";
  (void)new(str) int;
  const int* const* ptr_to_const_ptr_to_const = 0;
  (void)new(ptr_to_const_ptr_to_const) int;
  int* const* ptr_to_const_ptr = 0;
  (void)new(ptr_to_const_ptr) int;
  typedef const int* ConstIntPtr;
  ConstIntPtr cip = 0;
  (void)new(cip) int;
  typedef const void* ConstVoidPtr;
}

void const_placement_new_param(const void* ptr) {
  new (ptr) int;
}

template<typename T>
void const_template_placement_new(const T* storage) {
  (void)new(storage) int;
}

void const_template_placement_new_instantiation() {
  int x = 5;
  const_template_placement_new(&x);
}

void good_deletes()
{
  delete (int*)0;
  delete [](int*)0;
  delete (S*)0;
  ::delete (int*)0;
}

void bad_deletes()
{
  delete 0;
  delete [0] (int*)0;
  delete (void*)0;


  delete (T*)0;


  ::S::delete (int*)0;
}

struct X0 { };

struct X1 {
  operator int*();
  operator float();
};

struct X2 {
  operator int*();
  operator float*();
};

void test_delete_conv(X0 x0, X1 x1, X2 x2) {
  delete x0;
  delete x1;
  delete x2;
}


class X3 {
public:
  static void operator delete(void * mem, size_t size);
};

class X4 {
public:
  static void release(X3 *x);
  static void operator delete(void * mem, size_t size);
};


void X4::release(X3 *x) {
  delete x;
}

class X5 {
public:
  void Destroy() const { delete this; }
};

class Base {
public:
  static void *operator new(signed char) throw();
  static int operator new[] (size_t) throw();
};

class Tier {};
class Comp : public Tier {};

class Thai : public Base {
public:
  Thai(const Tier *adoptDictionary);
};

void loadEngineFor() {
  const Comp *dict;
  new Thai(dict);
}

template <class T> struct TBase {
  void* operator new(T size, int);
};

TBase<int> t1;

class X6 {
public:
  static void operator delete(void*, int);
};

class X7 {
public:
  static void operator delete(void*, int);
};

class X8 : public X6, public X7 {
};

void f(X8 *x8) {
  delete x8;
}

class X9 {
public:
  static void operator delete(void*, int);
  static void operator delete(void*, float);
};

void f(X9 *x9) {
  delete x9;
}

struct X10 {
  virtual ~X10();



};

struct X11 : X10 {






  void operator delete(void*, int);



};

void f() {
  X11 x11;





}

struct X12 {
  void* operator new(size_t, void*);
};

struct X13 : X12 {
  using X12::operator new;
};

static void* f(void* g)
{
    return new (g) X13();
}

class X14 {
public:
  static void operator delete(void*, const size_t);
};

void f(X14 *x14a, X14 *x14b) {
  delete x14a;
}

class X15 {
private:
  X15();
  ~X15();
};

void f(X15* x) {
  new X15();
  delete x;
}

namespace PR5918 {
  struct S { template<typename T> static void* operator new(size_t, T); };
  void test() {
    (void)new(0) S;
  }
}

namespace Test1 {

void f() {
  (void)new int[10](1, 2);

  typedef int T[10];
  (void)new T(1, 2);
}

template<typename T>
void g(unsigned i) {
  (void)new T[1](i);
}

template<typename T>
void h(unsigned i) {
  (void)new T(i);
}
template void h<unsigned>(unsigned);
template void h<unsigned[10]>(unsigned);

void i() {
  new W[2](1, 2, 3);

}

}


namespace PR7436 {
struct S1 {
  void* operator new(size_t);
  void operator delete(void* p);

private:
  void* operator new(size_t, void*);
  void operator delete(void*, void*);
};
class S2 {
  void* operator new(size_t);
  void operator delete(void* p);
};

void test(S1* s1, S2* s2) {
  delete s1;
  delete s2;
  (void)new S1();
  (void)new (0L) S1();
  (void)new S2();
}
}

namespace rdar8018245 {
  struct X0 {
    static const int value = 17;
  };

  const int X0::value;

  struct X1 {
    static int value;
  };

  int X1::value;

  template<typename T>
  int *f() {
    return new (int[T::value]);
  }

  template int *f<X0>();
  template int *f<X1>();

}

namespace Instantiate {
  template<typename T> struct X {
    operator T*();
  };

  void f(X<int> &xi) {
    delete xi;
  }
}

namespace PR7810 {
  struct X {

    static void operator delete(void *const);
  };
  struct Y {


    static void operator delete(void *volatile);



  };
}


namespace TemplateDestructors {
  struct S {
    virtual ~S() {}

    void* operator new(const size_t size);
    template<class T> void* operator new(const size_t, const int, T*);
    void operator delete(void*, const size_t);
    template<class T> void operator delete(void*, const size_t, const int, T*);
  };
}

namespace DeleteParam {
  struct X {
    void operator delete(X*);
  };

  struct Y {
    void operator delete(void* const);
  };
}



namespace PairedDelete {
  template <class T> struct A {
    A();
    void *operator new(size_t s, double d = 0);
    void operator delete(void *p, double d);
    void operator delete(void *p) {
      T::dealloc(p);
    }
  };

  A<int> *test() {
    return new A<int>();
  }
}

namespace PR7702 {
  void test1() {
    new DoesNotExist;
  }
}

namespace ArrayNewNeedsDtor {
  struct A { A(); private: ~A(); };



  struct B { B(); A a; };






  B *test9() {
    return new B[5];





  }
}

namespace DeleteIncompleteClass {
  struct A;
  extern A x;
  void f() { delete x; }
}

namespace DeleteIncompleteClassPointerError {
  struct A;
  void f(A *x) { 1+delete x; }



}

namespace PR10504 {
  struct A {
    virtual void foo() = 0;
  };
  void f(A *x) { delete x; }
}
# 600 "SemaCXX/new-delete.cpp"
struct PlacementArg {};
inline void *operator new[](size_t, const PlacementArg &) throw () {
  return 0;
}
inline void operator delete[](void *, const PlacementArg &) throw () {
}

namespace r150682 {

  template <typename X>
  struct S {
    struct Inner {};
    S() { new Inner[1]; }
  };

  struct T {
  };

  template<typename X>
  void tfn() {
    new (*(PlacementArg*)0) T[1];
  }

  void fn() {
    tfn<int>();
  }

}

namespace P12023 {
  struct CopyCounter
  {
      CopyCounter();
      CopyCounter(const CopyCounter&);
  };

  int main()
  {
    CopyCounter* f = new CopyCounter[10](CopyCounter());
      return 0;
  }
}

namespace PR12061 {
  template <class C> struct scoped_array {
    scoped_array(C* p = __null);
  };
  template <class Payload> struct Foo {
    Foo() : a_(new scoped_array<int>[5]) { }
    scoped_array< scoped_array<int> > a_;
  };
  class Bar {};
  Foo<Bar> x;

  template <class C> struct scoped_array2 {
    scoped_array2(C* p = __null, C* q = __null);
  };
  template <class Payload> struct Foo2 {
    Foo2() : a_(new scoped_array2<int>[5]) { }
    scoped_array2< scoped_array2<int> > a_;
  };
  class Bar2 {};
  Foo2<Bar2> x2;

  class MessageLoop {
  public:
    explicit MessageLoop(int type = 0);
  };
  template <class CookieStoreTestTraits>
  class CookieStoreTest {
  protected:
    CookieStoreTest() {
      new MessageLoop;
    }
  };
  struct CookieMonsterTestTraits {
  };
  class DeferredCookieTaskTest : public CookieStoreTest<CookieMonsterTestTraits>
  {
    DeferredCookieTaskTest() {}
  };
}

class DeletingPlaceholder {
  int* f() {
    delete f;
    return 0;
  }
  int* g(int, int) {
    delete g;
    return 0;
  }
};

namespace PR18544 {
  inline void *operator new(size_t);
}


inline void* operator new();

namespace {
template <class C>
struct A {
  void f() { this->::new; }
  void g() { this->::delete; }
};
}
# 720 "SemaCXX/new-delete.cpp"
int (*const_fold)[12] = new int[3][&const_fold + 12 - &const_fold];
