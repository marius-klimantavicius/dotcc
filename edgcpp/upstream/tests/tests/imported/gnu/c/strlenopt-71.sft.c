//type: rp
//options: 
# 0 "./strlenopt-71.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-71.c"






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
# 8 "./strlenopt-71.c" 2



typedef short unsigned int uint16_t;
typedef unsigned int uint32_t;




__attribute__ ((noclone, noinline, noipa)) void terminate (void)
{
  __builtin_abort ();
}
# 62 "./strlenopt-71.c"
char a[32];

__attribute__ ((noclone, noinline, noipa)) void
i16_1 (void)
{
  *(uint16_t*)a = (((uint16_t)(("12" "\0\0\0\0")[1]) << 8) + (uint16_t)(("12" "\0\0\0\0")[0]));
  *(uint16_t*)(a + 2) = (((uint16_t)(("3" "\0\0\0\0")[1]) << 8) + (uint16_t)(("3" "\0\0\0\0")[0]));
  do { const unsigned expect = strlen ("123"); const unsigned len = strlen (a); if (len != expect) { __builtin_printf ("line %i: strlen(%s) == %u failed: " "got %u with a = \"%.*s\"\n", 69, "a", expect, len, (int)sizeof a, a); terminate (); } if (memcmp (a, "123", expect + 1)) { __builtin_printf ("line %i: expected string \"%s\", " "got a = \"%.*s\"\n", 69, "123", (int)sizeof a, a); terminate (); } } while (0);

  *(uint16_t*)(a + 1) = (((uint16_t)(("23" "\0\0\0\0")[1]) << 8) + (uint16_t)(("23" "\0\0\0\0")[0]));
  do { const unsigned expect = strlen ("123"); const unsigned len = strlen (a); if (len != expect) { __builtin_printf ("line %i: strlen(%s) == %u failed: " "got %u with a = \"%.*s\"\n", 72, "a", expect, len, (int)sizeof a, a); terminate (); } if (memcmp (a, "123", expect + 1)) { __builtin_printf ("line %i: expected string \"%s\", " "got a = \"%.*s\"\n", 72, "123", (int)sizeof a, a); terminate (); } } while (0);

  *(uint16_t*)(a) = (((uint16_t)(("12" "\0\0\0\0")[1]) << 8) + (uint16_t)(("12" "\0\0\0\0")[0]));
  do { const unsigned expect = strlen ("123"); const unsigned len = strlen (a); if (len != expect) { __builtin_printf ("line %i: strlen(%s) == %u failed: " "got %u with a = \"%.*s\"\n", 75, "a", expect, len, (int)sizeof a, a); terminate (); } if (memcmp (a, "123", expect + 1)) { __builtin_printf ("line %i: expected string \"%s\", " "got a = \"%.*s\"\n", 75, "123", (int)sizeof a, a); terminate (); } } while (0);

  *(uint16_t*)(a + 1) = (((uint16_t)(("2" "\0\0\0\0")[1]) << 8) + (uint16_t)(("2" "\0\0\0\0")[0]));
  do { const unsigned expect = strlen ("12"); const unsigned len = strlen (a); if (len != expect) { __builtin_printf ("line %i: strlen(%s) == %u failed: " "got %u with a = \"%.*s\"\n", 78, "a", expect, len, (int)sizeof a, a); terminate (); } if (memcmp (a, "12", expect + 1)) { __builtin_printf ("line %i: expected string \"%s\", " "got a = \"%.*s\"\n", 78, "12", (int)sizeof a, a); terminate (); } } while (0);

  *(uint16_t*)(a + 3) = (((uint16_t)(("45" "\0\0\0\0")[1]) << 8) + (uint16_t)(("45" "\0\0\0\0")[0]));
  *(uint16_t*)(a + 2) = (((uint16_t)(("34" "\0\0\0\0")[1]) << 8) + (uint16_t)(("34" "\0\0\0\0")[0]));
  do { const unsigned expect = strlen ("12345"); const unsigned len = strlen (a); if (len != expect) { __builtin_printf ("line %i: strlen(%s) == %u failed: " "got %u with a = \"%.*s\"\n", 82, "a", expect, len, (int)sizeof a, a); terminate (); } if (memcmp (a, "12345", expect + 1)) { __builtin_printf ("line %i: expected string \"%s\", " "got a = \"%.*s\"\n", 82, "12345", (int)sizeof a, a); terminate (); } } while (0);
}

__attribute__ ((noclone, noinline, noipa)) void
i16_2 (void)
{
  strcpy (a, "12");
  strcat (a, "34");

  *(uint16_t*)a = (((uint16_t)(("12" "\0\0\0\0")[1]) << 8) + (uint16_t)(("12" "\0\0\0\0")[0]));
  do { const unsigned expect = strlen ("1234"); const unsigned len = strlen (a); if (len != expect) { __builtin_printf ("line %i: strlen(%s) == %u failed: " "got %u with a = \"%.*s\"\n", 92, "a", expect, len, (int)sizeof a, a); terminate (); } if (memcmp (a, "1234", expect + 1)) { __builtin_printf ("line %i: expected string \"%s\", " "got a = \"%.*s\"\n", 92, "1234", (int)sizeof a, a); terminate (); } } while (0);

  *(uint16_t*)(a + 1) = (((uint16_t)(("12" "\0\0\0\0")[1]) << 8) + (uint16_t)(("12" "\0\0\0\0")[0]));
  do { const unsigned expect = strlen ("1124"); const unsigned len = strlen (a); if (len != expect) { __builtin_printf ("line %i: strlen(%s) == %u failed: " "got %u with a = \"%.*s\"\n", 95, "a", expect, len, (int)sizeof a, a); terminate (); } if (memcmp (a, "1124", expect + 1)) { __builtin_printf ("line %i: expected string \"%s\", " "got a = \"%.*s\"\n", 95, "1124", (int)sizeof a, a); terminate (); } } while (0);

  *(uint16_t*)(a + 2) = (((uint16_t)(("12" "\0\0\0\0")[1]) << 8) + (uint16_t)(("12" "\0\0\0\0")[0]));
  do { const unsigned expect = strlen ("1112"); const unsigned len = strlen (a); if (len != expect) { __builtin_printf ("line %i: strlen(%s) == %u failed: " "got %u with a = \"%.*s\"\n", 98, "a", expect, len, (int)sizeof a, a); terminate (); } if (memcmp (a, "1112", expect + 1)) { __builtin_printf ("line %i: expected string \"%s\", " "got a = \"%.*s\"\n", 98, "1112", (int)sizeof a, a); terminate (); } } while (0);

  *(uint16_t*)(a + 3) = (((uint16_t)(("12" "\0\0\0\0")[1]) << 8) + (uint16_t)(("12" "\0\0\0\0")[0]));
  do { const unsigned expect = strlen ("11112"); const unsigned len = strlen (a); if (len != expect) { __builtin_printf ("line %i: strlen(%s) == %u failed: " "got %u with a = \"%.*s\"\n", 101, "a", expect, len, (int)sizeof a, a); terminate (); } if (memcmp (a, "11112", expect + 1)) { __builtin_printf ("line %i: expected string \"%s\", " "got a = \"%.*s\"\n", 101, "11112", (int)sizeof a, a); terminate (); } } while (0);

  *(uint16_t*)(a + 4) = (((uint16_t)(("12" "\0\0\0\0")[1]) << 8) + (uint16_t)(("12" "\0\0\0\0")[0]));
  do { const unsigned expect = strlen ("111112"); const unsigned len = strlen (a); if (len != expect) { __builtin_printf ("line %i: strlen(%s) == %u failed: " "got %u with a = \"%.*s\"\n", 104, "a", expect, len, (int)sizeof a, a); terminate (); } if (memcmp (a, "111112", expect + 1)) { __builtin_printf ("line %i: expected string \"%s\", " "got a = \"%.*s\"\n", 104, "111112", (int)sizeof a, a); terminate (); } } while (0);
}


__attribute__ ((noclone, noinline, noipa)) void
i32_1 (void)
{
  *(uint32_t*)a = (((uint32_t)(("1234" "\0\0\0\0")[3]) << 24) + ((uint32_t)(("1234" "\0\0\0\0")[2]) << 16) + ((uint32_t)(("1234" "\0\0\0\0")[1]) << 8) + (uint32_t)(("1234" "\0\0\0\0")[0]));
  do { const unsigned expect = strlen ("1234"); const unsigned len = strlen (a); if (len != expect) { __builtin_printf ("line %i: strlen(%s) == %u failed: " "got %u with a = \"%.*s\"\n", 112, "a", expect, len, (int)sizeof a, a); terminate (); } if (memcmp (a, "1234", expect + 1)) { __builtin_printf ("line %i: expected string \"%s\", " "got a = \"%.*s\"\n", 112, "1234", (int)sizeof a, a); terminate (); } } while (0);

  *(uint32_t*)(a + 1) = (((uint32_t)(("2345" "\0\0\0\0")[3]) << 24) + ((uint32_t)(("2345" "\0\0\0\0")[2]) << 16) + ((uint32_t)(("2345" "\0\0\0\0")[1]) << 8) + (uint32_t)(("2345" "\0\0\0\0")[0]));
  do { const unsigned expect = strlen ("12345"); const unsigned len = strlen (a); if (len != expect) { __builtin_printf ("line %i: strlen(%s) == %u failed: " "got %u with a = \"%.*s\"\n", 115, "a", expect, len, (int)sizeof a, a); terminate (); } if (memcmp (a, "12345", expect + 1)) { __builtin_printf ("line %i: expected string \"%s\", " "got a = \"%.*s\"\n", 115, "12345", (int)sizeof a, a); terminate (); } } while (0);
}

__attribute__ ((noclone, noinline, noipa)) void
i32_2 (void)
{
  strcpy (a, "12");
  strcat (a, "34");

  *(uint32_t*)a = (((uint32_t)(("1234" "\0\0\0\0")[3]) << 24) + ((uint32_t)(("1234" "\0\0\0\0")[2]) << 16) + ((uint32_t)(("1234" "\0\0\0\0")[1]) << 8) + (uint32_t)(("1234" "\0\0\0\0")[0]));
  do { const unsigned expect = strlen ("1234"); const unsigned len = strlen (a); if (len != expect) { __builtin_printf ("line %i: strlen(%s) == %u failed: " "got %u with a = \"%.*s\"\n", 125, "a", expect, len, (int)sizeof a, a); terminate (); } if (memcmp (a, "1234", expect + 1)) { __builtin_printf ("line %i: expected string \"%s\", " "got a = \"%.*s\"\n", 125, "1234", (int)sizeof a, a); terminate (); } } while (0);

  *(uint32_t*)(a + 4) = (((uint32_t)(("567" "\0\0\0\0")[3]) << 24) + ((uint32_t)(("567" "\0\0\0\0")[2]) << 16) + ((uint32_t)(("567" "\0\0\0\0")[1]) << 8) + (uint32_t)(("567" "\0\0\0\0")[0]));
  do { const unsigned expect = strlen ("1234567"); const unsigned len = strlen (a); if (len != expect) { __builtin_printf ("line %i: strlen(%s) == %u failed: " "got %u with a = \"%.*s\"\n", 128, "a", expect, len, (int)sizeof a, a); terminate (); } if (memcmp (a, "1234567", expect + 1)) { __builtin_printf ("line %i: expected string \"%s\", " "got a = \"%.*s\"\n", 128, "1234567", (int)sizeof a, a); terminate (); } } while (0);

  *(uint32_t*)(a + 7) = (((uint32_t)(("89\0" "\0\0\0\0")[3]) << 24) + ((uint32_t)(("89\0" "\0\0\0\0")[2]) << 16) + ((uint32_t)(("89\0" "\0\0\0\0")[1]) << 8) + (uint32_t)(("89\0" "\0\0\0\0")[0]));
  do { const unsigned expect = strlen ("123456789"); const unsigned len = strlen (a); if (len != expect) { __builtin_printf ("line %i: strlen(%s) == %u failed: " "got %u with a = \"%.*s\"\n", 131, "a", expect, len, (int)sizeof a, a); terminate (); } if (memcmp (a, "123456789", expect + 1)) { __builtin_printf ("line %i: expected string \"%s\", " "got a = \"%.*s\"\n", 131, "123456789", (int)sizeof a, a); terminate (); } } while (0);

  *(uint32_t*)(a + 3) = (((uint32_t)(("4567" "\0\0\0\0")[3]) << 24) + ((uint32_t)(("4567" "\0\0\0\0")[2]) << 16) + ((uint32_t)(("4567" "\0\0\0\0")[1]) << 8) + (uint32_t)(("4567" "\0\0\0\0")[0]));
  do { const unsigned expect = strlen ("123456789"); const unsigned len = strlen (a); if (len != expect) { __builtin_printf ("line %i: strlen(%s) == %u failed: " "got %u with a = \"%.*s\"\n", 134, "a", expect, len, (int)sizeof a, a); terminate (); } if (memcmp (a, "123456789", expect + 1)) { __builtin_printf ("line %i: expected string \"%s\", " "got a = \"%.*s\"\n", 134, "123456789", (int)sizeof a, a); terminate (); } } while (0);

  *(uint32_t*)(a + 2) = (((uint32_t)(("3456" "\0\0\0\0")[3]) << 24) + ((uint32_t)(("3456" "\0\0\0\0")[2]) << 16) + ((uint32_t)(("3456" "\0\0\0\0")[1]) << 8) + (uint32_t)(("3456" "\0\0\0\0")[0]));
  do { const unsigned expect = strlen ("123456789"); const unsigned len = strlen (a); if (len != expect) { __builtin_printf ("line %i: strlen(%s) == %u failed: " "got %u with a = \"%.*s\"\n", 137, "a", expect, len, (int)sizeof a, a); terminate (); } if (memcmp (a, "123456789", expect + 1)) { __builtin_printf ("line %i: expected string \"%s\", " "got a = \"%.*s\"\n", 137, "123456789", (int)sizeof a, a); terminate (); } } while (0);

  *(uint32_t*)(a + 1) = (((uint32_t)(("2345" "\0\0\0\0")[3]) << 24) + ((uint32_t)(("2345" "\0\0\0\0")[2]) << 16) + ((uint32_t)(("2345" "\0\0\0\0")[1]) << 8) + (uint32_t)(("2345" "\0\0\0\0")[0]));
  do { const unsigned expect = strlen ("123456789"); const unsigned len = strlen (a); if (len != expect) { __builtin_printf ("line %i: strlen(%s) == %u failed: " "got %u with a = \"%.*s\"\n", 140, "a", expect, len, (int)sizeof a, a); terminate (); } if (memcmp (a, "123456789", expect + 1)) { __builtin_printf ("line %i: expected string \"%s\", " "got a = \"%.*s\"\n", 140, "123456789", (int)sizeof a, a); terminate (); } } while (0);
}


__attribute__ ((noclone, noinline, noipa)) void
i32_3 (void)
{
  strcpy (a, "1234");
  strcat (a, "5678");

  *(uint32_t*)a = (((uint32_t)(("1234" "\0\0\0\0")[3]) << 24) + ((uint32_t)(("1234" "\0\0\0\0")[2]) << 16) + ((uint32_t)(("1234" "\0\0\0\0")[1]) << 8) + (uint32_t)(("1234" "\0\0\0\0")[0]));
  do { const unsigned expect = strlen ("12345678"); const unsigned len = strlen (a); if (len != expect) { __builtin_printf ("line %i: strlen(%s) == %u failed: " "got %u with a = \"%.*s\"\n", 151, "a", expect, len, (int)sizeof a, a); terminate (); } if (memcmp (a, "12345678", expect + 1)) { __builtin_printf ("line %i: expected string \"%s\", " "got a = \"%.*s\"\n", 151, "12345678", (int)sizeof a, a); terminate (); } } while (0);

  *(uint32_t*)(a + 1) = (((uint32_t)(("234" "\0\0\0\0")[3]) << 24) + ((uint32_t)(("234" "\0\0\0\0")[2]) << 16) + ((uint32_t)(("234" "\0\0\0\0")[1]) << 8) + (uint32_t)(("234" "\0\0\0\0")[0]));
  do { const unsigned expect = strlen ("1234"); const unsigned len = strlen (a); if (len != expect) { __builtin_printf ("line %i: strlen(%s) == %u failed: " "got %u with a = \"%.*s\"\n", 154, "a", expect, len, (int)sizeof a, a); terminate (); } if (memcmp (a, "1234", expect + 1)) { __builtin_printf ("line %i: expected string \"%s\", " "got a = \"%.*s\"\n", 154, "1234", (int)sizeof a, a); terminate (); } } while (0);

  *(uint32_t*)(a + 2) = (((uint32_t)(("3456" "\0\0\0\0")[3]) << 24) + ((uint32_t)(("3456" "\0\0\0\0")[2]) << 16) + ((uint32_t)(("3456" "\0\0\0\0")[1]) << 8) + (uint32_t)(("3456" "\0\0\0\0")[0]));
  do { const unsigned expect = strlen ("12345678"); const unsigned len = strlen (a); if (len != expect) { __builtin_printf ("line %i: strlen(%s) == %u failed: " "got %u with a = \"%.*s\"\n", 157, "a", expect, len, (int)sizeof a, a); terminate (); } if (memcmp (a, "12345678", expect + 1)) { __builtin_printf ("line %i: expected string \"%s\", " "got a = \"%.*s\"\n", 157, "12345678", (int)sizeof a, a); terminate (); } } while (0);

  *(uint32_t*)(a + 3) = (((uint32_t)(("4567" "\0\0\0\0")[3]) << 24) + ((uint32_t)(("4567" "\0\0\0\0")[2]) << 16) + ((uint32_t)(("4567" "\0\0\0\0")[1]) << 8) + (uint32_t)(("4567" "\0\0\0\0")[0]));
  do { const unsigned expect = strlen ("12345678"); const unsigned len = strlen (a); if (len != expect) { __builtin_printf ("line %i: strlen(%s) == %u failed: " "got %u with a = \"%.*s\"\n", 160, "a", expect, len, (int)sizeof a, a); terminate (); } if (memcmp (a, "12345678", expect + 1)) { __builtin_printf ("line %i: expected string \"%s\", " "got a = \"%.*s\"\n", 160, "12345678", (int)sizeof a, a); terminate (); } } while (0);

  *(uint32_t*)(a + 4) = (((uint32_t)(("5678" "\0\0\0\0")[3]) << 24) + ((uint32_t)(("5678" "\0\0\0\0")[2]) << 16) + ((uint32_t)(("5678" "\0\0\0\0")[1]) << 8) + (uint32_t)(("5678" "\0\0\0\0")[0]));
  do { const unsigned expect = strlen ("12345678"); const unsigned len = strlen (a); if (len != expect) { __builtin_printf ("line %i: strlen(%s) == %u failed: " "got %u with a = \"%.*s\"\n", 163, "a", expect, len, (int)sizeof a, a); terminate (); } if (memcmp (a, "12345678", expect + 1)) { __builtin_printf ("line %i: expected string \"%s\", " "got a = \"%.*s\"\n", 163, "12345678", (int)sizeof a, a); terminate (); } } while (0);

  *(uint32_t*)(a + 5) = (((uint32_t)(("6789" "\0\0\0\0")[3]) << 24) + ((uint32_t)(("6789" "\0\0\0\0")[2]) << 16) + ((uint32_t)(("6789" "\0\0\0\0")[1]) << 8) + (uint32_t)(("6789" "\0\0\0\0")[0]));
  do { const unsigned expect = strlen ("123456789"); const unsigned len = strlen (a); if (len != expect) { __builtin_printf ("line %i: strlen(%s) == %u failed: " "got %u with a = \"%.*s\"\n", 166, "a", expect, len, (int)sizeof a, a); terminate (); } if (memcmp (a, "123456789", expect + 1)) { __builtin_printf ("line %i: expected string \"%s\", " "got a = \"%.*s\"\n", 166, "123456789", (int)sizeof a, a); terminate (); } } while (0);

  *(uint32_t*)(a + 6) = (((uint32_t)(("789A" "\0\0\0\0")[3]) << 24) + ((uint32_t)(("789A" "\0\0\0\0")[2]) << 16) + ((uint32_t)(("789A" "\0\0\0\0")[1]) << 8) + (uint32_t)(("789A" "\0\0\0\0")[0]));
  do { const unsigned expect = strlen ("123456789A"); const unsigned len = strlen (a); if (len != expect) { __builtin_printf ("line %i: strlen(%s) == %u failed: " "got %u with a = \"%.*s\"\n", 169, "a", expect, len, (int)sizeof a, a); terminate (); } if (memcmp (a, "123456789A", expect + 1)) { __builtin_printf ("line %i: expected string \"%s\", " "got a = \"%.*s\"\n", 169, "123456789A", (int)sizeof a, a); terminate (); } } while (0);
}

volatile int vzero = 0;

__attribute__ ((noclone, noinline, noipa)) void
i32_4 (void)
{
  strcpy (a, "1234");
  strcat (a, "5678");

  *(uint32_t*)a = vzero ? (((uint32_t)(("1\0\0\0" "\0\0\0\0")[3]) << 24) + ((uint32_t)(("1\0\0\0" "\0\0\0\0")[2]) << 16) + ((uint32_t)(("1\0\0\0" "\0\0\0\0")[1]) << 8) + (uint32_t)(("1\0\0\0" "\0\0\0\0")[0])) : (((uint32_t)(("1234" "\0\0\0\0")[3]) << 24) + ((uint32_t)(("1234" "\0\0\0\0")[2]) << 16) + ((uint32_t)(("1234" "\0\0\0\0")[1]) << 8) + (uint32_t)(("1234" "\0\0\0\0")[0]));
  do { const unsigned expect = strlen ("12345678"); const unsigned len = strlen (a); if (len != expect) { __builtin_printf ("line %i: strlen(%s) == %u failed: " "got %u with a = \"%.*s\"\n", 181, "a", expect, len, (int)sizeof a, a); terminate (); } if (memcmp (a, "12345678", expect + 1)) { __builtin_printf ("line %i: expected string \"%s\", " "got a = \"%.*s\"\n", 181, "12345678", (int)sizeof a, a); terminate (); } } while (0);

  *(uint32_t*)a = vzero ? (((uint32_t)(("12\0\0" "\0\0\0\0")[3]) << 24) + ((uint32_t)(("12\0\0" "\0\0\0\0")[2]) << 16) + ((uint32_t)(("12\0\0" "\0\0\0\0")[1]) << 8) + (uint32_t)(("12\0\0" "\0\0\0\0")[0])) : (((uint32_t)(("1234" "\0\0\0\0")[3]) << 24) + ((uint32_t)(("1234" "\0\0\0\0")[2]) << 16) + ((uint32_t)(("1234" "\0\0\0\0")[1]) << 8) + (uint32_t)(("1234" "\0\0\0\0")[0]));
  do { const unsigned expect = strlen ("12345678"); const unsigned len = strlen (a); if (len != expect) { __builtin_printf ("line %i: strlen(%s) == %u failed: " "got %u with a = \"%.*s\"\n", 184, "a", expect, len, (int)sizeof a, a); terminate (); } if (memcmp (a, "12345678", expect + 1)) { __builtin_printf ("line %i: expected string \"%s\", " "got a = \"%.*s\"\n", 184, "12345678", (int)sizeof a, a); terminate (); } } while (0);

  *(uint32_t*)a = vzero ? (((uint32_t)(("123\0" "\0\0\0\0")[3]) << 24) + ((uint32_t)(("123\0" "\0\0\0\0")[2]) << 16) + ((uint32_t)(("123\0" "\0\0\0\0")[1]) << 8) + (uint32_t)(("123\0" "\0\0\0\0")[0])) : (((uint32_t)(("1234" "\0\0\0\0")[3]) << 24) + ((uint32_t)(("1234" "\0\0\0\0")[2]) << 16) + ((uint32_t)(("1234" "\0\0\0\0")[1]) << 8) + (uint32_t)(("1234" "\0\0\0\0")[0]));
  do { const unsigned expect = strlen ("12345678"); const unsigned len = strlen (a); if (len != expect) { __builtin_printf ("line %i: strlen(%s) == %u failed: " "got %u with a = \"%.*s\"\n", 187, "a", expect, len, (int)sizeof a, a); terminate (); } if (memcmp (a, "12345678", expect + 1)) { __builtin_printf ("line %i: expected string \"%s\", " "got a = \"%.*s\"\n", 187, "12345678", (int)sizeof a, a); terminate (); } } while (0);

  *(uint32_t*)a = vzero ? (((uint32_t)(("1234" "\0\0\0\0")[3]) << 24) + ((uint32_t)(("1234" "\0\0\0\0")[2]) << 16) + ((uint32_t)(("1234" "\0\0\0\0")[1]) << 8) + (uint32_t)(("1234" "\0\0\0\0")[0])) : (((uint32_t)(("1234" "\0\0\0\0")[3]) << 24) + ((uint32_t)(("1234" "\0\0\0\0")[2]) << 16) + ((uint32_t)(("1234" "\0\0\0\0")[1]) << 8) + (uint32_t)(("1234" "\0\0\0\0")[0]));
  do { const unsigned expect = strlen ("12345678"); const unsigned len = strlen (a); if (len != expect) { __builtin_printf ("line %i: strlen(%s) == %u failed: " "got %u with a = \"%.*s\"\n", 190, "a", expect, len, (int)sizeof a, a); terminate (); } if (memcmp (a, "12345678", expect + 1)) { __builtin_printf ("line %i: expected string \"%s\", " "got a = \"%.*s\"\n", 190, "12345678", (int)sizeof a, a); terminate (); } } while (0);

  *(uint32_t*)a = vzero ? (((uint32_t)(("1235" "\0\0\0\0")[3]) << 24) + ((uint32_t)(("1235" "\0\0\0\0")[2]) << 16) + ((uint32_t)(("1235" "\0\0\0\0")[1]) << 8) + (uint32_t)(("1235" "\0\0\0\0")[0])) : (((uint32_t)(("1234" "\0\0\0\0")[3]) << 24) + ((uint32_t)(("1234" "\0\0\0\0")[2]) << 16) + ((uint32_t)(("1234" "\0\0\0\0")[1]) << 8) + (uint32_t)(("1234" "\0\0\0\0")[0]));
  do { const unsigned expect = strlen ("12345678"); const unsigned len = strlen (a); if (len != expect) { __builtin_printf ("line %i: strlen(%s) == %u failed: " "got %u with a = \"%.*s\"\n", 193, "a", expect, len, (int)sizeof a, a); terminate (); } if (memcmp (a, "12345678", expect + 1)) { __builtin_printf ("line %i: expected string \"%s\", " "got a = \"%.*s\"\n", 193, "12345678", (int)sizeof a, a); terminate (); } } while (0);

  *(uint32_t*)a = vzero ? (((uint32_t)(("1234" "\0\0\0\0")[3]) << 24) + ((uint32_t)(("1234" "\0\0\0\0")[2]) << 16) + ((uint32_t)(("1234" "\0\0\0\0")[1]) << 8) + (uint32_t)(("1234" "\0\0\0\0")[0])) : (((uint32_t)(("123\0" "\0\0\0\0")[3]) << 24) + ((uint32_t)(("123\0" "\0\0\0\0")[2]) << 16) + ((uint32_t)(("123\0" "\0\0\0\0")[1]) << 8) + (uint32_t)(("123\0" "\0\0\0\0")[0]));
  do { const unsigned expect = strlen ("123"); const unsigned len = strlen (a); if (len != expect) { __builtin_printf ("line %i: strlen(%s) == %u failed: " "got %u with a = \"%.*s\"\n", 196, "a", expect, len, (int)sizeof a, a); terminate (); } if (memcmp (a, "123", expect + 1)) { __builtin_printf ("line %i: expected string \"%s\", " "got a = \"%.*s\"\n", 196, "123", (int)sizeof a, a); terminate (); } } while (0);

  *(uint32_t*)(a + 3) = vzero ? (((uint32_t)(("456\0" "\0\0\0\0")[3]) << 24) + ((uint32_t)(("456\0" "\0\0\0\0")[2]) << 16) + ((uint32_t)(("456\0" "\0\0\0\0")[1]) << 8) + (uint32_t)(("456\0" "\0\0\0\0")[0])) : (((uint32_t)(("4567" "\0\0\0\0")[3]) << 24) + ((uint32_t)(("4567" "\0\0\0\0")[2]) << 16) + ((uint32_t)(("4567" "\0\0\0\0")[1]) << 8) + (uint32_t)(("4567" "\0\0\0\0")[0]));
  do { const unsigned expect = strlen ("12345678"); const unsigned len = strlen (a); if (len != expect) { __builtin_printf ("line %i: strlen(%s) == %u failed: " "got %u with a = \"%.*s\"\n", 199, "a", expect, len, (int)sizeof a, a); terminate (); } if (memcmp (a, "12345678", expect + 1)) { __builtin_printf ("line %i: expected string \"%s\", " "got a = \"%.*s\"\n", 199, "12345678", (int)sizeof a, a); terminate (); } } while (0);
}


int main ()
{
  memset (a, 0, sizeof a);
  i16_1 ();

  memset (a, 0, sizeof a);
  i16_2 ();


  memset (a, 0, sizeof a);
  i32_1 ();

  memset (a, 0, sizeof a);
  i32_2 ();

  memset (a, 0, sizeof a);
  i32_3 ();

  memset (a, 0, sizeof a);
  i32_4 ();
}
