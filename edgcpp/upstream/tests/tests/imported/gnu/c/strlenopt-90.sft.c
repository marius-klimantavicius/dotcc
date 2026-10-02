//type: fp
//options: 
# 0 "./strlenopt-90.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-90.c"




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
# 6 "./strlenopt-90.c" 2
# 30 "./strlenopt-90.c"
__attribute__ ((noipa)) void test_0_2_4_0 (const char *s) { extern char a4[4]; char *d = a4; size_t len = strlen (s); size_t idx = 0; if (0 <= len && len <= 2) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
# 42 "./strlenopt-90.c"
__attribute__ ((noipa)) void test_2_3_4_0 (const char *s) { extern char a4[4]; char *d = a4; size_t len = strlen (s); size_t idx = 0; if (2 <= len && len <= 3) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_2_3_4_1 (const char *s) { extern char a4[4]; char *d = a4; size_t len = strlen (s); size_t idx = 1; if (2 <= len && len <= 3) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;





__attribute__ ((noipa)) void test_3_4_5_0 (const char *s) { extern char a5[5]; char *d = a5; size_t len = strlen (s); size_t idx = 0; if (3 <= len && len <= 4) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_3_4_5_1 (const char *s) { extern char a5[5]; char *d = a5; size_t len = strlen (s); size_t idx = 1; if (3 <= len && len <= 4) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_3_4_5_2 (const char *s) { extern char a5[5]; char *d = a5; size_t len = strlen (s); size_t idx = 2; if (3 <= len && len <= 4) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;





__attribute__ ((noipa)) void test_3_4_6_0 (const char *s) { extern char a6[6]; char *d = a6; size_t len = strlen (s); size_t idx = 0; if (3 <= len && len <= 4) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_3_4_6_1 (const char *s) { extern char a6[6]; char *d = a6; size_t len = strlen (s); size_t idx = 1; if (3 <= len && len <= 4) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_3_4_6_2 (const char *s) { extern char a6[6]; char *d = a6; size_t len = strlen (s); size_t idx = 2; if (3 <= len && len <= 4) { strcpy (d, s); d[idx] = 0; if (strlen (d) != idx) abort (); } } typedef void dummy_type;
