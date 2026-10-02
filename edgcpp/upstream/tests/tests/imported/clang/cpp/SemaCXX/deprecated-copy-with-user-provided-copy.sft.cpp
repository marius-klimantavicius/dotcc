//type: fp
//options:  --c++11: --c++11 --ms_compatibility: --c++11 -w: --c++11 -w --ms_compatibility
// RUN: %clang_cc1 -std=c++11 %s -Wdeprecated -verify
// RUN: %clang_cc1 -std=c++11 %s -Wdeprecated -verify -fms-compatibility
// RUN: %clang_cc1 -std=c++11 %s -Wdeprecated-copy-with-user-provided-copy -verify
// RUN: %clang_cc1 -std=c++11 %s -Wdeprecated-copy-with-user-provided-copy -verify -fms-compatibility

struct A {
  A &operator=(const A &); // expected-warning {{definition of implicit copy constructor for 'A' is deprecated because it has a user-provided copy assignment operator}}
};

void foo() {
  A a1;
  A a2(a1); // expected-note {{implicit copy constructor for 'A' first required here}}
}
