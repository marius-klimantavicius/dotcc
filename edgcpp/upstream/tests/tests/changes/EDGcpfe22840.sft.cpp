//type:fn
//options_all:--c++20
//remark:[6.2] Trailing requires-clauses on friend declarations
// 10/26/20 [EDGcpfe/22840]
//
// Trailing requires-clauses on friend declarations
//
// The front end now issues an error (as required by the forthcoming C++20
// standard) on a friend function declaration with a trailing requires-clause in
// a class template definition if that function declaration is not a definition.
template<typename T> class X {
  friend void f(T p) requires (sizeof(T) < 100);  // Now an error.
};
