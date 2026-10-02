//type: fp
//options: 
# 0 "./strlenopt-82.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-82.c"
# 10 "./strlenopt-82.c"
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
# 11 "./strlenopt-82.c" 2
# 46 "./strlenopt-82.c"
void elim_char_array_init_consecutive (void)
{
  char a[][10] = { "1", "12", "123", "1234", "12345", "12345" };

  if (!(strlen (a[0]) == 1)) do { extern void call_in_true_branch_not_eliminated_on_line_50 (void); call_in_true_branch_not_eliminated_on_line_50(); } while (0); else (void)0;
  if (!(strlen (a[1]) == 2)) do { extern void call_in_true_branch_not_eliminated_on_line_51 (void); call_in_true_branch_not_eliminated_on_line_51(); } while (0); else (void)0;
  if (!(strlen (a[2]) == 3)) do { extern void call_in_true_branch_not_eliminated_on_line_52 (void); call_in_true_branch_not_eliminated_on_line_52(); } while (0); else (void)0;
  if (!(strlen (a[3]) == 4)) do { extern void call_in_true_branch_not_eliminated_on_line_53 (void); call_in_true_branch_not_eliminated_on_line_53(); } while (0); else (void)0;
  if (!(strlen (a[4]) == 5)) do { extern void call_in_true_branch_not_eliminated_on_line_54 (void); call_in_true_branch_not_eliminated_on_line_54(); } while (0); else (void)0;
}

void elim_char_array_cpy_consecutive (void)
{
  char a[5][10];

  strcpy (a[0], "12345");
  strcpy (a[1], "1234");
  strcpy (a[2], "123");
  strcpy (a[3], "12");
  strcpy (a[4], "1");

  if (!(strlen (a[0]) == 5)) do { extern void call_in_true_branch_not_eliminated_on_line_67 (void); call_in_true_branch_not_eliminated_on_line_67(); } while (0); else (void)0;
  if (!(strlen (a[1]) == 4)) do { extern void call_in_true_branch_not_eliminated_on_line_68 (void); call_in_true_branch_not_eliminated_on_line_68(); } while (0); else (void)0;
  if (!(strlen (a[2]) == 3)) do { extern void call_in_true_branch_not_eliminated_on_line_69 (void); call_in_true_branch_not_eliminated_on_line_69(); } while (0); else (void)0;
  if (!(strlen (a[3]) == 2)) do { extern void call_in_true_branch_not_eliminated_on_line_70 (void); call_in_true_branch_not_eliminated_on_line_70(); } while (0); else (void)0;
  if (!(strlen (a[4]) == 1)) do { extern void call_in_true_branch_not_eliminated_on_line_71 (void); call_in_true_branch_not_eliminated_on_line_71(); } while (0); else (void)0;
}

void elim_clear_char_array_cpy_consecutive (void)
{
  char a[5][10] = { };

  strcpy (a[0], "12345");
  strcpy (a[1], "1234");
  strcpy (a[2], "123");
  strcpy (a[3], "12");
  strcpy (a[4], "1");

  if (!(strlen (a[0]) == 5)) do { extern void call_in_true_branch_not_eliminated_on_line_84 (void); call_in_true_branch_not_eliminated_on_line_84(); } while (0); else (void)0;
  if (!(strlen (a[1]) == 4)) do { extern void call_in_true_branch_not_eliminated_on_line_85 (void); call_in_true_branch_not_eliminated_on_line_85(); } while (0); else (void)0;
  if (!(strlen (a[2]) == 3)) do { extern void call_in_true_branch_not_eliminated_on_line_86 (void); call_in_true_branch_not_eliminated_on_line_86(); } while (0); else (void)0;
  if (!(strlen (a[3]) == 2)) do { extern void call_in_true_branch_not_eliminated_on_line_87 (void); call_in_true_branch_not_eliminated_on_line_87(); } while (0); else (void)0;
  if (!(strlen (a[4]) == 1)) do { extern void call_in_true_branch_not_eliminated_on_line_88 (void); call_in_true_branch_not_eliminated_on_line_88(); } while (0); else (void)0;
}

struct Consec
{
  char s1[sizeof "0123456789" "0123456789" "0123456789" "0123456789"];
  char s2[sizeof "0123456789" "0123456789" "0123456789" "0123456789"];
  const char *p1;
  const char *p2;
};

void elim_struct_init_consecutive (void)
{
  struct Consec a = { "0123456789", "0123456789", "0123456789", "0123456789" };

  if (!(strlen (a.s1) == sizeof "0123456789" - 1)) do { extern void call_in_true_branch_not_eliminated_on_line_103 (void); call_in_true_branch_not_eliminated_on_line_103(); } while (0); else (void)0;
  if (!(strlen (a.s2) == sizeof "0123456789" - 1)) do { extern void call_in_true_branch_not_eliminated_on_line_104 (void); call_in_true_branch_not_eliminated_on_line_104(); } while (0); else (void)0;
  if (!(strlen (a.p1) == sizeof "0123456789" - 1)) do { extern void call_in_true_branch_not_eliminated_on_line_105 (void); call_in_true_branch_not_eliminated_on_line_105(); } while (0); else (void)0;
  if (!(strlen (a.p2) == sizeof "0123456789" - 1)) do { extern void call_in_true_branch_not_eliminated_on_line_106 (void); call_in_true_branch_not_eliminated_on_line_106(); } while (0); else (void)0;
}

void elim_struct_array_init_consecutive (void)
{
  struct Consec a[2] = {
    { "0123456789", "0123456789" "0123456789", "0123456789" "0123456789" "0123456789", "0123456789" "0123456789" "0123456789" "0123456789" },
    { "0123456789" "0123456789" "0123456789" "0123456789", "0123456789" "0123456789" "0123456789", "0123456789" "0123456789", "0123456789" }
  };

  if (!(strlen (a[0].s1) == sizeof "0123456789" - 1)) do { extern void call_in_true_branch_not_eliminated_on_line_116 (void); call_in_true_branch_not_eliminated_on_line_116(); } while (0); else (void)0;
  if (!(strlen (a[0].s2) == sizeof "0123456789" "0123456789" - 1)) do { extern void call_in_true_branch_not_eliminated_on_line_117 (void); call_in_true_branch_not_eliminated_on_line_117(); } while (0); else (void)0;
  if (!(strlen (a[0].p1) == sizeof "0123456789" "0123456789" "0123456789" - 1)) do { extern void call_in_true_branch_not_eliminated_on_line_118 (void); call_in_true_branch_not_eliminated_on_line_118(); } while (0); else (void)0;
  if (!(strlen (a[0].p2) == sizeof "0123456789" "0123456789" "0123456789" "0123456789" - 1)) do { extern void call_in_true_branch_not_eliminated_on_line_119 (void); call_in_true_branch_not_eliminated_on_line_119(); } while (0); else (void)0;

  if (!(strlen (a[1].s1) == sizeof "0123456789" "0123456789" "0123456789" "0123456789" - 1)) do { extern void call_in_true_branch_not_eliminated_on_line_121 (void); call_in_true_branch_not_eliminated_on_line_121(); } while (0); else (void)0;
  if (!(strlen (a[1].s2) == sizeof "0123456789" "0123456789" "0123456789" - 1)) do { extern void call_in_true_branch_not_eliminated_on_line_122 (void); call_in_true_branch_not_eliminated_on_line_122(); } while (0); else (void)0;
  if (!(strlen (a[1].p1) == sizeof "0123456789" "0123456789" - 1)) do { extern void call_in_true_branch_not_eliminated_on_line_123 (void); call_in_true_branch_not_eliminated_on_line_123(); } while (0); else (void)0;
  if (!(strlen (a[1].p2) == sizeof "0123456789" - 1)) do { extern void call_in_true_branch_not_eliminated_on_line_124 (void); call_in_true_branch_not_eliminated_on_line_124(); } while (0); else (void)0;
}

struct NonConsec
{
  char s1[sizeof "0123456789" "0123456789" "0123456789" "0123456789"];
  int i1;
  char s2[sizeof "0123456789" "0123456789" "0123456789" "0123456789"];
  int i2;
  const char *p1;
  int i3;
  const char *p2;
  int i4;
};

void elim_struct_init_nonconsecutive (void)
{
  struct NonConsec b = { "0123456789", 123, "0123456789" "0123456789", 456, b.s1, 789, b.s2, 123 };

  if (!(strlen (b.s1) == sizeof "0123456789" - 1)) do { extern void call_in_true_branch_not_eliminated_on_line_143 (void); call_in_true_branch_not_eliminated_on_line_143(); } while (0); else (void)0;
  if (!(strlen (b.s2) == sizeof "0123456789" "0123456789" - 1)) do { extern void call_in_true_branch_not_eliminated_on_line_144 (void); call_in_true_branch_not_eliminated_on_line_144(); } while (0); else (void)0;
  if (!(strlen (b.p1) == sizeof "0123456789" - 1)) do { extern void call_in_true_branch_not_eliminated_on_line_145 (void); call_in_true_branch_not_eliminated_on_line_145(); } while (0); else (void)0;
  if (!(strlen (b.p2) == sizeof "0123456789" "0123456789" - 1)) do { extern void call_in_true_branch_not_eliminated_on_line_146 (void); call_in_true_branch_not_eliminated_on_line_146(); } while (0); else (void)0;
}

void elim_struct_assign_tmp_nonconsecutive (void)
{
  struct NonConsec b = { "a", 1, "b", 2, "c", 3, "d", 4 };

  b = (struct NonConsec){ "0123456789", 123, "0123456789" "0123456789", 456, "0123456789" "0123456789" "0123456789", 789, "0123456789" "0123456789" "0123456789" "0123456789", 123 };

  if (!(strlen (b.s1) == sizeof "0123456789" - 1)) do { extern void call_in_true_branch_not_eliminated_on_line_155 (void); call_in_true_branch_not_eliminated_on_line_155(); } while (0); else (void)0;
  if (!(strlen (b.s2) == sizeof "0123456789" "0123456789" - 1)) do { extern void call_in_true_branch_not_eliminated_on_line_156 (void); call_in_true_branch_not_eliminated_on_line_156(); } while (0); else (void)0;
  if (!(strlen (b.p1) == sizeof "0123456789" "0123456789" "0123456789" - 1)) do { extern void call_in_true_branch_not_eliminated_on_line_157 (void); call_in_true_branch_not_eliminated_on_line_157(); } while (0); else (void)0;
  if (!(strlen (b.p2) == sizeof "0123456789" "0123456789" "0123456789" "0123456789" - 1)) do { extern void call_in_true_branch_not_eliminated_on_line_158 (void); call_in_true_branch_not_eliminated_on_line_158(); } while (0); else (void)0;
}

const struct NonConsec bcst = {
  "0123456789" "0123456789" "0123456789" "0123456789", -1, "0123456789" "0123456789" "0123456789", -2, "0123456789" "0123456789", -3, "0123456789", -4
};

void elim_struct_assign_cst_nonconsecutive (void)
{
  struct NonConsec b = { "a", 1, "b", 2, "c", 3, "d" };

  b = bcst;

  if (!(strlen (b.s1) == sizeof "0123456789" "0123456789" "0123456789" "0123456789" - 1)) do { extern void call_in_true_branch_not_eliminated_on_line_171 (void); call_in_true_branch_not_eliminated_on_line_171(); } while (0); else (void)0;
  if (!(strlen (b.s2) == sizeof "0123456789" "0123456789" "0123456789" - 1)) do { extern void call_in_true_branch_not_eliminated_on_line_172 (void); call_in_true_branch_not_eliminated_on_line_172(); } while (0); else (void)0;
  if (!(strlen (b.p1) == sizeof "0123456789" "0123456789" - 1)) do { extern void call_in_true_branch_not_eliminated_on_line_173 (void); call_in_true_branch_not_eliminated_on_line_173(); } while (0); else (void)0;
  if (!(strlen (b.p2) == sizeof "0123456789" - 1)) do { extern void call_in_true_branch_not_eliminated_on_line_174 (void); call_in_true_branch_not_eliminated_on_line_174(); } while (0); else (void)0;
}

void elim_struct_copy_cst_nonconsecutive (void)
{
  struct NonConsec b = { "a", 1, "b", 2, "c", 3, "d" };
  memcpy (&b, &bcst, sizeof b);



  if (!(strlen (b.p1) == sizeof "0123456789" "0123456789" - 1)) do { extern void call_in_true_branch_not_eliminated_on_line_184 (void); call_in_true_branch_not_eliminated_on_line_184(); } while (0); else (void)0;
  if (!(strlen (b.p2) == sizeof "0123456789" - 1)) do { extern void call_in_true_branch_not_eliminated_on_line_185 (void); call_in_true_branch_not_eliminated_on_line_185(); } while (0); else (void)0;
}
# 1000 "./strlenopt-82.c"

int sink (void*);

void keep_init_nonconsecutive (void)
{
  struct NonConsec b = {
    "0123456789", 123, "0123456789" "0123456789", 456, b.s1, 789, b.s2,
    sink (&b)
  };

  if (strlen (b.s1) == sizeof "0123456789" - 1) do { extern void call_made_in_true_branch_on_line_1010 (void); call_made_in_true_branch_on_line_1010(); } while (0); else do { extern void call_made_in_false_branch_on_line_1010 (void); call_made_in_false_branch_on_line_1010(); } while (0);
  if (strlen (b.s2) == sizeof "0123456789" - 1) do { extern void call_made_in_true_branch_on_line_1011 (void); call_made_in_true_branch_on_line_1011(); } while (0); else do { extern void call_made_in_false_branch_on_line_1011 (void); call_made_in_false_branch_on_line_1011(); } while (0);
  if (strlen (b.p1) == sizeof "0123456789" - 1) do { extern void call_made_in_true_branch_on_line_1012 (void); call_made_in_true_branch_on_line_1012(); } while (0); else do { extern void call_made_in_false_branch_on_line_1012 (void); call_made_in_false_branch_on_line_1012(); } while (0);
  if (strlen (b.p2) == sizeof "0123456789" - 1) do { extern void call_made_in_true_branch_on_line_1013 (void); call_made_in_true_branch_on_line_1013(); } while (0); else do { extern void call_made_in_false_branch_on_line_1013 (void); call_made_in_false_branch_on_line_1013(); } while (0);
}

void keep_assign_tmp_nonconsecutive (void)
{
  struct NonConsec b = { "a", 1, "b", 2, "c", 3, "d", 4 };

  b = (struct NonConsec){
    "0123456789", 123, "0123456789" "0123456789", 456, "0123456789" "0123456789" "0123456789", 789, "0123456789" "0123456789" "0123456789" "0123456789",
    sink (&b)
  };

  if (strlen (b.s1) == sizeof "0123456789" - 1) do { extern void call_made_in_true_branch_on_line_1025 (void); call_made_in_true_branch_on_line_1025(); } while (0); else do { extern void call_made_in_false_branch_on_line_1025 (void); call_made_in_false_branch_on_line_1025(); } while (0);
  if (strlen (b.s2) == sizeof "0123456789" "0123456789" - 1) do { extern void call_made_in_true_branch_on_line_1026 (void); call_made_in_true_branch_on_line_1026(); } while (0); else do { extern void call_made_in_false_branch_on_line_1026 (void); call_made_in_false_branch_on_line_1026(); } while (0);
  if (strlen (b.p1) == sizeof "0123456789" "0123456789" "0123456789" - 1) do { extern void call_made_in_true_branch_on_line_1027 (void); call_made_in_true_branch_on_line_1027(); } while (0); else do { extern void call_made_in_false_branch_on_line_1027 (void); call_made_in_false_branch_on_line_1027(); } while (0);
  if (strlen (b.p2) == sizeof "0123456789" "0123456789" "0123456789" "0123456789" - 1) do { extern void call_made_in_true_branch_on_line_1028 (void); call_made_in_true_branch_on_line_1028(); } while (0); else do { extern void call_made_in_false_branch_on_line_1028 (void); call_made_in_false_branch_on_line_1028(); } while (0);
}
