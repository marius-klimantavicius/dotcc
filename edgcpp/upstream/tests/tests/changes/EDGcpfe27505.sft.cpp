//type:fp
//options_all:--c++20
//remark:[6.7] Short-circuit substitution of atomic constraints in trailing requires clauses
// 12/17/24 [EDGcpfe/27505]
//
// Short-circuit substitution of atomic constraints in trailing requires clauses
//
// When performing satisfaction checking of a trailing requires clause, each
// atomic constraint needs to be substituted and checked separately.  Previously,
// for a non-template member function of a nested class template, the front end
// substituted into the entire requires clause, which triggered the instantiation
// of X<1> in the example above.  Now, each atomic constraint is checked
// separately, and since the first disjunctive clause is satisfied, no further
// substitutions are performed.
template<int I>
struct X {
  static_assert(I != 1);  // Previously an error.  Now okay.
};
template<int I>
struct C {
  template<int J>
  struct D  {
    static int f() requires (I == J) || X<I>::v || X<J>::v;
  };
};
int i = C<1>::D<1>::f();
