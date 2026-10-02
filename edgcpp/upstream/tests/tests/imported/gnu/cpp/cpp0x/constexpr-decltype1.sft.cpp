//type: rp
//options: --c++11
# 0 "./cpp0x/constexpr-decltype1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp0x/constexpr-decltype1.C"



template <typename T, T V>
struct W { static constexpr T value() { return V; } };

template <typename T, T V>
struct X { typedef T type; static constexpr type value() { return V; } };

template <typename T, T V>
struct Y { using type = T; static constexpr type value() { return V; } };

template <typename T, T V>
struct Z { static constexpr decltype(V) value() { return V; } };

template <typename T, T V>
struct W_ { static constexpr T value = V; };

template <typename T, T V>
struct X_ { typedef T type; static constexpr type value = V; };

template <typename T, T V>
struct Y_ { using type = T; static constexpr type value = V; };

template <typename T, T V>
struct Z_ { static constexpr decltype(V) value = V; };


static_assert(W<int, 10>::value() == 10, "oops");
static_assert(X<int, 10>::value() == 10, "oops");
static_assert(Y<int, 10>::value() == 10, "oops");
static_assert(Z<int, 10>::value() == 10, "oops");
static_assert(W_<int, 10>::value == 10, "oops");
static_assert(X_<int, 10>::value == 10, "oops");
static_assert(Y_<int, 10>::value == 10, "oops");
static_assert(Z_<int, 10>::value == 10, "oops");

extern constexpr int a = 10;
static_assert(*W<const int*, &a>::value() == 10, "oops");
static_assert(*X<const int*, &a>::value() == 10, "oops");
static_assert(*Y<const int*, &a>::value() == 10, "oops");
static_assert(*Z<const int*, &a>::value() == 10, "oops");
static_assert(*W_<const int*, &a>::value == 10, "oops");
static_assert(*X_<const int*, &a>::value == 10, "oops");
static_assert(*Y_<const int*, &a>::value == 10, "oops");
static_assert(*Z_<const int*, &a>::value == 10, "oops");

template <int V> constexpr int b() { return V; }
static_assert((W<int(*)(), &b<10>>::value())() == 10, "oops");
static_assert((X<int(*)(), &b<10>>::value())() == 10, "oops");
static_assert((Y<int(*)(), &b<10>>::value())() == 10, "oops");
static_assert((Z<int(*)(), &b<10>>::value())() == 10, "oops");
static_assert(W_<int(*)(), &b<10>>::value() == 10, "oops");
static_assert(X_<int(*)(), &b<10>>::value() == 10, "oops");
static_assert(Y_<int(*)(), &b<10>>::value() == 10, "oops");
static_assert(Z_<int(*)(), &b<10>>::value() == 10, "oops");

constexpr struct C {
    constexpr int c1() const { return 10; }
    static constexpr int c2() { return 10; }
} c;

static_assert((c.*W<int(C::*)()const, &C::c1>::value())() == 10, "oops");
static_assert((c.*X<int(C::*)()const, &C::c1>::value())() == 10, "oops");
static_assert((c.*Y<int(C::*)()const, &C::c1>::value())() == 10, "oops");
static_assert((c.*Z<int(C::*)()const, &C::c1>::value())() == 10, "oops");
static_assert((c.*W_<int(C::*)()const, &C::c1>::value)() == 10, "oops");
static_assert((c.*X_<int(C::*)()const, &C::c1>::value)() == 10, "oops");
static_assert((c.*Y_<int(C::*)()const, &C::c1>::value)() == 10, "oops");
static_assert((c.*Z_<int(C::*)()const, &C::c1>::value)() == 10, "oops");

static_assert((W<int(*)(), &C::c2>::value())() == 10, "oops");
static_assert((X<int(*)(), &C::c2>::value())() == 10, "oops");
static_assert((Y<int(*)(), &C::c2>::value())() == 10, "oops");
static_assert((Z<int(*)(), &C::c2>::value())() == 10, "oops");
static_assert(W_<int(*)(), &C::c2>::value() == 10, "oops");
static_assert(X_<int(*)(), &C::c2>::value() == 10, "oops");
static_assert(Y_<int(*)(), &C::c2>::value() == 10, "oops");
static_assert(Z_<int(*)(), &C::c2>::value() == 10, "oops");


# 1 "/usr/include/assert.h" 1 3 4
# 36 "/usr/include/assert.h" 3 4
# 1 "/usr/include/features.h" 1 3 4
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
# 37 "/usr/include/assert.h" 2 3 4
# 65 "/usr/include/assert.h" 3 4

# 65 "/usr/include/assert.h" 3 4
extern "C" {


extern void __assert_fail (const char *__assertion, const char *__file,
      unsigned int __line, const char *__function)
     throw () __attribute__ ((__noreturn__));


extern void __assert_perror_fail (int __errnum, const char *__file,
      unsigned int __line, const char *__function)
     throw () __attribute__ ((__noreturn__));




extern void __assert (const char *__assertion, const char *__file, int __line)
     throw () __attribute__ ((__noreturn__));


}
# 83 "./cpp0x/constexpr-decltype1.C" 2


# 84 "./cpp0x/constexpr-decltype1.C"
template <typename T, T V>
constexpr typename X_<T, V>::type X_<T, V>::value;

int main() {
  C c;


  int t1 = X<int(*)(), &b<10>>::value()();
  int t2 = (c.*X_<int(C::*)()const, &C::c1>::value)();
  int t3 = X<int(*)(), &C::c2>::value()();

  
# 95 "./cpp0x/constexpr-decltype1.C" 3 4
 ((
# 95 "./cpp0x/constexpr-decltype1.C"
 t1 == 10
# 95 "./cpp0x/constexpr-decltype1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 95 "./cpp0x/constexpr-decltype1.C"
 "t1 == 10"
# 95 "./cpp0x/constexpr-decltype1.C" 3 4
 , "./cpp0x/constexpr-decltype1.C", 95, __PRETTY_FUNCTION__))
# 95 "./cpp0x/constexpr-decltype1.C"
                 ;
  
# 96 "./cpp0x/constexpr-decltype1.C" 3 4
 ((
# 96 "./cpp0x/constexpr-decltype1.C"
 t2 == 10
# 96 "./cpp0x/constexpr-decltype1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 96 "./cpp0x/constexpr-decltype1.C"
 "t2 == 10"
# 96 "./cpp0x/constexpr-decltype1.C" 3 4
 , "./cpp0x/constexpr-decltype1.C", 96, __PRETTY_FUNCTION__))
# 96 "./cpp0x/constexpr-decltype1.C"
                 ;
  
# 97 "./cpp0x/constexpr-decltype1.C" 3 4
 ((
# 97 "./cpp0x/constexpr-decltype1.C"
 t3 == 10
# 97 "./cpp0x/constexpr-decltype1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 97 "./cpp0x/constexpr-decltype1.C"
 "t3 == 10"
# 97 "./cpp0x/constexpr-decltype1.C" 3 4
 , "./cpp0x/constexpr-decltype1.C", 97, __PRETTY_FUNCTION__))
# 97 "./cpp0x/constexpr-decltype1.C"
                 ;
  return 0;
}
