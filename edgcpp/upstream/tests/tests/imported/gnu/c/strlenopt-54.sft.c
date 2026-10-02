//type: fp
//options: 
# 0 "./strlenopt-54.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-54.c"




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
# 6 "./strlenopt-54.c" 2
# 24 "./strlenopt-54.c"
void elim_after_duplicate_strcpy (void)
{
# 36 "./strlenopt-54.c"
  do { char a[2]; strcpy (a, ""); unsigned n0 = strlen (a); strcpy (a + 0, "1"); unsigned n1 = strlen (a); if (!(n0 == 0 && n1 == 1)) do { extern void call_in_true_branch_not_eliminated_on_line_36 (void); call_in_true_branch_not_eliminated_on_line_36(); } while (0); else (void)0; } while (0);

  do { char a[2]; strcpy (a, "1"); unsigned n0 = strlen (a); strcpy (a + 0, "2"); unsigned n1 = strlen (a); if (!(n0 == 1 && n1 == 1)) do { extern void call_in_true_branch_not_eliminated_on_line_38 (void); call_in_true_branch_not_eliminated_on_line_38(); } while (0); else (void)0; } while (0);
  do { char a[2]; strcpy (a, "1"); unsigned n0 = strlen (a); strcpy (a + 1, ""); unsigned n1 = strlen (a); if (!(n0 == 1 && n1 == 1)) do { extern void call_in_true_branch_not_eliminated_on_line_39 (void); call_in_true_branch_not_eliminated_on_line_39(); } while (0); else (void)0; } while (0);

  do { char a[3]; strcpy (a, "\0"); unsigned n0 = strlen (a); strcpy (a + 0, "1"); unsigned n1 = strlen (a); if (!(n0 == 0 && n1 == 1)) do { extern void call_in_true_branch_not_eliminated_on_line_41 (void); call_in_true_branch_not_eliminated_on_line_41(); } while (0); else (void)0; } while (0);
  do { char a[3]; strcpy (a, "1"); unsigned n0 = strlen (a); strcpy (a + 1, "2"); unsigned n1 = strlen (a); if (!(n0 == 1 && n1 == 2)) do { extern void call_in_true_branch_not_eliminated_on_line_42 (void); call_in_true_branch_not_eliminated_on_line_42(); } while (0); else (void)0; } while (0);

  do { char a[3]; strcpy (a, "12"); unsigned n0 = strlen (a); strcpy (a + 0, "23"); unsigned n1 = strlen (a); if (!(n0 == 2 && n1 == 2)) do { extern void call_in_true_branch_not_eliminated_on_line_44 (void); call_in_true_branch_not_eliminated_on_line_44(); } while (0); else (void)0; } while (0);
  do { char a[3]; strcpy (a, "12"); unsigned n0 = strlen (a); strcpy (a + 1, "3"); unsigned n1 = strlen (a); if (!(n0 == 2 && n1 == 2)) do { extern void call_in_true_branch_not_eliminated_on_line_45 (void); call_in_true_branch_not_eliminated_on_line_45(); } while (0); else (void)0; } while (0);
  do { char a[3]; strcpy (a, "12"); unsigned n0 = strlen (a); strcpy (a + 2, ""); unsigned n1 = strlen (a); if (!(n0 == 2 && n1 == 2)) do { extern void call_in_true_branch_not_eliminated_on_line_46 (void); call_in_true_branch_not_eliminated_on_line_46(); } while (0); else (void)0; } while (0);

  do { char a[4]; strcpy (a, "1"); unsigned n0 = strlen (a); strcpy (a + 1, "23"); unsigned n1 = strlen (a); if (!(n0 == 1 && n1 == 3)) do { extern void call_in_true_branch_not_eliminated_on_line_48 (void); call_in_true_branch_not_eliminated_on_line_48(); } while (0); else (void)0; } while (0);
  do { char a[4]; strcpy (a, "12"); unsigned n0 = strlen (a); strcpy (a + 1, "23"); unsigned n1 = strlen (a); if (!(n0 == 2 && n1 == 3)) do { extern void call_in_true_branch_not_eliminated_on_line_49 (void); call_in_true_branch_not_eliminated_on_line_49(); } while (0); else (void)0; } while (0);

  do { char a[4]; strcpy (a, "123"); unsigned n0 = strlen (a); strcpy (a + 0, "234"); unsigned n1 = strlen (a); if (!(n0 == 3 && n1 == 3)) do { extern void call_in_true_branch_not_eliminated_on_line_51 (void); call_in_true_branch_not_eliminated_on_line_51(); } while (0); else (void)0; } while (0);
  do { char a[4]; strcpy (a, "123"); unsigned n0 = strlen (a); strcpy (a + 1, "34"); unsigned n1 = strlen (a); if (!(n0 == 3 && n1 == 3)) do { extern void call_in_true_branch_not_eliminated_on_line_52 (void); call_in_true_branch_not_eliminated_on_line_52(); } while (0); else (void)0; } while (0);
  do { char a[4]; strcpy (a, "123"); unsigned n0 = strlen (a); strcpy (a + 2, "4"); unsigned n1 = strlen (a); if (!(n0 == 3 && n1 == 3)) do { extern void call_in_true_branch_not_eliminated_on_line_53 (void); call_in_true_branch_not_eliminated_on_line_53(); } while (0); else (void)0; } while (0);
  do { char a[4]; strcpy (a, "123"); unsigned n0 = strlen (a); strcpy (a + 3, ""); unsigned n1 = strlen (a); if (!(n0 == 3 && n1 == 3)) do { extern void call_in_true_branch_not_eliminated_on_line_54 (void); call_in_true_branch_not_eliminated_on_line_54(); } while (0); else (void)0; } while (0);

  do { char a[5]; strcpy (a, "1234"); unsigned n0 = strlen (a); strcpy (a + 0, "1"); unsigned n1 = strlen (a); if (!(n0 == 4 && n1 == 1)) do { extern void call_in_true_branch_not_eliminated_on_line_56 (void); call_in_true_branch_not_eliminated_on_line_56(); } while (0); else (void)0; } while (0);
  do { char a[5]; strcpy (a, "1234"); unsigned n0 = strlen (a); strcpy (a + 0, "12"); unsigned n1 = strlen (a); if (!(n0 == 4 && n1 == 2)) do { extern void call_in_true_branch_not_eliminated_on_line_57 (void); call_in_true_branch_not_eliminated_on_line_57(); } while (0); else (void)0; } while (0);
  do { char a[5]; strcpy (a, "1234"); unsigned n0 = strlen (a); strcpy (a + 0, "123"); unsigned n1 = strlen (a); if (!(n0 == 4 && n1 == 3)) do { extern void call_in_true_branch_not_eliminated_on_line_58 (void); call_in_true_branch_not_eliminated_on_line_58(); } while (0); else (void)0; } while (0);
  do { char a[5]; strcpy (a, "1234"); unsigned n0 = strlen (a); strcpy (a + 0, "1234"); unsigned n1 = strlen (a); if (!(n0 == 4 && n1 == 4)) do { extern void call_in_true_branch_not_eliminated_on_line_59 (void); call_in_true_branch_not_eliminated_on_line_59(); } while (0); else (void)0; } while (0);

  do { char a[5]; strcpy (a, "123"); unsigned n0 = strlen (a); strcpy (a + 1, "234"); unsigned n1 = strlen (a); if (!(n0 == 3 && n1 == 4)) do { extern void call_in_true_branch_not_eliminated_on_line_61 (void); call_in_true_branch_not_eliminated_on_line_61(); } while (0); else (void)0; } while (0);
  do { char a[6]; strcpy (a, "123"); unsigned n0 = strlen (a); strcpy (a + 2, "234"); unsigned n1 = strlen (a); if (!(n0 == 3 && n1 == 5)) do { extern void call_in_true_branch_not_eliminated_on_line_62 (void); call_in_true_branch_not_eliminated_on_line_62(); } while (0); else (void)0; } while (0);
}

void elim_after_init_memcpy (void)
{
# 75 "./strlenopt-54.c"
  do { char a[] = "\0"; memcpy (a + 0, "1", 2); if (!(strlen (a) == 1)) do { extern void call_in_true_branch_not_eliminated_on_line_75 (void); call_in_true_branch_not_eliminated_on_line_75(); } while (0); else (void)0; } while (0);
  do { char a[] = "\0\0"; memcpy (a + 0, "12", 3); if (!(strlen (a) == 2)) do { extern void call_in_true_branch_not_eliminated_on_line_76 (void); call_in_true_branch_not_eliminated_on_line_76(); } while (0); else (void)0; } while (0);


  do { char a[] = { '1', '2', '3', '4' }; memcpy (a + 0, "", 1); if (!(strlen (a) == 0)) do { extern void call_in_true_branch_not_eliminated_on_line_79 (void); call_in_true_branch_not_eliminated_on_line_79(); } while (0); else (void)0; } while (0);
  do { char a[] = { '1', '2', '3', '4' }; memcpy (a + 0, "1", 2); if (!(strlen (a) == 1)) do { extern void call_in_true_branch_not_eliminated_on_line_80 (void); call_in_true_branch_not_eliminated_on_line_80(); } while (0); else (void)0; } while (0);
  do { char a[] = { '1', '2', '3', '4' }; memcpy (a + 0, "12", 3); if (!(strlen (a) == 2)) do { extern void call_in_true_branch_not_eliminated_on_line_81 (void); call_in_true_branch_not_eliminated_on_line_81(); } while (0); else (void)0; } while (0);
  do { char a[] = { '1', '2', '3', '4' }; memcpy (a + 0, "123", 4); if (!(strlen (a) == 3)) do { extern void call_in_true_branch_not_eliminated_on_line_82 (void); call_in_true_branch_not_eliminated_on_line_82(); } while (0); else (void)0; } while (0);

  do { char a[] = "1234"; memcpy (a + 0, "2", 1); if (!(strlen (a) == 4)) do { extern void call_in_true_branch_not_eliminated_on_line_84 (void); call_in_true_branch_not_eliminated_on_line_84(); } while (0); else (void)0; } while (0);
  do { char a[] = "1234"; memcpy (a + 0, "23", 2); if (!(strlen (a) == 4)) do { extern void call_in_true_branch_not_eliminated_on_line_85 (void); call_in_true_branch_not_eliminated_on_line_85(); } while (0); else (void)0; } while (0);
  do { char a[] = "1234"; memcpy (a + 0, "234", 3); if (!(strlen (a) == 4)) do { extern void call_in_true_branch_not_eliminated_on_line_86 (void); call_in_true_branch_not_eliminated_on_line_86(); } while (0); else (void)0; } while (0);
  do { char a[] = "1234"; memcpy (a + 0, "2345", 4); if (!(strlen (a) == 4)) do { extern void call_in_true_branch_not_eliminated_on_line_87 (void); call_in_true_branch_not_eliminated_on_line_87(); } while (0); else (void)0; } while (0);
  do { char a[] = "1234"; memcpy (a + 0, "2345", 5); if (!(strlen (a) == 4)) do { extern void call_in_true_branch_not_eliminated_on_line_88 (void); call_in_true_branch_not_eliminated_on_line_88(); } while (0); else (void)0; } while (0);

  do { char a[] = "1234"; memcpy (a + 1, "2", 1); if (!(strlen (a) == 4)) do { extern void call_in_true_branch_not_eliminated_on_line_90 (void); call_in_true_branch_not_eliminated_on_line_90(); } while (0); else (void)0; } while (0);
  do { char a[] = "1234"; memcpy (a + 1, "23", 2); if (!(strlen (a) == 4)) do { extern void call_in_true_branch_not_eliminated_on_line_91 (void); call_in_true_branch_not_eliminated_on_line_91(); } while (0); else (void)0; } while (0);
  do { char a[] = "1234"; memcpy (a + 1, "234", 3); if (!(strlen (a) == 4)) do { extern void call_in_true_branch_not_eliminated_on_line_92 (void); call_in_true_branch_not_eliminated_on_line_92(); } while (0); else (void)0; } while (0);
  do { char a[] = "1234"; memcpy (a + 1, "234", 4); if (!(strlen (a) == 4)) do { extern void call_in_true_branch_not_eliminated_on_line_93 (void); call_in_true_branch_not_eliminated_on_line_93(); } while (0); else (void)0; } while (0);

  do { char a[] = "1234"; memcpy (a + 2, "3", 1); if (!(strlen (a) == 4)) do { extern void call_in_true_branch_not_eliminated_on_line_95 (void); call_in_true_branch_not_eliminated_on_line_95(); } while (0); else (void)0; } while (0);
  do { char a[] = "1234"; memcpy (a + 2, "3", 2); if (!(strlen (a) == 3)) do { extern void call_in_true_branch_not_eliminated_on_line_96 (void); call_in_true_branch_not_eliminated_on_line_96(); } while (0); else (void)0; } while (0);
  do { char a[] = "1234"; memcpy (a + 2, "34", 2); if (!(strlen (a) == 4)) do { extern void call_in_true_branch_not_eliminated_on_line_97 (void); call_in_true_branch_not_eliminated_on_line_97(); } while (0); else (void)0; } while (0);
  do { char a[] = "1234"; memcpy (a + 2, "34", 3); if (!(strlen (a) == 4)) do { extern void call_in_true_branch_not_eliminated_on_line_98 (void); call_in_true_branch_not_eliminated_on_line_98(); } while (0); else (void)0; } while (0);

  do { char a[] = "12\00034"; memcpy (a + 0, "1", 1); if (!(strlen (a) == 2)) do { extern void call_in_true_branch_not_eliminated_on_line_100 (void); call_in_true_branch_not_eliminated_on_line_100(); } while (0); else (void)0; } while (0);
  do { char a[] = "12\00034"; memcpy (a + 0, "1", 2); if (!(strlen (a) == 1)) do { extern void call_in_true_branch_not_eliminated_on_line_101 (void); call_in_true_branch_not_eliminated_on_line_101(); } while (0); else (void)0; } while (0);

  do { char a[] = "AB\000CD"; memcpy (a + 0, "ab", 2); if (!(strlen (a) == 2)) do { extern void call_in_true_branch_not_eliminated_on_line_103 (void); call_in_true_branch_not_eliminated_on_line_103(); } while (0); else (void)0; } while (0);
  do { char a[] = "AB\000CD"; memcpy (a + 0, "ab", 3); if (!(strlen (a) == 2)) do { extern void call_in_true_branch_not_eliminated_on_line_104 (void); call_in_true_branch_not_eliminated_on_line_104(); } while (0); else (void)0; } while (0);
  do { char a[] = "AB\000CD"; memcpy (a + 0, "ab\000c", 4); if (!(strlen (a) == 2)) do { extern void call_in_true_branch_not_eliminated_on_line_105 (void); call_in_true_branch_not_eliminated_on_line_105(); } while (0); else (void)0; } while (0);
}
