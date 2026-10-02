//type: rp
//options: 
# 0 "./expr/lval2.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./expr/lval2.C"







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

# 65 "/usr/include/assert.h" 3 4
extern "C" {


extern void __assert_fail (const char *__assertion, const char *__file,
      unsigned int __line, const char *__function)
     throw () __attribute__ ((__noreturn__));


extern void __assert_perror_fail (int __errnum, const char *__file,
      unsigned int __line, const char *__function)
     throw () __attribute__ ((__noreturn__));




extern void __assert (const char *__assertion, const char *__file, int __line)
     throw () __attribute__ ((__noreturn__));


}
# 9 "./expr/lval2.C" 2


# 10 "./expr/lval2.C"
enum Foo { A, B };

template<typename T> T &qMin(T &a, T &b)
{
  return a < b ? a : b;
}

int main (int, char **)
{
  Foo f = A;
  Foo g = B;
  Foo &h = qMin(f, g);
  
# 22 "./expr/lval2.C" 3 4
 ((
# 22 "./expr/lval2.C"
 &h == &f || &h == &g
# 22 "./expr/lval2.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 22 "./expr/lval2.C"
 "&h == &f || &h == &g"
# 22 "./expr/lval2.C" 3 4
 , "./expr/lval2.C", 22, __PRETTY_FUNCTION__))
# 22 "./expr/lval2.C"
                              ;
  const Foo &i = qMin((const Foo&)f, (const Foo&)g);
  
# 24 "./expr/lval2.C" 3 4
 ((
# 24 "./expr/lval2.C"
 &i == &f || &i == &g
# 24 "./expr/lval2.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 24 "./expr/lval2.C"
 "&i == &f || &i == &g"
# 24 "./expr/lval2.C" 3 4
 , "./expr/lval2.C", 24, __PRETTY_FUNCTION__))
# 24 "./expr/lval2.C"
                              ;
  return 0;
}
