//type: rp
//options: 
# 0 "./tree-ssa/pr47707.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/pr47707.C"


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
# 4 "./tree-ssa/pr47707.C" 2


# 5 "./tree-ssa/pr47707.C"
struct CH
{
  unsigned char ch : 3;
} ch;

__attribute__((noinline)) void MakeCheckOp (unsigned int *v1, unsigned int *v2)
{
 
# 12 "./tree-ssa/pr47707.C" 3 4
((
# 12 "./tree-ssa/pr47707.C"
*v1 == *v2
# 12 "./tree-ssa/pr47707.C" 3 4
) ? static_cast<void> (0) : __assert_fail (
# 12 "./tree-ssa/pr47707.C"
"*v1 == *v2"
# 12 "./tree-ssa/pr47707.C" 3 4
, "./tree-ssa/pr47707.C", 12, __PRETTY_FUNCTION__))
# 12 "./tree-ssa/pr47707.C"
                   ;

}

int main (void)
{

  int len;

  for (len = 4; len >= 1; len--)
  {
     unsigned v1, v2;
     ch.ch = len;
     v1 = ch.ch;
     v2 = len;
     MakeCheckOp (&v1, &v2);
  }
}
