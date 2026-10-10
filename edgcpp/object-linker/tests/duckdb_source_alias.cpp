// Source-only aliases retain their original RHS and substitution arguments.
// This is the libc++ _IsValidExpansion / __is_primary_template shape.
namespace alias_graph {
template<class...> using Void = void;
template<template<class...> class Test, class... Args, class = Test<Args...>>
int test_impl(int);
template<template<class...> class, class...> long test_impl(...);
template<template<class...> class Test, class... Args>
using Valid = decltype(test_impl<Test, Args...>(0));
template<class T> using Primary = typename T::primary_type;
template<class T> using IsPrimary = Valid<Primary, T>;
template<class> struct Box { int tag; };
template<class T> struct Owner { Box<IsPrimary<T>> value = {}; };
template<class T> struct OtherOwner { Box<Valid<Void, T>> value = {}; };
template<class T> struct EmptyOwner { Box<Valid<Void>> value = {}; };

template<class T> using Declared = decltype(T::value);
template<class T> using Parenthesized = decltype((T::value));
template<class T> using Typeof = __typeof__(T);
template<class T> using Underlying = __underlying_type(T);
template<class T> struct QueryOwner {
    Box<Declared<T>> declared = {};
    Box<Parenthesized<T>> reference = {};
    Box<Typeof<T>> same = {};
};
template<class T> struct EnumOwner { Box<Underlying<T>> value = {}; };
struct Concrete { using primary_type = int; static int value; };
static_assert(sizeof(IsPrimary<Concrete>) == sizeof(int));
static_assert(sizeof(Valid<Primary, int>) == sizeof(long));
static_assert(sizeof(Valid<Void>) == sizeof(int));
}
int main() { return 0; }
