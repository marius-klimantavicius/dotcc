//type: rp
//options: 
# 0 "./torture/hardbool.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/hardbool.c"


# 1 "/usr/include/assert.h" 1 3 4
# 36 "/usr/include/assert.h" 3 4
# 1 "/usr/include/features.h" 1 3 4
# 375 "/usr/include/features.h" 3 4
# 1 "/usr/include/sys/cdefs.h" 1 3 4
# 392 "/usr/include/sys/cdefs.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 393 "/usr/include/sys/cdefs.h" 2 3 4
# 376 "/usr/include/features.h" 2 3 4
# 399 "/usr/include/features.h" 3 4
# 1 "/usr/include/gnu/stubs.h" 1 3 4
# 10 "/usr/include/gnu/stubs.h" 3 4
# 1 "/usr/include/gnu/stubs-64.h" 1 3 4
# 11 "/usr/include/gnu/stubs.h" 2 3 4
# 400 "/usr/include/features.h" 2 3 4
# 37 "/usr/include/assert.h" 2 3 4
# 65 "/usr/include/assert.h" 3 4




# 68 "/usr/include/assert.h" 3 4
extern void __assert_fail (const char *__assertion, const char *__file,
      unsigned int __line, const char *__function)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__noreturn__));


extern void __assert_perror_fail (int __errnum, const char *__file,
      unsigned int __line, const char *__function)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__noreturn__));




extern void __assert (const char *__assertion, const char *__file, int __line)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__noreturn__));



# 4 "./torture/hardbool.c" 2
# 18 "./torture/hardbool.c"

# 18 "./torture/hardbool.c"
typedef char __attribute__ ((hardbool (0, ~0))) hbool;

typedef unsigned char __attribute__ ((__hardbool__ (1, 0))) zbool;

struct hs {
  hbool a[2];
  hbool x:2;
  hbool y:5;
  zbool z:1;
};

hbool var = 0;

struct hs x = { { 1, 0 }, 2, 0, 2 };

int f(hbool v) {
  return !v;
}

int g(int i) {
  return f(i);
}

hbool h(hbool x) {
  return x;
}

hbool h2(hbool x) {
  return h(x);
}

int hsx(struct hs v) {
  return v.x;
}

int ghs(hbool s) {
  struct hs v = { {s, !s}, s, !s, s };
  return hsx (v);
}

int t = (hbool)2;

void check_pfalse (hbool *p)
{
  
# 62 "./torture/hardbool.c" 3 4
 ((
# 62 "./torture/hardbool.c"
 !*p
# 62 "./torture/hardbool.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 62 "./torture/hardbool.c"
 "!*p"
# 62 "./torture/hardbool.c" 3 4
 , "./torture/hardbool.c", 62, __PRETTY_FUNCTION__))
# 62 "./torture/hardbool.c"
             ;
  
# 63 "./torture/hardbool.c" 3 4
 ((
# 63 "./torture/hardbool.c"
 *(char*)p == (char)0
# 63 "./torture/hardbool.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 63 "./torture/hardbool.c"
 "*(char*)p == (char)0"
# 63 "./torture/hardbool.c" 3 4
 , "./torture/hardbool.c", 63, __PRETTY_FUNCTION__))
# 63 "./torture/hardbool.c"
                                             ;
  
# 64 "./torture/hardbool.c" 3 4
 ((
# 64 "./torture/hardbool.c"
 !(int)(hbool)*p
# 64 "./torture/hardbool.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 64 "./torture/hardbool.c"
 "!(int)(hbool)*p"
# 64 "./torture/hardbool.c" 3 4
 , "./torture/hardbool.c", 64, __PRETTY_FUNCTION__))
# 64 "./torture/hardbool.c"
                         ;
}

void check_ptrue (hbool *p)
{
  
# 69 "./torture/hardbool.c" 3 4
 ((
# 69 "./torture/hardbool.c"
 *p
# 69 "./torture/hardbool.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 69 "./torture/hardbool.c"
 "*p"
# 69 "./torture/hardbool.c" 3 4
 , "./torture/hardbool.c", 69, __PRETTY_FUNCTION__))
# 69 "./torture/hardbool.c"
            ;
  
# 70 "./torture/hardbool.c" 3 4
 ((
# 70 "./torture/hardbool.c"
 *(char*)p == (char)~0
# 70 "./torture/hardbool.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 70 "./torture/hardbool.c"
 "*(char*)p == (char)~0"
# 70 "./torture/hardbool.c" 3 4
 , "./torture/hardbool.c", 70, __PRETTY_FUNCTION__))
# 70 "./torture/hardbool.c"
                                            ;
  
# 71 "./torture/hardbool.c" 3 4
 ((
# 71 "./torture/hardbool.c"
 (int)(hbool)*p
# 71 "./torture/hardbool.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 71 "./torture/hardbool.c"
 "(int)(hbool)*p"
# 71 "./torture/hardbool.c" 3 4
 , "./torture/hardbool.c", 71, __PRETTY_FUNCTION__))
# 71 "./torture/hardbool.c"
                        ;
}

void check_vfalse (hbool v)
{
  check_pfalse (&v);
}

void check_vtrue (hbool v)
{
  check_ptrue (&v);
}

int main () {
  check_pfalse (&var);
  var = !(int)(hbool)(_Bool)var;
  check_ptrue (&var);
  var = (zbool)var;
  check_ptrue (&var);

  check_ptrue (&x.a[0]);
  check_pfalse (&x.a[1]);
  check_vtrue (x.x);
  check_vfalse (x.y);
  check_vtrue (x.z);

  check_vtrue (t);

  check_vtrue (var && t);
  check_vfalse (!var || x.y);

  check_vfalse (f (2));
  check_vfalse (f (1));
  check_vtrue (f (0));

  check_vfalse (g (2));
  check_vfalse (g (1));
  check_vtrue (g (0));

  check_vtrue (h (2));
  check_vtrue (h (1));
  check_vfalse (h (0));

  check_vtrue (h2 (2));
  check_vtrue (h2 (1));
  check_vfalse (h2 (0));
}
