//type:fp
//options_all:--c++20 --g++
//remark:[6.6] Substitution for call to member function of class template
// 7/4/23   [EDGcpfe/24665,EDGcpfe/26480]
//
// Substitution for call to member function of class template
//
// Previously, the front end failed to substitute the member function g during
// constraint checking, causing the constraint to not be satisfied.  That is now
// fixed.
template<typename T> struct C {
  char g(T);
  int f() requires (sizeof(g(1)) == 1);
};
int i = C<int>().f();  // Previously a spurious error.  Now okay.
