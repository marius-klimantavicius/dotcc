//type: fp
//options: 
# 0 "./strlenopt-72.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-72.c"
# 15 "./strlenopt-72.c"
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
# 16 "./strlenopt-72.c" 2
# 42 "./strlenopt-72.c"
void test_copy (void)
{
  do { char a[2], b[2]; (a[0] = 0); memcpy (b, a, 1); if ((!(0 == strlen (b)))) do { extern void call_not_eliminated_on_line_44 (void); call_not_eliminated_on_line_44(); } while (0); else (void)0; } while (0);
  do { char a[2], b[2]; (a[0] = 0, a[1] = 0); memcpy (b, a, 2); if ((!(0 == strlen (b)))) do { extern void call_not_eliminated_on_line_45 (void); call_not_eliminated_on_line_45(); } while (0); else (void)0; } while (0);
  do { char a[2], b[2]; (a[0] = '1', a[1] = 0); memcpy (b, a, 2); if ((!(1 == strlen (b)))) do { extern void call_not_eliminated_on_line_46 (void); call_not_eliminated_on_line_46(); } while (0); else (void)0; } while (0);
  do { char a[4], b[4]; (a[0] = '1', a[1] = '2', a[2] = 0); memcpy (b, a, 3); if ((!(2 == strlen (b)))) do { extern void call_not_eliminated_on_line_47 (void); call_not_eliminated_on_line_47(); } while (0); else (void)0; } while (0);


  do { char a[4], b[4]; (a[0] = '1', a[1] = 0, a[2] = 0, a[3] = 0); memcpy (b, a, 2); if ((!(1 == strlen (b)))) do { extern void call_not_eliminated_on_line_50 (void); call_not_eliminated_on_line_50(); } while (0); else (void)0; } while (0);


  do { char a[4], b[4]; (a[0] = '1', a[1] = 0, a[2] = 0, a[3] = 0); memcpy (b, a, 4); if ((!(1 == strlen (b)))) do { extern void call_not_eliminated_on_line_53 (void); call_not_eliminated_on_line_53(); } while (0); else (void)0; } while (0);
  do { char a[4], b[4]; (a[0] = '1', a[1] = '2', a[2] = 0, a[3] = 0); memcpy (b, a, 3); if ((!(2 == strlen (b)))) do { extern void call_not_eliminated_on_line_54 (void); call_not_eliminated_on_line_54(); } while (0); else (void)0; } while (0);
  do { char a[4], b[4]; (a[0] = '1', a[1] = '2', a[2] = 0, a[3] = 0); memcpy (b, a, 4); if ((!(2 == strlen (b)))) do { extern void call_not_eliminated_on_line_55 (void); call_not_eliminated_on_line_55(); } while (0); else (void)0; } while (0);
  do { char a[4], b[4]; (a[0] = '1', a[1] = '2', a[2] = '3', a[3] = 0); memcpy (b, a, 4); if ((!(3 == strlen (b)))) do { extern void call_not_eliminated_on_line_56 (void); call_not_eliminated_on_line_56(); } while (0); else (void)0; } while (0);
  do { char a[5], b[5]; (a[0] = '1', a[1] = 0, a[2] = 0, a[3] = 0); memcpy (b, a, 4); if ((!(1 == strlen (b)))) do { extern void call_not_eliminated_on_line_57 (void); call_not_eliminated_on_line_57(); } while (0); else (void)0; } while (0);
  do { char a[5], b[5]; (a[0] = '1', a[1] = '2', a[2] = 0, a[3] = 0); memcpy (b, a, 4); if ((!(2 == strlen (b)))) do { extern void call_not_eliminated_on_line_58 (void); call_not_eliminated_on_line_58(); } while (0); else (void)0; } while (0);
  do { char a[5], b[5]; (a[0] = '1', a[1] = '2', a[2] = '3', a[3] = 0); memcpy (b, a, 4); if ((!(3 == strlen (b)))) do { extern void call_not_eliminated_on_line_59 (void); call_not_eliminated_on_line_59(); } while (0); else (void)0; } while (0);




  do { char a[5], b[5]; (a[0] = '1', a[1] = '2', a[2] = '3', a[3] = '4', a[4] = 0); memcpy (b, a, 5); if ((!(4 == strlen (b)))) do { extern void call_not_eliminated_on_line_64 (void); call_not_eliminated_on_line_64(); } while (0); else (void)0; } while (0);
}
