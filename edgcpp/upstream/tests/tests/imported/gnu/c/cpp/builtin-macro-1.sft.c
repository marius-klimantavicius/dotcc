//type: rp
//options: 
# 0 "./cpp/builtin-macro-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp/builtin-macro-1.c"
# 10 "./cpp/builtin-macro-1.c"
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



# 11 "./cpp/builtin-macro-1.c" 2






# 16 "./cpp/builtin-macro-1.c"
int
main()
{
  int L19 = 19; 
# 19 "./cpp/builtin-macro-1.c" 3 4
 ((
# 19 "./cpp/builtin-macro-1.c"
 L19 == 19
# 19 "./cpp/builtin-macro-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 19 "./cpp/builtin-macro-1.c"
 "L19 == 19"
# 19 "./cpp/builtin-macro-1.c" 3 4
 , "./cpp/builtin-macro-1.c", 19, __PRETTY_FUNCTION__))
# 19 "./cpp/builtin-macro-1.c"
 ;
     ;

  
# 22 "./cpp/builtin-macro-1.c" 3 4
 ((
# 22 "./cpp/builtin-macro-1.c"
 L19 == 19
# 22 "./cpp/builtin-macro-1.c" 3 4
 ) ? (void) (0) : __assert_fail (
# 22 "./cpp/builtin-macro-1.c"
 "L19 == 19"
# 22 "./cpp/builtin-macro-1.c" 3 4
 , "./cpp/builtin-macro-1.c", 22, __PRETTY_FUNCTION__))
# 22 "./cpp/builtin-macro-1.c"
                  ;




  return 0;
}
