//type: fp
//options: 
# 0 "./lto/pr83954_0.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/pr83954_0.c"

# 1 "./lto/pr83954.h" 1
struct foo;
extern struct foo *FOO_PTR_ARR[1];
# 3 "./lto/pr83954_0.c" 2

int main() {

  FOO_PTR_ARR[1] = 0;
  return 0;
}
