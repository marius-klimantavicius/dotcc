//type:fp
//remark:[4.5] Spurious error on friend class declaration using name of nested template
// 1/23/12  [EDGcpfe/11185,EDGcpfe/12355,EDGcpfe/12606]
//
// Spurious error on friend class declaration using name of nested template
//
// Previously, the front end issued a spurious redeclaration error on some uses
// of a nested class of a class template, when the class template contains a
// member class template with the same name as a friend class declaration that
// appeared in the nested class.
//
// This is now fixed.
template<typename T> struct S {
  struct N { friend struct F; };
  template<typename> struct F {};
};
S<int>::N p;  // Triggered a spurious error about "friend struct F;" being
              // and invalid redeclaration of itself.
