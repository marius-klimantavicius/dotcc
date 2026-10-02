//type: fp
//options: 
# 0 "./compat/abi/pr38736_y.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/abi/pr38736_y.C"





# 1 "./compat/abi/pr38736_x.C" 1



struct alignment_test_struct
{
  char space[4] __attribute__((__aligned__));
};

extern int aligned_y_avx (void);

int
aligned_y_avx (void)
{
  return __alignof__(struct alignment_test_struct);
}
# 7 "./compat/abi/pr38736_y.C" 2
