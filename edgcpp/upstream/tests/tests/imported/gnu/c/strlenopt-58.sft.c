//type: fp
//options: 
# 0 "./strlenopt-58.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-58.c"
# 9 "./strlenopt-58.c"
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
# 10 "./strlenopt-58.c" 2

typedef int wchar_t;

extern void* memchr (const void*, int, size_t);
# 35 "./strlenopt-58.c"
static const wchar_t wc = L'1';
static const wchar_t ws1[] = L"1";
static const wchar_t wsx[] = L"\x12345678";
static const wchar_t ws4[] = L"\x00123456\x12005678\x12340078\x12345600";

void test_wide (void)
{
  int i0 = 0;
  int i1 = i0 + 1;
  int i2 = i1 + 1;
  int i3 = i2 + 1;
  int i4 = i3 + 1;

  if (!(memchr (L"" + 1, 0, 0) == 0)) do { extern void call_in_true_branch_not_eliminated_on_line_48 (void); call_in_true_branch_not_eliminated_on_line_48(); } while (0); else (void)0;
  if (!(memchr (&wc + 1, 0, 0) == 0)) do { extern void call_in_true_branch_not_eliminated_on_line_49 (void); call_in_true_branch_not_eliminated_on_line_49(); } while (0); else (void)0;
  if (!(memchr (L"\x12345678", 0, sizeof (wchar_t)) == 0)) do { extern void call_in_true_branch_not_eliminated_on_line_50 (void); call_in_true_branch_not_eliminated_on_line_50(); } while (0); else (void)0;

  const size_t nb = sizeof ws4;
  const size_t nwb = sizeof (wchar_t);

  const char *pws1 = (const char*)ws1;
  const char *pws4 = (const char*)ws4;
  const char *pwsx = (const char*)wsx;


  if (!(memchr (ws1, 0, sizeof ws1) == pws1 + 1)) do { extern void call_in_true_branch_not_eliminated_on_line_60 (void); call_in_true_branch_not_eliminated_on_line_60(); } while (0); else (void)0;
  if (!(memchr (wsx, 0, sizeof wsx) == pwsx + sizeof *wsx)) do { extern void call_in_true_branch_not_eliminated_on_line_61 (void); call_in_true_branch_not_eliminated_on_line_61(); } while (0); else (void)0;

  if (!(memchr (&ws4[0], 0, nb) == pws4 + 3)) do { extern void call_in_true_branch_not_eliminated_on_line_63 (void); call_in_true_branch_not_eliminated_on_line_63(); } while (0); else (void)0;
  if (!(memchr (&ws4[1], 0, nb - 1 * nwb) == pws4 + 1 * nwb + 2)) do { extern void call_in_true_branch_not_eliminated_on_line_64 (void); call_in_true_branch_not_eliminated_on_line_64(); } while (0); else (void)0;
  if (!(memchr (&ws4[2], 0, nb - 2 * nwb) == pws4 + 2 * nwb + 1)) do { extern void call_in_true_branch_not_eliminated_on_line_65 (void); call_in_true_branch_not_eliminated_on_line_65(); } while (0); else (void)0;
  if (!(memchr (&ws4[3], 0, nb - 3 * nwb) == pws4 + 3 * nwb + 0)) do { extern void call_in_true_branch_not_eliminated_on_line_66 (void); call_in_true_branch_not_eliminated_on_line_66(); } while (0); else (void)0;
  if (!(memchr (&ws4[4], 0, nb - 4 * nwb) == pws4 + 4 * nwb + 0)) do { extern void call_in_true_branch_not_eliminated_on_line_67 (void); call_in_true_branch_not_eliminated_on_line_67(); } while (0); else (void)0;

  if (!(memchr (&ws4[i0], 0, nb) == pws4 + 3)) do { extern void call_in_true_branch_not_eliminated_on_line_69 (void); call_in_true_branch_not_eliminated_on_line_69(); } while (0); else (void)0;
  if (!(memchr (&ws4[i1], 0, nb - 1 * nwb) == pws4 + 1 * nwb + 2)) do { extern void call_in_true_branch_not_eliminated_on_line_70 (void); call_in_true_branch_not_eliminated_on_line_70(); } while (0); else (void)0;
  if (!(memchr (&ws4[i2], 0, nb - 2 * nwb) == pws4 + 2 * nwb + 1)) do { extern void call_in_true_branch_not_eliminated_on_line_71 (void); call_in_true_branch_not_eliminated_on_line_71(); } while (0); else (void)0;
  if (!(memchr (&ws4[i3], 0, nb - 3 * nwb) == pws4 + 3 * nwb + 0)) do { extern void call_in_true_branch_not_eliminated_on_line_72 (void); call_in_true_branch_not_eliminated_on_line_72(); } while (0); else (void)0;
  if (!(memchr (&ws4[i4], 0, nb - 4 * nwb) == pws4 + 4 * nwb + 0)) do { extern void call_in_true_branch_not_eliminated_on_line_73 (void); call_in_true_branch_not_eliminated_on_line_73(); } while (0); else (void)0;
# 90 "./strlenopt-58.c"
}
