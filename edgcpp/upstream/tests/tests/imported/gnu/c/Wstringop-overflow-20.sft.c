//type: fp
//options: 
# 0 "./Wstringop-overflow-20.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./Wstringop-overflow-20.c"






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
# 8 "./Wstringop-overflow-20.c" 2
# 27 "./Wstringop-overflow-20.c"
__attribute__ ((noipa)) void test_2_2_1_0 (const char *s) { size_t len = strlen (s); if (2 <= len && len <= 2) { extern char d[]; strcpy (d, s); d[0] = 0; extern char a1[1]; strcpy (a1, d); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_2_2_1_1 (const char *s) { size_t len = strlen (s); if (2 <= len && len <= 2) { extern char d[]; strcpy (d, s); d[1] = 0; extern char a1[1]; strcpy (a1, d); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_2_2_1_2 (const char *s) { size_t len = strlen (s); if (2 <= len && len <= 2) { extern char d[]; strcpy (d, s); d[2] = 0; extern char a1[1]; strcpy (a1, d); } } typedef void dummy_type;

__attribute__ ((noipa)) void test_2_3_1_0 (const char *s) { size_t len = strlen (s); if (2 <= len && len <= 3) { extern char d[]; strcpy (d, s); d[0] = 0; extern char a1[1]; strcpy (a1, d); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_2_3_1_1 (const char *s) { size_t len = strlen (s); if (2 <= len && len <= 3) { extern char d[]; strcpy (d, s); d[1] = 0; extern char a1[1]; strcpy (a1, d); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_2_3_1_2 (const char *s) { size_t len = strlen (s); if (2 <= len && len <= 3) { extern char d[]; strcpy (d, s); d[2] = 0; extern char a1[1]; strcpy (a1, d); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_2_3_1_3 (const char *s) { size_t len = strlen (s); if (2 <= len && len <= 3) { extern char d[]; strcpy (d, s); d[3] = 0; extern char a1[1]; strcpy (a1, d); } } typedef void dummy_type;

__attribute__ ((noipa)) void test_5_7_3_1 (const char *s) { size_t len = strlen (s); if (5 <= len && len <= 7) { extern char d[]; strcpy (d, s); d[1] = 0; extern char a3[3]; strcpy (a3, d); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_5_7_3_2 (const char *s) { size_t len = strlen (s); if (5 <= len && len <= 7) { extern char d[]; strcpy (d, s); d[2] = 0; extern char a3[3]; strcpy (a3, d); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_5_7_3_3 (const char *s) { size_t len = strlen (s); if (5 <= len && len <= 7) { extern char d[]; strcpy (d, s); d[3] = 0; extern char a3[3]; strcpy (a3, d); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_5_7_3_4 (const char *s) { size_t len = strlen (s); if (5 <= len && len <= 7) { extern char d[]; strcpy (d, s); d[4] = 0; extern char a3[3]; strcpy (a3, d); } } typedef void dummy_type;
__attribute__ ((noipa)) void test_5_7_3_5 (const char *s) { size_t len = strlen (s); if (5 <= len && len <= 7) { extern char d[]; strcpy (d, s); d[5] = 0; extern char a3[3]; strcpy (a3, d); } } typedef void dummy_type;
