//type: fp
//options: 
# 0 "./Wenum-int-mismatch-4.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./Wenum-int-mismatch-4.c"




# 1 "./Wenum-int-mismatch-1.c" 1




enum E { E1 = -1, E2 = 0, E3 = 1 };

int foo(void);
enum E foo(void) { return E2; }

void bar(int);
void bar(enum E);

extern int arr[10];
extern enum E arr[10];

extern int i;
extern enum E i;

extern int *p;
extern enum E *p;

enum E foo2(void) { return E2; }
int foo2(void);

void bar2(enum E);
void bar2(int);

extern enum E arr2[10];
extern int arr2[10];

extern enum E i2;
extern int i2;

extern enum E *p2;
extern int *p2;

enum F { F1 = -1, F2, F3 } __attribute__ ((__packed__));

enum F fn1(void);
signed char fn1(void);

signed char fn2(void);
enum F fn2(void);
# 6 "./Wenum-int-mismatch-4.c" 2
