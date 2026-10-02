//type: rp
//options: 
# 0 "./strlenopt-74.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-74.c"





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
# 7 "./strlenopt-74.c" 2







extern int last_line;
int nfails;

char buf[32];
# 35 "./strlenopt-74.c"
const char a8[12] = "01234567";
const char b8[12] = "76543210";
const char c4[12] = "0123";

int i0, i1 = 1, i2 = 2;

int last_line = 41;
# 1000 "./strlenopt-74.c"
__attribute__ ((noclone, noinline, noipa)) void test_1000(void) { memcpy (buf, (i0 ? (a8 + 0) : (b8 + 0)), (9)); const size_t len = strlen (buf); if (len != 8) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1000 - 1000 + last_line + 2, "i0 ? (a8 + 0) : (b8 + 0)", (size_t)8, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1001(void) { memcpy (buf, (i0 ? (a8 + 0) : (b8 + 1)), (8)); const size_t len = strlen (buf); if (len != 7) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1001 - 1000 + last_line + 2, "i0 ? (a8 + 0) : (b8 + 1)", (size_t)7, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1002(void) { memcpy (buf, (i0 ? (a8 + 0) : (b8 + 2)), (8)); const size_t len = strlen (buf); if (len != 6) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1002 - 1000 + last_line + 2, "i0 ? (a8 + 0) : (b8 + 2)", (size_t)6, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1003(void) { memcpy (buf, (i0 ? (a8 + 0) : (b8 + 2)), (7)); const size_t len = strlen (buf); if (len != 6) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1003 - 1000 + last_line + 2, "i0 ? (a8 + 0) : (b8 + 2)", (size_t)6, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1004(void) { memcpy (buf, (i0 ? (a8 + 0) : (b8 + 3)), (8)); const size_t len = strlen (buf); if (len != 5) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1004 - 1000 + last_line + 2, "i0 ? (a8 + 0) : (b8 + 3)", (size_t)5, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1005(void) { memcpy (buf, (i0 ? (a8 + 0) : (b8 + 3)), (7)); const size_t len = strlen (buf); if (len != 5) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1005 - 1000 + last_line + 2, "i0 ? (a8 + 0) : (b8 + 3)", (size_t)5, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1006(void) { memcpy (buf, (i0 ? (a8 + 0) : (b8 + 3)), (6)); const size_t len = strlen (buf); if (len != 5) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1006 - 1000 + last_line + 2, "i0 ? (a8 + 0) : (b8 + 3)", (size_t)5, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1007(void) { memcpy (buf, (i0 ? (a8 + 0) : (b8 + 4)), (8)); const size_t len = strlen (buf); if (len != 4) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1007 - 1000 + last_line + 2, "i0 ? (a8 + 0) : (b8 + 4)", (size_t)4, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1008(void) { memcpy (buf, (i0 ? (a8 + 0) : (b8 + 4)), (7)); const size_t len = strlen (buf); if (len != 4) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1008 - 1000 + last_line + 2, "i0 ? (a8 + 0) : (b8 + 4)", (size_t)4, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1009(void) { memcpy (buf, (i0 ? (a8 + 0) : (b8 + 4)), (6)); const size_t len = strlen (buf); if (len != 4) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1009 - 1000 + last_line + 2, "i0 ? (a8 + 0) : (b8 + 4)", (size_t)4, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1010(void) { memcpy (buf, (i0 ? (a8 + 0) : (b8 + 4)), (5)); const size_t len = strlen (buf); if (len != 4) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1010 - 1000 + last_line + 2, "i0 ? (a8 + 0) : (b8 + 4)", (size_t)4, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1011(void) { memcpy (buf, (i0 ? (a8 + 0) : (b8 + 5)), (7)); const size_t len = strlen (buf); if (len != 3) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1011 - 1000 + last_line + 2, "i0 ? (a8 + 0) : (b8 + 5)", (size_t)3, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1012(void) { memcpy (buf, (i0 ? (a8 + 0) : (b8 + 5)), (6)); const size_t len = strlen (buf); if (len != 3) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1012 - 1000 + last_line + 2, "i0 ? (a8 + 0) : (b8 + 5)", (size_t)3, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1013(void) { memcpy (buf, (i0 ? (a8 + 0) : (b8 + 5)), (5)); const size_t len = strlen (buf); if (len != 3) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1013 - 1000 + last_line + 2, "i0 ? (a8 + 0) : (b8 + 5)", (size_t)3, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1014(void) { memcpy (buf, (i0 ? (a8 + 0) : (b8 + 5)), (4)); const size_t len = strlen (buf); if (len != 3) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1014 - 1000 + last_line + 2, "i0 ? (a8 + 0) : (b8 + 5)", (size_t)3, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1015(void) { memcpy (buf, (i0 ? (a8 + 0) : (b8 + 6)), (3)); const size_t len = strlen (buf); if (len != 2) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1015 - 1000 + last_line + 2, "i0 ? (a8 + 0) : (b8 + 6)", (size_t)2, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1016(void) { memcpy (buf, (i0 ? (a8 + 0) : (b8 + 7)), (2)); const size_t len = strlen (buf); if (len != 1) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1016 - 1000 + last_line + 2, "i0 ? (a8 + 0) : (b8 + 7)", (size_t)1, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1017(void) { memcpy (buf, (i0 ? (a8 + 1) : (b8 + 0)), (8)); const size_t len = strlen (buf); if (len != 8) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1017 - 1000 + last_line + 2, "i0 ? (a8 + 1) : (b8 + 0)", (size_t)8, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1018(void) { memcpy (buf, (i0 ? (a8 + 2) : (b8 + 0)), (7)); const size_t len = strlen (buf); if (len != 7) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1018 - 1000 + last_line + 2, "i0 ? (a8 + 2) : (b8 + 0)", (size_t)7, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1019(void) { memcpy (buf, (i0 ? (a8 + 1) : (b8 + 1)), (8)); const size_t len = strlen (buf); if (len != 7) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1019 - 1000 + last_line + 2, "i0 ? (a8 + 1) : (b8 + 1)", (size_t)7, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1020(void) { memcpy (buf, (i0 ? (a8 + 1) : (b8 + 2)), (7)); const size_t len = strlen (buf); if (len != 6) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1020 - 1000 + last_line + 2, "i0 ? (a8 + 1) : (b8 + 2)", (size_t)6, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1021(void) { memcpy (buf, (i0 ? (a8 + 2) : (b8 + 1)), (8)); const size_t len = strlen (buf); if (len != 7) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1021 - 1000 + last_line + 2, "i0 ? (a8 + 2) : (b8 + 1)", (size_t)7, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1022(void) { memcpy (buf, (i0 ? (a8 + 2) : (b8 + 2)), (7)); const size_t len = strlen (buf); if (len != 6) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1022 - 1000 + last_line + 2, "i0 ? (a8 + 2) : (b8 + 2)", (size_t)6, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1023(void) { memcpy (buf, (i0 ? (a8 + 0) : (b8 + 0)), (9)); const size_t len = strlen (buf); if (len != 8) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1023 - 1000 + last_line + 2, "i0 ? (a8 + 0) : (b8 + 0)", (size_t)8, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1024(void) { memcpy (buf, (i0 ? (a8 + 0) : (b8 + 1)), (8)); const size_t len = strlen (buf); if (len != 7) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1024 - 1000 + last_line + 2, "i0 ? (a8 + 0) : (b8 + 1)", (size_t)7, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1025(void) { memcpy (buf, (i0 ? (a8 + 0) : (b8 + 2)), (7)); const size_t len = strlen (buf); if (len != 6) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1025 - 1000 + last_line + 2, "i0 ? (a8 + 0) : (b8 + 2)", (size_t)6, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1026(void) { memcpy (buf, (i0 ? (a8 + 1) : (b8 + 0)), (9)); const size_t len = strlen (buf); if (len != 8) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1026 - 1000 + last_line + 2, "i0 ? (a8 + 1) : (b8 + 0)", (size_t)8, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1027(void) { memcpy (buf, (i0 ? (a8 + 2) : (b8 + 0)), (9)); const size_t len = strlen (buf); if (len != 8) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1027 - 1000 + last_line + 2, "i0 ? (a8 + 2) : (b8 + 0)", (size_t)8, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1028(void) { memcpy (buf, (i0 ? (a8 + 1) : (b8 + 1)), (8)); const size_t len = strlen (buf); if (len != 7) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1028 - 1000 + last_line + 2, "i0 ? (a8 + 1) : (b8 + 1)", (size_t)7, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1029(void) { memcpy (buf, (i0 ? (a8 + 1) : (b8 + 2)), (7)); const size_t len = strlen (buf); if (len != 6) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1029 - 1000 + last_line + 2, "i0 ? (a8 + 1) : (b8 + 2)", (size_t)6, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1030(void) { memcpy (buf, (i0 ? (a8 + 2) : (b8 + 1)), (8)); const size_t len = strlen (buf); if (len != 7) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1030 - 1000 + last_line + 2, "i0 ? (a8 + 2) : (b8 + 1)", (size_t)7, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1031(void) { memcpy (buf, (i0 ? (a8 + 2) : (b8 + 2)), (7)); const size_t len = strlen (buf); if (len != 6) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1031 - 1000 + last_line + 2, "i0 ? (a8 + 2) : (b8 + 2)", (size_t)6, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1032(void) { memcpy (buf, (i0 ? (a8 + 0) : (c4 + 0)), (9)); const size_t len = strlen (buf); if (len != 4) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1032 - 1000 + last_line + 2, "i0 ? (a8 + 0) : (c4 + 0)", (size_t)4, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1033(void) { memcpy (buf, (i0 ? (a8 + 0) : (c4 + 1)), (9)); const size_t len = strlen (buf); if (len != 3) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1033 - 1000 + last_line + 2, "i0 ? (a8 + 0) : (c4 + 1)", (size_t)3, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1034(void) { memcpy (buf, (i0 ? (a8 + 0) : (c4 + 3)), (9)); const size_t len = strlen (buf); if (len != 1) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1034 - 1000 + last_line + 2, "i0 ? (a8 + 0) : (c4 + 3)", (size_t)1, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1035(void) { memcpy (buf, (i0 ? (a8 + 0) : (c4 + 4)), (8)); const size_t len = strlen (buf); if (len != 0) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1035 - 1000 + last_line + 2, "i0 ? (a8 + 0) : (c4 + 4)", (size_t)0, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1036(void) { memcpy (buf, (i0 ? (a8 + 1) : (c4 + 0)), (8)); const size_t len = strlen (buf); if (len != 4) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1036 - 1000 + last_line + 2, "i0 ? (a8 + 1) : (c4 + 0)", (size_t)4, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1037(void) { memcpy (buf, (i0 ? (a8 + 1) : (c4 + 1)), (8)); const size_t len = strlen (buf); if (len != 3) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1037 - 1000 + last_line + 2, "i0 ? (a8 + 1) : (c4 + 1)", (size_t)3, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1038(void) { memcpy (buf, (i0 ? (a8 + 1) : (c4 + 2)), (8)); const size_t len = strlen (buf); if (len != 2) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1038 - 1000 + last_line + 2, "i0 ? (a8 + 1) : (c4 + 2)", (size_t)2, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1039(void) { memcpy (buf, (i0 ? (a8 + 1) : (c4 + 3)), (8)); const size_t len = strlen (buf); if (len != 1) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1039 - 1000 + last_line + 2, "i0 ? (a8 + 1) : (c4 + 3)", (size_t)1, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1040(void) { memcpy (buf, (i0 ? (a8 + 1) : (c4 + 4)), (8)); const size_t len = strlen (buf); if (len != 0) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1040 - 1000 + last_line + 2, "i0 ? (a8 + 1) : (c4 + 4)", (size_t)0, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1041(void) { memcpy (buf, (i0 ? (a8 + 2) : (c4 + 0)), (8)); const size_t len = strlen (buf); if (len != 4) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1041 - 1000 + last_line + 2, "i0 ? (a8 + 2) : (c4 + 0)", (size_t)4, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1042(void) { memcpy (buf, (i0 ? (a8 + 2) : (c4 + 1)), (8)); const size_t len = strlen (buf); if (len != 3) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1042 - 1000 + last_line + 2, "i0 ? (a8 + 2) : (c4 + 1)", (size_t)3, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1043(void) { memcpy (buf, (i0 ? (a8 + 2) : (c4 + 2)), (8)); const size_t len = strlen (buf); if (len != 2) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1043 - 1000 + last_line + 2, "i0 ? (a8 + 2) : (c4 + 2)", (size_t)2, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1044(void) { memcpy (buf, (i0 ? (a8 + 2) : (c4 + 3)), (8)); const size_t len = strlen (buf); if (len != 1) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1044 - 1000 + last_line + 2, "i0 ? (a8 + 2) : (c4 + 3)", (size_t)1, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1045(void) { memcpy (buf, (i0 ? (a8 + 2) : (c4 + 4)), (8)); const size_t len = strlen (buf); if (len != 0) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1045 - 1000 + last_line + 2, "i0 ? (a8 + 2) : (c4 + 4)", (size_t)0, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1046(void) { memcpy (buf, ((i0 ? a8 : b8) + 1), (8)); const size_t len = strlen (buf); if (len != 7) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1046 - 1000 + last_line + 2, "(i0 ? a8 : b8) + 1", (size_t)7, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1047(void) { memcpy (buf, ((i0 ? a8 : b8) + 2), (8)); const size_t len = strlen (buf); if (len != 6) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1047 - 1000 + last_line + 2, "(i0 ? a8 : b8) + 2", (size_t)6, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1048(void) { memcpy (buf, ((i0 ? a8 : b8) + 2), (7)); const size_t len = strlen (buf); if (len != 6) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1048 - 1000 + last_line + 2, "(i0 ? a8 : b8) + 2", (size_t)6, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1049(void) { memcpy (buf, ((i0 ? a8 : b8) + 3), (3)); const size_t len = strlen (buf); if (len != 3) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1049 - 1000 + last_line + 2, "(i0 ? a8 : b8) + 3", (size_t)3, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1050(void) { memcpy (buf, ((i0 ? a8 : b8) + 3), (1)); const size_t len = strlen (buf); if (len != 1) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1050 - 1000 + last_line + 2, "(i0 ? a8 : b8) + 3", (size_t)1, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1051(void) { memcpy (buf, ((i0 ? a8 : c4) + 1), (8)); const size_t len = strlen (buf); if (len != 3) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1051 - 1000 + last_line + 2, "(i0 ? a8 : c4) + 1", (size_t)3, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1052(void) { memcpy (buf, ((i0 ? a8 : c4) + 3), (8)); const size_t len = strlen (buf); if (len != 1) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1052 - 1000 + last_line + 2, "(i0 ? a8 : c4) + 3", (size_t)1, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1053(void) { memcpy (buf, ((i0 ? a8 : c4) + 4), (8)); const size_t len = strlen (buf); if (len != 0) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1053 - 1000 + last_line + 2, "(i0 ? a8 : c4) + 4", (size_t)0, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1054(void) { memcpy (buf, ((i0 ? a8 + 1: b8 + 2) + 1), (9)); const size_t len = strlen (buf); if (len != 5) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1054 - 1000 + last_line + 2, "(i0 ? a8 + 1: b8 + 2) + 1", (size_t)5, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1055(void) { memcpy (buf, ((i0 ? a8 + i1: b8 + i2) + 1), (8)); const size_t len = strlen (buf); if (len != 5) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1055 - 1000 + last_line + 2, "(i0 ? a8 + i1: b8 + i2) + 1", (size_t)5, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1056(void) { memcpy (buf, ((i0 ? a8 + i1: b8 + 2) + 1), (8)); const size_t len = strlen (buf); if (len != 5) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1056 - 1000 + last_line + 2, "(i0 ? a8 + i1: b8 + 2) + 1", (size_t)5, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1057(void) { memcpy (buf, ((i0 ? a8 + i2: b8 + i1) + 1), (8)); const size_t len = strlen (buf); if (len != 6) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1057 - 1000 + last_line + 2, "(i0 ? a8 + i2: b8 + i1) + 1", (size_t)6, len); } } typedef void DummyType;
__attribute__ ((noclone, noinline, noipa)) void test_1058(void) { memcpy (buf, ((i0 ? a8 + 2: b8 + i1) + 1), (8)); const size_t len = strlen (buf); if (len != 6) { ++nfails; __builtin_printf ("line %i: strlen(%s) == %zu failed: " "got %zu\n", 1058 - 1000 + last_line + 2, "(i0 ? a8 + 2: b8 + i1) + 1", (size_t)6, len); } } typedef void DummyType;



int main (void)
{
  test_1000 (); memset (buf, 0, sizeof buf);
  test_1001 (); memset (buf, 0, sizeof buf);
  test_1002 (); memset (buf, 0, sizeof buf);
  test_1003 (); memset (buf, 0, sizeof buf);
  test_1004 (); memset (buf, 0, sizeof buf);
  test_1005 (); memset (buf, 0, sizeof buf);
  test_1006 (); memset (buf, 0, sizeof buf);
  test_1007 (); memset (buf, 0, sizeof buf);
  test_1008 (); memset (buf, 0, sizeof buf);
  test_1009 (); memset (buf, 0, sizeof buf);

  test_1010 (); memset (buf, 0, sizeof buf);
  test_1011 (); memset (buf, 0, sizeof buf);
  test_1012 (); memset (buf, 0, sizeof buf);
  test_1013 (); memset (buf, 0, sizeof buf);
  test_1014 (); memset (buf, 0, sizeof buf);
  test_1015 (); memset (buf, 0, sizeof buf);
  test_1016 (); memset (buf, 0, sizeof buf);
  test_1017 (); memset (buf, 0, sizeof buf);
  test_1018 (); memset (buf, 0, sizeof buf);
  test_1019 (); memset (buf, 0, sizeof buf);

  test_1020 (); memset (buf, 0, sizeof buf);
  test_1021 (); memset (buf, 0, sizeof buf);
  test_1022 (); memset (buf, 0, sizeof buf);
  test_1023 (); memset (buf, 0, sizeof buf);
  test_1024 (); memset (buf, 0, sizeof buf);
  test_1025 (); memset (buf, 0, sizeof buf);
  test_1026 (); memset (buf, 0, sizeof buf);
  test_1027 (); memset (buf, 0, sizeof buf);
  test_1028 (); memset (buf, 0, sizeof buf);
  test_1029 (); memset (buf, 0, sizeof buf);

  test_1030 (); memset (buf, 0, sizeof buf);
  test_1031 (); memset (buf, 0, sizeof buf);
  test_1032 (); memset (buf, 0, sizeof buf);
  test_1033 (); memset (buf, 0, sizeof buf);
  test_1034 (); memset (buf, 0, sizeof buf);
  test_1035 (); memset (buf, 0, sizeof buf);
  test_1036 (); memset (buf, 0, sizeof buf);
  test_1037 (); memset (buf, 0, sizeof buf);
  test_1038 (); memset (buf, 0, sizeof buf);
  test_1039 (); memset (buf, 0, sizeof buf);

  test_1040 (); memset (buf, 0, sizeof buf);
  test_1041 (); memset (buf, 0, sizeof buf);
  test_1042 (); memset (buf, 0, sizeof buf);
  test_1043 (); memset (buf, 0, sizeof buf);
  test_1044 (); memset (buf, 0, sizeof buf);
  test_1045 (); memset (buf, 0, sizeof buf);
  test_1046 (); memset (buf, 0, sizeof buf);
  test_1047 (); memset (buf, 0, sizeof buf);
  test_1048 (); memset (buf, 0, sizeof buf);
  test_1049 (); memset (buf, 0, sizeof buf);

  test_1050 (); memset (buf, 0, sizeof buf);
  test_1051 (); memset (buf, 0, sizeof buf);
  test_1052 (); memset (buf, 0, sizeof buf);
  test_1053 (); memset (buf, 0, sizeof buf);
  test_1054 (); memset (buf, 0, sizeof buf);
  test_1055 (); memset (buf, 0, sizeof buf);
  test_1056 (); memset (buf, 0, sizeof buf);
  test_1057 (); memset (buf, 0, sizeof buf);
  test_1058 (); memset (buf, 0, sizeof buf);

  if (nfails)
    abort ();
}
