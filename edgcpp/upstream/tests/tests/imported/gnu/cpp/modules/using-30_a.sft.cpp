//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules" }
// { dg-module-cmi M }

export module M;

namespace {
  using A = int;
  typedef int B;

  struct Internal {};
  using C = Internal;
  typedef Internal D;
}
