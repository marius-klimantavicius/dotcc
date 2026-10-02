//type: fp
//options: 
# 0 "./strlenopt-89.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-89.c"




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
# 6 "./strlenopt-89.c" 2
# 29 "./strlenopt-89.c"
__attribute__ ((noipa)) void test_1_store_nul_0 (const char *s) { extern char a1[1]; char *d = a1; size_t len = 1 - 1; size_t idx = 0; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;

__attribute__ ((noipa)) void test_2_store_nul_0 (const char *s) { extern char a2[2]; char *d = a2; size_t len = 2 - 1; size_t idx = 0; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_2_store_nul_1 (const char *s) { extern char a2[2]; char *d = a2; size_t len = 2 - 1; size_t idx = 1; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;


__attribute__ ((noipa)) void test_3_store_nul_0 (const char *s) { extern char a3[3]; char *d = a3; size_t len = 3 - 1; size_t idx = 0; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_3_store_nul_1 (const char *s) { extern char a3[3]; char *d = a3; size_t len = 3 - 1; size_t idx = 1; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_3_store_nul_2 (const char *s) { extern char a3[3]; char *d = a3; size_t len = 3 - 1; size_t idx = 2; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;

__attribute__ ((noipa)) void test_4_store_nul_0 (const char *s) { extern char a4[4]; char *d = a4; size_t len = 4 - 1; size_t idx = 0; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_4_store_nul_1 (const char *s) { extern char a4[4]; char *d = a4; size_t len = 4 - 1; size_t idx = 1; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_4_store_nul_2 (const char *s) { extern char a4[4]; char *d = a4; size_t len = 4 - 1; size_t idx = 2; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_4_store_nul_3 (const char *s) { extern char a4[4]; char *d = a4; size_t len = 4 - 1; size_t idx = 3; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;

__attribute__ ((noipa)) void test_5_store_nul_0 (const char *s) { extern char a5[5]; char *d = a5; size_t len = 5 - 1; size_t idx = 0; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_5_store_nul_1 (const char *s) { extern char a5[5]; char *d = a5; size_t len = 5 - 1; size_t idx = 1; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_5_store_nul_2 (const char *s) { extern char a5[5]; char *d = a5; size_t len = 5 - 1; size_t idx = 2; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_5_store_nul_3 (const char *s) { extern char a5[5]; char *d = a5; size_t len = 5 - 1; size_t idx = 3; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_5_store_nul_4 (const char *s) { extern char a5[5]; char *d = a5; size_t len = 5 - 1; size_t idx = 4; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;

__attribute__ ((noipa)) void test_6_store_nul_0 (const char *s) { extern char a6[6]; char *d = a6; size_t len = 6 - 1; size_t idx = 0; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_6_store_nul_1 (const char *s) { extern char a6[6]; char *d = a6; size_t len = 6 - 1; size_t idx = 1; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_6_store_nul_2 (const char *s) { extern char a6[6]; char *d = a6; size_t len = 6 - 1; size_t idx = 2; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_6_store_nul_3 (const char *s) { extern char a6[6]; char *d = a6; size_t len = 6 - 1; size_t idx = 3; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_6_store_nul_4 (const char *s) { extern char a6[6]; char *d = a6; size_t len = 6 - 1; size_t idx = 4; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_6_store_nul_5 (const char *s) { extern char a6[6]; char *d = a6; size_t len = 6 - 1; size_t idx = 5; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;

__attribute__ ((noipa)) void test_7_store_nul_0 (const char *s) { extern char a7[7]; char *d = a7; size_t len = 7 - 1; size_t idx = 0; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_7_store_nul_1 (const char *s) { extern char a7[7]; char *d = a7; size_t len = 7 - 1; size_t idx = 1; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_7_store_nul_2 (const char *s) { extern char a7[7]; char *d = a7; size_t len = 7 - 1; size_t idx = 2; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_7_store_nul_3 (const char *s) { extern char a7[7]; char *d = a7; size_t len = 7 - 1; size_t idx = 3; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_7_store_nul_4 (const char *s) { extern char a7[7]; char *d = a7; size_t len = 7 - 1; size_t idx = 4; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_7_store_nul_5 (const char *s) { extern char a7[7]; char *d = a7; size_t len = 7 - 1; size_t idx = 5; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_7_store_nul_6 (const char *s) { extern char a7[7]; char *d = a7; size_t len = 7 - 1; size_t idx = 6; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;

__attribute__ ((noipa)) void test_8_store_nul_0 (const char *s) { extern char a8[8]; char *d = a8; size_t len = 8 - 1; size_t idx = 0; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_8_store_nul_1 (const char *s) { extern char a8[8]; char *d = a8; size_t len = 8 - 1; size_t idx = 1; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_8_store_nul_2 (const char *s) { extern char a8[8]; char *d = a8; size_t len = 8 - 1; size_t idx = 2; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_8_store_nul_3 (const char *s) { extern char a8[8]; char *d = a8; size_t len = 8 - 1; size_t idx = 3; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_8_store_nul_4 (const char *s) { extern char a8[8]; char *d = a8; size_t len = 8 - 1; size_t idx = 4; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_8_store_nul_5 (const char *s) { extern char a8[8]; char *d = a8; size_t len = 8 - 1; size_t idx = 5; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_8_store_nul_6 (const char *s) { extern char a8[8]; char *d = a8; size_t len = 8 - 1; size_t idx = 6; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_8_store_nul_7 (const char *s) { extern char a8[8]; char *d = a8; size_t len = 8 - 1; size_t idx = 7; if (strlen (s) == len) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
