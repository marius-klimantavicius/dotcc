//type: fp
//options: : --c89 --strict_gnu
// RUN: %clang_cc1 -fsyntax-only -Wno-unused-value -verify %s
// RUN: %clang_cc1 -fsyntax-only -Wno-unused-value -verify -std=gnu89 %s

// expected-no-diagnostics
void foo(void *vp) {
  sizeof(*vp);
  sizeof(*(vp));
  void inner(typeof(*vp));
}
