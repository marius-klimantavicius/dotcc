//type:fn
//options_all:--g++
//remark:[4.2] GNU compatibility: static_cast to pointer to cv-qualified derived class
// 10/6/09  [EDGcpfe/9593]
//
// GNU compatibility: static_cast to pointer to cv-qualified derived class
//
// In versions 3.4 through at least 4.4 of g++, there is a bug such that
// using static_cast to convert a pointer to a cv-unqualified base class into
// a pointer to a cv-qualified derived class drops the cv-qualification from
// the result type.  The front end now emulates this bug in g++ mode when
// gnu_version is >= 30400.
struct B { };
struct D: B { };
D* g(B* p) {
  return static_cast<const D*>(p);  // Accepted when the 4.2 change was made;
                                    // later versions diagnose a return type
                                    // mismatch.
}
