//type: fp
//options: 
# 0 "./Wstring-compare.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./Wstring-compare.c"





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
# 7 "./Wstring-compare.c" 2



void sink (int, ...);

struct S { char a4[4], c; };

extern char a4[4];
extern char a5[5];
extern char b4[4];




void strcmp_array_lit (void)
{
  if (strcmp (a4, "1234"))

    sink (0, a4);

  int cmp;
  cmp = strcmp (a4, "1234");
  if (cmp)
    sink (0, a4);

  sink (0 == strcmp (a4, "4321"), a4, "4321");
  sink (0 == strcmp (a4, "12345"), a4, "12345");
  sink (0 == strcmp (a4, "123456"), a4, "123456");
  sink (0 == strcmp ("1234", a4), "1234", a4);
  sink (0 == strcmp ("12345", a4), "12345", a4);
  sink (0 == strcmp ("123456", a4), "123456", a4);
}


void strcmp_array_pstr (void)
{
  const char *s4 = "1234";

  {
    if (strcmp (a4, s4))

      sink (1, a4);
    else
      sink (0, a4);
  }

  {
    int c;
    c = strcmp (a4, s4);
    if (c)
      sink (1, a4);
    else
      sink (0, a4);
  }

  const char *t4 = "4321";
  const char *s5 = "12345";
  const char *s6 = "123456";

  sink (0 == strcmp (a4, t4), a4, t4);
  sink (0 == strcmp (a4, s5), a4, s5);
  sink (0 == strcmp (a4, s6), a4, s6);
  sink (0 == strcmp (s4, a4), s4, a4);
  sink (0 == strcmp (s5, a4), s5, a4);
  sink (0 == strcmp (s6, a4), s6, a4);
}


void strcmp_array_cond_pstr (int i)
{
  const char *s4 = i ? "1234" : "4321";
  sink (0 == strcmp (a4, s4), a4, s4);
  sink (0 == strcmp (a5, s4), a5, s4);
}

void strcmp_array_copy (void)
{
  char s[8];

  {
    strcpy (s, "1234");
    if (strcmp (a4, s))

      sink (1, a4);
    else
      sink (0, a4);
  }

  {
    strcpy (s, "1234");

    int c;
    c = strcmp (a4, s);
    if (c)
      sink (1, a4);
    else
      sink (0, a4);
  }

  strcpy (s, "4321");
  sink (0 == strcmp (a4, s), a4, s);
  strcpy (s, "12345");
  sink (0 == strcmp (a4, s), a4, s);
  strcpy (s, "123456");
  sink (0 == strcmp (a4, s), a4, s);
  strcpy (s, "4321");
  sink (0 == strcmp (s, a4), s, a4);
  strcpy (s, "54321");
  sink (0 == strcmp (s, a4), s, a4);
  strcpy (s, "654321");
  sink (0 == strcmp (s, a4), s, a4);
}


void strcmp_member_array_lit (const struct S *p)
{

  sink (0 == strcmp (p->a4, "1234"), p->a4, "1234");
}





void strncmp_array_lit (void)
{
  if (strncmp (a4, "12345", 5))

    sink (0, a4);

  int cmp;
  cmp = strncmp (a4, "54321", 5);
  if (cmp)
    sink (0, a4);


  sink (0 == strncmp (a4, "4321", 4), a4, "4321");
  sink (0 == strncmp (a4, "654321", 4), a4, "654321");

  sink (0 == strncmp (a4, "12345", 5), a4, "12345");
  sink (0 == strncmp (a4, "123456", 6), a4, "123456");

  sink (0 == strncmp ("1234", a4, 4), "1234", a4);
  sink (0 == strncmp ("12345", a4, 4), "12345", a4);

  sink (0 == strncmp ("12345", a4, 5), "12345", a4);
  sink (0 == strncmp ("123456", a4, 6), "123456", a4);
}


void strncmp_strarray_copy (void)
{
  {
    char a[] = "1234";
    char b[6];
    strcpy (b, "12345");
    if (strncmp (a, b, 5))

      sink (0, a, b);
  }

  {
    char a[] = "4321";
    char b[6];
    strcpy (b, "54321");
    int cmp;
    cmp = strncmp (a, b, 5);
    if (cmp)
      sink (0, a, b);
  }

  strcpy (a4, "abc");
  sink (0 == strncmp (a4, "54321", 5), a4, "54321");
}
