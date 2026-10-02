//type: fp
//options: 
# 0 "./strlenopt-44.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-44.c"




# 1 "./range.h" 1
# 11 "./range.h"
typedef int int32_t;
typedef long int ptrdiff_t;
typedef long unsigned int size_t;

static inline ptrdiff_t signed_value (void)
{
  extern volatile ptrdiff_t signed_value_source;
  return signed_value_source;
}

static inline size_t unsigned_value (void)
{
  extern volatile size_t unsigned_value_source;
  return unsigned_value_source;
}

static inline ptrdiff_t signed_range (ptrdiff_t min, ptrdiff_t max)
{
  ptrdiff_t val = signed_value ();
  return val < min || max < val ? min : val;
}

static inline ptrdiff_t signed_anti_range (ptrdiff_t min, ptrdiff_t max)
{
  ptrdiff_t val = signed_value ();
  return min <= val && val <= max ? min == (-0x7fffffffffffffffL - 1) ? max + 1 : min - 1 : val;
}

static inline size_t unsigned_range (size_t min, size_t max)
{
  size_t val = unsigned_value ();
  return val < min || max < val ? min : val;
}

static inline size_t unsigned_anti_range (size_t min, size_t max)
{
  size_t val = unsigned_value ();
  return min <= val && val <= max ? min == 0 ? max + 1 : min - 1 : val;
}
# 6 "./strlenopt-44.c" 2
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
# 7 "./strlenopt-44.c" 2
# 53 "./strlenopt-44.c"
void test_elim_range (char c)
{
  do { char a[] = "1"; a[0] = unsigned_range ((1), (2)); if (!(strlen (a) == 1)) do { extern void call_in_true_branch_not_eliminated_on_line_55 (void); call_in_true_branch_not_eliminated_on_line_55(); } while (0); else (void)0; } while (0);
  do { char a[] = "1"; a[0] = unsigned_range ((1), (127)); if (!(strlen (a) == 1)) do { extern void call_in_true_branch_not_eliminated_on_line_56 (void); call_in_true_branch_not_eliminated_on_line_56(); } while (0); else (void)0; } while (0);
  do { char a[] = "1"; a[0] = unsigned_range (('0'), ('9')); if (!(strlen (a) == 1)) do { extern void call_in_true_branch_not_eliminated_on_line_57 (void); call_in_true_branch_not_eliminated_on_line_57(); } while (0); else (void)0; } while (0);

  do { char a[] = "12"; a[0] = unsigned_range ((1), (127)); if (!(strlen (a) == 2)) do { extern void call_in_true_branch_not_eliminated_on_line_59 (void); call_in_true_branch_not_eliminated_on_line_59(); } while (0); else (void)0; } while (0);
  do { char a[] = "12"; a[1] = unsigned_range ((1), (127)); if (!(strlen (a) == 2)) do { extern void call_in_true_branch_not_eliminated_on_line_60 (void); call_in_true_branch_not_eliminated_on_line_60(); } while (0); else (void)0; } while (0);

  do { char a[] = "123"; a[0] = unsigned_range ((1), (9)); if (!(strlen (a) == 3)) do { extern void call_in_true_branch_not_eliminated_on_line_62 (void); call_in_true_branch_not_eliminated_on_line_62(); } while (0); else (void)0; } while (0);
  do { char a[] = "123"; a[1] = unsigned_range ((10), (99)); if (!(strlen (a) == 3)) do { extern void call_in_true_branch_not_eliminated_on_line_63 (void); call_in_true_branch_not_eliminated_on_line_63(); } while (0); else (void)0; } while (0);
  do { char a[] = "123"; a[2] = unsigned_range ((100), (127)); if (!(strlen (a) == 3)) do { extern void call_in_true_branch_not_eliminated_on_line_64 (void); call_in_true_branch_not_eliminated_on_line_64(); } while (0); else (void)0; } while (0);
}

void test_elim_anti_range (const char *s)
{
  char c = *s++;
  do { char a[] = "123"; a[0] = c ? c : 'x'; if (!(strlen (a) == 3)) do { extern void call_in_true_branch_not_eliminated_on_line_70 (void); call_in_true_branch_not_eliminated_on_line_70(); } while (0); else (void)0; } while (0);

  c = *s++;
  do { char a[] = "1234"; a[1] = c ? c : 'y'; if (!(strlen (a) == 4)) do { extern void call_in_true_branch_not_eliminated_on_line_73 (void); call_in_true_branch_not_eliminated_on_line_73(); } while (0); else (void)0; } while (0);

  c = *s++;
  do { char a[] = "123"; a[2] = c ? c : 'z'; if (!(strlen (a) == 3)) do { extern void call_in_true_branch_not_eliminated_on_line_76 (void); call_in_true_branch_not_eliminated_on_line_76(); } while (0); else (void)0; } while (0);
}
# 1000 "./strlenopt-44.c"

void test_keep (void)
{
  size_t uchar_max = (unsigned char)-1;

  do { char a[] = "1"; a[0] = unsigned_range ((1), (uchar_max + 1)); if (strlen (a) == 1) do { extern void call_made_in_true_branch_on_line_1005 (void); call_made_in_true_branch_on_line_1005(); } while (0); else do { extern void call_made_in_false_branch_on_line_1005 (void); call_made_in_false_branch_on_line_1005(); } while (0); } while (0);
  do { char a[] = "1\0\3"; a[1] = unsigned_range ((1), (2)); if (strlen (a) == 2) do { extern void call_made_in_true_branch_on_line_1006 (void); call_made_in_true_branch_on_line_1006(); } while (0); else do { extern void call_made_in_false_branch_on_line_1006 (void); call_made_in_false_branch_on_line_1006(); } while (0); } while (0);
}
