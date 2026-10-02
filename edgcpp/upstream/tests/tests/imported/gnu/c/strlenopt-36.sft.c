//type: fp
//options: 
# 0 "./strlenopt-36.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-36.c"





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
# 7 "./strlenopt-36.c" 2

extern char a7[7], a6[6], a5[5], a4[4], a3[3], a2[2], a1[1];
extern char a0[0];
extern char ax[];

extern void failure_on_line (int);
# 23 "./strlenopt-36.c"
void test_array (void)
{
  if (!(strlen (a7) < sizeof a7)) do { failure_on_line (25); } while (0); else (void)0;
  if (!(strlen (a6) < sizeof a6)) do { failure_on_line (26); } while (0); else (void)0;
  if (!(strlen (a5) < sizeof a5)) do { failure_on_line (27); } while (0); else (void)0;
  if (!(strlen (a4) < sizeof a4)) do { failure_on_line (28); } while (0); else (void)0;
  if (!(strlen (a3) < sizeof a3)) do { failure_on_line (29); } while (0); else (void)0;





}
