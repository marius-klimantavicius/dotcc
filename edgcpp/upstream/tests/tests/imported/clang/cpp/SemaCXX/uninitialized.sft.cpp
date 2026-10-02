//type: fp
//options:  --c++17: --c++17: --c++20
# 1 "SemaCXX/uninitialized.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 479 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/uninitialized.cpp" 2
# 20 "SemaCXX/uninitialized.cpp"
# 1 "SemaCXX/uninitialized.cpp" 1
# 10 "SemaCXX/uninitialized.cpp" 3
namespace std {
template <class T, class... U>
constexpr T* construct_at(T* ptr, U&&... args) {
  return ::new (static_cast<void*>(ptr)) T(static_cast<U&&>(args)...);
}
}
# 21 "SemaCXX/uninitialized.cpp" 2

void* operator new(long unsigned int, void*);


namespace std {
inline namespace foo {
template <class T> struct remove_reference { typedef T type; };
template <class T> struct remove_reference<T&> { typedef T type; };
template <class T> struct remove_reference<T&&> { typedef T type; };

template <class T> typename remove_reference<T>::type&& move(T&& t);
}
}

int foo(int x);
int bar(int* x);
int boo(int& x);
int far(const int& x);
int moved(int&& x);
int &ref(int x);



int a = a;
int b = b + 1;
int c = (c + c);
int e = static_cast<long>(e) + 1;
int f = foo(f);


int g = sizeof(g);
void* ptr = &ptr;
int h = bar(&h);
int i = boo(i);
int j = far(j);
int k = __alignof__(k);

int l = k ? l : l;
int m = 1 + (k ? m : m);
int n = -n;
int o = std::move(o);
const int p = std::move(p);
int q = moved(std::move(q));
int r = std::move((p ? q : (18, r)));
int s = r ?: s;
int t = t ?: s;
int u = (foo(u), s);
int v = (u += v);
int w = (w += 10);
int x = x++;
int y = ((s ? (y, v) : (77, y))++, sizeof(y));
int z = ++ref(z);
int aa = (ref(aa) += 10);
int bb = bb ? x : y;

void test_stuff () {
  int a = a;
  int b = b + 1;
  int c = (c + c);
  int d = ({ d + d ;});
  int e = static_cast<long>(e) + 1;
  int f = foo(f);


  int g = sizeof(g);
  void* ptr = &ptr;
  int h = bar(&h);
  int i = boo(i);
  int j = far(j);
  int k = __alignof__(k);

  int l = k ? l : l;
  int m = 1 + (k ? m : m);
  int n = -n;
  int o = std::move(o);
  const int p = std::move(p);
  int q = moved(std::move(q));
  int r = std::move((p ? q : (18, r)));
  int s = r ?: s;
  int t = t ?: s;
  int u = (foo(u), s);
  int v = (u += v);
  int w = (w += 10);
  int x = x++;
  int y = ((s ? (y, v) : (77, y))++, sizeof(y));
  int z = ++ref(z);
  int aa = (ref(aa) += 10);
  int bb = bb ? x : y;

  for (;;) {
    int a = a;
    int b = b + 1;
    int c = (c + c);
    int d = ({ d + d ;});
    int e = static_cast<long>(e) + 1;
    int f = foo(f);


    int g = sizeof(g);
    void* ptr = &ptr;
    int h = bar(&h);
    int i = boo(i);
    int j = far(j);
    int k = __alignof__(k);

    int l = k ? l : l;
    int m = 1 + (k ? m : m);
    int n = -n;
    int o = std::move(o);
    const int p = std::move(p);
    int q = moved(std::move(q));
    int r = std::move((p ? q : (18, r)));
    int s = r ?: s;
    int t = t ?: s;
    int u = (foo(u), s);
    int v = (u += v);
    int w = (w += 10);
    int x = x++;
    int y = ((s ? (y, v) : (77, y))++, sizeof(y));
    int z = ++ref(z);
    int aa = (ref(aa) += 10);
    int bb = bb ? x : y;

  }
}

void test_comma() {
  int a;
  int b = (a, a ?: 2);
  int c = (a, a, b, c);
  int d;
  int e = (foo(d), e, b);
  int f;
  f = f + 1, 2;
  int h;
  int g = (h, g, 2);
}

namespace member_ptr {
struct A {
  int x;
  int y;
  A(int x) : x{x} {}
};

void test_member_ptr() {
  int A::* px = &A::x;
  A a{a.*px};
  A b = b;
}
}

namespace const_ptr {
void foo(int *a);
void bar(const int *a);
void foobar(const int **a);

void test_const_ptr() {
  int a;
  int b;
  foo(&a);
  bar(&b);
  b = a + b;
  int *ptr;
  const int *ptr2;
  foo(ptr);
  foobar(&ptr2);
  int *ptr3;
  const int *ptr4;
  bar(ptr3);
  bar(ptr4);
}
}


struct S {
  int x;
  int y;
  const int z = 5;
  void *ptr;

  S(bool (*)[1]) : x(x) {}
  S(bool (*)[2]) : x(x + 1) {}
  S(bool (*)[3]) : x(x + x) {}
  S(bool (*)[4]) : x(static_cast<long>(x) + 1) {}
  S(bool (*)[5]) : x(foo(x)) {}


  S(char (*)[1]) : x(sizeof(x)) {}
  S(char (*)[2]) : ptr(&ptr) {}
  S(char (*)[3]) : x(bar(&x)) {}
  S(char (*)[4]) : x(boo(x)) {}
  S(char (*)[5]) : x(far(x)) {}
  S(char (*)[6]) : x(__alignof__(x)) {}

  S(int (*)[1]) : x(0), y(x ? y : y) {}
  S(int (*)[2]) : x(0), y(1 + (x ? y : y)) {}
  S(int (*)[3]) : x(-x) {}
  S(int (*)[4]) : x(std::move(x)) {}
  S(int (*)[5]) : z(std::move(z)) {}
  S(int (*)[6]) : x(moved(std::move(x))) {}
  S(int (*)[7]) : x(0), y(std::move((x ? x : (18, y)))) {}
  S(int (*)[8]) : x(0), y(x ?: y) {}
  S(int (*)[9]) : x(0), y(y ?: x) {}
  S(int (*)[10]) : x(0), y((foo(y), x)) {}
  S(int (*)[11]) : x(0), y(x += y) {}
  S(int (*)[12]) : x(x += 10) {}
  S(int (*)[13]) : x(x++) {}
  S(int (*)[14]) : x(0), y(((x ? (y, x) : (77, y))++, sizeof(y))) {}
  S(int (*)[15]) : x(++ref(x)) {}
  S(int (*)[16]) : x((ref(x) += 10)) {}
  S(int (*)[17]) : x(0), y(y ? x : x) {}
};


class A {

  public:
    enum count { ONE, TWO, THREE };
    int num;
    static int count;
    int get() const { return num; }
    int get2() { return num; }
    int set(int x) { num = x; return num; }
    static int zero() { return 0; }

    A() {}
    A(A const &a) {}
    A(int x) {}
    A(int *x) {}
    A(A *a) {}
    A(A &&a) {}
    ~A();
    bool operator!();
    bool operator!=(const A&);
};

bool operator!=(int, const A&);

A getA() { return A(); }
A getA(int x) { return A(); }
A getA(A* a) { return A(); }
A getA(A a) { return A(); }
A moveA(A&& a) { return A(); }
A const_refA(const A& a) { return A(); }

void setupA(bool x) {
  A a1;
  a1.set(a1.get());
  A a2(a1.get());
  A a3(a1);
  A a4(&a4);
  A a5(a5.zero());
  A a6(a6.ONE);
  A a7 = getA();
  A a8 = getA(a8.TWO);
  A a9 = getA(&a9);
  A a10(a10.count);

  A a11(a11);
  A a12(a12.get());
  A a13(a13.num);
  A a14 = A(a14);
  A a15 = getA(a15.num);
  A a16(&a16.num);
  A a17(a17.get2());
  A a18 = x ? a18 : a17;
  A a19 = getA(x ? a19 : a17);
  A a20{a20};
  A a21 = {a21};



  A *a22 = new A(a22->count);
  A *a23 = new A(a23->ONE);
  A *a24 = new A(a24->TWO);
  A *a25 = new A(a25->zero());

  A *a26 = new A(a26->get());
  A *a27 = new A(a27->get2());
  A *a28 = new A(a28->num);

  const A a29(a29);
  const A a30 = a30;

  A a31 = std::move(a31);
  A a32 = moveA(std::move(a32));
  A a33 = A(std::move(a33));
  A a34(std::move(a34));
  A a35 = std::move(x ? a34 : (37, a35));

  A a36 = const_refA(a36);
  A a37(const_refA(a37));

  A a38({a38});
  A a39 = {a39};
  A a40 = A({a40});

  A a41 = !a41;
  A a42 = !(a42);
  A a43 = a43 != a42;
  A a44 = a43 != a44;
  A a45 = a45 != a45;
  A a46 = 0 != a46;

  A a47(a47.set(a47.num));
  A a48(a47.set(a48.num));
  A a49(a47.set(a48.num));
}

bool cond;

A a1;
A a2(a1.get());
A a3(a1);
A a4(&a4);
A a5(a5.zero());
A a6(a6.ONE);
A a7 = getA();
A a8 = getA(a8.TWO);
A a9 = getA(&a9);
A a10(a10.count);

A a11(a11);
A a12(a12.get());
A a13(a13.num);
A a14 = A(a14);
A a15 = getA(a15.num);
A a16(&a16.num);
A a17(a17.get2());
A a18 = cond ? a18 : a17;
A a19 = getA(cond ? a19 : a17);
A a20{a20};
A a21 = {a21};

A *a22 = new A(a22->count);
A *a23 = new A(a23->ONE);
A *a24 = new A(a24->TWO);
A *a25 = new A(a25->zero());

A *a26 = new A(a26->get());
A *a27 = new A(a27->get2());
A *a28 = new A(a28->num);

const A a29(a29);
const A a30 = a30;

A a31 = std::move(a31);
A a32 = moveA(std::move(a32));
A a33 = A(std::move(a33));
A a34(std::move(a34));
A a35 = std::move(x ? a34 : (37, a35));

A a36 = const_refA(a36);
A a37(const_refA(a37));

A a38({a38});
A a39 = {a39};
A a40 = A({a40});

A a41 = !a41;
A a42 = !(a42);
A a43 = a43 != a42;
A a44 = a43 != a44;
A a45 = a45 != a45;

A a46 = 0 != a46;

A a47(a47.set(a47.num));
A a48(a47.set(a48.num));
A a49(a47.set(a48.num));

class T {
  A a, a2;
  const A c_a;
  A* ptr_a;

  T() {}
  T(bool (*)[1]) : a() {}
  T(bool (*)[2]) : a2(a.get()) {}
  T(bool (*)[3]) : a2(a) {}
  T(bool (*)[4]) : a(&a) {}
  T(bool (*)[5]) : a(a.zero()) {}
  T(bool (*)[6]) : a(a.ONE) {}
  T(bool (*)[7]) : a(getA()) {}
  T(bool (*)[8]) : a2(getA(a.TWO)) {}
  T(bool (*)[9]) : a(getA(&a)) {}
  T(bool (*)[10]) : a(a.count) {}

  T(bool (*)[11]) : a(a) {}
  T(bool (*)[12]) : a(a.get()) {}
  T(bool (*)[13]) : a(a.num) {}
  T(bool (*)[14]) : a(A(a)) {}
  T(bool (*)[15]) : a(getA(a.num)) {}
  T(bool (*)[16]) : a(&a.num) {}
  T(bool (*)[17]) : a(a.get2()) {}
  T(bool (*)[18]) : a2(cond ? a2 : a) {}
  T(bool (*)[19]) : a2(cond ? a2 : a) {}
  T(bool (*)[20]) : a{a} {}
  T(bool (*)[21]) : a({a}) {}

  T(bool (*)[22]) : ptr_a(new A(ptr_a->count)) {}
  T(bool (*)[23]) : ptr_a(new A(ptr_a->ONE)) {}
  T(bool (*)[24]) : ptr_a(new A(ptr_a->TWO)) {}
  T(bool (*)[25]) : ptr_a(new A(ptr_a->zero())) {}

  T(bool (*)[26]) : ptr_a(new A(ptr_a->get())) {}
  T(bool (*)[27]) : ptr_a(new A(ptr_a->get2())) {}
  T(bool (*)[28]) : ptr_a(new A(ptr_a->num)) {}

  T(bool (*)[29]) : c_a(c_a) {}
  T(bool (*)[30]) : c_a(A(c_a)) {}

  T(bool (*)[31]) : a(std::move(a)) {}
  T(bool (*)[32]) : a(moveA(std::move(a))) {}
  T(bool (*)[33]) : a(A(std::move(a))) {}
  T(bool (*)[34]) : a(A(std::move(a))) {}
  T(bool (*)[35]) : a2(std::move(x ? a : (37, a2))) {}

  T(bool (*)[36]) : a(const_refA(a)) {}
  T(bool (*)[37]) : a(A(const_refA(a))) {}

  T(bool (*)[38]) : a({a}) {}
  T(bool (*)[39]) : a{a} {}
  T(bool (*)[40]) : a({a}) {}

  T(bool (*)[41]) : a(!a) {}
  T(bool (*)[42]) : a(!(a)) {}
  T(bool (*)[43]) : a(), a2(a2 != a) {}
  T(bool (*)[44]) : a(), a2(a != a2) {}
  T(bool (*)[45]) : a(a != a) {}
  T(bool (*)[46]) : a(0 != a) {}

  T(bool (*)[47]) : a2(a2.set(a2.num)) {}
  T(bool (*)[48]) : a2(a.set(a2.num)) {}
  T(bool (*)[49]) : a2(a.set(a.num)) {}

};

struct B {

  int x;
  int *y;
};

B getB() { return B(); };
B getB(int x) { return B(); };
B getB(int *x) { return B(); };
B getB(B *b) { return B(); };
B moveB(B &&b) { return B(); };

B* getPtrB() { return 0; };
B* getPtrB(int x) { return 0; };
B* getPtrB(int *x) { return 0; };
B* getPtrB(B **b) { return 0; };

void setupB(bool x) {
  B b1;
  B b2(b1);
  B b3 = { 5, &b3.x };
  B b4 = getB();
  B b5 = getB(&b5);
  B b6 = getB(&b6.x);


  (void) b2;
  (void) b4;

  B b7(b7);
  B b8 = getB(b8.x);
  B b9 = getB(b9.y);
  B b10 = getB(-b10.x);

  B* b11 = 0;
  B* b12(b11);
  B* b13 = getPtrB();
  B* b14 = getPtrB(&b14);

  (void) b12;
  (void) b13;

  B* b15 = getPtrB(b15->x);
  B* b16 = getPtrB(b16->y);

  B b17 = { b17.x = 5, b17.y = 0 };
  B b18 = { b18.x + 1, b18.y };

  const B b19 = b19;
  const B b20(b20);

  B b21 = std::move(b21);
  B b22 = moveB(std::move(b22));
  B b23 = B(std::move(b23));
  B b24 = std::move(x ? b23 : (18, b24));
}

B b1;
B b2(b1);
B b3 = { 5, &b3.x };
B b4 = getB();
B b5 = getB(&b5);
B b6 = getB(&b6.x);

B b7(b7);
B b8 = getB(b8.x);
B b9 = getB(b9.y);
B b10 = getB(-b10.x);

B* b11 = 0;
B* b12(b11);
B* b13 = getPtrB();
B* b14 = getPtrB(&b14);

B* b15 = getPtrB(b15->x);
B* b16 = getPtrB(b16->y);

B b17 = { b17.x = 5, b17.y = 0 };
B b18 = { b18.x + 1, b18.y };

const B b19 = b19;
const B b20(b20);

B b21 = std::move(b21);
B b22 = moveB(std::move(b22));
B b23 = B(std::move(b23));
B b24 = std::move(x ? b23 : (18, b24));

class U {
  B b1, b2;
  B *ptr1, *ptr2;
  const B constb = {};

  U() {}
  U(bool (*)[1]) : b1() {}
  U(bool (*)[2]) : b2(b1) {}
  U(bool (*)[3]) : b1{ 5, &b1.x } {}
  U(bool (*)[4]) : b1(getB()) {}
  U(bool (*)[5]) : b1(getB(&b1)) {}
  U(bool (*)[6]) : b1(getB(&b1.x)) {}

  U(bool (*)[7]) : b1(b1) {}
  U(bool (*)[8]) : b1(getB(b1.x)) {}
  U(bool (*)[9]) : b1(getB(b1.y)) {}
  U(bool (*)[10]) : b1(getB(-b1.x)) {}

  U(bool (*)[11]) : ptr1(0) {}
  U(bool (*)[12]) : ptr1(0), ptr2(ptr1) {}
  U(bool (*)[13]) : ptr1(getPtrB()) {}
  U(bool (*)[14]) : ptr1(getPtrB(&ptr1)) {}

  U(bool (*)[15]) : ptr1(getPtrB(ptr1->x)) {}
  U(bool (*)[16]) : ptr2(getPtrB(ptr2->y)) {}

  U(bool (*)[17]) : b1 { b1.x = 5, b1.y = 0 } {}
  U(bool (*)[18]) : b1 { b1.x + 1, b1.y } {}

  U(bool (*)[19]) : constb(constb) {}
  U(bool (*)[20]) : constb(B(constb)) {}

  U(bool (*)[21]) : b1(std::move(b1)) {}
  U(bool (*)[22]) : b1(moveB(std::move(b1))) {}
  U(bool (*)[23]) : b1(B(std::move(b1))) {}
  U(bool (*)[24]) : b2(std::move(x ? b1 : (18, b2))) {}
};

struct C { char a[100], *e; } car = { .e = car.a };

namespace rdar10398199 {
  class FooBase { protected: ~FooBase() {} };
  class Foo : public FooBase {
  public:
    operator int&() const;
  };
  void stuff();
  template <typename T> class FooImpl : public Foo {
    T val;
  public:
    FooImpl(const T &x) : val(x) {}
    ~FooImpl() { stuff(); }
  };

  template <typename T> FooImpl<T> makeFoo(const T& x) {
    return FooImpl<T>(x);
  }

  void test() {
    const Foo &x = makeFoo(42);
    const int&y = makeFoo(42u);
    (void)x;
    (void)y;
  };
}



int pr12325(int params) {
  int x = ({
    while (false)
      ;
    int _v = params;
    if (false)
      ;
    _v;
  });
  return x;
}


int test_lambda() {
  auto f1 = [] (int x, int y) { int z; return x + y + z; };
  return f1(1, 2);
}

namespace {
  struct A {
    enum { A1 };
    static int A2() {return 5;}
    int A3;
    int A4() { return 5;}
  };

  struct B {
    A a;
  };

  struct C {
    C() {}
    C(int x) {}
    static A a;
    B b;
  };
  A C::a = A();


  struct D {
    C c;
    D(char (*)[1]) : c(c.b.a.A1) {}
    D(char (*)[2]) : c(c.b.a.A2()) {}
    D(char (*)[3]) : c(c.b.a.A3) {}
    D(char (*)[4]) : c(c.b.a.A4()) {}


    D(char (*)[5]) : c(c.a.A1) {}
    D(char (*)[6]) : c(c.a.A2()) {}
    D(char (*)[7]) : c(c.a.A3) {}
    D(char (*)[8]) : c(c.a.A4()) {}
  };

  struct E {
    int b = 1;
    int c = 1;
    int a;

    E(char (*)[1]) : a(a ? b : c) {}
    E(char (*)[2]) : a(b ? a : a) {}
    E(char (*)[3]) : a(b ? (a) : c) {}
    E(char (*)[4]) : a(b ? c : (a+c)) {}
    E(char (*)[5]) : a(b ? c : b) {}

    E(char (*)[6]) : a(a ?: a) {}
    E(char (*)[7]) : a(b ?: a) {}
    E(char (*)[8]) : a(a ?: c) {}
    E(char (*)[9]) : a(b ?: c) {}

    E(char (*)[10]) : a((a, a, b)) {}
    E(char (*)[11]) : a((c + a, a + 1, b)) {}
    E(char (*)[12]) : a((b + c, c, a)) {}
    E(char (*)[13]) : a((a, a, a, a)) {}
    E(char (*)[14]) : a((b, c, c)) {}
    E(char (*)[15]) : a(b ?: a) {}
    E(char (*)[16]) : a(a ?: b) {}
  };

  struct F {
    int a;
    F* f;
    F(int) {}
    F() {}
  };

  int F::*ptr = &F::a;
  F* F::*f_ptr = &F::f;
  struct G {
    F f1, f2;
    F *f3, *f4;
    G(char (*)[1]) : f1(f1) {}
    G(char (*)[2]) : f2(f1) {}
    G(char (*)[3]) : f2(F()) {}

    G(char (*)[4]) : f1(f1.*ptr) {}
    G(char (*)[5]) : f2(f1.*ptr) {}

    G(char (*)[6]) : f3(f3) {}
    G(char (*)[7]) : f3(f3->*f_ptr) {}
    G(char (*)[8]) : f3(new F(f3->*ptr)) {}
  };

  struct H {
    H() : a(a) {}
    const A a;
  };
}

namespace statics {
  static int a = a;
  static int b = b + 1;
  static int c = (c + c);
  static int e = static_cast<long>(e) + 1;
  static int f = foo(f);


  static int g = sizeof(g);
  int gg = g;
  static void* ptr = &ptr;
  static int h = bar(&h);
  static int i = boo(i);
  static int j = far(j);
  static int k = __alignof__(k);

  static int l = k ? l : l;
  static int m = 1 + (k ? m : m);
  static int n = -n;
  static int o = std::move(o);
  static const int p = std::move(p);
  static int q = moved(std::move(q));
  static int r = std::move((p ? q : (18, r)));
  static int s = r ?: s;
  static int t = t ?: s;
  static int u = (foo(u), s);
  static int v = (u += v);
  static int w = (w += 10);
  static int x = x++;
  static int y = ((s ? (y, v) : (77, y))++, sizeof(y));
  static int z = ++ref(z);
  static int aa = (ref(aa) += 10);
  static int bb = bb ? x : y;


  void test() {
    static int a = a;
    static int b = b + 1;
    static int c = (c + c);
    static int d = ({ d + d ;});
    static int e = static_cast<long>(e) + 1;
    static int f = foo(f);


    static int g = sizeof(g);
    static void* ptr = &ptr;
    static int h = bar(&h);
    static int i = boo(i);
    static int j = far(j);
    static int k = __alignof__(k);

    static int l = k ? l : l;
    static int m = 1 + (k ? m : m);
    static int n = -n;
    static int o = std::move(o);
    static const int p = std::move(p);
    static int q = moved(std::move(q));
    static int r = std::move((p ? q : (18, r)));
    static int s = r ?: s;
    static int t = t ?: s;
    static int u = (foo(u), s);
    static int v = (u += v);
    static int w = (w += 10);
    static int x = x++;
    static int y = ((s ? (y, v) : (77, y))++, sizeof(y));
    static int z = ++ref(z);
    static int aa = (ref(aa) += 10);
    static int bb = bb ? x : y;

    for (;;) {
      static int a = a;
      static int b = b + 1;
      static int c = (c + c);
      static int d = ({ d + d ;});
      static int e = static_cast<long>(e) + 1;
      static int f = foo(f);


      static int g = sizeof(g);
      static void* ptr = &ptr;
      static int h = bar(&h);
      static int i = boo(i);
      static int j = far(j);
      static int k = __alignof__(k);

      static int l = k ? l : l;
      static int m = 1 + (k ? m : m);
      static int n = -n;
      static int o = std::move(o);
      static const int p = std::move(p);
      static int q = moved(std::move(q));
      static int r = std::move((p ? q : (18, r)));
      static int s = r ?: s;
      static int t = t ?: s;
      static int u = (foo(u), s);
      static int v = (u += v);
      static int w = (w += 10);
      static int x = x++;
      static int y = ((s ? (y, v) : (77, y))++, sizeof(y));
      static int z = ++ref(z);
      static int aa = (ref(aa) += 10);
      static int bb = bb ? x : y;
    }
  }
}

namespace in_class_initializers {
  struct S {
    S() : a(a + 1) {}
    int a = 42;
  };

  struct T {
    T() : b(a + 1) {}
    int a = 42;
    int b;
  };

  struct U {
    U() : a(b + 1), b(a + 1) {}
    int a = 42;
    int b = 1;
  };
}

namespace references {
  int &a = a;
  int &b(b);
  int &c = a ? b : c;
  int &d{d};
  int &e = d ?: e;
  int &f = f ?: d;

  int &return_ref1(int);
  int &return_ref2(int&);

  int &g = return_ref1(g);
  int &h = return_ref2(h);

  struct S {
    S() : a(a) {}
    int &a;
  };

  void test() {
    int &a = a;
    int &b(b);
    int &c = a ? b : c;
    int &d{d};
  }

  struct T {
    T()
     : a(b), b(a) {}
    int &a, &b;
    int &c = c;
  };

  int x;
  struct U {
    U() : b(a) {}
    int &a = x;
    int &b;
  };
}

namespace operators {
  struct A {
    A(bool);
    bool operator==(A);
  };

  A makeA();

  A a1 = a1 = makeA();
  A a2 = a2 == a1;
  A a3 = a2 == a3;

  int x = x = 5;
}

namespace lambdas {
  struct A {
    template<typename T> A(T) {}
    int x;
  };
  A a0([] { return a0.x; });
  void f() {
    A a1([=] {
      return a1.x;
    });
    A a2([&] { return a2.x; });
    A a3([=] { return a3.x; }());
    A a4([&] { return a4.x; }());
    A a5([&] { return a5; }());
    A a6([&] { return a5.x; }());
    A a7 = [&a7] { return a7; }();
  }
}

namespace record_fields {
  bool x;
  struct A {
    A() {}
    A get();
    static A num();
    static A copy(A);
    static A something(A&);
  };

  A ref(A&);
  A const_ref(const A&);
  A pointer(A*);
  A normal(A);
  A rref(A&&);

  struct B {
    A a;
    B(char (*)[1]) : a(a) {}
    B(char (*)[2]) : a(a.get()) {}
    B(char (*)[3]) : a(a.num()) {}
    B(char (*)[4]) : a(a.copy(a)) {}
    B(char (*)[5]) : a(a.something(a)) {}
    B(char (*)[6]) : a(ref(a)) {}
    B(char (*)[7]) : a(const_ref(a)) {}
    B(char (*)[8]) : a(pointer(&a)) {}
    B(char (*)[9]) : a(normal(a)) {}
    B(char (*)[10]) : a(std::move(a)) {}
    B(char (*)[11]) : a(A(std::move(a))) {}
    B(char (*)[12]) : a(rref(std::move(a))) {}
    B(char (*)[13]) : a(std::move(x ? a : (25, a))) {}
  };
  struct C {
    C() {}
    A a1 = a1;
    A a2 = a2.get();
    A a3 = a3.num();
    A a4 = a4.copy(a4);
    A a5 = a5.something(a5);
    A a6 = ref(a6);
    A a7 = const_ref(a7);
    A a8 = pointer(&a8);
    A a9 = normal(a9);
    const A a10 = a10;
    A a11 = std::move(a11);
    A a12 = A(std::move(a12));
    A a13 = rref(std::move(a13));
    A a14 = std::move(x ? a13 : (22, a14));
  };
  struct D {
    A a1 = a1;
    A a2 = a2.get();
    A a3 = a3.num();
    A a4 = a4.copy(a4);
    A a5 = a5.something(a5);
    A a6 = ref(a6);
    A a7 = const_ref(a7);
    A a8 = pointer(&a8);
    A a9 = normal(a9);
    const A a10 = a10;
    A a11 = std::move(a11);
    A a12 = A(std::move(a12));
    A a13 = rref(std::move(a13));
    A a14 = std::move(x ? a13 : (22, a14));
  };
  D d;
  struct E {
    A a1 = a1;
    A a2 = a2.get();
    A a3 = a3.num();
    A a4 = a4.copy(a4);
    A a5 = a5.something(a5);
    A a6 = ref(a6);
    A a7 = const_ref(a7);
    A a8 = pointer(&a8);
    A a9 = normal(a9);
    const A a10 = a10;
    A a11 = std::move(a11);
    A a12 = A(std::move(a12));
    A a13 = rref(std::move(a13));
    A a14 = std::move(x ? a13 : (22, a14));
  };
}

namespace cross_field_warnings {
  struct A {
    int a, b;
    A() {}
    A(char (*)[1]) : b(a) {}
    A(char (*)[2]) : a(b) {}
  };

  struct B {
    int a = b;
    int b;
    B() {}
  };

  struct C {
    int a;
    int b = a;
    C(char (*)[1]) : a(5) {}
    C(char (*)[2]) {}
  };

  struct D {
    int a;
    int &b;
    int &c = a;
    int d = b;
    D() : b(a) {}
  };

  struct E {
    int a;
    int get();
    static int num();
    E() {}
    E(int) {}
  };

  struct F {
    int a;
    E e;
    int b;
    F(char (*)[1]) : a(e.get()) {}
    F(char (*)[2]) : a(e.num()) {}
    F(char (*)[3]) : e(a) {}
    F(char (*)[4]) : a(4), e(a) {}
    F(char (*)[5]) : e(b) {}
    F(char (*)[6]) : e(b), b(4) {}
  };

  struct G {
    G(const A&) {};
  };

  struct H {
    A a1;
    G g;
    A a2;
    H() : g(a1) {}
    H(int) : g(a2) {}
  };

  struct I {
    I(int*) {}
  };

  struct J : public I {
    int *a;
    int *b;
    int c;
    J() : I((a = new int(5))), b(a), c(*a) {}
  };

  struct K {
    int a = (b = 5);
    int b = b + 5;
  };

  struct L {
    int a = (b = 5);
    int b = b + 5;
    L() : a(5) {}
  };

  struct M { };

  struct N : public M {
    int a;
    int b;
    N() : b(a) { }
  };

  struct O {
    int x = 42;
    int get() { return x; }
  };

  struct P {
    O o;
    int x = o.get();
    P() : x(o.get()) { }
  };

  struct Q {
    int a;
    int b;
    int &c;
    Q() :
      a(c = 5),
      b(c),
      c(a) {}
  };

  struct R {
    int a;
    int b;
    int c;
    int d = a + b + c;
    R() : a(c = 5), b(c), c(a) {}
  };



  struct T {
    int x;
    int y;
    T(bool b)
        : x(b ? (y = 5) : (1 + y)),
          y(y + 1) {}
    T(int b)
        : x(!b ? (1 + y) : (y = 5)),
          y(y + 1) {}
  };

}

namespace base_class {
  struct A {
    A (int) {}
  };

  struct B : public A {
    int x;
    B() : A(x) {}
  };

  struct C : public A {
    int x;
    int y;
    C() : A(y = 4), x(y) {}
  };
}

namespace delegating_constructor {
  struct A {
    A(int);
    A(int&, int);

    A(char (*)[1]) : A(x) {}

    A(char (*)[2]) : A(x, x) {}


    A(char (*)[3]) : A(x, 0) {}

    int x;
  };
}

namespace init_list {
  int num = 5;
  struct A { int i1, i2; };
  struct B { A a1, a2; };

  A a1{1,2};
  A a2{a2.i1 + 2};
  A a3 = {a3.i1 + 2};
  A a4 = A{a4.i2 + 2};

  B b1 = { {}, {} };
  B b2 = { {}, b2.a1 };
  B b3 = { b3.a1 };
  B b4 = { {}, b4.a2} ;
  B b5 = { b5.a2 };

  B b6 = { {b6.a1.i1} };
  B b7 = { {0, b7.a1.i1} };
  B b8 = { {}, {b8.a1.i1} };
  B b9 = { {}, {0, b9.a1.i1} };

  B b10 = { {b10.a1.i2} };
  B b11 = { {0, b11.a1.i2} };
  B b12 = { {}, {b12.a1.i2} };
  B b13 = { {}, {0, b13.a1.i2} };

  B b14 = { {b14.a2.i1} };
  B b15 = { {0, b15.a2.i1} };
  B b16 = { {}, {b16.a2.i1} };
  B b17 = { {}, {0, b17.a2.i1} };

  B b18 = { {b18.a2.i2} };
  B b19 = { {0, b19.a2.i2} };
  B b20 = { {}, {b20.a2.i2} };
  B b21 = { {}, {0, b21.a2.i2} };

  B b22 = { {b18.a2.i2 + 5} };

  struct C {int a; int& b; int c; };
  C c1 = { 0, num, 0 };
  C c2 = { 1, num, c2.b };
  C c3 = { c3.b, num };
  C c4 = { 0, c4.b, 0 };
  C c5 = { 0, c5.c, 0 };
  C c6 = { c6.b, num, 0 };
  C c7 = { 0, c7.a, 0 };

  struct D {int &a; int &b; };
  D d1 = { num, num };
  D d2 = { num, d2.a };
  D d3 = { d3.b, num };


  struct Awrapper {
    A a1{1,2};
    A a2{a2.i1 + 2};
    A a3 = {a3.i1 + 2};
    A a4 = A{a4.i2 + 2};
    Awrapper() {}
    Awrapper(int) :
      a1{1,2},
      a2{a2.i1 + 2},
      a3{a3.i1 + 2},
      a4{a4.i2 + 2}
    {}
  };

  struct Bwrapper {
    B b1 = { {}, {} };
    B b2 = { {}, b2.a1 };
    B b3 = { b3.a1 };
    B b4 = { {}, b4.a2} ;
    B b5 = { b5.a2 };

    B b6 = { {b6.a1.i1} };
    B b7 = { {0, b7.a1.i1} };
    B b8 = { {}, {b8.a1.i1} };
    B b9 = { {}, {0, b9.a1.i1} };

    B b10 = { {b10.a1.i2} };
    B b11 = { {0, b11.a1.i2} };
    B b12 = { {}, {b12.a1.i2} };
    B b13 = { {}, {0, b13.a1.i2} };

    B b14 = { {b14.a2.i1} };
    B b15 = { {0, b15.a2.i1} };
    B b16 = { {}, {b16.a2.i1} };
    B b17 = { {}, {0, b17.a2.i1} };

    B b18 = { {b18.a2.i2} };
    B b19 = { {0, b19.a2.i2} };
    B b20 = { {}, {b20.a2.i2} };
    B b21 = { {}, {0, b21.a2.i2} };

    B b22 = { {b18.a2.i2 + 5} };
    Bwrapper() {}
    Bwrapper(int) :
      b1{ {}, {} },
      b2{ {}, b2.a1 },
      b3{ b3.a1 },
      b4{ {}, b4.a2},
      b5{ b5.a2 },

      b6{ {b6.a1.i1} },
      b7{ {0, b7.a1.i1} },
      b8{ {}, {b8.a1.i1} },
      b9{ {}, {0, b9.a1.i1} },

      b10{ {b10.a1.i2} },
      b11{ {0, b11.a1.i2} },
      b12{ {}, {b12.a1.i2} },
      b13{ {}, {0, b13.a1.i2} },

      b14{ {b14.a2.i1} },
      b15{ {0, b15.a2.i1} },
      b16{ {}, {b16.a2.i1} },
      b17{ {}, {0, b17.a2.i1} },

      b18{ {b18.a2.i2} },
      b19{ {0, b19.a2.i2} },
      b20{ {}, {b20.a2.i2} },
      b21{ {}, {0, b21.a2.i2} },

      b22{ {b18.a2.i2 + 5} }
    {}
  };

  struct Cwrapper {
    C c1 = { 0, num, 0 };
    C c2 = { 1, num, c2.b };
    C c3 = { c3.b, num };
    C c4 = { 0, c4.b, 0 };
    C c5 = { 0, c5.c, 0 };
    C c6 = { c6.b, num, 0 };
    C c7 = { 0, c7.a, 0 };

    Cwrapper() {}
    Cwrapper(int) :
      c1{ 0, num, 0 },
      c2{ 1, num, c2.b },
      c3{ c3.b, num },
      c4{ 0, c4.b, 0 },
      c5{ 0, c5.c, 0 },
      c6{ c6.b, num, 0 },
      c7{ 0, c7.a, 0 }
    {}
  };

  struct Dwrapper {
    D d1 = { num, num };
    D d2 = { num, d2.a };
    D d3 = { d3.b, num };
    Dwrapper() {}
    Dwrapper(int) :
      d1{ num, num },
      d2{ num, d2.a },
      d3{ d3.b, num }
    {}
  };

  struct E {
    E();
    E foo();
    E* operator->();
  };

  struct F { F(E); };

  struct EFComposed {
    F f;
    E e;
    EFComposed() : f{ e->foo() }, e() {}
  };
}

namespace template_class {
class Foo {
 public:
    int *Create() { return nullptr; }
};

template <typename T>
class A {
public:

  A() : ptr(foo->Create()) {}

private:
  Foo *foo = new Foo;
  int *ptr;
};

template <typename T>
class B {
public:

  B() : ptr(foo->Create()) {}

private:
  Foo *foo;
  int *ptr;
};

template <typename T>
class C {
public:
  C() : ptr(foo->Create()) {}

private:
  Foo *foo;
  int *ptr;
};

C<int> c;


}

namespace base_class_access {
struct A {
  A();
  A(int);

  int i;
  int foo();

  static int bar();
};

struct B : public A {
  B(int (*)[1]) : A() {}
  B(int (*)[2]) : A(bar()) {}

  B(int (*)[3]) : A(i) {}


  B(int (*)[4]) : A(foo()) {}

};

struct C {
  C(int) {}
};

struct D : public C, public A {
  D(int (*)[1]) : C(0) {}
  D(int (*)[2]) : C(bar()) {}

  D(int (*)[3]) : C(i) {}


  D(int (*)[4]) : C(foo()) {}

};

}

namespace value {
template <class T> T move(T t);
template <class T> T notmove(T t);
}
namespace lvalueref {
template <class T> T move(T& t);
template <class T> T notmove(T& t);
}
namespace rvalueref {
template <class T> T move(T&& t);
template <class T> T notmove(T&& t);
}

namespace move_test {
int a1 = std::move(a1);
int a2 = value::move(a2);
int a3 = value::notmove(a3);
int a4 = lvalueref::move(a4);
int a5 = lvalueref::notmove(a5);
int a6 = rvalueref::move(a6);
int a7 = rvalueref::notmove(a7);

void test() {
  int a1 = std::move(a1);
  int a2 = value::move(a2);
  int a3 = value::notmove(a3);
  int a4 = lvalueref::move(a4);
  int a5 = lvalueref::notmove(a5);
  int a6 = rvalueref::move(a6);
  int a7 = rvalueref::notmove(a7);
}

class A {
  int a;
  A(int (*) [1]) : a(std::move(a)) {}
  A(int (*) [2]) : a(value::move(a)) {}
  A(int (*) [3]) : a(value::notmove(a)) {}
  A(int (*) [4]) : a(lvalueref::move(a)) {}
  A(int (*) [5]) : a(lvalueref::notmove(a)) {}
  A(int (*) [6]) : a(rvalueref::move(a)) {}
  A(int (*) [7]) : a(rvalueref::notmove(a)) {}
};
}

void array_capture(bool b) {
  const char fname[] = "array_capture";
  if (b) {
    int unused;
  } else {
    [fname]{};
  }
}

void if_switch_init_stmt(int k) {
  if (int n = 0; (n == k || k > 5)) {}

  if (int n; (n == k || k > 5)) {}

  switch (int n = 0; (n == k || k > 5)) {}

  switch (int n; (n == k || k > 5)) {}
}

template<typename T> struct Outer {
  struct Inner {
    int a = 1;
    int b;
    Inner() : b(a) {}
  };
};
Outer<int>::Inner outerinner;

struct Polymorphic { virtual ~Polymorphic() { } };

template<class... Bases>
struct Inherit : Bases... {
  int g1;
};

template<class... Bases>
struct InheritWithExplicit : Bases... {
  int g2 [[clang::require_explicit_initialization]];
};

struct Special {};

template<>
struct Inherit<Special> {
  int g3 [[clang::require_explicit_initialization]];
};

template<>
struct InheritWithExplicit<Special> {
  int g4;
};

void aggregate() {
  struct NonAgg {
    NonAgg() { }
    [[clang::require_explicit_initialization]] int na;
  };
  NonAgg nonagg;
  (void)nonagg;

  struct S {
    [[clang::require_explicit_initialization]] int s1;
    int s2;
    int s3 = 12;
    [[clang::require_explicit_initialization]] int s4 = 100;
    static void foo(S) { }
  };

  struct C {


    [[clang::require_explicit_initialization]]

    int c1;
    C() = default;
  };

  struct D : S {
    int d1;
    int d2 [[clang::require_explicit_initialization]];
  };

  struct D2 : D {
  };

  struct E {
    int e1;
    D e2 [[clang::require_explicit_initialization]];
    struct {
      [[clang::require_explicit_initialization]] D e3;
      D2 e4 [[clang::require_explicit_initialization]];
    };
  };

  struct CopyAndMove {
    CopyAndMove() = default;
    CopyAndMove(const CopyAndMove &) {}
    CopyAndMove(CopyAndMove &&) {}
  };
  struct Embed {
    int embed1;
    int embed2 [[clang::require_explicit_initialization]];
    CopyAndMove force_separate_move_ctor;
  };
  struct EmbedDerived : Embed {};
  struct F {
    Embed f1;

    explicit F(const char(&)[1]) : f1() {

      ::new(static_cast<void*>(&f1)) decltype(f1);

      std::construct_at(&f1);





      ::new(static_cast<void*>(&f1)) decltype(f1){1};
    }




    explicit F(const char(&)[2]) : f1{1, 2} { }


    explicit F(const char(&)[3]) : f1{} {}

    explicit F(const char(&)[4]) : f1{1} {}

    explicit F(const char(&)[5]) : f1{.embed1 = 1} {}
  };
  F ctors[] = {
      F(""),
      F("_"),
      F("__"),
      F("___"),
      F("____")
  };

  struct MoveOrCopy {
    Embed e;
    EmbedDerived ed;
    F f;

    MoveOrCopy(const MoveOrCopy &c) : e(c.e), ed(c.ed), f(c.f) {}

    MoveOrCopy(MoveOrCopy &&c)
        : e(std::move(c.e)), ed(std::move(c.ed)), f(std::move(c.f)) {}
  };
  F copy1(ctors[0]);
  (void)copy1;
  F move1(std::move(ctors[0]));
  (void)move1;
  F copy2{ctors[0]};
  (void)copy2;
  F move2{std::move(ctors[0])};
  (void)move2;
  F copy3 = ctors[0];
  (void)copy3;
  F move3 = std::move(ctors[0]);
  (void)move3;
  F copy4 = {ctors[0]};
  (void)copy4;
  F move4 = {std::move(ctors[0])};
  (void)move4;

  S::foo(S{1, 2, 3, 4});
  S::foo(S{.s1 = 100, .s4 = 100});
  S::foo(S{.s1 = 100});

  (void)sizeof(S{});

  S s{.s1 = 100, .s4 = 100};
  (void)s;

  S t{.s4 = 100};
  (void)t;

  S *ptr1 = new S;
  delete ptr1;

  S *ptr2 = new S{.s1 = 100, .s4 = 100};
  delete ptr2;
# 1699 "SemaCXX/uninitialized.cpp"
  C a;
  (void)a;




  D b{.d2 = 1};
  (void)b;




  D c{.d1 = 5};




  c = {{}, 0};
  (void)c;




  D d;
  (void)d;
# 1737 "SemaCXX/uninitialized.cpp"
  E e;
  (void)e;

  InheritWithExplicit<> agg;
  (void)agg;

  InheritWithExplicit<Polymorphic> polymorphic;
  (void)polymorphic;

  Inherit<Special> specialized_explicit;
  (void)specialized_explicit;

  InheritWithExplicit<Special> specialized_implicit;
  (void)specialized_implicit;
}
