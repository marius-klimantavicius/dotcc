//type: fp
//options: 
# 0 "./torture/harden-cfr-noret-no-nothrow.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/harden-cfr-noret-no-nothrow.c"







# 1 "./torture/../../c-c++-common/torture/harden-cfr-noret.c" 1
# 10 "./torture/../../c-c++-common/torture/harden-cfr-noret.c"
extern void __attribute__ ((__noreturn__)) g (void);

void f(int i) {
  if (i)

    g ();

}

void __attribute__ ((__noinline__, __noclone__))
h(void) {

  g ();
}


void h2(void) {

  h ();

}
# 9 "./torture/harden-cfr-noret-no-nothrow.c" 2
