//type: rp
//options: 
//options_all: --gnu_version=80200 -tused -e 200 --no_wrap
# 1 "./ext/mv17.C"
# 1 "<built-in>"
# 1 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 1 "<command-line>" 2
# 1 "./ext/mv17.C"






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
# 8 "./ext/mv17.C" 2



# 10 "./ext/mv17.C"
int foo () __attribute__((target("default")));
int foo () __attribute__((target("bmi")));
int foo () __attribute__((target("bmi2")));


int bar () __attribute__((target("default")));
int bar () __attribute__((target("bmi")));
int bar () __attribute__((target("bmi2")));
int bar () __attribute__((target("arch=btver2")));
int bar () __attribute__((target("arch=haswell")));

int main ()
{
  int val = foo ();

  if (__builtin_cpu_supports ("bmi2"))
    
# 26 "./ext/mv17.C" 3 4
   ((
# 26 "./ext/mv17.C"
   val == 2
# 26 "./ext/mv17.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 26 "./ext/mv17.C"
   "val == 2"
# 26 "./ext/mv17.C" 3 4
   , "./ext/mv17.C", 26, __PRETTY_FUNCTION__))
# 26 "./ext/mv17.C"
                    ;
  else if (__builtin_cpu_supports ("bmi"))
    
# 28 "./ext/mv17.C" 3 4
   ((
# 28 "./ext/mv17.C"
   val == 1
# 28 "./ext/mv17.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 28 "./ext/mv17.C"
   "val == 1"
# 28 "./ext/mv17.C" 3 4
   , "./ext/mv17.C", 28, __PRETTY_FUNCTION__))
# 28 "./ext/mv17.C"
                    ;
  else
    
# 30 "./ext/mv17.C" 3 4
   ((
# 30 "./ext/mv17.C"
   val == 0
# 30 "./ext/mv17.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 30 "./ext/mv17.C"
   "val == 0"
# 30 "./ext/mv17.C" 3 4
   , "./ext/mv17.C", 30, __PRETTY_FUNCTION__))
# 30 "./ext/mv17.C"
                    ;

  val = bar ();

  if (__builtin_cpu_is ("btver2"))
    
# 35 "./ext/mv17.C" 3 4
   ((
# 35 "./ext/mv17.C"
   val == 5
# 35 "./ext/mv17.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 35 "./ext/mv17.C"
   "val == 5"
# 35 "./ext/mv17.C" 3 4
   , "./ext/mv17.C", 35, __PRETTY_FUNCTION__))
# 35 "./ext/mv17.C"
                    ;
  else if (__builtin_cpu_is ("haswell"))
    
# 37 "./ext/mv17.C" 3 4
   ((
# 37 "./ext/mv17.C"
   val == 6
# 37 "./ext/mv17.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 37 "./ext/mv17.C"
   "val == 6"
# 37 "./ext/mv17.C" 3 4
   , "./ext/mv17.C", 37, __PRETTY_FUNCTION__))
# 37 "./ext/mv17.C"
                    ;
  else if (__builtin_cpu_supports ("bmi2"))
    
# 39 "./ext/mv17.C" 3 4
   ((
# 39 "./ext/mv17.C"
   val == 2
# 39 "./ext/mv17.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 39 "./ext/mv17.C"
   "val == 2"
# 39 "./ext/mv17.C" 3 4
   , "./ext/mv17.C", 39, __PRETTY_FUNCTION__))
# 39 "./ext/mv17.C"
                    ;
  else if (__builtin_cpu_supports ("bmi"))
    
# 41 "./ext/mv17.C" 3 4
   ((
# 41 "./ext/mv17.C"
   val == 1
# 41 "./ext/mv17.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 41 "./ext/mv17.C"
   "val == 1"
# 41 "./ext/mv17.C" 3 4
   , "./ext/mv17.C", 41, __PRETTY_FUNCTION__))
# 41 "./ext/mv17.C"
                    ;
  else
    
# 43 "./ext/mv17.C" 3 4
   ((
# 43 "./ext/mv17.C"
   val == 0
# 43 "./ext/mv17.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 43 "./ext/mv17.C"
   "val == 0"
# 43 "./ext/mv17.C" 3 4
   , "./ext/mv17.C", 43, __PRETTY_FUNCTION__))
# 43 "./ext/mv17.C"
                    ;

  return 0;
}

int __attribute__ ((target("default")))
foo ()
{
  return 0;
}

int __attribute__ ((target("bmi")))
foo ()
{
  return 1;
}
int __attribute__ ((target("bmi2")))
foo ()
{
  return 2;
}

int __attribute__ ((target("default")))
bar ()
{
  return 0;
}

int __attribute__ ((target("bmi")))
bar ()
{
  return 1;
}
int __attribute__ ((target("bmi2")))
bar ()
{
  return 2;
}

int __attribute__ ((target("arch=btver2")))
bar ()
{
  return 5;
}

int __attribute__ ((target("arch=haswell")))
bar ()
{
  return 6;
}
