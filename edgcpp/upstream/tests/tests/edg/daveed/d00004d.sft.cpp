/*
//remark:Variadic macros (C9X style, reserve __VA_ARGS__)
//type:fn
//name:
//options:
//options_all:--c --variadic_macros
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
*/

/* Negative tests: catch misuses of __VA_ARGS__ */

#if defined __VA_ARGS__
#endif

#if __VA_ARGS__
#endif


#define __VA_ARGS__ 1
#define OLM __VA_ARGS__
#define FLM(__VA_ARGS__)

#ifdef __VA_ARGS__
#ifndef __VA_ARGS__
#endif
#endif

#if defined __VA_ARGS__
#if defined(__VA_ARGS__)
#endif
#endif

#if __VA_ARGS__
#endif

#if 0
#elif __VA_ARGS__
#endif

#pragma __VA_ARGS__

#assert __VA_ARGS__

#undef __VA_ARGS__

struct __VA_ARGS__;

/*
$ eccp --variadic_macros t4.c
"t4.c", line 3: error: the identifier __VA_ARGS__ can only appear in the
          replacement lists of variadic macros
  #if defined __VA_ARGS__
              ^

"t4.c", line 6: error: the identifier __VA_ARGS__ can only appear in the
          replacement lists of variadic macros
  #if __VA_ARGS__
      ^

"t4.c", line 10: error: the identifier __VA_ARGS__ can only appear in the
          replacement lists of variadic macros
  #define __VA_ARGS__ 1
          ^

"t4.c", line 12: error: the identifier __VA_ARGS__ can only appear in the
          replacement lists of variadic macros
  #define FLM(__VA_ARGS__)
              ^

"t4.c", line 14: error: the identifier __VA_ARGS__ can only appear in the
          replacement lists of variadic macros
  #ifdef __VA_ARGS__
         ^

"t4.c", line 15: error: the identifier __VA_ARGS__ can only appear in the
          replacement lists of variadic macros
  #ifndef __VA_ARGS__
          ^

"t4.c", line 19: error: the identifier __VA_ARGS__ can only appear in the
          replacement lists of variadic macros
  #if defined __VA_ARGS__
              ^

"t4.c", line 20: error: the identifier __VA_ARGS__ can only appear in the
          replacement lists of variadic macros
  #if defined(__VA_ARGS__)
              ^

"t4.c", line 24: error: the identifier __VA_ARGS__ can only appear in the
          replacement lists of variadic macros
  #if __VA_ARGS__
      ^

"t4.c", line 28: error: the identifier __VA_ARGS__ can only appear in the
          replacement lists of variadic macros
  #elif __VA_ARGS__
        ^

"t4.c", line 31: error: the identifier __VA_ARGS__ can only appear in the
          replacement lists of variadic macros
  #pragma __VA_ARGS__
          ^

"t4.c", line 31: warning: unrecognized #pragma
  #pragma __VA_ARGS__
          ^

"t4.c", line 33: error: the identifier __VA_ARGS__ can only appear in the
          replacement lists of variadic macros
  #assert __VA_ARGS__
          ^

"t4.c", line 35: error: the identifier __VA_ARGS__ can only appear in the
          replacement lists of variadic macros

  #undef __VA_ARGS__
         ^

"t4.c", line 37: error: the identifier __VA_ARGS__ can only appear in the
          replacement lists of variadic macros
  struct __VA_ARGS__;
         ^

14 errors detected in the compilation of "t4.c".
*/

