//type: lp
//options: 
//options_all: --set_flag=no_checking_pragmas
# 0 "./ext/gnu-inline-global-redecl.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ext/gnu-inline-global-redecl.C"
# 11 "./ext/gnu-inline-global-redecl.C"
# 1 "./ext/gnu-inline-common.h" 1
# 12 "./ext/gnu-inline-global-redecl.C" 2

 extern int fn (void);
 __attribute__((gnu_inline)) inline int fn (void) { return 0; }
 extern int fn (void);

int main () {
  fn ();
}
