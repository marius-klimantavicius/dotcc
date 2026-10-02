//type: fn
//options:  --c++03 --c++03 --ms_compatibility: --c++11: --c++11 --ms_compatibility: --c++14: --c++14 --ms_compatibility: --c++17: --c++17 --ms_compatibility: --c++20: --c++20 --ms_compatibility: --c++14 -DNO_DEPRECATED_FLAGS: --c++14 -DNO_DEPRECATED_FLAGS --ms_compatibility
# 1 "SemaCXX/deprecated.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 431 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/deprecated.cpp" 2
# 15 "SemaCXX/deprecated.cpp"
# 1 "SemaCXX/Inputs/register.h" 1
# 2 "SemaCXX/Inputs/register.h" 3


inline void f() { register int k; }
# 16 "SemaCXX/deprecated.cpp" 2

namespace std {
  struct type_info {};
}

void g() throw();
void h() throw(int);
void i() throw(...);
# 34 "SemaCXX/deprecated.cpp"
void stuff(register int q) {





  register int n;






  register int m asm("rbx");

  int k = ({ register int n = (n); n; });
  bool b;
  ++b;






  b++;






  char *p = "foo";





}

struct S { int n; void operator+(int); };
struct T : private S {
  S::n;





  S::operator+;





};
# 120 "SemaCXX/deprecated.cpp"
struct X {
  friend int operator,(X, X);
  void operator[](int);
};
void array_index_comma() {
  int arr[123];
  (void)arr[(void)1, 2];
  (void)arr[X(), X()];
  X()[(void)1, 2];
  X()[X(), X()];







  (void)arr[((void)1, 2)];
  (void)arr[(X(), X())];
  (void)((void)1,2)[arr];
  (void)(X(), X())[arr];
  X()[((void)1, 2)];
  X()[(X(), X())];
}

namespace DeprecatedVolatile {
  volatile int n = 1;
  void use(int);
  void f() {

    n = 5;




    (void)typeid(n = 5);
    (n = 5, 0);
    use(n = 5);
    int q = n = 5;
    q = n = 5;




    (void)sizeof(q = n = 5);
    (void)typeid(use(n = 5));
    (void)__alignof(+(n = 5));



    (void)sizeof(n = 5);
    (void)__alignof(n = 5);

    (n = 5);

    volatile bool b = true;
    if (b = true) {}
    for (b = true;
         b = true;
         b = true) {}
    for (volatile bool x = true;
         volatile bool y = true;
        ) {}


    ++n;
    --n;
    n++;
    n--;
    n += 5;
    n *= 3;
    n /= 2;
    n %= 42;
    n &= 2;
    n |= 2;
    n ^= 2;

    (void)__is_trivially_assignable(volatile int&, int);
# 206 "SemaCXX/deprecated.cpp"
  }
  volatile int g(
      volatile int n,
      volatile int (*p)(
        volatile int m)
      );






  template<typename T> T f(T v);
  int use_f = f<volatile int>(0);


  struct UDT {
    UDT(volatile const UDT&);
    UDT &operator=(const UDT&);
    UDT &operator=(const UDT&) volatile;
    UDT operator+=(const UDT&) volatile;
  };
  void h(UDT a) {
    volatile UDT b = a;
    volatile UDT c = b;
    a = c = a;
    b += a;
  }

  volatile struct amber jurassic();

  void trex(volatile short left_arm, volatile struct amber right_arm);


  void fly(volatile struct pterosaur* pteranodon);
}

namespace ArithConv {
  enum E { e } e2;
  enum F { f };
  bool b1 = e == e2;
  bool b2 = e == f;
  bool b3 = e == 0.0;
  bool b4 = 0.0 == f;
  int n1 = true ? e : f;
  int n2 = true ? e : 0.0;
}

namespace ArrayComp {
  int arr1[3], arr2[4];
  bool b1 = arr1 == arr2;

  bool b2 = arr1 < arr2;

  __attribute__((weak)) int arr3[3];
  bool b3 = arr1 == arr3;
  bool b4 = arr1 < arr3;




  int (&f())[3];
  bool b6 = arr1 == f();
  bool b7 = arr1 == +f();
}

namespace GH90073 {
[[deprecated]] int f1() {
  [[deprecated]] int a;

  a = 0;
  return a;
}

[[deprecated]] void f2([[deprecated]] int x) {

  x = 4;
}

int main() {
  f1();
  f2(1);
  return 0;
}
}

# 1 "/usr/include/system-header.h" 1 3
void system_header_function(void) throw();
