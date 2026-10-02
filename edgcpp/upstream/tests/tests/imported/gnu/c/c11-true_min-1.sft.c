//type: rp
//options: --c11
# 0 "./c11-true_min-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c11-true_min-1.c"
# 10 "./c11-true_min-1.c"
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/float.h" 1 3 4
# 11 "./c11-true_min-1.c" 2

int main(){
  volatile float f = 1.40129846432481707092372958328991613e-45F
# 13 "./c11-true_min-1.c"
                                ;
  volatile double d = ((double)4.94065645841246544176568792868221372e-324L)
# 14 "./c11-true_min-1.c"
                                 ;
  volatile long double l = 3.64519953188247460252840593361941982e-4951L
# 15 "./c11-true_min-1.c"
                                       ;
  if (f == 0 || d == 0 || l == 0)
    __builtin_abort ();
  return 0;
}
