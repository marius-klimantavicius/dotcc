//type:fp
//options_all:--c++11
//remark:[4.10.1] Non-constant return in prototype instantiations of constexpr function
// 6/3/15   [EDGcpfe/14560,EDGcpfe/16225]
//
// Non-constant return in prototype instantiations of constexpr function
//
// A constexpr function template with a return statement that produces a non-
// constant expression in all instantiations is invalid in C++11.  Although the
// standard makes a diagnostic optional in such cases, the front end often does
// manage to identify such errors.  Other compilers do not, however, and to
// improve compatibility with such compilers the severity of the front end's
// diagnostic has been lowered to a warning in such cases (except in strict
// mode).
template<typename> struct D {
  D();
  constexpr D m() const {
    return D();  // Previously this always triggered an error.  Now it
  }              // only elicits a warning in nonstrict C++11 modes.
};
