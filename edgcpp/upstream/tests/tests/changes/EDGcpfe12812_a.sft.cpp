//type:fn
//options_all:--c++11
//remark:[4.8] Criterion for user-provided member functions
// 7/12/13  [EDGcpfe/12812,EDGcpfe/14282]
//
// Criterion for user-provided member functions
//
// The C++11 standard defines a function to be user-provided if it is explicitly
// declared and neither defaulted nor deleted on its first declaration.  However,
// that definition was different earlier in the standardization process, and that
// is what we previously implemented: deleted member functions were always
// considered to be user-provided (and the front end erroneously made implicitly-
// declared deleted functions user-provided as well).  The front end has now been
// updated to implement the newer definition, which changes the validity of
// certain cases.
//
// Previously, S was not considered an aggregate type because it included a
// user-provided constructor (the deleted copy constructor), and so the
// initialization of s was invalid because no matching constructor is present
// in S.  However, with the new rules, the deleted constructor is no longer
// considered to be user-provided, and S is therefore an aggregate type, which
// in turn makes the initialization of s valid.
//
// As part of this change, a fix was made also to more reliably mark classes
// with implicitly deleted copy functions (constructors and/or copy assignment
// operators) as being not bitwise copyable.
//
// In this example, S has a deleted copy constructor because of the user-declared
// move assignment operator.  Struct S should therefore not be considered
// bitwise copyable, but previously the front end failed to mark the class as
// such.  That in turn caused it to fail to diagnose the use of the deleted copy
// copy constructor in some contexts (such as the conversion needed for the
// conditional expression in this example).  This is now fixed.
struct S {
  S&& operator=(S&&);
};
S f();
void g(S x) {
  true ? f() : x;  // Now an error because it makes use of the
}                  // deleted copy constructor.
