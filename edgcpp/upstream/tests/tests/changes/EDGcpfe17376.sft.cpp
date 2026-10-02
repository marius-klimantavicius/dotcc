//type:fp
//remark:[4.12] Dependent decltypes and overloaded function template declarations
// 9/27/16  [EDGcpfe/17376]
//
// Dependent decltypes and overloaded function template declarations
//
// The front end sometimes failed to distinguish dependent decltype constructs in
// overloaded function template declarations.  As a result, it treated a later
// declaration as redeclaring a former one, and spurious errors often ensued.
//
// This is now fixed.
template<typename> struct C {};
template<typename T> C<decltype(T::x)> g() { return 0; }
template<typename T> C<decltype(T::y)> g() { return 0; }
  // Previously triggered a spurious error claiming that the template
  // has already been defined.  Now accepted (there are two overloaded
  // templates g).
