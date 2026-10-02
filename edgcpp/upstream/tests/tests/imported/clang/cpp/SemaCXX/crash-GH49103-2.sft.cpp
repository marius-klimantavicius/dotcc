//type: fn
//options:  --c++03: --c++11: --c++14: --c++17: --c++20: --c++23: --c++26
// RUN: %clang_cc1 -verify -std=c++98 %s
// RUN: %clang_cc1 -verify -std=c++11 %s
// RUN: %clang_cc1 -verify -std=c++14 %s
// RUN: %clang_cc1 -verify -std=c++17 %s
// RUN: %clang_cc1 -verify -std=c++20 %s
// RUN: %clang_cc1 -verify -std=c++23 %s
// RUN: %clang_cc1 -verify -std=c++2c %s

// https://github.com/llvm/llvm-project/issues/49103

template<class> struct A; // expected-note 0+ {{}}
struct S : __make_integer_seq<A, int, 42> { }; // expected-error 0+ {{}}
S s;
