//type:fp
//remark:[6.0] Deduction of operator templates
// 10/16/19 [EDGcpfe/21916]
//
// Deduction of operator templates
//
// Operator functions have certain constraints on their parameters.
// most operators require that at least one parameter have a class or enumeration
// type, or a reference type for such a type.  Previously, the front end failed to
// diagnose violations of such constraints when instantiating operator templates.
// Now such violations are handled as deduction failures.
//
// Previously, this resulted in an ambiguity error: The operator== template
// instantiated to operator==(D *const &, B const*) conflicted with the builtin
// equality operator.  Now, that operator== instance is discarded because neither
// of its parameters is a class or enumeration type.
 template<typename T> struct X { operator T const&(); };
 struct B {};
 struct D : public B {};
 template<typename T> bool operator==(T const&, B const*);
 bool g(D *x, X<B*> y) {
   return x == y;  // Previously an ambiguity error.  Now picks the
 }                 // built-in
