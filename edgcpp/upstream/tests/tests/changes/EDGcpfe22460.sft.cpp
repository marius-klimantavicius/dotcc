//type:fp
//options_all:--g++
//remark:[6.1] GNU compatibility: Deferred evaluation of default function arguments for
// 3/13/20  [EDGcpfe/22460]
//
// GNU compatibility: Deferred evaluation of default function arguments for
// function templates
//
// GCC defers validation of a default function argument for a function template
// until it's required, even when the argument is non-dependent.
//
// This is now accepted in GNU emulation mode.
struct X;
struct A {
  template<class c> A(c &&, X = X()); // Incomplete type error for "X"
};
