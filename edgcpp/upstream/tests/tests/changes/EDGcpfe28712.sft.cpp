//type:fp
//options_all:--c++20
//remark:Template-head requires clauses on implicit deduction guides
// 6/2/26   [EDGcpfe/28712]
//
// Template-head requires clauses on implicit deduction guides
//
// Previously, implicit deduction guides synthesized from a constructor template
// did not include template-head requires clauses from the class or constructor
// template.  Class template argument deduction (CTAD) could therefore fail or
// select an unsuitable guide.
template<typename ... Ts>
struct C {
  C(Ts ...);
  template<typename U> requires false
  C(U u);
};
C c(1);  // Previously a spurious error.  Now okay.
