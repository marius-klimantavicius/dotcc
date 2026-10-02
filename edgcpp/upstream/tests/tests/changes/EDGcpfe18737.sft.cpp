//type:fp
//options_all:--c++14
//remark:[5.0] Failure to deduce return type due to mutually dependent instantiations
// 11/3/17  [EDGcpfe/18737]
//
// Failure to deduce return type due to mutually dependent instantiations
//
// When calling a template function with a deduced return type, the function
// must be instantiated immediately to determine its return type.  In some cases,
// that instantiation can trigger the instantiation of an inline function with a
// known return type, but that instantiation in turn may require the original
// deduced return type.  Such a mutual recursive situation previously produced a
// spurious error about the return type not being deducible.
//
// This problem (which did not occur in GCC, Clang, or Microsoft modes) is now
// fixed.
template<typename Fn> struct F {
  Fn f;
  decltype(auto) operator()() { return f(*this); }
};
template<typename Fn> F<Fn> g(Fn f) { return { f }; }
int main() {
  auto zero = g([](auto& self) -> int {
                  return 0;
                  self();  // Previously triggered an error about
                });        // F::operator()'s return type not being
  return zero();           // deducible.
}
