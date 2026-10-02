//type: fp
//options: 
# 0 "./strlenopt-77.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-77.c"





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
# 7 "./strlenopt-77.c" 2
# 25 "./strlenopt-77.c"
char a[32];

void lower_bound_assign_1 (void)
{
  a[0] = '1';
  if (!!(strlen (a) < 1)) do { extern void call_in_true_branch_not_eliminated_on_line_30 (void); call_in_true_branch_not_eliminated_on_line_30(); } while (0); else (void)0;
}

void lower_bound_assign_2 (void)
{
  a[0] = '1';
  a[1] = '2';
  if (!!(strlen (a) < 2)) do { extern void call_in_true_branch_not_eliminated_on_line_37 (void); call_in_true_branch_not_eliminated_on_line_37(); } while (0); else (void)0;
}

void lower_bound_assign_3 (void)
{
  a[0] = '1';
  a[1] = '2';
  a[2] = '3';
  if (!!(strlen (a) < 3)) do { extern void call_in_true_branch_not_eliminated_on_line_45 (void); call_in_true_branch_not_eliminated_on_line_45(); } while (0); else (void)0;
}

void lower_bound_memcpy (void)
{
  memcpy (a, "123", 3);
  if (!!(strlen (a) < 3)) do { extern void call_in_true_branch_not_eliminated_on_line_51 (void); call_in_true_branch_not_eliminated_on_line_51(); } while (0); else (void)0;
}

void lower_bound_memcpy_memcpy_2 (void)
{
  memcpy (a, "123", 3);
  memcpy (a + 2, "345", 3);
  if (!!(strlen (a) < 5)) do { extern void call_in_true_branch_not_eliminated_on_line_58 (void); call_in_true_branch_not_eliminated_on_line_58(); } while (0); else (void)0;
}

void lower_bound_memcpy_memcpy_3 (void)
{
  memcpy (a, "123", 3);
  memcpy (a + 3, "456", 3);
  if (!!(strlen (a) < 6)) do { extern void call_in_true_branch_not_eliminated_on_line_65 (void); call_in_true_branch_not_eliminated_on_line_65(); } while (0); else (void)0;
}
# 76 "./strlenopt-77.c"
void lower_bound_strcpy_strcat_assign (void)
{
  strcpy (a, "123");
  strcat (a, "45");
  a[5] = '6';
  if (!!(strlen (a) < 6)) do { extern void call_in_true_branch_not_eliminated_on_line_81 (void); call_in_true_branch_not_eliminated_on_line_81(); } while (0); else (void)0;
}
