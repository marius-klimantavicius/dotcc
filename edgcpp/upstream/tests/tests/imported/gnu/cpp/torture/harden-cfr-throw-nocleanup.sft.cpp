//type: fp
//options: 
# 0 "./torture/harden-cfr-throw-nocleanup.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/harden-cfr-throw-nocleanup.C"







# 1 "./torture/harden-cfr-throw.C" 1






void __attribute__ ((__optimize__ (1))) h2(void);
void __attribute__ ((__optimize__ (1))) h2b(void);





extern void g (void);
extern void g2 (void);

void f(int i) {
  if (i)
    g ();

}

void f2(int i) {
  if (i)
    g ();
  else
    g2 ();

}

void h(void) {
  try {
    g ();
  } catch (...) {
    throw;
  }

}

struct needs_cleanup {
  ~needs_cleanup();
};

void h2(void) {
  needs_cleanup y;
  g();

}

extern void __attribute__ ((__nothrow__)) another_cleanup (void*);

void h2b(void) {
  int x __attribute__ ((cleanup (another_cleanup)));
  g();

}

void h3(void) {
  try {
    throw 1;
  } catch (...) {
  }

}

void h4(void) {
  throw 1;

}
# 9 "./torture/harden-cfr-throw-nocleanup.C" 2
