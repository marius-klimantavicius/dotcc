//type: fp
//options:  --ms_compatibility
// RUN: %clang_cc1 -fsyntax-only -fms-compatibility %s

template<typename T>
struct X {
  enum E *e;
};

X<int> xi;
