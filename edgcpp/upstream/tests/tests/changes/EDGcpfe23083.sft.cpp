//type:fp
//options_all:--c++14 --gnu_version=70300
//remark:[6.3] Accessing constexpr variables with mutable members
// 3/9/21   [EDGcpfe/23083,EDGcpfe/23735,EDGcpfe/24027]
//
// Accessing constexpr variables with mutable members
//
// The front end previously did not permit accessing a constexpr variable with a
// mutable member even if the mutable member itself was not examined.  Now such
// cases can be accepted.
struct S {
  int i;
  mutable int mi;
};
constexpr S cs{ 1, 2 };
constexpr int x = cs.i;  // Previously an error.  Now okay.
