//type:fp
//options_all:--c++17
//remark:[6.8] Access checking for implicit deduction guides
// 2/27/25  [EDGcpfe/25300,EDGcpfe/27874]
//
// Access checking for implicit deduction guides
//
// Previously, the front end did not grant an implicit deduction guide member
// access privileges to its class during substitution, resulting in spurious class
// template argument deduction failures.
template<int I, int V> struct B { };
template<int I>
class C {
  static constexpr int v = I;
public:
  C(int, B<I, v>);
};
C c{1, B<1, 1>{}};  // Previously a spurious error.  Now okay.
