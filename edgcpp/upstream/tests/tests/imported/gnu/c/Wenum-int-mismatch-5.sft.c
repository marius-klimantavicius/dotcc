//type: fp
//options: 
# 0 "./Wenum-int-mismatch-5.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./Wenum-int-mismatch-5.c"




# 1 "./Wenum-int-mismatch-2.c" 1




enum E { E1 = 0, E2, E3 };

unsigned int foo(void);
enum E foo(void) { return E2; }

void bar(unsigned int);
void bar(enum E);

extern enum E arr[10];
extern unsigned int arr[10];

extern unsigned int i;
extern enum E i;

extern unsigned int *p;
extern enum E *p;

enum E foo2(void) { return E2; }
unsigned int foo2(void);

void bar2(enum E);
void bar2(unsigned int);

extern unsigned int arr2[10];
extern enum E arr2[10];

extern enum E i2;
extern unsigned int i2;

extern enum E *p2;
extern unsigned int *p2;

enum F { F1 = 1u, F2, F3 } __attribute__ ((__packed__));

enum F fn1(void);
unsigned char fn1(void);

unsigned char fn2(void);
enum F fn2(void);
# 6 "./Wenum-int-mismatch-5.c" 2
