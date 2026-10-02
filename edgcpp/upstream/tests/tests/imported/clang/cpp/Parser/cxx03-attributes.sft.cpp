//type: fp
//options:  --c++03
// RUN: %clang_cc1 -fsyntax-only -verify -std=c++03 %s
// expected-no-diagnostics

struct S {
    S([[clang::lifetimebound]] int&) {}
};
