//type: fn
//options: 
# 1 "SemaCXX/reinterpret-cast.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 482 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/reinterpret-cast.cpp" 2


# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stdint.h" 1
# 56 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stdint.h"
# 1 "/usr/include/stdint.h" 1 3 4
# 25 "/usr/include/stdint.h" 3 4
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
# 26 "/usr/include/stdint.h" 2 3 4
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
# 4 "SemaCXX/reinterpret-cast.cpp" 2

enum test { testval = 1 };
struct structure { int m; };
typedef void (*fnptr)();


void self_conversion()
{



  int i = 0;
  (void)reinterpret_cast<int>(i);

  test e = testval;
  (void)reinterpret_cast<test>(e);


  int *pi = 0;
  (void)reinterpret_cast<int*>(pi);

  const int structure::*psi = 0;
  (void)reinterpret_cast<const int structure::*>(psi);

  const int ci = 0;
  (void)reinterpret_cast<const int>(i);

  structure s;
  (void)reinterpret_cast<structure>(s);

  float f = 0.0f;
  (void)reinterpret_cast<float>(f);
}


void integral_conversion()
{
  void *vp = reinterpret_cast<void*>(testval);
  intptr_t i = reinterpret_cast<intptr_t>(vp);
  (void)reinterpret_cast<float*>(i);
  fnptr fnp = reinterpret_cast<fnptr>(i);
  (void)reinterpret_cast<char>(fnp);
  (void)reinterpret_cast<intptr_t>(fnp);
}

void pointer_conversion()
{
  int *p1 = 0;
  float *p2 = reinterpret_cast<float*>(p1);
  structure *p3 = reinterpret_cast<structure*>(p2);
  typedef int **ppint;
  ppint *deep = reinterpret_cast<ppint*>(p3);
  (void)reinterpret_cast<fnptr*>(deep);
}

void constness()
{
  int ***const ipppc = 0;

  int const *icp = reinterpret_cast<int const*>(ipppc);

  (void)reinterpret_cast<int*>(icp);

  int const *const **icpcpp = reinterpret_cast<int const* const**>(ipppc);

  int *ip = reinterpret_cast<int*>(icpcpp);

  (void)reinterpret_cast<int const*>(ip);

  (void)reinterpret_cast<int const* const* const*>(ipppc);





  int i = 0;

  (void)reinterpret_cast<const int>(i);

  (void)reinterpret_cast<int *const>(ip);
}

void fnptrs()
{
  typedef int (*fnptr2)(int);
  fnptr fp = 0;
  (void)reinterpret_cast<fnptr2>(fp);
  void *vp = reinterpret_cast<void*>(fp);
  (void)reinterpret_cast<fnptr>(vp);
}

void refs()
{
  long l = 0;
  char &c = reinterpret_cast<char&>(l);

  (void)reinterpret_cast<int&>(&c);
}

void memptrs()
{
  const int structure::*psi = 0;
  (void)reinterpret_cast<const float structure::*>(psi);
  (void)reinterpret_cast<int structure::*>(psi);

  void (structure::*psf)() = 0;
  (void)reinterpret_cast<int (structure::*)()>(psf);

  (void)reinterpret_cast<void (structure::*)()>(psi);
  (void)reinterpret_cast<int structure::*>(psf);



  (void)reinterpret_cast<void (structure::*)()>(0);
  (void)reinterpret_cast<int structure::*>(0);
}

namespace PR5545 {

class A;
class B;
void (A::*a)();
void (B::*b)() = reinterpret_cast<void (B::*)()>(a);
}

void const_arrays() {
  typedef char STRING[10];
  const STRING *s;
  const char *c;

  (void)reinterpret_cast<char *>(s);
  (void)reinterpret_cast<const STRING *>(c);
}

namespace PR9564 {
  struct a { int a : 10; }; a x;
  int *y = &reinterpret_cast<int&>(x.a);

  __attribute((ext_vector_type(4))) typedef float v4;
  float& w(v4 &a) { return reinterpret_cast<float&>(a[1]); }
}

void dereference_reinterpret_cast() {
  struct A {};
  typedef A A2;
  class B {};
  typedef B B2;
  A a;
  B b;
  A2 a2;
  B2 b2;
  long l;
  double d;
  float f;
  char c;
  unsigned char uc;
  void* v_ptr;
  (void)reinterpret_cast<double&>(l);
  (void)*reinterpret_cast<double*>(&l);
  (void)reinterpret_cast<double&>(f);
  (void)*reinterpret_cast<double*>(&f);
  (void)reinterpret_cast<float&>(l);
  (void)*reinterpret_cast<float*>(&l);
  (void)reinterpret_cast<float&>(d);
  (void)*reinterpret_cast<float*>(&d);


  (void)*(reinterpret_cast<double*>(&l));
  (void)*((reinterpret_cast<double*>((&l))));


  (void)reinterpret_cast<A&>(b);
  (void)*reinterpret_cast<A*>(&b);
  (void)reinterpret_cast<B&>(a);
  (void)*reinterpret_cast<B*>(&a);
  (void)reinterpret_cast<A2&>(b2);
  (void)*reinterpret_cast<A2*>(&b2);
  (void)reinterpret_cast<B2&>(a2);
  (void)*reinterpret_cast<B2*>(&a2);


  (void)reinterpret_cast<A&>(a);
  (void)*reinterpret_cast<A*>(&a);
  (void)reinterpret_cast<B&>(b);
  (void)*reinterpret_cast<B*>(&b);
  (void)reinterpret_cast<long&>(l);
  (void)*reinterpret_cast<long*>(&l);
  (void)reinterpret_cast<double&>(d);
  (void)*reinterpret_cast<double*>(&d);
  (void)reinterpret_cast<char&>(c);
  (void)*reinterpret_cast<char*>(&c);


  (void)reinterpret_cast<A&>(c);
  (void)*reinterpret_cast<A*>(&c);
  (void)reinterpret_cast<B&>(c);
  (void)*reinterpret_cast<B*>(&c);
  (void)reinterpret_cast<long&>(c);
  (void)*reinterpret_cast<long*>(&c);
  (void)reinterpret_cast<double&>(c);
  (void)*reinterpret_cast<double*>(&c);
  (void)reinterpret_cast<char&>(l);
  (void)*reinterpret_cast<char*>(&l);
  (void)reinterpret_cast<char&>(d);
  (void)*reinterpret_cast<char*>(&d);
  (void)reinterpret_cast<char&>(f);
  (void)*reinterpret_cast<char*>(&f);


  (void)*reinterpret_cast<A*>(v_ptr);
  (void)*reinterpret_cast<B*>(v_ptr);
  (void)*reinterpret_cast<long*>(v_ptr);
  (void)*reinterpret_cast<double*>(v_ptr);
  (void)*reinterpret_cast<float*>(v_ptr);


  (void)*reinterpret_cast<void*>(&a);
  (void)*reinterpret_cast<void*>(&b);
  (void)*reinterpret_cast<void*>(&l);
  (void)*reinterpret_cast<void*>(&d);
  (void)*reinterpret_cast<void*>(&f);
}

void reinterpret_cast_allowlist () {

  int a;
  float b;
  (void)reinterpret_cast<int&>(a);
  (void)*reinterpret_cast<int*>(&a);
  (void)reinterpret_cast<float&>(b);
  (void)*reinterpret_cast<float*>(&b);


  (void)reinterpret_cast<const int&>(a);
  (void)*reinterpret_cast<const int*>(&a);
  (void)reinterpret_cast<volatile int&>(a);
  (void)*reinterpret_cast<volatile int*>(&a);
  (void)reinterpret_cast<const volatile int&>(a);
  (void)*reinterpret_cast<const volatile int*>(&a);
  (void)reinterpret_cast<const float&>(b);
  (void)*reinterpret_cast<const float*>(&b);
  (void)reinterpret_cast<volatile float&>(b);
  (void)*reinterpret_cast<volatile float*>(&b);
  (void)reinterpret_cast<const volatile float&>(b);
  (void)*reinterpret_cast<const volatile float*>(&b);



  signed d;
  unsigned e;
  (void)reinterpret_cast<signed&>(d);
  (void)*reinterpret_cast<signed*>(&d);
  (void)reinterpret_cast<signed&>(e);
  (void)*reinterpret_cast<signed*>(&e);
  (void)reinterpret_cast<unsigned&>(d);
  (void)*reinterpret_cast<unsigned*>(&d);
  (void)reinterpret_cast<unsigned&>(e);
  (void)*reinterpret_cast<unsigned*>(&e);



  (void)reinterpret_cast<const signed&>(d);
  (void)*reinterpret_cast<const signed*>(&d);
  (void)reinterpret_cast<const signed&>(e);
  (void)*reinterpret_cast<const signed*>(&e);
  (void)reinterpret_cast<const unsigned&>(d);
  (void)*reinterpret_cast<const unsigned*>(&d);
  (void)reinterpret_cast<const unsigned&>(e);
  (void)*reinterpret_cast<const unsigned*>(&e);
  (void)reinterpret_cast<volatile signed&>(d);
  (void)*reinterpret_cast<volatile signed*>(&d);
  (void)reinterpret_cast<volatile signed&>(e);
  (void)*reinterpret_cast<volatile signed*>(&e);
  (void)reinterpret_cast<volatile unsigned&>(d);
  (void)*reinterpret_cast<volatile unsigned*>(&d);
  (void)reinterpret_cast<volatile unsigned&>(e);
  (void)*reinterpret_cast<volatile unsigned*>(&e);
  (void)reinterpret_cast<const volatile signed&>(d);
  (void)*reinterpret_cast<const volatile signed*>(&d);
  (void)reinterpret_cast<const volatile signed&>(e);
  (void)*reinterpret_cast<const volatile signed*>(&e);
  (void)reinterpret_cast<const volatile unsigned&>(d);
  (void)*reinterpret_cast<const volatile unsigned*>(&d);
  (void)reinterpret_cast<const volatile unsigned&>(e);
  (void)*reinterpret_cast<const volatile unsigned*>(&e);
# 300 "SemaCXX/reinterpret-cast.cpp"
  (void)reinterpret_cast<char&>(a);
  (void)*reinterpret_cast<char*>(&a);
  (void)reinterpret_cast<unsigned char&>(a);
  (void)*reinterpret_cast<unsigned char*>(&a);
  (void)reinterpret_cast<char&>(b);
  (void)*reinterpret_cast<char*>(&b);
  (void)reinterpret_cast<unsigned char&>(b);
  (void)*reinterpret_cast<unsigned char*>(&b);
}

namespace templated {
template <typename TARGETTYPE, typename UATYPE>
void cast_uninstantiated() {
  const UATYPE* data;
  (void)*reinterpret_cast<const TARGETTYPE*>(data);
}


template <typename TARGETTYPE, typename UATYPE>
void cast_instantiated_badly() {
  const UATYPE* data;
  (void)*reinterpret_cast<const TARGETTYPE*>(data);
}

template <typename TARGETTYPE, typename UATYPE>
void cast_instantiated_well() {
  const UATYPE* data;
  (void)*reinterpret_cast<const TARGETTYPE*>(data);
}

template <typename TARGETTYPE>
void cast_one_tmpl_arg_uninstantiated() {
  const int* data;
  (void)*reinterpret_cast<const TARGETTYPE*>(data);
}

template <typename TARGETTYPE>
void cast_one_tmpl_arg_instantiated_badly() {
  const float* data;
  (void)*reinterpret_cast<const TARGETTYPE*>(data);
}

template <typename TARGETTYPE>
void cast_one_tmpl_arg_instantiated_well() {
  const float* data;
  (void)*reinterpret_cast<const TARGETTYPE*>(data);
}

template <int size>
void cast_nontype_template_true_positive_noninstantiated() {
  const float *data;
  const int arr[size];
  (void)*reinterpret_cast<const int*>(data);
}

template <int size>
void cast_nontype_template_true_negative_noninstantiated() {
  const int data[size];
  (void)*reinterpret_cast<const int*>(data);
}

void top() {
  cast_instantiated_badly<int, float>();

  cast_instantiated_well<int, int>();
  cast_one_tmpl_arg_instantiated_badly<int>();

  cast_one_tmpl_arg_instantiated_well<float>();
}

template<typename T, typename U>
void cast_template_dependent_type_noninstantiated(T** x)
{
    (void)*reinterpret_cast<U**>(x);
}

template<typename T, typename U>
void cast_template_dependent_member_type_noninstantiated(typename T::X x)
{
    (void)*reinterpret_cast<typename U::Y>(x);
}

}
