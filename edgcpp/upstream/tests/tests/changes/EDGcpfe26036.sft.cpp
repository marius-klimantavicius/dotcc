//type:fp
//options_all:--c++20
//remark:[6.6] Constraint checking when synthesizing copy/move constructors
// 7/7/23   [EDGcpfe/26036,EDGcpfe/26082,EDGcpfe/26121]
//
// Constraint checking when synthesizing copy/move constructors
//
// Previously, the front end failed to check the associated constraints of
// non-template constructors during overload resolution for synthesized copy/move
// constructors.
template<int>
struct B {
  B(const B&);
  B(B &&) requires false;
};
struct D : public B<0> { };  // Previously a spurious error.  Now okay.
D &&g();
D d(g());
