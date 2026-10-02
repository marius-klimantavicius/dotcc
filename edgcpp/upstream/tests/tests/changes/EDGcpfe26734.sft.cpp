//type:fp
//options_all:--c++20
//remark:[6.6] Evaluation of consteval functions
// 10/20/23 [EDGcpfe/26734]
//
// Evaluation of consteval functions
//
// Previously, the front end constant-evaluated each call f() independently,
// triggering spurious errors because the address of a consteval function is not
// permitted as part of the result of a top-level constant-expression.  However,
// in this case the top-level constant-expression is "f() == f()", whose result
// does not include the address of g().  The problem is now fixed (and the
// example is now accepted).
consteval void g() {}
consteval auto f() { return &g; }
static_assert(f() == f());  // Previously an error.  Now okay.
