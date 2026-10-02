//type:fp
//options_all:--c++20
//remark:[6.0] C++20: More implicit moves in return statements and throw expressions
// 8/6/19   [EDGcpfe/21596]
//
// C++20: More implicit moves in return statements and throw expressions
//
// Committee document P1825R0 added additional contexts where implicit moves are
// possible.  Rvalue reference variables are now candidates for implicit moves,
// and throw expressions now match return statements for when an implicit move is
// possible, so long as the scope of the thrown entity does not extend past the
// try block containing the throw.
struct base {
  base();
  base(base const &) = delete;
  base(base &&);
};
struct derived : base {};
base h(base &&b, derived&& d, bool c) {
  if (c)
    throw b; // Now calls move constructor
  return d;  // Now calls move constructor
}
