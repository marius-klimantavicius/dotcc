//type: fp
//options:  --c++17
// RUN: %clang_cc1 -fsyntax-only -Wunused -std=c++1z -verify %s
// expected-no-diagnostics

void f();
[[maybe_unused]] void f();

void f() {
}
