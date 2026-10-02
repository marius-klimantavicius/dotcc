//type:fp
//options_all:--c++17
//remark:[5.1] Spurious error on initializer for static data member template
// 5/13/19  [EDGcpfe/19757,EDGcpfe/21050]
//
// Spurious error on initializer for static data member template
//
// The front end previously issued a spurious error on initializers for static
// data member templates that are introduced by an assignment token ("=") followed
// by a left parenthesis.
//
// That is now fixed.
struct S {
  template<typename T> static constexpr T *const sm = (T*)0;
        // Previously triggered spurious error about a missing ")".
};      // Now okay.
