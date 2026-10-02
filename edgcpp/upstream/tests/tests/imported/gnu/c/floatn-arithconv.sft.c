//type: fp
//options: 
# 0 "./floatn-arithconv.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./floatn-arithconv.c"
# 13 "./floatn-arithconv.c"
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/float.h" 1 3 4
# 14 "./floatn-arithconv.c" 2

int i;
# 29 "./floatn-arithconv.c"
void
f (void)
{
  do { typedef __typeof__ ((float) 0 + (double) 1) restype; typedef __typeof__ (i ? (float) 0 : (double) 1) restype2; typedef double exptype; extern restype v1; extern restype2 v1; extern exptype v1; } while (0);

  do { typedef __typeof__ ((double) 0 + (_Float32) 1) restype; typedef __typeof__ (i ? (double) 0 : (_Float32) 1) restype2; typedef double exptype; extern restype v2; extern restype2 v2; extern exptype v2; } while (0);


  do { typedef __typeof__ ((double) 0 + (_Float64) 1) restype; typedef __typeof__ (i ? (double) 0 : (_Float64) 1) restype2; typedef _Float64 exptype; extern restype v3; extern restype2 v3; extern exptype v3; } while (0);


  do { typedef __typeof__ ((double) 0 + (_Float32x) 1) restype; typedef __typeof__ (i ? (double) 0 : (_Float32x) 1) restype2; typedef double exptype; extern restype v4; extern restype2 v4; extern exptype v4; } while (0);


  do { typedef __typeof__ ((float) 0 + (_Float32) 1) restype; typedef __typeof__ (i ? (float) 0 : (_Float32) 1) restype2; typedef _Float32 exptype; extern restype v5; extern restype2 v5; extern exptype v5; } while (0);


  do { typedef __typeof__ ((_Float32x) 0 + (_Float64) 1) restype; typedef __typeof__ (i ? (_Float32x) 0 : (_Float64) 1) restype2; typedef _Float64 exptype; extern restype v6; extern restype2 v6; extern exptype v6; } while (0);

  do { typedef __typeof__ ((_Float32) 0 + (_Float64) 1) restype; typedef __typeof__ (i ? (_Float32) 0 : (_Float64) 1) restype2; typedef _Float64 exptype; extern restype v7; extern restype2 v7; extern exptype v7; } while (0);
  do { typedef __typeof__ ((_Float32) 0 + (_Float32x) 1) restype; typedef __typeof__ (i ? (_Float32) 0 : (_Float32x) 1) restype2; typedef _Float32x exptype; extern restype v8; extern restype2 v8; extern exptype v8; } while (0);
}
