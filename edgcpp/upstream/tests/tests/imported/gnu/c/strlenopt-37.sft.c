//type: fp
//options: 
# 0 "./strlenopt-37.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-37.c"





# 1 "./strlenopt.h" 1





typedef long unsigned int size_t;
extern void abort (void);
void *calloc (size_t, size_t);
void *malloc (size_t);
void free (void *);
char *strdup (const char *);
size_t strlen (const char *);
size_t strnlen (const char *, size_t);
void *memcpy (void *__restrict, const void *__restrict, size_t);
void *memmove (void *, const void *, size_t);
char *strcpy (char *__restrict, const char *__restrict);
char *strcat (char *__restrict, const char *__restrict);
char *strchr (const char *, int);
int strcmp (const char *, const char *);
int strncmp (const char *, const char *, size_t);
void *memset (void *, int, size_t);
int memcmp (const void *, const void *, size_t);
int strcmp (const char *, const char *);





int sprintf (char * __restrict, const char *__restrict, ...);
int snprintf (char * __restrict, size_t, const char *__restrict, ...);
# 7 "./strlenopt-37.c" 2

extern char ax[];

struct MemArray7 { char a7[7]; };
struct MemArray6 { char a6[6]; };
struct MemArray5 { char a5[5]; };
struct MemArray4 { char a4[4]; };
struct MemArray3 { char a3[3]; };
struct MemArray2 { char a2[2]; };
struct MemArray1 { char a1[1]; };
struct MemArray0 { int n; char a0[0]; };
struct MemArrayX { int n; char ax[]; };

struct MemArrays
{
  struct MemArray7 *ma7;
  struct MemArray6 *ma6;
  struct MemArray5 *ma5;
  struct MemArray4 *ma4;
  struct MemArray3 *ma3;
  struct MemArray2 *ma2;
  struct MemArray1 *ma1;
  struct MemArray0 *ma0;
  struct MemArrayX *max;
};

extern void if_stmt_on_line (int);
extern void else_stmt_on_line (int);




void test_memarray_lt (struct MemArrays *p)
{
  (!!(strlen (p->ma7->a7) < sizeof p->ma7->a7) ? if_stmt_on_line (41) : else_stmt_on_line (41));
  (!!(strlen (p->ma6->a6) < sizeof p->ma6->a6) ? if_stmt_on_line (42) : else_stmt_on_line (42));
  (!!(strlen (p->ma5->a5) < sizeof p->ma5->a5) ? if_stmt_on_line (43) : else_stmt_on_line (43));
  (!!(strlen (p->ma4->a4) < sizeof p->ma4->a4) ? if_stmt_on_line (44) : else_stmt_on_line (44));
  (!!(strlen (p->ma3->a3) < sizeof p->ma3->a3) ? if_stmt_on_line (45) : else_stmt_on_line (45));
  (!!(strlen (p->ma2->a2) < sizeof p->ma2->a2) ? if_stmt_on_line (46) : else_stmt_on_line (46));
  (!!(strlen (p->ma1->a1) < sizeof p->ma1->a1) ? if_stmt_on_line (47) : else_stmt_on_line (47));

  (!!(strlen (p->ma0->a0) < 1) ? if_stmt_on_line (49) : else_stmt_on_line (49));
  (!!(strlen (p->max->ax) < 1) ? if_stmt_on_line (50) : else_stmt_on_line (50));
}

void test_memarray_eq (struct MemArrays *p)
{
  (!!(strlen (p->ma7->a7) == sizeof p->ma7->a7) ? if_stmt_on_line (55) : else_stmt_on_line (55));
  (!!(strlen (p->ma6->a6) == sizeof p->ma6->a6) ? if_stmt_on_line (56) : else_stmt_on_line (56));
  (!!(strlen (p->ma5->a5) == sizeof p->ma5->a5) ? if_stmt_on_line (57) : else_stmt_on_line (57));
  (!!(strlen (p->ma4->a4) == sizeof p->ma4->a4) ? if_stmt_on_line (58) : else_stmt_on_line (58));
  (!!(strlen (p->ma3->a3) == sizeof p->ma3->a3) ? if_stmt_on_line (59) : else_stmt_on_line (59));
  (!!(strlen (p->ma2->a2) == sizeof p->ma2->a2) ? if_stmt_on_line (60) : else_stmt_on_line (60));
  (!!(strlen (p->ma1->a1) == sizeof p->ma1->a1) ? if_stmt_on_line (61) : else_stmt_on_line (61));

  (!!(strlen (p->ma0->a0) == 1) ? if_stmt_on_line (63) : else_stmt_on_line (63));
  (!!(strlen (p->max->ax) == 1) ? if_stmt_on_line (64) : else_stmt_on_line (64));
}

void test_memarray_gt (struct MemArrays *p)
{
  (!!(strlen (p->ma7->a7) > sizeof p->ma7->a7) ? if_stmt_on_line (69) : else_stmt_on_line (69));
  (!!(strlen (p->ma6->a6) > sizeof p->ma6->a6) ? if_stmt_on_line (70) : else_stmt_on_line (70));
  (!!(strlen (p->ma5->a5) > sizeof p->ma5->a5) ? if_stmt_on_line (71) : else_stmt_on_line (71));
  (!!(strlen (p->ma4->a4) > sizeof p->ma4->a4) ? if_stmt_on_line (72) : else_stmt_on_line (72));
  (!!(strlen (p->ma3->a3) > sizeof p->ma3->a3) ? if_stmt_on_line (73) : else_stmt_on_line (73));
  (!!(strlen (p->ma2->a2) > sizeof p->ma2->a2) ? if_stmt_on_line (74) : else_stmt_on_line (74));
  (!!(strlen (p->ma1->a1) > sizeof p->ma1->a1) ? if_stmt_on_line (75) : else_stmt_on_line (75));

  (!!(strlen (p->ma0->a0) > 1) ? if_stmt_on_line (77) : else_stmt_on_line (77));
  (!!(strlen (p->max->ax) > 1) ? if_stmt_on_line (78) : else_stmt_on_line (78));
 }
