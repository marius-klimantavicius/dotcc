//type: rp
//options: 
//options_all: --gnu_version=80200 -tused -e 200 --no_wrap
# 1 "./ext/mv2.C"
# 1 "<built-in>"
# 1 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 1 "<command-line>" 2
# 1 "./ext/mv2.C"






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
# 8 "./ext/mv2.C" 2



# 10 "./ext/mv2.C"
int foo () __attribute__ ((target ("default")));


int foo () __attribute__ ((target ("mmx")));
int foo () __attribute__ ((target ("sse")));
int foo () __attribute__ ((target ("sse2")));
int foo () __attribute__ ((target ("sse3")));
int foo () __attribute__ ((target ("ssse3")));
int foo () __attribute__ ((target ("sse4.1")));
int foo () __attribute__ ((target ("sse4.2")));
int foo () __attribute__ ((target ("popcnt")));
int foo () __attribute__ ((target ("avx")));
int foo () __attribute__ ((target ("avx2")));
int foo () __attribute__ ((target ("avx512f")));

int main ()
{
  int val = foo ();

  if (__builtin_cpu_supports ("avx512f"))
    
# 30 "./ext/mv2.C" 3 4
   ((
# 30 "./ext/mv2.C"
   val == 11
# 30 "./ext/mv2.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 30 "./ext/mv2.C"
   "val == 11"
# 30 "./ext/mv2.C" 3 4
   , "./ext/mv2.C", 30, __PRETTY_FUNCTION__))
# 30 "./ext/mv2.C"
                     ;
  else if (__builtin_cpu_supports ("avx2"))
    
# 32 "./ext/mv2.C" 3 4
   ((
# 32 "./ext/mv2.C"
   val == 10
# 32 "./ext/mv2.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 32 "./ext/mv2.C"
   "val == 10"
# 32 "./ext/mv2.C" 3 4
   , "./ext/mv2.C", 32, __PRETTY_FUNCTION__))
# 32 "./ext/mv2.C"
                     ;
  else if (__builtin_cpu_supports ("avx"))
    
# 34 "./ext/mv2.C" 3 4
   ((
# 34 "./ext/mv2.C"
   val == 9
# 34 "./ext/mv2.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 34 "./ext/mv2.C"
   "val == 9"
# 34 "./ext/mv2.C" 3 4
   , "./ext/mv2.C", 34, __PRETTY_FUNCTION__))
# 34 "./ext/mv2.C"
                    ;
  else if (__builtin_cpu_supports ("popcnt"))
    
# 36 "./ext/mv2.C" 3 4
   ((
# 36 "./ext/mv2.C"
   val == 8
# 36 "./ext/mv2.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 36 "./ext/mv2.C"
   "val == 8"
# 36 "./ext/mv2.C" 3 4
   , "./ext/mv2.C", 36, __PRETTY_FUNCTION__))
# 36 "./ext/mv2.C"
                    ;
  else if (__builtin_cpu_supports ("sse4.2"))
    
# 38 "./ext/mv2.C" 3 4
   ((
# 38 "./ext/mv2.C"
   val == 7
# 38 "./ext/mv2.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 38 "./ext/mv2.C"
   "val == 7"
# 38 "./ext/mv2.C" 3 4
   , "./ext/mv2.C", 38, __PRETTY_FUNCTION__))
# 38 "./ext/mv2.C"
                    ;
  else if (__builtin_cpu_supports ("sse4.1"))
    
# 40 "./ext/mv2.C" 3 4
   ((
# 40 "./ext/mv2.C"
   val == 6
# 40 "./ext/mv2.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 40 "./ext/mv2.C"
   "val == 6"
# 40 "./ext/mv2.C" 3 4
   , "./ext/mv2.C", 40, __PRETTY_FUNCTION__))
# 40 "./ext/mv2.C"
                    ;
  else if (__builtin_cpu_supports ("ssse3"))
    
# 42 "./ext/mv2.C" 3 4
   ((
# 42 "./ext/mv2.C"
   val == 5
# 42 "./ext/mv2.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 42 "./ext/mv2.C"
   "val == 5"
# 42 "./ext/mv2.C" 3 4
   , "./ext/mv2.C", 42, __PRETTY_FUNCTION__))
# 42 "./ext/mv2.C"
                    ;
  else if (__builtin_cpu_supports ("sse3"))
    
# 44 "./ext/mv2.C" 3 4
   ((
# 44 "./ext/mv2.C"
   val == 4
# 44 "./ext/mv2.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 44 "./ext/mv2.C"
   "val == 4"
# 44 "./ext/mv2.C" 3 4
   , "./ext/mv2.C", 44, __PRETTY_FUNCTION__))
# 44 "./ext/mv2.C"
                    ;
  else if (__builtin_cpu_supports ("sse2"))
    
# 46 "./ext/mv2.C" 3 4
   ((
# 46 "./ext/mv2.C"
   val == 3
# 46 "./ext/mv2.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 46 "./ext/mv2.C"
   "val == 3"
# 46 "./ext/mv2.C" 3 4
   , "./ext/mv2.C", 46, __PRETTY_FUNCTION__))
# 46 "./ext/mv2.C"
                    ;
  else if (__builtin_cpu_supports ("sse"))
    
# 48 "./ext/mv2.C" 3 4
   ((
# 48 "./ext/mv2.C"
   val == 2
# 48 "./ext/mv2.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 48 "./ext/mv2.C"
   "val == 2"
# 48 "./ext/mv2.C" 3 4
   , "./ext/mv2.C", 48, __PRETTY_FUNCTION__))
# 48 "./ext/mv2.C"
                    ;
  else if (__builtin_cpu_supports ("mmx"))
    
# 50 "./ext/mv2.C" 3 4
   ((
# 50 "./ext/mv2.C"
   val == 1
# 50 "./ext/mv2.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 50 "./ext/mv2.C"
   "val == 1"
# 50 "./ext/mv2.C" 3 4
   , "./ext/mv2.C", 50, __PRETTY_FUNCTION__))
# 50 "./ext/mv2.C"
                    ;
  else
    
# 52 "./ext/mv2.C" 3 4
   ((
# 52 "./ext/mv2.C"
   val == 0
# 52 "./ext/mv2.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 52 "./ext/mv2.C"
   "val == 0"
# 52 "./ext/mv2.C" 3 4
   , "./ext/mv2.C", 52, __PRETTY_FUNCTION__))
# 52 "./ext/mv2.C"
                    ;

  return 0;
}

int __attribute__ ((target("default")))
foo ()
{
  return 0;
}

int __attribute__ ((target("mmx")))
foo ()
{
  return 1;
}

int __attribute__ ((target("sse")))
foo ()
{
  return 2;
}

int __attribute__ ((target("sse2")))
foo ()
{
  return 3;
}

int __attribute__ ((target("sse3")))
foo ()
{
  return 4;
}

int __attribute__ ((target("ssse3")))
foo ()
{
  return 5;
}

int __attribute__ ((target("sse4.1")))
foo ()
{
  return 6;
}

int __attribute__ ((target("sse4.2")))
foo ()
{
  return 7;
}

int __attribute__ ((target("popcnt")))
foo ()
{
  return 8;
}

int __attribute__ ((target("avx")))
foo ()
{
  return 9;
}

int __attribute__ ((target("avx2")))
foo ()
{
  return 10;
}

int __attribute__ ((target("avx512f")))
foo ()
{
  return 11;
}
