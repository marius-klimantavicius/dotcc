//type: rp
//options: 
# 0 "./torture/pr99954.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/pr99954.c"


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



# 4 "./torture/pr99954.c" 2




# 7 "./torture/pr99954.c"
typedef union container { int value; } container;

void move(container* end, container* start) {
    container* p;
    for (p = end; p > start; p--) {
 (p)->value = (p-1)->value;
    }
}



int main(int argc, char* argv[]) {
    container vals[100];
    int i;
    for (i=0; i<100; i++) {
        vals[i].value = argc + i;
    }
    move(&vals[100 -1], &vals[0]);
    
# 25 "./torture/pr99954.c" 3 4
   ((
# 25 "./torture/pr99954.c"
   vals[0].value == argc + 0
# 25 "./torture/pr99954.c" 3 4
   ) ? (void) (0) : __assert_fail (
# 25 "./torture/pr99954.c"
   "vals[0].value == argc + 0"
# 25 "./torture/pr99954.c" 3 4
   , "./torture/pr99954.c", 25, __PRETTY_FUNCTION__))
# 25 "./torture/pr99954.c"
                                    ;
    for (i=1; i<100; i++) {
        
# 27 "./torture/pr99954.c" 3 4
       ((
# 27 "./torture/pr99954.c"
       vals[i].value == argc + i - 1
# 27 "./torture/pr99954.c" 3 4
       ) ? (void) (0) : __assert_fail (
# 27 "./torture/pr99954.c"
       "vals[i].value == argc + i - 1"
# 27 "./torture/pr99954.c" 3 4
       , "./torture/pr99954.c", 27, __PRETTY_FUNCTION__))
# 27 "./torture/pr99954.c"
                                            ;
    }
    return 0;
}
