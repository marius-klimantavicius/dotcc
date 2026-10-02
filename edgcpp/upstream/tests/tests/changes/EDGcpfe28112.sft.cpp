//type:fp
//options_all:--c++14
//remark:[6.8] Over-eager instantiation of primary variable template type
// 7/23/25  [EDGcpfe/28112,EDGcpfe/28246]
//
// Over-eager instantiation of primary variable template type
//
// Previously, the front end attempted to instantiate the type of the primary
// variable template, specialized with T=void, when parsing the explicit template
// specialization declaration.  That is now fixed.
template<typename T>
struct C {
  T t;  // Previously a spurious error.  Now okay
};
template<typename T> C<T> var;
template<> C<void *> var<void>;
