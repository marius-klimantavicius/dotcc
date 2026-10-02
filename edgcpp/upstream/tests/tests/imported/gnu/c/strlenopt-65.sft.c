//type: fp
//options: 
# 0 "./strlenopt-65.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-65.c"





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
# 7 "./strlenopt-65.c" 2
# 55 "./strlenopt-65.c"
const char s0[1] = "";
const char s00[2] = "\0";
const char s10[2] = "1";
const char s20[2] = "2";

void sink (void*, ...);

void test_strcmp_elim (void)
{



  do { char a[8], b[8]; sink (a, b); memcpy (a, s00, sizeof s00 - 1); memcpy (b, s10, sizeof s10 - 1); if (!(0 != strcmp ("\0", "1"))) do { extern void call_in_true_branch_not_eliminated_on_line_67 (void); call_in_true_branch_not_eliminated_on_line_67(); } while (0); else (void)0; } while (0);
  do { char a[8], b[8]; sink (a, b); memcpy (a, s00, sizeof s00 - 1); memcpy (b, s10, sizeof s10 - 1); if (!(0 != strcmp ("\0", b))) do { extern void call_in_true_branch_not_eliminated_on_line_68 (void); call_in_true_branch_not_eliminated_on_line_68(); } while (0); else (void)0; } while (0);
  do { char a[8], b[8]; sink (a, b); memcpy (a, s00, sizeof s00 - 1); memcpy (b, s10, sizeof s10 - 1); if (!(0 != strcmp ("\0", s10))) do { extern void call_in_true_branch_not_eliminated_on_line_69 (void); call_in_true_branch_not_eliminated_on_line_69(); } while (0); else (void)0; } while (0);

  do { char a[8], b[8]; sink (a, b); memcpy (a, s00, sizeof s00 - 1); memcpy (b, s10, sizeof s10 - 1); if (!(0 != strcmp (s0, "1"))) do { extern void call_in_true_branch_not_eliminated_on_line_71 (void); call_in_true_branch_not_eliminated_on_line_71(); } while (0); else (void)0; } while (0);
  do { char a[8], b[8]; sink (a, b); memcpy (a, s00, sizeof s00 - 1); memcpy (b, s10, sizeof s10 - 1); if (!(0 != strcmp (s0, b))) do { extern void call_in_true_branch_not_eliminated_on_line_72 (void); call_in_true_branch_not_eliminated_on_line_72(); } while (0); else (void)0; } while (0);
  do { char a[8], b[8]; sink (a, b); memcpy (a, s00, sizeof s00 - 1); memcpy (b, s10, sizeof s10 - 1); if (!(0 != strcmp (s0, s10))) do { extern void call_in_true_branch_not_eliminated_on_line_73 (void); call_in_true_branch_not_eliminated_on_line_73(); } while (0); else (void)0; } while (0);

  do { char a[8], b[8]; sink (a, b); memcpy (a, "\0", sizeof "\0" - 1); memcpy (b, "1", sizeof "1" - 1); if (!(0 != strcmp (s0, "1"))) do { extern void call_in_true_branch_not_eliminated_on_line_75 (void); call_in_true_branch_not_eliminated_on_line_75(); } while (0); else (void)0; } while (0);
  do { char a[8], b[8]; sink (a, b); memcpy (a, "\0", sizeof "\0" - 1); memcpy (b, "1", sizeof "1" - 1); if (!(0 != strcmp (s0, b))) do { extern void call_in_true_branch_not_eliminated_on_line_76 (void); call_in_true_branch_not_eliminated_on_line_76(); } while (0); else (void)0; } while (0);
  do { char a[8], b[8]; sink (a, b); memcpy (a, "\0", sizeof "\0" - 1); memcpy (b, "1", sizeof "1" - 1); if (!(0 != strcmp (s0, s10))) do { extern void call_in_true_branch_not_eliminated_on_line_77 (void); call_in_true_branch_not_eliminated_on_line_77(); } while (0); else (void)0; } while (0);

  do { char a[8], b[8]; sink (a, b); memcpy (a, "2", sizeof "2" - 1); memcpy (b, "\0", sizeof "\0" - 1); if (!(0 != strcmp ("2", "\0"))) do { extern void call_in_true_branch_not_eliminated_on_line_79 (void); call_in_true_branch_not_eliminated_on_line_79(); } while (0); else (void)0; } while (0);
  do { char a[8], b[8]; sink (a, b); memcpy (a, "2", sizeof "2" - 1); memcpy (b, "\0", sizeof "\0" - 1); if (!(0 != strcmp (s20, s0))) do { extern void call_in_true_branch_not_eliminated_on_line_80 (void); call_in_true_branch_not_eliminated_on_line_80(); } while (0); else (void)0; } while (0);

  do { char a[8], b[8]; sink (a, b); memcpy (a, "\0", sizeof "\0" - 1); memcpy (b, "1", sizeof "1" - 1); if (!(0 != strcmp (a, b))) do { extern void call_in_true_branch_not_eliminated_on_line_82 (void); call_in_true_branch_not_eliminated_on_line_82(); } while (0); else (void)0; } while (0);
  do { char a[8], b[8]; sink (a, b); memcpy (a, "2", sizeof "2" - 1); memcpy (b, "\0", sizeof "\0" - 1); if (!(0 != strcmp (a, b))) do { extern void call_in_true_branch_not_eliminated_on_line_83 (void); call_in_true_branch_not_eliminated_on_line_83(); } while (0); else (void)0; } while (0);

  do { char a[8], b[8]; sink (a, b); memcpy (a, "4\0", sizeof "4\0" - 1); memcpy (b, "44", sizeof "44" - 1); if (!(0 != strcmp (a, b))) do { extern void call_in_true_branch_not_eliminated_on_line_85 (void); call_in_true_branch_not_eliminated_on_line_85(); } while (0); else (void)0; } while (0);
  do { char a[8], b[8]; sink (a, b); memcpy (a, "55", sizeof "55" - 1); memcpy (b, "5\0", sizeof "5\0" - 1); if (!(0 != strcmp (a, b))) do { extern void call_in_true_branch_not_eliminated_on_line_86 (void); call_in_true_branch_not_eliminated_on_line_86(); } while (0); else (void)0; } while (0);

  do { char a[8], b[8]; sink (a, b); memcpy (a, "666\0", sizeof "666\0" - 1); memcpy (b, "6666", sizeof "6666" - 1); if (!(0 != strcmp (a, "6666"))) do { extern void call_in_true_branch_not_eliminated_on_line_88 (void); call_in_true_branch_not_eliminated_on_line_88(); } while (0); else (void)0; } while (0);
  do { char a[8], b[8]; sink (a, b); memcpy (a, "666\0", sizeof "666\0" - 1); memcpy (b, "6666", sizeof "6666" - 1); if (!(0 != strcmp (a, b))) do { extern void call_in_true_branch_not_eliminated_on_line_89 (void); call_in_true_branch_not_eliminated_on_line_89(); } while (0); else (void)0; } while (0);
  do { char a[8], b[8]; sink (a, b); memcpy (a, "7777", sizeof "7777" - 1); memcpy (b, "777\0", sizeof "777\0" - 1); if (!(0 != strcmp (a, b))) do { extern void call_in_true_branch_not_eliminated_on_line_90 (void); call_in_true_branch_not_eliminated_on_line_90(); } while (0); else (void)0; } while (0);







}

const char s123[] = "123";
const char s1230[] = "123\0";

const char s1234[] = "1234";
const char s12340[] = "1234\0";

void test_strncmp_elim (void)
{



  do { char a[8], b[8]; sink (a, b); memcpy (a, s1230, sizeof s1230 - 1); memcpy (b, s1234, sizeof s1234 - 1); if (!(0 != strncmp ("123", "1234", 4))) do { extern void call_in_true_branch_not_eliminated_on_line_111 (void); call_in_true_branch_not_eliminated_on_line_111(); } while (0); else (void)0; } while (0);
  do { char a[8], b[8]; sink (a, b); memcpy (a, s1234, sizeof s1234 - 1); memcpy (b, s1230, sizeof s1230 - 1); if (!(0 != strncmp ("1234", "123", 4))) do { extern void call_in_true_branch_not_eliminated_on_line_112 (void); call_in_true_branch_not_eliminated_on_line_112(); } while (0); else (void)0; } while (0);

  do { char a[8], b[8]; sink (a, b); memcpy (a, s1230, sizeof s1230 - 1); memcpy (b, s1234, sizeof s1234 - 1); if (!(0 != strncmp ("123", s1234, 4))) do { extern void call_in_true_branch_not_eliminated_on_line_114 (void); call_in_true_branch_not_eliminated_on_line_114(); } while (0); else (void)0; } while (0);
  do { char a[8], b[8]; sink (a, b); memcpy (a, s1234, sizeof s1234 - 1); memcpy (b, s1230, sizeof s1230 - 1); if (!(0 != strncmp ("1234", s123, 4))) do { extern void call_in_true_branch_not_eliminated_on_line_115 (void); call_in_true_branch_not_eliminated_on_line_115(); } while (0); else (void)0; } while (0);

  do { char a[8], b[8]; sink (a, b); memcpy (a, s1230, sizeof s1230 - 1); memcpy (b, s1234, sizeof s1234 - 1); if (!(0 != strncmp (s123, "1234", 4))) do { extern void call_in_true_branch_not_eliminated_on_line_117 (void); call_in_true_branch_not_eliminated_on_line_117(); } while (0); else (void)0; } while (0);
  do { char a[8], b[8]; sink (a, b); memcpy (a, s1234, sizeof s1234 - 1); memcpy (b, s1230, sizeof s1230 - 1); if (!(0 != strncmp (s1234, "123", 4))) do { extern void call_in_true_branch_not_eliminated_on_line_118 (void); call_in_true_branch_not_eliminated_on_line_118(); } while (0); else (void)0; } while (0);

  do { char a[8], b[8]; sink (a, b); memcpy (a, s1230, sizeof s1230 - 1); memcpy (b, s1234, sizeof s1234 - 1); if (!(0 != strncmp (s123, b, 4))) do { extern void call_in_true_branch_not_eliminated_on_line_120 (void); call_in_true_branch_not_eliminated_on_line_120(); } while (0); else (void)0; } while (0);
  do { char a[8], b[8]; sink (a, b); memcpy (a, s1234, sizeof s1234 - 1); memcpy (b, s1230, sizeof s1230 - 1); if (!(0 != strncmp (s1234, b, 4))) do { extern void call_in_true_branch_not_eliminated_on_line_121 (void); call_in_true_branch_not_eliminated_on_line_121(); } while (0); else (void)0; } while (0);

  do { char a[8], b[8]; sink (a, b); memcpy (a, s1230, sizeof s1230 - 1); memcpy (b, s1234, sizeof s1234 - 1); if (!(0 != strncmp (a, b, 4))) do { extern void call_in_true_branch_not_eliminated_on_line_123 (void); call_in_true_branch_not_eliminated_on_line_123(); } while (0); else (void)0; } while (0);
  do { char a[8], b[8]; sink (a, b); memcpy (a, s1234, sizeof s1234 - 1); memcpy (b, s1230, sizeof s1230 - 1); if (!(0 != strncmp (a, b, 4))) do { extern void call_in_true_branch_not_eliminated_on_line_124 (void); call_in_true_branch_not_eliminated_on_line_124(); } while (0); else (void)0; } while (0);

  do { char a[8], b[8]; sink (a, b); memcpy (a, "123\0", sizeof "123\0" - 1); memcpy (b, "1234", sizeof "1234" - 1); if (!(0 != strncmp (a, b, 5))) do { extern void call_in_true_branch_not_eliminated_on_line_126 (void); call_in_true_branch_not_eliminated_on_line_126(); } while (0); else (void)0; } while (0);
  do { char a[8], b[8]; sink (a, b); memcpy (a, "1234", sizeof "1234" - 1); memcpy (b, "123\0", sizeof "123\0" - 1); if (!(0 != strncmp (a, b, 5))) do { extern void call_in_true_branch_not_eliminated_on_line_127 (void); call_in_true_branch_not_eliminated_on_line_127(); } while (0); else (void)0; } while (0);
}
# 1000 "./strlenopt-65.c"

void test_strcmp_keep (const char *s, const char *t)
{



  do { char a[8], b[8]; sink (a, b); memcpy (a, "123", sizeof "123" - 1); memcpy (b, "123\0", sizeof "123\0" - 1); if (0 == strcmp (a, b)) do { extern void call_made_in_true_branch_on_line_1006 (void); call_made_in_true_branch_on_line_1006(); } while (0); else do { extern void call_made_in_false_branch_on_line_1006 (void); call_made_in_false_branch_on_line_1006(); } while (0); } while (0);
  do { char a[8], b[8]; sink (a, b); memcpy (a, "123\0", sizeof "123\0" - 1); memcpy (b, "123", sizeof "123" - 1); if (0 == strcmp (a, b)) do { extern void call_made_in_true_branch_on_line_1007 (void); call_made_in_true_branch_on_line_1007(); } while (0); else do { extern void call_made_in_false_branch_on_line_1007 (void); call_made_in_false_branch_on_line_1007(); } while (0); } while (0);

  {
    char a[8], b[8];
    sink (a, b);
    strcpy (a, s);
    strcpy (b, t);
    if (0 == strcmp (a, b)) do { extern void call_made_in_true_branch_on_line_1014 (void); call_made_in_true_branch_on_line_1014(); } while (0); else do { extern void call_made_in_false_branch_on_line_1014 (void); call_made_in_false_branch_on_line_1014(); } while (0);
  }
}


void test_strncmp_keep (const char *s, const char *t)
{



  do { char a[8], b[8]; sink (a, b); memcpy (a, "1", sizeof "1" - 1); memcpy (b, "1", sizeof "1" - 1); if (0 == strncmp (a, b, 2)) do { extern void call_made_in_true_branch_on_line_1024 (void); call_made_in_true_branch_on_line_1024(); } while (0); else do { extern void call_made_in_false_branch_on_line_1024 (void); call_made_in_false_branch_on_line_1024(); } while (0); } while (0);

  do { char a[8], b[8]; sink (a, b); memcpy (a, "1\0", sizeof "1\0" - 1); memcpy (b, "1", sizeof "1" - 1); if (0 == strncmp (a, b, 2)) do { extern void call_made_in_true_branch_on_line_1026 (void); call_made_in_true_branch_on_line_1026(); } while (0); else do { extern void call_made_in_false_branch_on_line_1026 (void); call_made_in_false_branch_on_line_1026(); } while (0); } while (0);
  do { char a[8], b[8]; sink (a, b); memcpy (a, "1", sizeof "1" - 1); memcpy (b, "1\0", sizeof "1\0" - 1); if (0 == strncmp (a, b, 2)) do { extern void call_made_in_true_branch_on_line_1027 (void); call_made_in_true_branch_on_line_1027(); } while (0); else do { extern void call_made_in_false_branch_on_line_1027 (void); call_made_in_false_branch_on_line_1027(); } while (0); } while (0);

  do { char a[8], b[8]; sink (a, b); memcpy (a, "12\0", sizeof "12\0" - 1); memcpy (b, "12", sizeof "12" - 1); if (0 == strncmp (a, b, 2)) do { extern void call_made_in_true_branch_on_line_1029 (void); call_made_in_true_branch_on_line_1029(); } while (0); else do { extern void call_made_in_false_branch_on_line_1029 (void); call_made_in_false_branch_on_line_1029(); } while (0); } while (0);
  do { char a[8], b[8]; sink (a, b); memcpy (a, "12", sizeof "12" - 1); memcpy (b, "12\0", sizeof "12\0" - 1); if (0 == strncmp (a, b, 2)) do { extern void call_made_in_true_branch_on_line_1030 (void); call_made_in_true_branch_on_line_1030(); } while (0); else do { extern void call_made_in_false_branch_on_line_1030 (void); call_made_in_false_branch_on_line_1030(); } while (0); } while (0);

  do { char a[8], b[8]; sink (a, b); memcpy (a, "111\0", sizeof "111\0" - 1); memcpy (b, "111", sizeof "111" - 1); if (0 == strncmp (a, b, 3)) do { extern void call_made_in_true_branch_on_line_1032 (void); call_made_in_true_branch_on_line_1032(); } while (0); else do { extern void call_made_in_false_branch_on_line_1032 (void); call_made_in_false_branch_on_line_1032(); } while (0); } while (0);
  do { char a[8], b[8]; sink (a, b); memcpy (a, "112", sizeof "112" - 1); memcpy (b, "112\0", sizeof "112\0" - 1); if (0 == strncmp (a, b, 3)) do { extern void call_made_in_true_branch_on_line_1033 (void); call_made_in_true_branch_on_line_1033(); } while (0); else do { extern void call_made_in_false_branch_on_line_1033 (void); call_made_in_false_branch_on_line_1033(); } while (0); } while (0);

  {
    char a[8], b[8];
    sink (a, b);
    strcpy (a, s);
    strcpy (b, t);
    if (0 == strncmp (a, b, sizeof a)) do { extern void call_made_in_true_branch_on_line_1040 (void); call_made_in_true_branch_on_line_1040(); } while (0); else do { extern void call_made_in_false_branch_on_line_1040 (void); call_made_in_false_branch_on_line_1040(); } while (0);
  }
}
