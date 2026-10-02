//type: fp
//options: 
# 0 "./tree-ssa/vrp120.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/vrp120.c"


# 1 "./tree-ssa/vrp113.c" 1



int f(int a) {
    switch (a & 1) {
      case 0:
      case 1: return 3;
      case 2: return 5;
      case 3: return 7;
      case 4: return 11;
      case 5: return 13;
      case 6: return 17;
      case 7: return 19;
    }
}
# 4 "./tree-ssa/vrp120.c" 2
