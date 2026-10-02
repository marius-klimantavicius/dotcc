//type: fp
//options: --c99 --strict_gnu
# 0 "./pr15749-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr15749-1.c"







# 1 "./pr15749-1.h" 1



       
# 5 "./pr15749-1.h" 3


# 6 "./pr15749-1.h" 3
struct flex { int a; int b[]; };
union rf1 { struct flex a; int b; };
union rf2 { int a; struct flex b; };
union rf3 { int a; union rf1 b; };
union rf4 { union rf2 a; int b; };

struct t0 { struct flex a; };
struct t1 { union rf1 a; };
struct t2 { union rf2 a; };
struct t3 { union rf3 a; };
struct t4 { union rf4 a; };

void f0 (struct flex[]);
void f1 (union rf1[]);
void f2 (union rf2[]);
void f3 (union rf3[]);
void f4 (union rf4[]);

struct flex a0[1];
union rf1 a1[1];
union rf2 a2[1];
union rf3 a3[1];
union rf4 a4[1];
# 9 "./pr15749-1.c" 2
