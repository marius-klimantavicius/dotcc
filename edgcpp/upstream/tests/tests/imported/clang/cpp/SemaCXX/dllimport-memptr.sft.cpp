//type: fp
//options:  -w --ms_extensions --c++11 -w --ms_extensions: --c++17
// RUN: %clang_cc1 -triple x86_64-windows-msvc -fms-extensions -verify -std=c++11 %s
// RUN: %clang_cc1 -triple x86_64-windows-msvc -fms-extensions -verify -std=c++17 %s

// expected-no-diagnostics

struct __declspec(dllimport) Foo { int get_a(); };
template <int (Foo::*Getter)()> struct HasValue { };
HasValue<&Foo::get_a> hv;
