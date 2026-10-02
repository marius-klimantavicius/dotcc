//type:fp
//options_all:--c++11
//remark:[4.8] Deleted special member functions and C++11 SFINAE rules
// 6/24/13  [EDGcpfe/14222]
//
// Deleted special member functions and C++11 SFINAE rules
//
// The front end previously did not always fail template deduction on a reference
// to a deleted special member function, as would be required by the C++11 SFINAE
// rules (see Changes entry of 4/26/10 for EDGcpfe/9169).
//
// In this example, selecting candidate (1) for the call in main() would require
// the use of the deleted copy constructor of S.  So the C++11 SFINAE rules
// remove that candidate from the set, and that is now correctly done: Candidate
// (2) is then selected for the call.  Previously, both candidates competed in
// overload resolution, and that resulted in an ambiguity (since the parameter
// types are otherwise identical).
struct S {
  S();
  S(S const&) = delete;
} s;
template<class T> int f(decltype(T(s)) *p);  // (1)
template<class T> char f(decltype(T()) *p);  // (2)
int main() {
  f<S>(new S);  // Should pick (2).  Previously reported as ambiguous.
}
