//type:fp
//remark:[4.9] Definition of "standard layout"
// 11/20/13 [EDGcpfe/14682]
//
// Definition of "standard layout"
//
// The __is_standard_layout type trait helper produces "true" for types that are
// scalar types or "standard layout class types".  The latter is a term from the
// C++11 standard that turns out to be open to interpretation for some classes
// that have a direct base class that is itself derived from a nonempty base.
// The front end's behavior has now been changed to match that of the GCC compiler
// (which is expected to be the interpretation that will be selected when the C++
// standards committee resolves this open issue).
struct B { int i; };
struct C: B {};
struct D: C {};
static_assert(__is_standard_layout(D), "?");
  // Previously, this static assertion failed.  Now it succeeds.
