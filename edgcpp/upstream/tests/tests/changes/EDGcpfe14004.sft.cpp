//type:fp
//options_all:--c++11
//remark:[4.10] C++11: implicit initialization in aggregate initializers
// 5/27/14  [EDGcpfe/14004,EDGcpfe/14869]
//
// C++11: implicit initialization in aggregate initializers
//
// C++11 changed the initialization of aggregate members without a corresponding
// initializer in an aggregate initializer from "value initialization" (the C++03
// behavior) to being initialized from an empty initializer list.  This is
// notably different from the C++03 behavior in that an initializer-list
// constructor is considered for such initialization.
//
// The front end now implements this new rule (introduced in the language through
// Core issue 1070) in its C++11 modes (except in some clang and GNU modes).
//
// Note that this required some changes in lowering that back ends doing their
// own lowering may have to emulate: Specifically, previously any arguments for
// a constructor selected for the initialization of an array were default
// arguments; now, they may be arguments representing a generated empty
// initializer-list.
//
// Also, in Cfront ABI configuration (i.e., IA64_ABI set to FALSE) the mangling
// of an aggregate initializer appearing in template signatures (an unusual case)
// may change if the aggregate initializer leaves out some element initializers.
//
// The encoding of A{} in the mangled name of f<A>() is now different in the
// Cfront ABI.
#include <initializer_list>
struct S {
  S(std::initializer_list<int>);
};
S x[1] = {};  // Now accepted in C++11 modes.

struct A { union {int m = 37;} u; };
template <class T> auto f() -> decltype(T(* new A{}));
int main() { f<A>(); }
