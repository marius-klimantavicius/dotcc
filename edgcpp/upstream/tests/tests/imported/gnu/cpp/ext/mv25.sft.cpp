//type: rp
//options: 
//options_all: --gnu_version=80200 -tused -e 200 --no_wrap
# 1 "./ext/mv25.C"
# 1 "<built-in>"
# 1 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 1 "<command-line>" 2
# 1 "./ext/mv25.C"






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
# 8 "./ext/mv25.C" 2



# 10 "./ext/mv25.C"
int foo () __attribute__((target("default")));
int foo () __attribute__((target("pclmul")));

int main ()
{
  int val = foo ();

  if (__builtin_cpu_supports ("pclmul"))
    
# 18 "./ext/mv25.C" 3 4
   ((
# 18 "./ext/mv25.C"
   val == 1
# 18 "./ext/mv25.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 18 "./ext/mv25.C"
   "val == 1"
# 18 "./ext/mv25.C" 3 4
   , "./ext/mv25.C", 18, __PRETTY_FUNCTION__))
# 18 "./ext/mv25.C"
                    ;
  else
    
# 20 "./ext/mv25.C" 3 4
   ((
# 20 "./ext/mv25.C"
   val == 0
# 20 "./ext/mv25.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 20 "./ext/mv25.C"
   "val == 0"
# 20 "./ext/mv25.C" 3 4
   , "./ext/mv25.C", 20, __PRETTY_FUNCTION__))
# 20 "./ext/mv25.C"
                    ;

  return 0;
}

int __attribute__ ((target("default")))
foo ()
{
  return 0;
}

int __attribute__ ((target("pclmul")))
foo ()
{
  return 1;
}
