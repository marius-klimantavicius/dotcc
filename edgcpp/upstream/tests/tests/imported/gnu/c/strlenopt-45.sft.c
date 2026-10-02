//type: fp
//options: 
# 0 "./strlenopt-45.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-45.c"






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
# 8 "./strlenopt-45.c" 2




typedef long unsigned int size_t;

extern void abort (void);
extern size_t strnlen (const char *, size_t);
# 46 "./strlenopt-45.c"
extern char c;
extern char a1[1];
extern char a3[3];
extern char a5[5];
extern char a3_7[3][7];
extern char ax[];

void elim_strnlen_arr_cst (void)
{




  if (!(strnlen (&c, 0) == 0)) do { extern void call_in_true_branch_not_eliminated_on_line_59 (void); call_in_true_branch_not_eliminated_on_line_59(); } while (0); else (void)0;
  if (!(strnlen (&c, 1) < 2)) do { extern void call_in_true_branch_not_eliminated_on_line_60 (void); call_in_true_branch_not_eliminated_on_line_60(); } while (0); else (void)0;
  if (!(strnlen (&c, 2) == 0)) do { extern void call_in_true_branch_not_eliminated_on_line_61 (void); call_in_true_branch_not_eliminated_on_line_61(); } while (0); else (void)0;
  if (!(strnlen (&c, 9) == 0)) do { extern void call_in_true_branch_not_eliminated_on_line_62 (void); call_in_true_branch_not_eliminated_on_line_62(); } while (0); else (void)0;
  if (!(strnlen (&c, 0x7fffffffffffffffL) == 0)) do { extern void call_in_true_branch_not_eliminated_on_line_63 (void); call_in_true_branch_not_eliminated_on_line_63(); } while (0); else (void)0;
  if (!(strnlen (&c, 0xffffffffffffffffUL) == 0)) do { extern void call_in_true_branch_not_eliminated_on_line_64 (void); call_in_true_branch_not_eliminated_on_line_64(); } while (0); else (void)0;
  if (!(strnlen (&c, -1) == 0)) do { extern void call_in_true_branch_not_eliminated_on_line_65 (void); call_in_true_branch_not_eliminated_on_line_65(); } while (0); else (void)0;

  if (!(strnlen (a1, 0) == 0)) do { extern void call_in_true_branch_not_eliminated_on_line_67 (void); call_in_true_branch_not_eliminated_on_line_67(); } while (0); else (void)0;
  if (!(strnlen (a1, 1) < 2)) do { extern void call_in_true_branch_not_eliminated_on_line_68 (void); call_in_true_branch_not_eliminated_on_line_68(); } while (0); else (void)0;
  if (!(strnlen (a1, 2) == 0)) do { extern void call_in_true_branch_not_eliminated_on_line_69 (void); call_in_true_branch_not_eliminated_on_line_69(); } while (0); else (void)0;
  if (!(strnlen (a1, 9) == 0)) do { extern void call_in_true_branch_not_eliminated_on_line_70 (void); call_in_true_branch_not_eliminated_on_line_70(); } while (0); else (void)0;
  if (!(strnlen (a1, 0x7fffffffffffffffL) == 0)) do { extern void call_in_true_branch_not_eliminated_on_line_71 (void); call_in_true_branch_not_eliminated_on_line_71(); } while (0); else (void)0;
  if (!(strnlen (a1, 0xffffffffffffffffUL) == 0)) do { extern void call_in_true_branch_not_eliminated_on_line_72 (void); call_in_true_branch_not_eliminated_on_line_72(); } while (0); else (void)0;
  if (!(strnlen (a1, -1) == 0)) do { extern void call_in_true_branch_not_eliminated_on_line_73 (void); call_in_true_branch_not_eliminated_on_line_73(); } while (0); else (void)0;

  if (!(strnlen (a3, 0) == 0)) do { extern void call_in_true_branch_not_eliminated_on_line_75 (void); call_in_true_branch_not_eliminated_on_line_75(); } while (0); else (void)0;
  if (!(strnlen (a3, 1) < 2)) do { extern void call_in_true_branch_not_eliminated_on_line_76 (void); call_in_true_branch_not_eliminated_on_line_76(); } while (0); else (void)0;
  if (!(strnlen (a3, 2) < 3)) do { extern void call_in_true_branch_not_eliminated_on_line_77 (void); call_in_true_branch_not_eliminated_on_line_77(); } while (0); else (void)0;
  if (!(strnlen (a3, 3) < 4)) do { extern void call_in_true_branch_not_eliminated_on_line_78 (void); call_in_true_branch_not_eliminated_on_line_78(); } while (0); else (void)0;
  if (!(strnlen (a3, 9) < 4)) do { extern void call_in_true_branch_not_eliminated_on_line_79 (void); call_in_true_branch_not_eliminated_on_line_79(); } while (0); else (void)0;
  if (!(strnlen (a3, 0x7fffffffffffffffL) < 4)) do { extern void call_in_true_branch_not_eliminated_on_line_80 (void); call_in_true_branch_not_eliminated_on_line_80(); } while (0); else (void)0;
  if (!(strnlen (a3, 0xffffffffffffffffUL) < 4)) do { extern void call_in_true_branch_not_eliminated_on_line_81 (void); call_in_true_branch_not_eliminated_on_line_81(); } while (0); else (void)0;
  if (!(strnlen (a3, -1) < 4)) do { extern void call_in_true_branch_not_eliminated_on_line_82 (void); call_in_true_branch_not_eliminated_on_line_82(); } while (0); else (void)0;

  if (!(strnlen (a3_7[0], 0) == 0)) do { extern void call_in_true_branch_not_eliminated_on_line_84 (void); call_in_true_branch_not_eliminated_on_line_84(); } while (0); else (void)0;
  if (!(strnlen (a3_7[0], 1) < 2)) do { extern void call_in_true_branch_not_eliminated_on_line_85 (void); call_in_true_branch_not_eliminated_on_line_85(); } while (0); else (void)0;
  if (!(strnlen (a3_7[0], 2) < 3)) do { extern void call_in_true_branch_not_eliminated_on_line_86 (void); call_in_true_branch_not_eliminated_on_line_86(); } while (0); else (void)0;
  if (!(strnlen (a3_7[0], 3) < 4)) do { extern void call_in_true_branch_not_eliminated_on_line_87 (void); call_in_true_branch_not_eliminated_on_line_87(); } while (0); else (void)0;
  if (!(strnlen (a3_7[0], 9) <= 9)) do { extern void call_in_true_branch_not_eliminated_on_line_88 (void); call_in_true_branch_not_eliminated_on_line_88(); } while (0); else (void)0;
  if (!(strnlen (a3_7[0], 0x7fffffffffffffffL) <= sizeof a3_7)) do { extern void call_in_true_branch_not_eliminated_on_line_89 (void); call_in_true_branch_not_eliminated_on_line_89(); } while (0); else (void)0;
  if (!(strnlen (a3_7[0], 0xffffffffffffffffUL) <= sizeof a3_7)) do { extern void call_in_true_branch_not_eliminated_on_line_90 (void); call_in_true_branch_not_eliminated_on_line_90(); } while (0); else (void)0;
  if (!(strnlen (a3_7[0], -1) <= sizeof a3_7)) do { extern void call_in_true_branch_not_eliminated_on_line_91 (void); call_in_true_branch_not_eliminated_on_line_91(); } while (0); else (void)0;

  if (!(strnlen (a3_7[2], 0) == 0)) do { extern void call_in_true_branch_not_eliminated_on_line_93 (void); call_in_true_branch_not_eliminated_on_line_93(); } while (0); else (void)0;
  if (!(strnlen (a3_7[2], 1) < 2)) do { extern void call_in_true_branch_not_eliminated_on_line_94 (void); call_in_true_branch_not_eliminated_on_line_94(); } while (0); else (void)0;
  if (!(strnlen (a3_7[2], 2) < 3)) do { extern void call_in_true_branch_not_eliminated_on_line_95 (void); call_in_true_branch_not_eliminated_on_line_95(); } while (0); else (void)0;
  if (!(strnlen (a3_7[2], 3) < 4)) do { extern void call_in_true_branch_not_eliminated_on_line_96 (void); call_in_true_branch_not_eliminated_on_line_96(); } while (0); else (void)0;
  if (!(strnlen (a3_7[2], 9) <= 9)) do { extern void call_in_true_branch_not_eliminated_on_line_97 (void); call_in_true_branch_not_eliminated_on_line_97(); } while (0); else (void)0;
  if (!(strnlen (a3_7[2], 0x7fffffffffffffffL) < sizeof a3_7)) do { extern void call_in_true_branch_not_eliminated_on_line_98 (void); call_in_true_branch_not_eliminated_on_line_98(); } while (0); else (void)0;
  if (!(strnlen (a3_7[2], 0xffffffffffffffffUL) < sizeof a3_7)) do { extern void call_in_true_branch_not_eliminated_on_line_99 (void); call_in_true_branch_not_eliminated_on_line_99(); } while (0); else (void)0;
  if (!(strnlen (a3_7[2], -1) < sizeof a3_7)) do { extern void call_in_true_branch_not_eliminated_on_line_100 (void); call_in_true_branch_not_eliminated_on_line_100(); } while (0); else (void)0;

  if (!(strnlen ((char*)a3_7, 0) == 0)) do { extern void call_in_true_branch_not_eliminated_on_line_102 (void); call_in_true_branch_not_eliminated_on_line_102(); } while (0); else (void)0;
  if (!(strnlen ((char*)a3_7, 1) < 2)) do { extern void call_in_true_branch_not_eliminated_on_line_103 (void); call_in_true_branch_not_eliminated_on_line_103(); } while (0); else (void)0;
  if (!(strnlen ((char*)a3_7, 2) < 3)) do { extern void call_in_true_branch_not_eliminated_on_line_104 (void); call_in_true_branch_not_eliminated_on_line_104(); } while (0); else (void)0;
  if (!(strnlen ((char*)a3_7, 3) < 4)) do { extern void call_in_true_branch_not_eliminated_on_line_105 (void); call_in_true_branch_not_eliminated_on_line_105(); } while (0); else (void)0;
  if (!(strnlen ((char*)a3_7, 9) < 10)) do { extern void call_in_true_branch_not_eliminated_on_line_106 (void); call_in_true_branch_not_eliminated_on_line_106(); } while (0); else (void)0;
  if (!(strnlen ((char*)a3_7, 19) < 20)) do { extern void call_in_true_branch_not_eliminated_on_line_107 (void); call_in_true_branch_not_eliminated_on_line_107(); } while (0); else (void)0;
  if (!(strnlen ((char*)a3_7, 21) <= sizeof a3_7)) do { extern void call_in_true_branch_not_eliminated_on_line_108 (void); call_in_true_branch_not_eliminated_on_line_108(); } while (0); else (void)0;
  if (!(strnlen ((char*)a3_7, 23) <= sizeof a3_7)) do { extern void call_in_true_branch_not_eliminated_on_line_109 (void); call_in_true_branch_not_eliminated_on_line_109(); } while (0); else (void)0;
  if (!(strnlen ((char*)a3_7, 0x7fffffffffffffffL) <= sizeof a3_7)) do { extern void call_in_true_branch_not_eliminated_on_line_110 (void); call_in_true_branch_not_eliminated_on_line_110(); } while (0); else (void)0;
  if (!(strnlen ((char*)a3_7, 0xffffffffffffffffUL) <= sizeof a3_7)) do { extern void call_in_true_branch_not_eliminated_on_line_111 (void); call_in_true_branch_not_eliminated_on_line_111(); } while (0); else (void)0;
  if (!(strnlen ((char*)a3_7, -1) <= sizeof a3_7)) do { extern void call_in_true_branch_not_eliminated_on_line_112 (void); call_in_true_branch_not_eliminated_on_line_112(); } while (0); else (void)0;

  if (!(strnlen (ax, 0) == 0)) do { extern void call_in_true_branch_not_eliminated_on_line_114 (void); call_in_true_branch_not_eliminated_on_line_114(); } while (0); else (void)0;
  if (!(strnlen (ax, 1) < 2)) do { extern void call_in_true_branch_not_eliminated_on_line_115 (void); call_in_true_branch_not_eliminated_on_line_115(); } while (0); else (void)0;
  if (!(strnlen (ax, 2) < 3)) do { extern void call_in_true_branch_not_eliminated_on_line_116 (void); call_in_true_branch_not_eliminated_on_line_116(); } while (0); else (void)0;
  if (!(strnlen (ax, 9) < 10)) do { extern void call_in_true_branch_not_eliminated_on_line_117 (void); call_in_true_branch_not_eliminated_on_line_117(); } while (0); else (void)0;
  if (!(strnlen (ax, 0x7fffffffffffffffL) < 0x7fffffffffffffffL)) do { extern void call_in_true_branch_not_eliminated_on_line_118 (void); call_in_true_branch_not_eliminated_on_line_118(); } while (0); else (void)0;
  if (!(strnlen (ax, 0xffffffffffffffffUL) < 0x7fffffffffffffffL)) do { extern void call_in_true_branch_not_eliminated_on_line_119 (void); call_in_true_branch_not_eliminated_on_line_119(); } while (0); else (void)0;
  if (!(strnlen (ax, -1) < 0x7fffffffffffffffL)) do { extern void call_in_true_branch_not_eliminated_on_line_120 (void); call_in_true_branch_not_eliminated_on_line_120(); } while (0); else (void)0;
}


void elim_strnlen_str_cst (void)
{
  const char *s0 = "";
  const char *s1 = "1";
  const char *s3 = "123";

  if (!(strnlen (s0, 0) == 0)) do { extern void call_in_true_branch_not_eliminated_on_line_130 (void); call_in_true_branch_not_eliminated_on_line_130(); } while (0); else (void)0;
  if (!(strnlen (s0, 1) == 0)) do { extern void call_in_true_branch_not_eliminated_on_line_131 (void); call_in_true_branch_not_eliminated_on_line_131(); } while (0); else (void)0;
  if (!(strnlen (s0, 9) == 0)) do { extern void call_in_true_branch_not_eliminated_on_line_132 (void); call_in_true_branch_not_eliminated_on_line_132(); } while (0); else (void)0;
  if (!(strnlen (s0, 0x7fffffffffffffffL) == 0)) do { extern void call_in_true_branch_not_eliminated_on_line_133 (void); call_in_true_branch_not_eliminated_on_line_133(); } while (0); else (void)0;
  if (!(strnlen (s0, 0xffffffffffffffffUL) == 0)) do { extern void call_in_true_branch_not_eliminated_on_line_134 (void); call_in_true_branch_not_eliminated_on_line_134(); } while (0); else (void)0;
  if (!(strnlen (s0, -1) == 0)) do { extern void call_in_true_branch_not_eliminated_on_line_135 (void); call_in_true_branch_not_eliminated_on_line_135(); } while (0); else (void)0;

  if (!(strnlen (s1, 0) == 0)) do { extern void call_in_true_branch_not_eliminated_on_line_137 (void); call_in_true_branch_not_eliminated_on_line_137(); } while (0); else (void)0;
  if (!(strnlen (s1, 1) == 1)) do { extern void call_in_true_branch_not_eliminated_on_line_138 (void); call_in_true_branch_not_eliminated_on_line_138(); } while (0); else (void)0;
  if (!(strnlen (s1, 9) == 1)) do { extern void call_in_true_branch_not_eliminated_on_line_139 (void); call_in_true_branch_not_eliminated_on_line_139(); } while (0); else (void)0;
  if (!(strnlen (s1, 0x7fffffffffffffffL) == 1)) do { extern void call_in_true_branch_not_eliminated_on_line_140 (void); call_in_true_branch_not_eliminated_on_line_140(); } while (0); else (void)0;
  if (!(strnlen (s1, 0xffffffffffffffffUL) == 1)) do { extern void call_in_true_branch_not_eliminated_on_line_141 (void); call_in_true_branch_not_eliminated_on_line_141(); } while (0); else (void)0;
  if (!(strnlen (s1, -2) == 1)) do { extern void call_in_true_branch_not_eliminated_on_line_142 (void); call_in_true_branch_not_eliminated_on_line_142(); } while (0); else (void)0;

  if (!(strnlen (s3, 0) == 0)) do { extern void call_in_true_branch_not_eliminated_on_line_144 (void); call_in_true_branch_not_eliminated_on_line_144(); } while (0); else (void)0;
  if (!(strnlen (s3, 1) == 1)) do { extern void call_in_true_branch_not_eliminated_on_line_145 (void); call_in_true_branch_not_eliminated_on_line_145(); } while (0); else (void)0;
  if (!(strnlen (s3, 2) == 2)) do { extern void call_in_true_branch_not_eliminated_on_line_146 (void); call_in_true_branch_not_eliminated_on_line_146(); } while (0); else (void)0;
  if (!(strnlen (s3, 3) == 3)) do { extern void call_in_true_branch_not_eliminated_on_line_147 (void); call_in_true_branch_not_eliminated_on_line_147(); } while (0); else (void)0;
  if (!(strnlen (s3, 9) == 3)) do { extern void call_in_true_branch_not_eliminated_on_line_148 (void); call_in_true_branch_not_eliminated_on_line_148(); } while (0); else (void)0;
  if (!(strnlen (s3, 0x7fffffffffffffffL) == 3)) do { extern void call_in_true_branch_not_eliminated_on_line_149 (void); call_in_true_branch_not_eliminated_on_line_149(); } while (0); else (void)0;
  if (!(strnlen (s3, 0xffffffffffffffffUL) == 3)) do { extern void call_in_true_branch_not_eliminated_on_line_150 (void); call_in_true_branch_not_eliminated_on_line_150(); } while (0); else (void)0;
  if (!(strnlen (s3, -2) == 3)) do { extern void call_in_true_branch_not_eliminated_on_line_151 (void); call_in_true_branch_not_eliminated_on_line_151(); } while (0); else (void)0;
}

void elim_strnlen_range (char *s)
{
  const char *s0 = "";
  const char *s1 = "1";
  const char *s3 = "123";

  size_t n_0_1 = (size_t)s & 1;
  size_t n_0_2 = ((size_t)s & 3) < 3 ? ((size_t)s & 3) : 2;
  size_t n_0_3 = (size_t)s & 3;
  size_t n_1_2 = n_0_1 + 1;

  if (!(strnlen (s0, n_0_1) == 0)) do { extern void call_in_true_branch_not_eliminated_on_line_165 (void); call_in_true_branch_not_eliminated_on_line_165(); } while (0); else (void)0;
  if (!(strnlen (s0, n_0_2) == 0)) do { extern void call_in_true_branch_not_eliminated_on_line_166 (void); call_in_true_branch_not_eliminated_on_line_166(); } while (0); else (void)0;
  if (!(strnlen (s0, n_1_2) == 0)) do { extern void call_in_true_branch_not_eliminated_on_line_167 (void); call_in_true_branch_not_eliminated_on_line_167(); } while (0); else (void)0;

  if (!(strnlen (s1, n_0_1) < 2)) do { extern void call_in_true_branch_not_eliminated_on_line_169 (void); call_in_true_branch_not_eliminated_on_line_169(); } while (0); else (void)0;
  if (!(strnlen (s1, n_0_2) < 2)) do { extern void call_in_true_branch_not_eliminated_on_line_170 (void); call_in_true_branch_not_eliminated_on_line_170(); } while (0); else (void)0;
  if (!(strnlen (s1, n_0_3) < 2)) do { extern void call_in_true_branch_not_eliminated_on_line_171 (void); call_in_true_branch_not_eliminated_on_line_171(); } while (0); else (void)0;

  if (!(strnlen (s1, n_1_2) > 0)) do { extern void call_in_true_branch_not_eliminated_on_line_173 (void); call_in_true_branch_not_eliminated_on_line_173(); } while (0); else (void)0;
  if (!(strnlen (s1, n_1_2) < 2)) do { extern void call_in_true_branch_not_eliminated_on_line_174 (void); call_in_true_branch_not_eliminated_on_line_174(); } while (0); else (void)0;

  if (!(strnlen (s3, n_0_1) < 2)) do { extern void call_in_true_branch_not_eliminated_on_line_176 (void); call_in_true_branch_not_eliminated_on_line_176(); } while (0); else (void)0;
  if (!(strnlen (s3, n_0_2) < 3)) do { extern void call_in_true_branch_not_eliminated_on_line_177 (void); call_in_true_branch_not_eliminated_on_line_177(); } while (0); else (void)0;
  if (!(strnlen (s3, n_0_3) < 4)) do { extern void call_in_true_branch_not_eliminated_on_line_178 (void); call_in_true_branch_not_eliminated_on_line_178(); } while (0); else (void)0;

  if (!(strnlen (s3, n_1_2) > 0)) do { extern void call_in_true_branch_not_eliminated_on_line_180 (void); call_in_true_branch_not_eliminated_on_line_180(); } while (0); else (void)0;
  if (!(strnlen (s3, n_1_2) < 4)) do { extern void call_in_true_branch_not_eliminated_on_line_181 (void); call_in_true_branch_not_eliminated_on_line_181(); } while (0); else (void)0;
}
# 1000 "./strlenopt-45.c"

void keep_strnlen_arr_cst (void)
{
  if (strnlen (&c, 1) == 0) do { extern void call_made_in_true_branch_on_line_1003 (void); call_made_in_true_branch_on_line_1003(); } while (0); else do { extern void call_made_in_false_branch_on_line_1003 (void); call_made_in_false_branch_on_line_1003(); } while (0);
  if (strnlen (&c, 1) == 1) do { extern void call_made_in_true_branch_on_line_1004 (void); call_made_in_true_branch_on_line_1004(); } while (0); else do { extern void call_made_in_false_branch_on_line_1004 (void); call_made_in_false_branch_on_line_1004(); } while (0);

  if (strnlen (a1, 1) == 0) do { extern void call_made_in_true_branch_on_line_1006 (void); call_made_in_true_branch_on_line_1006(); } while (0); else do { extern void call_made_in_false_branch_on_line_1006 (void); call_made_in_false_branch_on_line_1006(); } while (0);
  if (strnlen (a1, 1) == 1) do { extern void call_made_in_true_branch_on_line_1007 (void); call_made_in_true_branch_on_line_1007(); } while (0); else do { extern void call_made_in_false_branch_on_line_1007 (void); call_made_in_false_branch_on_line_1007(); } while (0);

  if (strnlen (ax, 9) < 9) do { extern void call_made_in_true_branch_on_line_1009 (void); call_made_in_true_branch_on_line_1009(); } while (0); else do { extern void call_made_in_false_branch_on_line_1009 (void); call_made_in_false_branch_on_line_1009(); } while (0);
}

struct FlexArrays
{
  char c;
  char a0[0];
  char a1[1];
};

void keep_strnlen_memarr_cst (struct FlexArrays *p)
{
  if (strnlen (&p->c, 1) == 0) do { extern void call_made_in_true_branch_on_line_1021 (void); call_made_in_true_branch_on_line_1021(); } while (0); else do { extern void call_made_in_false_branch_on_line_1021 (void); call_made_in_false_branch_on_line_1021(); } while (0);
  if (strnlen (&p->c, 1) == 1) do { extern void call_made_in_true_branch_on_line_1022 (void); call_made_in_true_branch_on_line_1022(); } while (0); else do { extern void call_made_in_false_branch_on_line_1022 (void); call_made_in_false_branch_on_line_1022(); } while (0);
# 1032 "./strlenopt-45.c"
  if (strnlen (p->a1, 1) == 0) do { extern void call_made_in_true_branch_on_line_1032 (void); call_made_in_true_branch_on_line_1032(); } while (0); else do { extern void call_made_in_false_branch_on_line_1032 (void); call_made_in_false_branch_on_line_1032(); } while (0);
  if (strnlen (p->a1, 1) == 1) do { extern void call_made_in_true_branch_on_line_1033 (void); call_made_in_true_branch_on_line_1033(); } while (0); else do { extern void call_made_in_false_branch_on_line_1033 (void); call_made_in_false_branch_on_line_1033(); } while (0);

  if (strnlen (p->a1, 2) == 0) do { extern void call_made_in_true_branch_on_line_1035 (void); call_made_in_true_branch_on_line_1035(); } while (0); else do { extern void call_made_in_false_branch_on_line_1035 (void); call_made_in_false_branch_on_line_1035(); } while (0);
  if (strnlen (p->a1, 2) == 1) do { extern void call_made_in_true_branch_on_line_1036 (void); call_made_in_true_branch_on_line_1036(); } while (0); else do { extern void call_made_in_false_branch_on_line_1036 (void); call_made_in_false_branch_on_line_1036(); } while (0);
  if (strnlen (p->a1, 2) == 2) do { extern void call_made_in_true_branch_on_line_1037 (void); call_made_in_true_branch_on_line_1037(); } while (0); else do { extern void call_made_in_false_branch_on_line_1037 (void); call_made_in_false_branch_on_line_1037(); } while (0);

  if (strnlen (p->a1, 9) < 9) do { extern void call_made_in_true_branch_on_line_1039 (void); call_made_in_true_branch_on_line_1039(); } while (0); else do { extern void call_made_in_false_branch_on_line_1039 (void); call_made_in_false_branch_on_line_1039(); } while (0);
}
