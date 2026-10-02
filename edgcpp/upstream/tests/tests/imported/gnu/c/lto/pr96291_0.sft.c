//type: fp
//options: 
# 0 "./lto/pr96291_0.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/pr96291_0.c"


# 1 "./lto/pr96291.h" 1
void e(void);
void f(void);
void a(void *, void *);
void c(int);
# 4 "./lto/pr96291_0.c" 2

static void * b;
void c(int d) {
  f();
  a(b, b);
}

void e(void) { c(0); }
