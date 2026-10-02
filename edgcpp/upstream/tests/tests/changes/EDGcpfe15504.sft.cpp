//type:fp
//options_all:--microsoft_version 1927
//remark:[6.1] Folding of source location builtins
// 5/15/20  [EDGcpfe/15504,EDGcpfe/20605,EDGcpfe/22391]
//
// Folding of source location builtins
//
// The builtin functions __builtin_COLUMN, __builtin_LINE, __builtin_FILE, and
// __builtin_FUNCTION are now generally folded in the front end.  In most cases
// the source location reported is that of the call of the builtin itself,
// but for calls in default arguments, the source location is that of the call
// to the routine with the default argument.  Similarly, if the builtin call
// is in a default member initializer, the reported location is that of the
// constructor performing the initialization.  The latter case cannot be
// determined by the front end and is left in the IL or replaced during lowering
// (in configurations that use lowering).  These builtins are available when
// emulating the latest versions of GCC, clang, or MSVC.
// with --microsoft_version 1927:
extern "C" int printf(const char *,...);
#define print(loc) \
  printf("%s: FILE=%s, FUNCTION=%s, LINE=%d, COLUMN=%d\n", (loc), \
         __builtin_FILE(), __builtin_FUNCTION(), __builtin_LINE(), \
         __builtin_COLUMN())
struct A {
  int x = (print("in A::x"), 0);
} a;
struct B {
  int x;
  B() : x((print("in B()"), 0)) {}
} b;
void def_arg_fn(int x = (print("in def_arg_fn"), 0)) {}
int main() {
  print("in main");
  def_arg_fn();
  return 0;
}
