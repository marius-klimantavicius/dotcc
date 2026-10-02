//type: fn
//options:  --c++11 --exceptions: --c++11 -DMSABI
# 1 "SemaCXX/destructor.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 452 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/destructor.cpp" 2
# 32 "SemaCXX/destructor.cpp"
# 1 "SemaCXX/destructor.cpp" 1
# 10 "SemaCXX/destructor.cpp" 3
namespace dnvd {

struct SystemB {
  virtual void foo();
};

template <typename T>
class simple_ptr {
public:
  simple_ptr(T* t): _ptr(t) {}
  ~simple_ptr() { delete _ptr; }


  T& operator*() const { return *_ptr; }
private:
  T* _ptr;
};
}
# 33 "SemaCXX/destructor.cpp" 2

class A {
public:
  ~A();
};

class B {
public:
  ~B() { }
};

class C {
public:
  (~C)() { }
};

struct D {
  static void ~D(int, ...) const { }






};

struct D2 {
  void ~D2() { }

};


struct E;

typedef E E_typedef;
struct E {
  ~E_typedef();
};

struct F {
  (~F)();
  ~F();
};

~;
~undef();
~operator+(int, int);
~F(){}

struct G {
  ~G();
};

G::~G() { }

struct H {
  ~H(void) { }
};

struct X {};

struct Y {
  ~X();
};

namespace PR6421 {
  class T;

  class QGenericArgument
  {
    template<typename U>
    void foo(T t)
    { }

    void disconnect()
    {
      T* t;
      bob<QGenericArgument>(t);
    }
  };
}

namespace PR6709 {






  template<class T> class X { T v; ~X() { ++*v; } };
  void a(X<int> x) {}
}

struct X0 { virtual ~X0() throw(); };
struct X1 : public X0 { };



namespace test6 {
  template <class T> class A {
  public:
    void *operator new(long unsigned int);
    void operator delete(void *p) {
      T::deleteIt(p);
    }




    virtual ~A() {}
  };




  class B : A<int> { B(); };
  B::B() {}
}



namespace test7 {
  struct A {
    ~A() const;
  };
  struct B : A {};

  void test() {
    B *b;
    b->~B();
  }
}

namespace nonvirtualdtor {
struct S1 {
  virtual void m();
};

struct S2 {
  ~S2();
  virtual void m();
};

struct S3 : public S1 {
  virtual void m();
};

struct S4 : public S2 {
  virtual void m();
};

struct B {
  virtual ~B();
  virtual void m();
};

struct S5 : public B {
  virtual void m();
};

struct S6 {
  virtual void m();
private:
  ~S6();
};

struct S7 {
  virtual void m();
protected:
  ~S7();
};

struct S8 {} s8;

UnknownType S8::~S8() {
  s8.~S8();
}

template<class T> class TS : public B {
  virtual void m();
};

TS<int> baz;

template<class T> class TS2 {
  virtual void m();
};

TS2<int> foo;
}

namespace dnvd {
struct NP {};

struct B {
  virtual void foo();
};

struct D: B {};

struct F final : B {};

struct VB {
  virtual void foo();
  virtual ~VB();
};

struct VD: VB {};

struct VF final: VB {};

template <typename T>
class simple_ptr2 {
public:
  simple_ptr2(T* t): _ptr(t) {}
  ~simple_ptr2() { delete _ptr; }
  T& operator*() const { return *_ptr; }
private:
  T* _ptr;
};

void use(B&);
void use(SystemB&);
void use(VB&);

void nowarnstack() {
  B b; use(b);
  D d; use(d);
  F f; use(f);
  VB vb; use(vb);
  VD vd; use(vd);
  VF vf; use(vf);
}

void nowarnnonpoly() {
  {
    NP* np = new NP();
    delete np;
  }
  {
    NP* np = new NP[4];
    delete[] np;
  }
}


void nowarnarray() {
  {
    B* b = new B[4];
    delete[] b;
  }
  {
    D* d = new D[4];
    delete[] d;
  }
  {
    VB* vb = new VB[4];
    delete[] vb;
  }
  {
    VD* vd = new VD[4];
    delete[] vd;
  }
}

template <typename T>
void nowarntemplate() {
  {
    T* t = new T();
    delete t;
  }
  {
    T* t = new T[4];
    delete[] t;
  }
}

void nowarn0() {
  {
    F* f = new F();
    delete f;
  }
  {
    VB* vb = new VB();
    delete vb;
  }
  {
    VB* vb = new VD();
    delete vb;
  }
  {
    VD* vd = new VD();
    delete vd;
  }
  {
    VF* vf = new VF();
    delete vf;
  }
}

void nowarn0_explicit_dtor(F* f, VB* vb, VD* vd, VF* vf) {
  f->~F();
  f->~F();
  vb->~VB();
  vd->~VD();
  vf->~VF();
}

void warn0() {
  {
    B* b = new B();
    delete b;
  }
  {
    B* b = new D();
    delete b;
  }
  {
    D* d = new D();
    delete d;
  }
}


template <class>
struct __is_destructible_apply { typedef int type; };
struct __two {char __lx[2];};
template <typename _Tp>
struct __is_destructor_wellformed {
  template <typename _Tp1>
  static char __test(typename __is_destructible_apply<
                       decltype(_Tp1().~_Tp1())>::type);
  template <typename _Tp1>
  static __two __test (...);

  static const bool value = sizeof(__test<_Tp>(12)) == sizeof(char);
};

void warn0_explicit_dtor(B* b, B& br, D* d) {
  b->~B();
  b->B::~B();


  (void)__is_destructor_wellformed<B>::value;

  br.~B();
  br.B::~B();

  d->~D();
  d->D::~D();
}

void nowarn1() {
  {
    simple_ptr<F> f(new F());
    use(*f);
  }
  {
    simple_ptr<VB> vb(new VB());
    use(*vb);
  }
  {
    simple_ptr<VB> vb(new VD());
    use(*vb);
  }
  {
    simple_ptr<VD> vd(new VD());
    use(*vd);
  }
  {
    simple_ptr<VF> vf(new VF());
    use(*vf);
  }
  {
    simple_ptr<SystemB> sb(new SystemB());
    use(*sb);
  }
}

void warn1() {
  {
    simple_ptr<B> b(new B());
    use(*b);
  }
  {
    simple_ptr2<B> b(new D());
    use(*b);
  }
  {
    simple_ptr<D> d(new D());
    use(*d);
  }
}
}

namespace PR9238 {
  class B { public: ~B(); };
  class C : virtual B { public: ~C() { } };
}

namespace PR7900 {
  struct A {
  };
  struct B : public A {
  };
  void foo() {
    B b;
    b.~B();
    b.~A();
    (&b)->~A();
  }
}

namespace PR16892 {
  auto p = &A::~A;
}

namespace PR20238 {
struct S {
  volatile ~S() { }
};
}

namespace PR22668 {
struct S {
};
void f(S s) {
  (s.~S)();
}
void g(S s) {
  (s.~S);
}
}

class Invalid {
    ~Invalid();
    UnknownType xx;
};


Invalid::~Invalid() {}

namespace PR30361 {
template <typename T>
struct C1 {
  ~C1() {}
  operator C1<T>* () { return nullptr; }
  void foo1();
};

template<typename T>
void C1<T>::foo1() {
  C1::operator C1<T>*();
  C1::~C1();
}

void foo1() {
  C1<int> x;
  x.foo1();
}
}

namespace DtorTypedef {
  struct A { ~A(); };
  using A = A;
  DtorTypedef::A::~A() {}


  struct B { ~B(); };
  namespace N { using B = B; }
  N::B::~B() {}

#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wdtor-typedef"
  struct C { ~C(); };
  namespace N { using C = C; }
  N::C::~C() {}
#pragma clang diagnostic pop
}





namespace PR44978 {


  namespace n {
    class Foo {};
  }
  class Foo {};
  using namespace n;
  static void func(n::Foo *p) { p->~Foo(); }



  struct Z;
  struct X { using T = Z; };
  struct Y { using T = int; };
  struct Z : X, Y {};
  void f(Z *p) { p->~T(); }





  using T = Z;
  void g(Z *p) { p->~T(); }


  struct Q {};
  namespace { using U = Q; }
  using U = int;
  void f(Q *p) { p->~U(); }


  template<typename T> void f(T *p) { p->~U(); }
}

namespace crash_on_invalid_base_dtor {
struct Test {
  virtual ~Test();
};
struct Baz : public Test {
  Baz() {}
  ~Baz() = defaul;
};
struct Foo : public Baz {
  Foo() {}
};
}

namespace GH89544 {
class Foo {
  ~Foo() = {}


};

static_assert(!__is_trivially_constructible(Foo), "");
static_assert(!__is_trivially_constructible(Foo, const Foo &), "");
static_assert(!__is_trivially_constructible(Foo, Foo &&), "");
}

namespace GH97230 {
struct X {
  ~X() = defaul;
};
struct Y : X {} y1{ };
}

namespace GH121706 {
struct A {
  *&~A();
};

struct B {
  *&&~B();
};

struct C {
  *const ~C();
};

struct D {
  *const * ~D();
};

struct E {
  *E::*~E();
};

struct F {
  *F::*const ~F();
};

struct G {
  ****~G();
};

struct H {
  **~H();
};

struct I {
  *~I();
};

struct J {
  *&~J();
};

struct K {
  **&&~K();
};
}
