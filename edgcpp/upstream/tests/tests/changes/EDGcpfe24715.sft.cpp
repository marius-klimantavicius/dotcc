//type:fp
//options_all:--c++20
//remark:[6.3] __VA_OPT__ and expansion of macro arguments
// 9/20/21  [EDGcpfe/24715]
//
// __VA_OPT__ and expansion of macro arguments
//
// The preprocessor previously failed to expand a macro argument passed to an
// ellipsis if __VA_ARGS__ does not appear in the macro definition.  However,
// the use of __VA_OPT__ in the definition should also trigger expansion of
// such macro arguments.  This is now fixed.
#define Z() /* nothing */
int f();
#define M(...) f(__VA_OPT__(,))
int i = M(Z());   // Previously erroneously expanded to "f(,)" because
                  // the argument to M was not treated as empty
