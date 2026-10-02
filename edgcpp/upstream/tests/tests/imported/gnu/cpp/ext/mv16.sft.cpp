//type: rp
//options: 
//options_all: --gnu_version=80200 -tused -e 200 --no_wrap
# 1 "./ext/mv16.C"
# 1 "<built-in>"
# 1 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 1 "<command-line>" 2
# 1 "./ext/mv16.C"
# 9 "./ext/mv16.C"
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
# 10 "./ext/mv16.C" 2


# 11 "./ext/mv16.C"
int __attribute__ ((target("default")))
foo ()
{
  return 0;
}

int __attribute__ ((target("arch=nehalem")))
foo ()
{
  return 4;
}

int __attribute__ ((target("arch=westmere")))
foo ()
{
  return 5;
}

int __attribute__ ((target("arch=sandybridge")))
foo ()
{
  return 8;
}

int __attribute__ ((target("arch=ivybridge")))
foo ()
{
  return 9;
}

int __attribute__ ((target("arch=haswell")))
foo ()
{
  return 12;
}

int __attribute__ ((target("arch=broadwell"))) foo () {
  return 13;
}

int __attribute__ ((target("arch=skylake"))) foo () {
  return 14;
}

int __attribute__ ((target("arch=skylake-avx512"))) foo () {
  return 15;
}

int __attribute__ ((target("arch=cannonlake"))) foo () {
  return 16;
}

int __attribute__ ((target("arch=icelake-client"))) foo () {
  return 17;
}

int __attribute__ ((target("arch=icelake-server"))) foo () {
  return 18;
}

int main ()
{
  int val = foo ();

  if (__builtin_cpu_is ("nehalem"))
    
# 76 "./ext/mv16.C" 3 4
   ((
# 76 "./ext/mv16.C"
   val == 4
# 76 "./ext/mv16.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 76 "./ext/mv16.C"
   "val == 4"
# 76 "./ext/mv16.C" 3 4
   , "./ext/mv16.C", 76, __PRETTY_FUNCTION__))
# 76 "./ext/mv16.C"
                    ;
  else if (__builtin_cpu_is ("westmere"))
    
# 78 "./ext/mv16.C" 3 4
   ((
# 78 "./ext/mv16.C"
   val == 5
# 78 "./ext/mv16.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 78 "./ext/mv16.C"
   "val == 5"
# 78 "./ext/mv16.C" 3 4
   , "./ext/mv16.C", 78, __PRETTY_FUNCTION__))
# 78 "./ext/mv16.C"
                    ;
  else if (__builtin_cpu_is ("sandybridge"))
    
# 80 "./ext/mv16.C" 3 4
   ((
# 80 "./ext/mv16.C"
   val == 8
# 80 "./ext/mv16.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 80 "./ext/mv16.C"
   "val == 8"
# 80 "./ext/mv16.C" 3 4
   , "./ext/mv16.C", 80, __PRETTY_FUNCTION__))
# 80 "./ext/mv16.C"
                    ;
  else if (__builtin_cpu_is ("ivybridge"))
    
# 82 "./ext/mv16.C" 3 4
   ((
# 82 "./ext/mv16.C"
   val == 9
# 82 "./ext/mv16.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 82 "./ext/mv16.C"
   "val == 9"
# 82 "./ext/mv16.C" 3 4
   , "./ext/mv16.C", 82, __PRETTY_FUNCTION__))
# 82 "./ext/mv16.C"
                    ;
  else if (__builtin_cpu_is ("haswell"))
    
# 84 "./ext/mv16.C" 3 4
   ((
# 84 "./ext/mv16.C"
   val == 12
# 84 "./ext/mv16.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 84 "./ext/mv16.C"
   "val == 12"
# 84 "./ext/mv16.C" 3 4
   , "./ext/mv16.C", 84, __PRETTY_FUNCTION__))
# 84 "./ext/mv16.C"
                     ;
  else if (__builtin_cpu_is ("broadwell"))
    
# 86 "./ext/mv16.C" 3 4
   ((
# 86 "./ext/mv16.C"
   val == 13
# 86 "./ext/mv16.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 86 "./ext/mv16.C"
   "val == 13"
# 86 "./ext/mv16.C" 3 4
   , "./ext/mv16.C", 86, __PRETTY_FUNCTION__))
# 86 "./ext/mv16.C"
                     ;
  else if (__builtin_cpu_is ("skylake"))
    
# 88 "./ext/mv16.C" 3 4
   ((
# 88 "./ext/mv16.C"
   val == 14
# 88 "./ext/mv16.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 88 "./ext/mv16.C"
   "val == 14"
# 88 "./ext/mv16.C" 3 4
   , "./ext/mv16.C", 88, __PRETTY_FUNCTION__))
# 88 "./ext/mv16.C"
                     ;
  else if (__builtin_cpu_is ("skylake-avx512"))
    
# 90 "./ext/mv16.C" 3 4
   ((
# 90 "./ext/mv16.C"
   val == 15
# 90 "./ext/mv16.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 90 "./ext/mv16.C"
   "val == 15"
# 90 "./ext/mv16.C" 3 4
   , "./ext/mv16.C", 90, __PRETTY_FUNCTION__))
# 90 "./ext/mv16.C"
                     ;
  else if (__builtin_cpu_is ("cannonlake"))
    
# 92 "./ext/mv16.C" 3 4
   ((
# 92 "./ext/mv16.C"
   val == 16
# 92 "./ext/mv16.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 92 "./ext/mv16.C"
   "val == 16"
# 92 "./ext/mv16.C" 3 4
   , "./ext/mv16.C", 92, __PRETTY_FUNCTION__))
# 92 "./ext/mv16.C"
                     ;
  else if (__builtin_cpu_is ("icelake-client"))
    
# 94 "./ext/mv16.C" 3 4
   ((
# 94 "./ext/mv16.C"
   val == 17
# 94 "./ext/mv16.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 94 "./ext/mv16.C"
   "val == 17"
# 94 "./ext/mv16.C" 3 4
   , "./ext/mv16.C", 94, __PRETTY_FUNCTION__))
# 94 "./ext/mv16.C"
                     ;
  else if (__builtin_cpu_is ("icelake-server"))
    
# 96 "./ext/mv16.C" 3 4
   ((
# 96 "./ext/mv16.C"
   val == 18
# 96 "./ext/mv16.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 96 "./ext/mv16.C"
   "val == 18"
# 96 "./ext/mv16.C" 3 4
   , "./ext/mv16.C", 96, __PRETTY_FUNCTION__))
# 96 "./ext/mv16.C"
                     ;
  else
    
# 98 "./ext/mv16.C" 3 4
   ((
# 98 "./ext/mv16.C"
   val == 0
# 98 "./ext/mv16.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 98 "./ext/mv16.C"
   "val == 0"
# 98 "./ext/mv16.C" 3 4
   , "./ext/mv16.C", 98, __PRETTY_FUNCTION__))
# 98 "./ext/mv16.C"
                    ;

  return 0;
}
