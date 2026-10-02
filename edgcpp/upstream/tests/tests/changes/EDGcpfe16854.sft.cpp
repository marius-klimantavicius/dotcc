//type:fp
//options_all:--g++ --c++11
//remark:[4.11] GNU C++ compatibility: member access expressions for constant members
// 2/20/16  [EDGcpfe/16854,EDGcpfe/16891]
//
// GNU C++ compatibility: member access expressions for constant members
//
// The C++ Standard requires that in a member access expression like x.y or
// p->y, where "y" is a member that is not a nonstatic data member, the object
// expression is evaluated and discarded.  As a result, in standard C++, such
// expressions can only appear in constant expressions if both the member and
// the object expression are constant.  In C++11 mode, g++ does not enforce
// this rule for an object expression with no side effects, and the front end
// has now been changed to emulate this behavior in g++ mode.
// with --g++ --c++11:
struct S {
  enum { z, y };
} s;
void g(const S* x) {
  static_assert(x->y == 1, "");  // Previously an error, now accepted
}
