//type: fp
//options: 
# 0 "./strlenopt-78.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-78.c"







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
# 9 "./strlenopt-78.c" 2

extern void* memchr (const void*, int, size_t);

struct A1 { char n, a[1]; };
struct A2 { char n, a[2]; };
struct A3 { char n, a[3]; };
struct Ax { char n, a[]; };

const struct A1 a1_0 = { 0 };
const struct A1 a1_0_ = { 0, { } };
const struct A1 a1_0_0 = { 0, { 0 } };

const struct A2 a2_1_ = { 1, { } };
const struct A2 a2_1_1 = { 1, { 1 } };
const struct A2 a2_1_1_0 = { 1, { 1, 0 } };

const struct A3 aa3_1_[2] = { { 1 } };

const struct Ax ax = { 3, { 3, 2, 1, 0 } };

struct BxA1 { int n; struct A1 a[]; };
struct BxA2 { int n; struct A2 a[]; };

const struct BxA2 bx = { 2, { { 2, { 2, 1 } }, { 2, { 1, 0 } } } };
# 57 "./strlenopt-78.c"
int schr1, schr1__, schr1_0, schr2_1, schr2, schr2_1_0, schrax, schrbx;

void test_strchr_flexarray (void)
{
  schr1 = 0 != strchr (a1_0.a, '1');
  schr1__ = 0 != strchr (a1_0_.a, '2');
  schr1_0 = 0 != strchr (a1_0_0.a, '3');

  schr2 = 0 != strchr (a2_1_.a, '4');
  schr2_1 = 0 != strchr (a2_1_1.a, '\001');
  schr2_1_0 = 0 != strchr (a2_1_1_0.a, '\001');

  schrax = 0 != strchr (ax.a, '\001');
  schrbx = 0 != strchr (bx.a[1].a, '\0');
# 80 "./strlenopt-78.c"
}


int scmp1, scmp1__, scmp1_0, scmp2_1, scmp2, scmp2_1_0, scmpax, scmpbx;

void test_strcmp_flexarray (void)
{
  scmp1 = 0 == strcmp (a1_0.a, "1");
  scmp1__ = 0 == strcmp (a1_0_.a, "2");
  scmp1_0 = 0 == strcmp (a1_0_0.a, "3");

  scmp2 = 0 == strcmp (a2_1_.a, "4");
  scmp2_1 = 0 == strcmp (a2_1_1.a, "\001");
  scmp2_1_0 = 0 == strcmp (a2_1_1_0.a, "\001");

  scmpax = 0 == strcmp (ax.a, "\003\002\001");
  scmpbx = 0 == strcmp (bx.a[1].a, "\001");
# 106 "./strlenopt-78.c"
}


int len1, len1__, len1_0, len2_1, len2, len2_1_0, lenax, lenbx;

void test_strlen_flexarray (void)
{
  len1 = strlen (a1_0.a);
  len1__ = strlen (a1_0_.a);
  len1_0 = strlen (a1_0_0.a);

  len2 = strlen (a2_1_.a);
  len2_1 = strlen (a2_1_1.a);
  len2_1_0 = strlen (a2_1_1_0.a);

  lenax = strlen (ax.a);
  lenbx = strlen (bx.a[1].a);
# 132 "./strlenopt-78.c"
}


int schraa3, scmpaa3, lenaa3;

void test_trailing_array_empty_init (void)
{
  schraa3 = ((aa3_1_[0].a == strchr (aa3_1_[0].a, 0))
      + (aa3_1_[1].a == strchr (aa3_1_[1].a, 0)));

  scmpaa3 = strcmp (aa3_1_[0].a, aa3_1_[1].a);
  lenaa3 = strlen (aa3_1_[0].a) + strlen (aa3_1_[1].a);




}

union U4 { char a[4]; int i; };
const union U4 u4[2] = { { "123" } };

int ulen0, ulen1;

void test_union_init (void)
{
  ulen0 = strlen (u4[0].a);
  ulen1 = strlen (u4[1].a);



}
