//type: fp
//options: 
# 0 "./analyzer/20020129-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/20020129-1.c"

# 1 "./analyzer/../../gcc.c-torture/compile/20020129-1.c" 1




typedef struct
{
  long long a[10];
} A;

void bar (A *);

typedef int (*B)(int);

void foo (void)
{
  static A a;
  bar (&a);
  (*(B)&a) (1);
}
# 3 "./analyzer/20020129-1.c" 2
