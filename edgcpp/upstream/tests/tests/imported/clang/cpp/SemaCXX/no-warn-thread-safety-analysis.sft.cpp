//type: fp
//options:  --c++11 --exceptions -DUSE_CAPABILITY=0: --c++11 --exceptions -DUSE_CAPABILITY=1: --c++17 --exceptions -DUSE_CAPABILITY=0: --c++17 --exceptions -DUSE_CAPABILITY=1
// RUN: %clang_cc1 -fsyntax-only -verify -std=c++11 -Wthread-safety -Wthread-safety-pointer -Wthread-safety-beta -Wno-thread-safety-negative -fcxx-exceptions -DUSE_CAPABILITY=0 %s
// RUN: %clang_cc1 -fsyntax-only -verify -std=c++11 -Wthread-safety -Wthread-safety-pointer -Wthread-safety-beta -Wno-thread-safety-negative -fcxx-exceptions -DUSE_CAPABILITY=1 %s
// RUN: %clang_cc1 -fsyntax-only -verify -std=c++17 -Wthread-safety -Wthread-safety-pointer -Wthread-safety-beta -Wno-thread-safety-negative -fcxx-exceptions -DUSE_CAPABILITY=0 %s
// RUN: %clang_cc1 -fsyntax-only -verify -std=c++17 -Wthread-safety -Wthread-safety-pointer -Wthread-safety-beta -Wno-thread-safety-negative -fcxx-exceptions -DUSE_CAPABILITY=1 %s
// expected-no-diagnostics

struct foo {
  ~foo();
};
struct bar : foo {};
struct baz : bar {};
baz foobar(baz a) { return a; }
