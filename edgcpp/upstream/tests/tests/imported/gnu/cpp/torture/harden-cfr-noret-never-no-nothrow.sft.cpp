//type: fp
//options: 
# 0 "./torture/harden-cfr-noret-never-no-nothrow.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/harden-cfr-noret-never-no-nothrow.C"







# 1 "./torture/harden-cfr-noret-no-nothrow.C" 1
# 10 "./torture/harden-cfr-noret-no-nothrow.C"
void __attribute__ ((__noreturn__)) h (void);


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
# 14 "./torture/harden-cfr-noret-no-nothrow.C" 2
# 9 "./torture/harden-cfr-noret-never-no-nothrow.C" 2
