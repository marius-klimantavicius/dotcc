//type: fp
//options: 
# 0 "./lto/pr94822_0.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/pr94822_0.c"


# 1 "./lto/pr94822.h" 1
typedef struct {
  int i;
  int ints[];
} struct_t;
# 4 "./lto/pr94822_0.c" 2

extern struct_t my_struct;

int main() {
 return my_struct.ints[1];
}
