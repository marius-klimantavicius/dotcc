//type: rp
//options: 
//options_all: --gnu_version=80200 -tused -e 200 --no_wrap
# 1 "./ext/mv1.C"
# 1 "<built-in>"
# 1 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 1 "<command-line>" 2
# 1 "./ext/mv1.C"





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
# 7 "./ext/mv1.C" 2



# 9 "./ext/mv1.C"
int foo ();
int foo () __attribute__ ((target("default")));



int foo () __attribute__ ((target("arch=corei7,popcnt")));


int foo () __attribute__ ((target("ssse3,avx2")));



int foo () __attribute__((target("arch=core2")));
int foo () __attribute__((target("arch=corei7")));
int foo () __attribute__((target("arch=atom")));

int foo () __attribute__((target("avx")));
int foo () __attribute__ ((target("arch=core2,sse4.2")));

int foo () __attribute__((target("arch=amdfam10")));
int foo () __attribute__((target("arch=bdver1")));
int foo () __attribute__((target("arch=bdver2")));

int (*p)() = &foo;
int main ()
{
  int val = foo ();
  
# 36 "./ext/mv1.C" 3 4
 ((
# 36 "./ext/mv1.C"
 val == (*p)()
# 36 "./ext/mv1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 36 "./ext/mv1.C"
 "val == (*p)()"
# 36 "./ext/mv1.C" 3 4
 , "./ext/mv1.C", 36, __PRETTY_FUNCTION__))
# 36 "./ext/mv1.C"
                        ;



  if (__builtin_cpu_is ("bdver1"))
    
# 41 "./ext/mv1.C" 3 4
   ((
# 41 "./ext/mv1.C"
   val == 1
# 41 "./ext/mv1.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 41 "./ext/mv1.C"
   "val == 1"
# 41 "./ext/mv1.C" 3 4
   , "./ext/mv1.C", 41, __PRETTY_FUNCTION__))
# 41 "./ext/mv1.C"
                    ;
  else if (__builtin_cpu_is ("bdver2"))
    
# 43 "./ext/mv1.C" 3 4
   ((
# 43 "./ext/mv1.C"
   val == 2
# 43 "./ext/mv1.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 43 "./ext/mv1.C"
   "val == 2"
# 43 "./ext/mv1.C" 3 4
   , "./ext/mv1.C", 43, __PRETTY_FUNCTION__))
# 43 "./ext/mv1.C"
                    ;
  else if (__builtin_cpu_supports ("avx2")
    && __builtin_cpu_supports ("ssse3"))
    
# 46 "./ext/mv1.C" 3 4
   ((
# 46 "./ext/mv1.C"
   val == 3
# 46 "./ext/mv1.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 46 "./ext/mv1.C"
   "val == 3"
# 46 "./ext/mv1.C" 3 4
   , "./ext/mv1.C", 46, __PRETTY_FUNCTION__))
# 46 "./ext/mv1.C"
                    ;
  else if (__builtin_cpu_supports ("avx"))
    
# 48 "./ext/mv1.C" 3 4
   ((
# 48 "./ext/mv1.C"
   val == 4
# 48 "./ext/mv1.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 48 "./ext/mv1.C"
   "val == 4"
# 48 "./ext/mv1.C" 3 4
   , "./ext/mv1.C", 48, __PRETTY_FUNCTION__))
# 48 "./ext/mv1.C"
                    ;
  else if (__builtin_cpu_is ("corei7")
    && __builtin_cpu_supports ("popcnt"))
    
# 51 "./ext/mv1.C" 3 4
   ((
# 51 "./ext/mv1.C"
   val == 5
# 51 "./ext/mv1.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 51 "./ext/mv1.C"
   "val == 5"
# 51 "./ext/mv1.C" 3 4
   , "./ext/mv1.C", 51, __PRETTY_FUNCTION__))
# 51 "./ext/mv1.C"
                    ;
  else if (__builtin_cpu_is ("corei7"))
    
# 53 "./ext/mv1.C" 3 4
   ((
# 53 "./ext/mv1.C"
   val == 6
# 53 "./ext/mv1.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 53 "./ext/mv1.C"
   "val == 6"
# 53 "./ext/mv1.C" 3 4
   , "./ext/mv1.C", 53, __PRETTY_FUNCTION__))
# 53 "./ext/mv1.C"
                    ;
  else if (__builtin_cpu_is ("amdfam10h"))
    
# 55 "./ext/mv1.C" 3 4
   ((
# 55 "./ext/mv1.C"
   val == 7
# 55 "./ext/mv1.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 55 "./ext/mv1.C"
   "val == 7"
# 55 "./ext/mv1.C" 3 4
   , "./ext/mv1.C", 55, __PRETTY_FUNCTION__))
# 55 "./ext/mv1.C"
                    ;
  else if (__builtin_cpu_is ("core2")
    && __builtin_cpu_supports ("sse4.2"))
    
# 58 "./ext/mv1.C" 3 4
   ((
# 58 "./ext/mv1.C"
   val == 8
# 58 "./ext/mv1.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 58 "./ext/mv1.C"
   "val == 8"
# 58 "./ext/mv1.C" 3 4
   , "./ext/mv1.C", 58, __PRETTY_FUNCTION__))
# 58 "./ext/mv1.C"
                    ;
  else if (__builtin_cpu_is ("core2"))
    
# 60 "./ext/mv1.C" 3 4
   ((
# 60 "./ext/mv1.C"
   val == 9
# 60 "./ext/mv1.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 60 "./ext/mv1.C"
   "val == 9"
# 60 "./ext/mv1.C" 3 4
   , "./ext/mv1.C", 60, __PRETTY_FUNCTION__))
# 60 "./ext/mv1.C"
                    ;
  else if (__builtin_cpu_is ("atom"))
    
# 62 "./ext/mv1.C" 3 4
   ((
# 62 "./ext/mv1.C"
   val == 10
# 62 "./ext/mv1.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 62 "./ext/mv1.C"
   "val == 10"
# 62 "./ext/mv1.C" 3 4
   , "./ext/mv1.C", 62, __PRETTY_FUNCTION__))
# 62 "./ext/mv1.C"
                     ;
  else
    
# 64 "./ext/mv1.C" 3 4
   ((
# 64 "./ext/mv1.C"
   val == 0
# 64 "./ext/mv1.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 64 "./ext/mv1.C"
   "val == 0"
# 64 "./ext/mv1.C" 3 4
   , "./ext/mv1.C", 64, __PRETTY_FUNCTION__))
# 64 "./ext/mv1.C"
                    ;

  return 0;
}

int __attribute__ ((target("default")))
foo ()
{
  return 0;
}

int __attribute__ ((target("arch=corei7,popcnt")))
foo ()
{
  return 5;
}
int __attribute__ ((target("avx2,ssse3")))
foo ()
{
  return 3;
}

int __attribute__ ((target("arch=core2")))
foo ()
{
  return 9;
}

int __attribute__ ((target("arch=corei7")))
foo ()
{
  return 6;
}

int __attribute__ ((target("arch=atom")))
foo ()
{
  return 10;
}

int __attribute__ ((target("avx")))
foo ()
{
  return 4;
}

int __attribute__ ((target("arch=core2,sse4.2")))
foo ()
{
  return 8;
}

int __attribute__ ((target("arch=amdfam10")))
foo ()
{
  return 7;
}

int __attribute__ ((target("arch=bdver1")))
foo ()
{
  return 1;
}

int __attribute__ ((target("arch=bdver2")))
foo ()
{
  return 2;
}
