//type:fp
//options_all:--ms_c++20
//remark:[6.6] Microsoft/Clang: template constraint checking when synthesizing copy/move
// 7/7/23   [EDGcpfe/26373,EDGcpfe/26395]
//
// Microsoft/Clang: template constraint checking when synthesizing copy/move
// special members
//
// In modes where template constraints are not checked during template argument
// deduction (i.e., Microsoft and Clang modes), the front end previously failed to
// check template constraints during overload resolution for synthesized copy/move
// special members.
struct B {
  B(const B&);
  template<typename T> requires false
  B(T &&);
};
struct D : public B { };  // Previously a spurious error.  Now okay.
