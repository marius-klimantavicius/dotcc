//type: fp
//options: 
# 0 "./lto/pr81440_1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/pr81440_1.c"
# 1 "./lto/pr81440.h" 1
typedef struct {
  int i;
  int ints[];
} struct_t;
# 2 "./lto/pr81440_1.c" 2

struct_t my_struct = {
 20,
 { 1, 2 }
};
