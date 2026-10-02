//type:fp
//remark:[4.9] Instantiation errors during the generation of special members
// 11/5/13  [EDGcpfe/13735]
//
// Instantiation errors during the generation of special members
//
// When generating the declarations of special member functions of a class (e.g.,
// copy constructors or copy assignment operators) the front end may trigger
// instantiation errors even if the special member isn't used.  In particular,
// the corresponding special members of subobjects are examined to
// This examination may trigger the instantiation of member templates, which in
// turn can lead to instantiation errors.
//
// Previously, in GNU C++ mode, the front end instantiated E<A> while doing
// overload resolution on A::operator= to determine the exception specification
// for the generated copy assignment operator of B.  That in turn triggered an
// error because A::error does not exist.  In other modes, overload resolution
// on A::operator= was needed to determine if the generate copy assignment
// operator of B is implicitly "deleted".  However, these errors are not always
// triggered by GNU and Microsoft compilers.  To more closely match these
// compilers' behavior, two changes have been made.
//
// First, in nonstrict C++ modes, the front end now delays the generation of
// exception specifications for generated special member functions until the
// exception specification is needed (if at all).  A special member function
// with an indeterminate exception specification has an exception specification
// with the flag "indeterminate" set to TRUE (this mechanism was already used for
// certain generated default constructors in the presence of field initializers).
// Delaying the generation of exception specifications in this way avoids
// triggering instantiation errors in some cases.
//
// Second, when selecting an overloaded copy/move assignment operator, member
// operator templates are sometimes discarded early (i.e., without instantiating
// them) if it can be determined that a nontemplate candidate is an obviously
// better match.
template<typename T> struct E { typedef typename T::error type; };
struct A {
  A& operator=(const A&);
  template <class U> typename E<U>::type operator=(U&);
};
struct B : A { };
