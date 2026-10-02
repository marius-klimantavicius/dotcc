//type: fp
//options: 
# 0 "./strlenopt-40.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-40.c"





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
# 7 "./strlenopt-40.c" 2
# 42 "./strlenopt-40.c"
typedef char A3[3], A5[5], A7[7], AX[];

typedef A3 A7_3[7];
typedef A3 AX_3[];
typedef A5 A7_5[7];
typedef A7 A5_7[5];

extern A7_3 a7_3;
extern A5_7 a5_7;
extern AX_3 ax_3;

extern A3 a3;
extern A7 a5;
extern A7 a7;
extern AX ax;

extern A3 *pa3;
extern A5 *pa5;
extern A7 *pa7;

extern A7_3 *pa7_3;
extern AX_3 *pax_3;
extern A5_7 *pa5_7;
extern A7_5 *pa7_5;

extern char *ptr;

struct MemArrays0 {
  A7_3 a7_3;
  A5_7 a5_7;
  char a3[3], a5[5], a0[0];
};
struct MemArraysX { char a3[3], a5[5], ax[]; };
struct MemArrays7 { char a3[3], a5[5], a7[7]; };

struct MemArrays0 ma0_3_5_7[3][5][7];

void elim_strings (int i)
{
  if (!(strlen (i < 0 ? "123" : "321") == 3)) do { extern void call_in_true_branch_not_eliminated_on_line_81 (void); call_in_true_branch_not_eliminated_on_line_81(); } while (0); else (void)0;
  if (!!(strlen (i < 0 ? "123" : "321") > 3)) do { extern void call_in_false_branch_not_eliminated_on_line_82 (void); call_in_false_branch_not_eliminated_on_line_82(); } while (0); else (void)0;
  if (!!(strlen (i < 0 ? "123" : "321") < 3)) do { extern void call_in_false_branch_not_eliminated_on_line_83 (void); call_in_false_branch_not_eliminated_on_line_83(); } while (0); else (void)0;

  if (!(strlen (i < 0 ? "123" : "4321") >= 3)) do { extern void call_in_true_branch_not_eliminated_on_line_85 (void); call_in_true_branch_not_eliminated_on_line_85(); } while (0); else (void)0;
  if (!!(strlen (i < 0 ? "123" : "4321") > 4)) do { extern void call_in_false_branch_not_eliminated_on_line_86 (void); call_in_false_branch_not_eliminated_on_line_86(); } while (0); else (void)0;
  if (!!(strlen (i < 0 ? "123" : "4321") < 3)) do { extern void call_in_false_branch_not_eliminated_on_line_87 (void); call_in_false_branch_not_eliminated_on_line_87(); } while (0); else (void)0;

  if (!(strlen (i < 0 ? "1234" : "321") >= 3)) do { extern void call_in_true_branch_not_eliminated_on_line_89 (void); call_in_true_branch_not_eliminated_on_line_89(); } while (0); else (void)0;
  if (!!(strlen (i < 0 ? "1234" : "321") < 3)) do { extern void call_in_false_branch_not_eliminated_on_line_90 (void); call_in_false_branch_not_eliminated_on_line_90(); } while (0); else (void)0;
  if (!!(strlen (i < 0 ? "1234" : "321") > 4)) do { extern void call_in_false_branch_not_eliminated_on_line_91 (void); call_in_false_branch_not_eliminated_on_line_91(); } while (0); else (void)0;

  if (!(strlen (i < 0 ? "123" : "4321") <= 4)) do { extern void call_in_true_branch_not_eliminated_on_line_93 (void); call_in_true_branch_not_eliminated_on_line_93(); } while (0); else (void)0;
  if (!(strlen (i < 0 ? "1234" : "321") <= 4)) do { extern void call_in_true_branch_not_eliminated_on_line_94 (void); call_in_true_branch_not_eliminated_on_line_94(); } while (0); else (void)0;

  if (!(strlen (i < 0 ? "1" : "123456789") <= 9)) do { extern void call_in_true_branch_not_eliminated_on_line_96 (void); call_in_true_branch_not_eliminated_on_line_96(); } while (0); else (void)0;
  if (!(strlen (i < 0 ? "1" : "123456789") >= 1)) do { extern void call_in_true_branch_not_eliminated_on_line_97 (void); call_in_true_branch_not_eliminated_on_line_97(); } while (0); else (void)0;
}




void elim_global_arrays (int i)
{



  if (!(strlen (a7_3[0]) < sizeof a7_3)) do { extern void call_in_true_branch_not_eliminated_on_line_108 (void); call_in_true_branch_not_eliminated_on_line_108(); } while (0); else (void)0;
  if (!(strlen (a7_3[1]) < sizeof a7_3 - sizeof *a7_3)) do { extern void call_in_true_branch_not_eliminated_on_line_109 (void); call_in_true_branch_not_eliminated_on_line_109(); } while (0); else (void)0;
  if (!(strlen (a7_3[6]) < sizeof a7_3 - 5 * sizeof *a7_3)) do { extern void call_in_true_branch_not_eliminated_on_line_110 (void); call_in_true_branch_not_eliminated_on_line_110(); } while (0); else (void)0;
  if (!(strlen (a7_3[i]) < sizeof a7_3)) do { extern void call_in_true_branch_not_eliminated_on_line_111 (void); call_in_true_branch_not_eliminated_on_line_111(); } while (0); else (void)0;

  if (!(strlen (a5_7[0]) < sizeof a5_7)) do { extern void call_in_true_branch_not_eliminated_on_line_113 (void); call_in_true_branch_not_eliminated_on_line_113(); } while (0); else (void)0;
  if (!(strlen (a5_7[1]) < sizeof a5_7 - sizeof *a5_7)) do { extern void call_in_true_branch_not_eliminated_on_line_114 (void); call_in_true_branch_not_eliminated_on_line_114(); } while (0); else (void)0;
  if (!(strlen (a5_7[4]) < sizeof a5_7 - 3 * sizeof *a5_7)) do { extern void call_in_true_branch_not_eliminated_on_line_115 (void); call_in_true_branch_not_eliminated_on_line_115(); } while (0); else (void)0;
  if (!(strlen (a5_7[i]) < sizeof a5_7)) do { extern void call_in_true_branch_not_eliminated_on_line_116 (void); call_in_true_branch_not_eliminated_on_line_116(); } while (0); else (void)0;




  if (!(strlen (ax_3[0]) < 0x7fffffffffffffffL)) do { extern void call_in_true_branch_not_eliminated_on_line_121 (void); call_in_true_branch_not_eliminated_on_line_121(); } while (0); else (void)0;
  if (!(strlen (ax_3[1]) < 0x7fffffffffffffffL)) do { extern void call_in_true_branch_not_eliminated_on_line_122 (void); call_in_true_branch_not_eliminated_on_line_122(); } while (0); else (void)0;
  if (!(strlen (ax_3[9]) < 0x7fffffffffffffffL)) do { extern void call_in_true_branch_not_eliminated_on_line_123 (void); call_in_true_branch_not_eliminated_on_line_123(); } while (0); else (void)0;
  if (!(strlen (ax_3[i]) < 0x7fffffffffffffffL)) do { extern void call_in_true_branch_not_eliminated_on_line_124 (void); call_in_true_branch_not_eliminated_on_line_124(); } while (0); else (void)0;

  if (!(strlen (a3) < sizeof a3)) do { extern void call_in_true_branch_not_eliminated_on_line_126 (void); call_in_true_branch_not_eliminated_on_line_126(); } while (0); else (void)0;
  if (!(strlen (a7) < sizeof a7)) do { extern void call_in_true_branch_not_eliminated_on_line_127 (void); call_in_true_branch_not_eliminated_on_line_127(); } while (0); else (void)0;

  if (!(strlen (ax) != 0x7fffffffffffffffL)) do { extern void call_in_true_branch_not_eliminated_on_line_129 (void); call_in_true_branch_not_eliminated_on_line_129(); } while (0); else (void)0;


}

void elim_pointer_to_arrays (void)
{




  if (!(strlen (*pa7) < 0x7fffffffffffffffL)) do { extern void call_in_true_branch_not_eliminated_on_line_140 (void); call_in_true_branch_not_eliminated_on_line_140(); } while (0); else (void)0;
  if (!(strlen (*pa5) < 0x7fffffffffffffffL)) do { extern void call_in_true_branch_not_eliminated_on_line_141 (void); call_in_true_branch_not_eliminated_on_line_141(); } while (0); else (void)0;
  if (!(strlen (*pa3) < 0x7fffffffffffffffL)) do { extern void call_in_true_branch_not_eliminated_on_line_142 (void); call_in_true_branch_not_eliminated_on_line_142(); } while (0); else (void)0;

  if (!(strlen ((*pa7_3)[0]) < 0x7fffffffffffffffL)) do { extern void call_in_true_branch_not_eliminated_on_line_144 (void); call_in_true_branch_not_eliminated_on_line_144(); } while (0); else (void)0;
  if (!(strlen ((*pa7_3)[1]) < 0x7fffffffffffffffL)) do { extern void call_in_true_branch_not_eliminated_on_line_145 (void); call_in_true_branch_not_eliminated_on_line_145(); } while (0); else (void)0;
  if (!(strlen ((*pa7_3)[6]) < 0x7fffffffffffffffL)) do { extern void call_in_true_branch_not_eliminated_on_line_146 (void); call_in_true_branch_not_eliminated_on_line_146(); } while (0); else (void)0;

  if (!(strlen ((*pax_3)[0]) < 0x7fffffffffffffffL)) do { extern void call_in_true_branch_not_eliminated_on_line_148 (void); call_in_true_branch_not_eliminated_on_line_148(); } while (0); else (void)0;
  if (!(strlen ((*pax_3)[1]) < 0x7fffffffffffffffL)) do { extern void call_in_true_branch_not_eliminated_on_line_149 (void); call_in_true_branch_not_eliminated_on_line_149(); } while (0); else (void)0;
  if (!(strlen ((*pax_3)[9]) < 0x7fffffffffffffffL)) do { extern void call_in_true_branch_not_eliminated_on_line_150 (void); call_in_true_branch_not_eliminated_on_line_150(); } while (0); else (void)0;

  if (!(strlen ((*pa5_7)[0]) < 0x7fffffffffffffffL)) do { extern void call_in_true_branch_not_eliminated_on_line_152 (void); call_in_true_branch_not_eliminated_on_line_152(); } while (0); else (void)0;
  if (!(strlen ((*pa5_7)[1]) < 0x7fffffffffffffffL)) do { extern void call_in_true_branch_not_eliminated_on_line_153 (void); call_in_true_branch_not_eliminated_on_line_153(); } while (0); else (void)0;
  if (!(strlen ((*pa5_7)[4]) < 0x7fffffffffffffffL)) do { extern void call_in_true_branch_not_eliminated_on_line_154 (void); call_in_true_branch_not_eliminated_on_line_154(); } while (0); else (void)0;
}

void elim_global_arrays_and_strings (int i)
{
  if (!(strlen (i < 0 ? a3 : "") < 3)) do { extern void call_in_true_branch_not_eliminated_on_line_159 (void); call_in_true_branch_not_eliminated_on_line_159(); } while (0); else (void)0;
  if (!(strlen (i < 0 ? a3 : "1") < 3)) do { extern void call_in_true_branch_not_eliminated_on_line_160 (void); call_in_true_branch_not_eliminated_on_line_160(); } while (0); else (void)0;
  if (!(strlen (i < 0 ? a3 : "12") < 3)) do { extern void call_in_true_branch_not_eliminated_on_line_161 (void); call_in_true_branch_not_eliminated_on_line_161(); } while (0); else (void)0;
  if (!(strlen (i < 0 ? a3 : "123") < 4)) do { extern void call_in_true_branch_not_eliminated_on_line_162 (void); call_in_true_branch_not_eliminated_on_line_162(); } while (0); else (void)0;

  if (!!(strlen (i < 0 ? a3 : "") > 3)) do { extern void call_in_false_branch_not_eliminated_on_line_164 (void); call_in_false_branch_not_eliminated_on_line_164(); } while (0); else (void)0;
  if (!!(strlen (i < 0 ? a3 : "1") > 3)) do { extern void call_in_false_branch_not_eliminated_on_line_165 (void); call_in_false_branch_not_eliminated_on_line_165(); } while (0); else (void)0;
  if (!!(strlen (i < 0 ? a3 : "12") > 3)) do { extern void call_in_false_branch_not_eliminated_on_line_166 (void); call_in_false_branch_not_eliminated_on_line_166(); } while (0); else (void)0;
  if (!!(strlen (i < 0 ? a3 : "123") > 4)) do { extern void call_in_false_branch_not_eliminated_on_line_167 (void); call_in_false_branch_not_eliminated_on_line_167(); } while (0); else (void)0;

  if (!(strlen (i < 0 ? a7 : "") < 7)) do { extern void call_in_true_branch_not_eliminated_on_line_169 (void); call_in_true_branch_not_eliminated_on_line_169(); } while (0); else (void)0;
  if (!(strlen (i < 0 ? a7 : "1") < 7)) do { extern void call_in_true_branch_not_eliminated_on_line_170 (void); call_in_true_branch_not_eliminated_on_line_170(); } while (0); else (void)0;
  if (!(strlen (i < 0 ? a7 : "12") < 7)) do { extern void call_in_true_branch_not_eliminated_on_line_171 (void); call_in_true_branch_not_eliminated_on_line_171(); } while (0); else (void)0;
  if (!(strlen (i < 0 ? a7 : "123") < 7)) do { extern void call_in_true_branch_not_eliminated_on_line_172 (void); call_in_true_branch_not_eliminated_on_line_172(); } while (0); else (void)0;
  if (!(strlen (i < 0 ? a7 : "123456") < 7)) do { extern void call_in_true_branch_not_eliminated_on_line_173 (void); call_in_true_branch_not_eliminated_on_line_173(); } while (0); else (void)0;
  if (!(strlen (i < 0 ? a7 : "1234567") < 8)) do { extern void call_in_true_branch_not_eliminated_on_line_174 (void); call_in_true_branch_not_eliminated_on_line_174(); } while (0); else (void)0;

  if (!!(strlen (i < 0 ? a7 : "") > 6)) do { extern void call_in_false_branch_not_eliminated_on_line_176 (void); call_in_false_branch_not_eliminated_on_line_176(); } while (0); else (void)0;
  if (!!(strlen (i < 0 ? a7 : "1") > 6)) do { extern void call_in_false_branch_not_eliminated_on_line_177 (void); call_in_false_branch_not_eliminated_on_line_177(); } while (0); else (void)0;
  if (!!(strlen (i < 0 ? a7 : "12") > 6)) do { extern void call_in_false_branch_not_eliminated_on_line_178 (void); call_in_false_branch_not_eliminated_on_line_178(); } while (0); else (void)0;
  if (!!(strlen (i < 0 ? a7 : "123") > 6)) do { extern void call_in_false_branch_not_eliminated_on_line_179 (void); call_in_false_branch_not_eliminated_on_line_179(); } while (0); else (void)0;
  if (!!(strlen (i < 0 ? a7 : "123456") > 7)) do { extern void call_in_false_branch_not_eliminated_on_line_180 (void); call_in_false_branch_not_eliminated_on_line_180(); } while (0); else (void)0;
  if (!!(strlen (i < 0 ? a7 : "1234567") > 8)) do { extern void call_in_false_branch_not_eliminated_on_line_181 (void); call_in_false_branch_not_eliminated_on_line_181(); } while (0); else (void)0;
}

void elim_member_arrays_obj (int i)
{
  if (!(strlen (ma0_3_5_7[0][0][0].a3) < sizeof ma0_3_5_7)) do { extern void call_in_true_branch_not_eliminated_on_line_186 (void); call_in_true_branch_not_eliminated_on_line_186(); } while (0); else (void)0;
  if (!(strlen (ma0_3_5_7[0][0][1].a3) < sizeof ma0_3_5_7)) do { extern void call_in_true_branch_not_eliminated_on_line_187 (void); call_in_true_branch_not_eliminated_on_line_187(); } while (0); else (void)0;
  if (!(strlen (ma0_3_5_7[0][0][2].a3) < sizeof ma0_3_5_7)) do { extern void call_in_true_branch_not_eliminated_on_line_188 (void); call_in_true_branch_not_eliminated_on_line_188(); } while (0); else (void)0;
  if (!(strlen (ma0_3_5_7[0][0][6].a3) < sizeof ma0_3_5_7)) do { extern void call_in_true_branch_not_eliminated_on_line_189 (void); call_in_true_branch_not_eliminated_on_line_189(); } while (0); else (void)0;

  if (!(strlen (ma0_3_5_7[1][0][0].a3) < sizeof ma0_3_5_7)) do { extern void call_in_true_branch_not_eliminated_on_line_191 (void); call_in_true_branch_not_eliminated_on_line_191(); } while (0); else (void)0;
  if (!(strlen (ma0_3_5_7[2][0][1].a3) < sizeof ma0_3_5_7)) do { extern void call_in_true_branch_not_eliminated_on_line_192 (void); call_in_true_branch_not_eliminated_on_line_192(); } while (0); else (void)0;

  if (!(strlen (ma0_3_5_7[1][1][0].a3) < sizeof ma0_3_5_7)) do { extern void call_in_true_branch_not_eliminated_on_line_194 (void); call_in_true_branch_not_eliminated_on_line_194(); } while (0); else (void)0;
  if (!(strlen (ma0_3_5_7[2][4][6].a3) < sizeof ma0_3_5_7)) do { extern void call_in_true_branch_not_eliminated_on_line_195 (void); call_in_true_branch_not_eliminated_on_line_195(); } while (0); else (void)0;

  if (!(strlen (ma0_3_5_7[0][0][0].a5) < sizeof ma0_3_5_7)) do { extern void call_in_true_branch_not_eliminated_on_line_197 (void); call_in_true_branch_not_eliminated_on_line_197(); } while (0); else (void)0;
  if (!(strlen (ma0_3_5_7[0][0][1].a5) < sizeof ma0_3_5_7)) do { extern void call_in_true_branch_not_eliminated_on_line_198 (void); call_in_true_branch_not_eliminated_on_line_198(); } while (0); else (void)0;
  if (!(strlen (ma0_3_5_7[0][0][2].a5) < sizeof ma0_3_5_7)) do { extern void call_in_true_branch_not_eliminated_on_line_199 (void); call_in_true_branch_not_eliminated_on_line_199(); } while (0); else (void)0;
  if (!(strlen (ma0_3_5_7[0][0][6].a5) < sizeof ma0_3_5_7)) do { extern void call_in_true_branch_not_eliminated_on_line_200 (void); call_in_true_branch_not_eliminated_on_line_200(); } while (0); else (void)0;

  if (!(strlen (ma0_3_5_7[1][0][0].a5) < sizeof ma0_3_5_7)) do { extern void call_in_true_branch_not_eliminated_on_line_202 (void); call_in_true_branch_not_eliminated_on_line_202(); } while (0); else (void)0;
  if (!(strlen (ma0_3_5_7[2][0][1].a5) < sizeof ma0_3_5_7)) do { extern void call_in_true_branch_not_eliminated_on_line_203 (void); call_in_true_branch_not_eliminated_on_line_203(); } while (0); else (void)0;

  if (!(strlen (ma0_3_5_7[1][1][0].a5) < sizeof ma0_3_5_7)) do { extern void call_in_true_branch_not_eliminated_on_line_205 (void); call_in_true_branch_not_eliminated_on_line_205(); } while (0); else (void)0;
  if (!(strlen (ma0_3_5_7[2][4][6].a5) < sizeof ma0_3_5_7)) do { extern void call_in_true_branch_not_eliminated_on_line_206 (void); call_in_true_branch_not_eliminated_on_line_206(); } while (0); else (void)0;

  if (!(strlen (ma0_3_5_7[0][0][0].a7_3[0]) < sizeof ma0_3_5_7)) do { extern void call_in_true_branch_not_eliminated_on_line_208 (void); call_in_true_branch_not_eliminated_on_line_208(); } while (0); else (void)0;
  if (!(strlen (ma0_3_5_7[2][4][6].a7_3[2]) < sizeof ma0_3_5_7)) do { extern void call_in_true_branch_not_eliminated_on_line_209 (void); call_in_true_branch_not_eliminated_on_line_209(); } while (0); else (void)0;

  if (!(strlen (ma0_3_5_7[0][0][0].a5_7[0]) < sizeof ma0_3_5_7)) do { extern void call_in_true_branch_not_eliminated_on_line_211 (void); call_in_true_branch_not_eliminated_on_line_211(); } while (0); else (void)0;
  if (!(strlen (ma0_3_5_7[2][4][6].a5_7[4]) < sizeof ma0_3_5_7)) do { extern void call_in_true_branch_not_eliminated_on_line_212 (void); call_in_true_branch_not_eliminated_on_line_212(); } while (0); else (void)0;
}
# 1000 "./strlenopt-40.c"




void keep_global_arrays (int i)
{
  if (strlen (a7_3[0]) < 2) do { extern void call_made_in_true_branch_on_line_1006 (void); call_made_in_true_branch_on_line_1006(); } while (0); else do { extern void call_made_in_false_branch_on_line_1006 (void); call_made_in_false_branch_on_line_1006(); } while (0);
  if (strlen (a7_3[1]) < 2) do { extern void call_made_in_true_branch_on_line_1007 (void); call_made_in_true_branch_on_line_1007(); } while (0); else do { extern void call_made_in_false_branch_on_line_1007 (void); call_made_in_false_branch_on_line_1007(); } while (0);
  if (strlen (a7_3[6]) < 2) do { extern void call_made_in_true_branch_on_line_1008 (void); call_made_in_true_branch_on_line_1008(); } while (0); else do { extern void call_made_in_false_branch_on_line_1008 (void); call_made_in_false_branch_on_line_1008(); } while (0);
  if (strlen (a7_3[i]) < 2) do { extern void call_made_in_true_branch_on_line_1009 (void); call_made_in_true_branch_on_line_1009(); } while (0); else do { extern void call_made_in_false_branch_on_line_1009 (void); call_made_in_false_branch_on_line_1009(); } while (0);

  if (strlen (a5_7[0]) < 6) do { extern void call_made_in_true_branch_on_line_1011 (void); call_made_in_true_branch_on_line_1011(); } while (0); else do { extern void call_made_in_false_branch_on_line_1011 (void); call_made_in_false_branch_on_line_1011(); } while (0);
  if (strlen (a5_7[1]) < 6) do { extern void call_made_in_true_branch_on_line_1012 (void); call_made_in_true_branch_on_line_1012(); } while (0); else do { extern void call_made_in_false_branch_on_line_1012 (void); call_made_in_false_branch_on_line_1012(); } while (0);
  if (strlen (a5_7[4]) < 6) do { extern void call_made_in_true_branch_on_line_1013 (void); call_made_in_true_branch_on_line_1013(); } while (0); else do { extern void call_made_in_false_branch_on_line_1013 (void); call_made_in_false_branch_on_line_1013(); } while (0);
  if (strlen (a5_7[i]) < 6) do { extern void call_made_in_true_branch_on_line_1014 (void); call_made_in_true_branch_on_line_1014(); } while (0); else do { extern void call_made_in_false_branch_on_line_1014 (void); call_made_in_false_branch_on_line_1014(); } while (0);





  if (strlen (a5_7[0]) > sizeof a5_7 - 2) do { extern void call_made_in_true_branch_on_line_1020 (void); call_made_in_true_branch_on_line_1020(); } while (0); else do { extern void call_made_in_false_branch_on_line_1020 (void); call_made_in_false_branch_on_line_1020(); } while (0);
  if (strlen (a5_7[1]) > sizeof a5_7 - sizeof a5_7[1] - 2) do { extern void call_made_in_true_branch_on_line_1021 (void); call_made_in_true_branch_on_line_1021(); } while (0); else do { extern void call_made_in_false_branch_on_line_1021 (void); call_made_in_false_branch_on_line_1021(); } while (0);
  if (strlen (a5_7[i]) > sizeof a5_7 - 2) do { extern void call_made_in_true_branch_on_line_1022 (void); call_made_in_true_branch_on_line_1022(); } while (0); else do { extern void call_made_in_false_branch_on_line_1022 (void); call_made_in_false_branch_on_line_1022(); } while (0);

  if (strlen (ax_3[0]) < 2) do { extern void call_made_in_true_branch_on_line_1024 (void); call_made_in_true_branch_on_line_1024(); } while (0); else do { extern void call_made_in_false_branch_on_line_1024 (void); call_made_in_false_branch_on_line_1024(); } while (0);
  if (strlen (ax_3[1]) < 2) do { extern void call_made_in_true_branch_on_line_1025 (void); call_made_in_true_branch_on_line_1025(); } while (0); else do { extern void call_made_in_false_branch_on_line_1025 (void); call_made_in_false_branch_on_line_1025(); } while (0);
  if (strlen (ax_3[2]) < 2) do { extern void call_made_in_true_branch_on_line_1026 (void); call_made_in_true_branch_on_line_1026(); } while (0); else do { extern void call_made_in_false_branch_on_line_1026 (void); call_made_in_false_branch_on_line_1026(); } while (0);
  if (strlen (ax_3[i]) < 2) do { extern void call_made_in_true_branch_on_line_1027 (void); call_made_in_true_branch_on_line_1027(); } while (0); else do { extern void call_made_in_false_branch_on_line_1027 (void); call_made_in_false_branch_on_line_1027(); } while (0);




  if (strlen (ax_3[0]) > 3) do { extern void call_made_in_true_branch_on_line_1032 (void); call_made_in_true_branch_on_line_1032(); } while (0); else do { extern void call_made_in_false_branch_on_line_1032 (void); call_made_in_false_branch_on_line_1032(); } while (0);
  if (strlen (ax_3[1]) > 9) do { extern void call_made_in_true_branch_on_line_1033 (void); call_made_in_true_branch_on_line_1033(); } while (0); else do { extern void call_made_in_false_branch_on_line_1033 (void); call_made_in_false_branch_on_line_1033(); } while (0);
  if (strlen (ax_3[2]) > 99) do { extern void call_made_in_true_branch_on_line_1034 (void); call_made_in_true_branch_on_line_1034(); } while (0); else do { extern void call_made_in_false_branch_on_line_1034 (void); call_made_in_false_branch_on_line_1034(); } while (0);
  if (strlen (ax_3[i]) > 999) do { extern void call_made_in_true_branch_on_line_1035 (void); call_made_in_true_branch_on_line_1035(); } while (0); else do { extern void call_made_in_false_branch_on_line_1035 (void); call_made_in_false_branch_on_line_1035(); } while (0);

  if (strlen (a3) < 2) do { extern void call_made_in_true_branch_on_line_1037 (void); call_made_in_true_branch_on_line_1037(); } while (0); else do { extern void call_made_in_false_branch_on_line_1037 (void); call_made_in_false_branch_on_line_1037(); } while (0);
  if (strlen (a7) < 6) do { extern void call_made_in_true_branch_on_line_1038 (void); call_made_in_true_branch_on_line_1038(); } while (0); else do { extern void call_made_in_false_branch_on_line_1038 (void); call_made_in_false_branch_on_line_1038(); } while (0);

  if (strlen (a3 + i) < 2) do { extern void call_made_in_true_branch_on_line_1040 (void); call_made_in_true_branch_on_line_1040(); } while (0); else do { extern void call_made_in_false_branch_on_line_1040 (void); call_made_in_false_branch_on_line_1040(); } while (0);
  if (strlen (a7 + i) < 2) do { extern void call_made_in_true_branch_on_line_1041 (void); call_made_in_true_branch_on_line_1041(); } while (0); else do { extern void call_made_in_false_branch_on_line_1041 (void); call_made_in_false_branch_on_line_1041(); } while (0);



  if (strlen (ax) != 0x7fffffffffffffffL - 2) do { extern void call_made_in_true_branch_on_line_1045 (void); call_made_in_true_branch_on_line_1045(); } while (0); else do { extern void call_made_in_false_branch_on_line_1045 (void); call_made_in_false_branch_on_line_1045(); } while (0);
  if (strlen (ax) < 0x7fffffffffffffffL - 2) do { extern void call_made_in_true_branch_on_line_1046 (void); call_made_in_true_branch_on_line_1046(); } while (0); else do { extern void call_made_in_false_branch_on_line_1046 (void); call_made_in_false_branch_on_line_1046(); } while (0);
  if (strlen (ax) < 999) do { extern void call_made_in_true_branch_on_line_1047 (void); call_made_in_true_branch_on_line_1047(); } while (0); else do { extern void call_made_in_false_branch_on_line_1047 (void); call_made_in_false_branch_on_line_1047(); } while (0);
  if (strlen (ax) < 1) do { extern void call_made_in_true_branch_on_line_1048 (void); call_made_in_true_branch_on_line_1048(); } while (0); else do { extern void call_made_in_false_branch_on_line_1048 (void); call_made_in_false_branch_on_line_1048(); } while (0);
}

void keep_pointer_to_arrays (int i)
{
  if (strlen (*pa7) < 6) do { extern void call_made_in_true_branch_on_line_1053 (void); call_made_in_true_branch_on_line_1053(); } while (0); else do { extern void call_made_in_false_branch_on_line_1053 (void); call_made_in_false_branch_on_line_1053(); } while (0);
  if (strlen (*pa5) < 4) do { extern void call_made_in_true_branch_on_line_1054 (void); call_made_in_true_branch_on_line_1054(); } while (0); else do { extern void call_made_in_false_branch_on_line_1054 (void); call_made_in_false_branch_on_line_1054(); } while (0);
  if (strlen (*pa3) < 2) do { extern void call_made_in_true_branch_on_line_1055 (void); call_made_in_true_branch_on_line_1055(); } while (0); else do { extern void call_made_in_false_branch_on_line_1055 (void); call_made_in_false_branch_on_line_1055(); } while (0);






  if (strlen (*pa7) > sizeof *pa7) do { extern void call_made_in_true_branch_on_line_1062 (void); call_made_in_true_branch_on_line_1062(); } while (0); else do { extern void call_made_in_false_branch_on_line_1062 (void); call_made_in_false_branch_on_line_1062(); } while (0);
  if (strlen (*pa5) > sizeof *pa5) do { extern void call_made_in_true_branch_on_line_1063 (void); call_made_in_true_branch_on_line_1063(); } while (0); else do { extern void call_made_in_false_branch_on_line_1063 (void); call_made_in_false_branch_on_line_1063(); } while (0);
  if (strlen (*pa3) > sizeof *pa3) do { extern void call_made_in_true_branch_on_line_1064 (void); call_made_in_true_branch_on_line_1064(); } while (0); else do { extern void call_made_in_false_branch_on_line_1064 (void); call_made_in_false_branch_on_line_1064(); } while (0);

  if (strlen ((*pa7_3)[0]) < 2) do { extern void call_made_in_true_branch_on_line_1066 (void); call_made_in_true_branch_on_line_1066(); } while (0); else do { extern void call_made_in_false_branch_on_line_1066 (void); call_made_in_false_branch_on_line_1066(); } while (0);
  if (strlen ((*pa7_3)[1]) < 2) do { extern void call_made_in_true_branch_on_line_1067 (void); call_made_in_true_branch_on_line_1067(); } while (0); else do { extern void call_made_in_false_branch_on_line_1067 (void); call_made_in_false_branch_on_line_1067(); } while (0);
  if (strlen ((*pa7_3)[6]) < 2) do { extern void call_made_in_true_branch_on_line_1068 (void); call_made_in_true_branch_on_line_1068(); } while (0); else do { extern void call_made_in_false_branch_on_line_1068 (void); call_made_in_false_branch_on_line_1068(); } while (0);
  if (strlen ((*pa7_3)[i]) < 2) do { extern void call_made_in_true_branch_on_line_1069 (void); call_made_in_true_branch_on_line_1069(); } while (0); else do { extern void call_made_in_false_branch_on_line_1069 (void); call_made_in_false_branch_on_line_1069(); } while (0);


  if (strlen ((*pa7_3)[0]) > sizeof *pa7_3) do { extern void call_made_in_true_branch_on_line_1072 (void); call_made_in_true_branch_on_line_1072(); } while (0); else do { extern void call_made_in_false_branch_on_line_1072 (void); call_made_in_false_branch_on_line_1072(); } while (0);
  if (strlen ((*pa7_3)[i]) > sizeof *pa7_3) do { extern void call_made_in_true_branch_on_line_1073 (void); call_made_in_true_branch_on_line_1073(); } while (0); else do { extern void call_made_in_false_branch_on_line_1073 (void); call_made_in_false_branch_on_line_1073(); } while (0);

  if (strlen ((*pax_3)[0]) < 2) do { extern void call_made_in_true_branch_on_line_1075 (void); call_made_in_true_branch_on_line_1075(); } while (0); else do { extern void call_made_in_false_branch_on_line_1075 (void); call_made_in_false_branch_on_line_1075(); } while (0);
  if (strlen ((*pax_3)[1]) < 2) do { extern void call_made_in_true_branch_on_line_1076 (void); call_made_in_true_branch_on_line_1076(); } while (0); else do { extern void call_made_in_false_branch_on_line_1076 (void); call_made_in_false_branch_on_line_1076(); } while (0);
  if (strlen ((*pax_3)[9]) < 2) do { extern void call_made_in_true_branch_on_line_1077 (void); call_made_in_true_branch_on_line_1077(); } while (0); else do { extern void call_made_in_false_branch_on_line_1077 (void); call_made_in_false_branch_on_line_1077(); } while (0);
  if (strlen ((*pax_3)[i]) < 2) do { extern void call_made_in_true_branch_on_line_1078 (void); call_made_in_true_branch_on_line_1078(); } while (0); else do { extern void call_made_in_false_branch_on_line_1078 (void); call_made_in_false_branch_on_line_1078(); } while (0);


  if (strlen ((*pax_3)[0]) > 3) do { extern void call_made_in_true_branch_on_line_1081 (void); call_made_in_true_branch_on_line_1081(); } while (0); else do { extern void call_made_in_false_branch_on_line_1081 (void); call_made_in_false_branch_on_line_1081(); } while (0);
  if (strlen ((*pax_3)[i]) > 333) do { extern void call_made_in_true_branch_on_line_1082 (void); call_made_in_true_branch_on_line_1082(); } while (0); else do { extern void call_made_in_false_branch_on_line_1082 (void); call_made_in_false_branch_on_line_1082(); } while (0);

  if (strlen ((*pa5_7)[0]) < 6) do { extern void call_made_in_true_branch_on_line_1084 (void); call_made_in_true_branch_on_line_1084(); } while (0); else do { extern void call_made_in_false_branch_on_line_1084 (void); call_made_in_false_branch_on_line_1084(); } while (0);
  if (strlen ((*pa5_7)[1]) < 6) do { extern void call_made_in_true_branch_on_line_1085 (void); call_made_in_true_branch_on_line_1085(); } while (0); else do { extern void call_made_in_false_branch_on_line_1085 (void); call_made_in_false_branch_on_line_1085(); } while (0);
  if (strlen ((*pa5_7)[4]) < 6) do { extern void call_made_in_true_branch_on_line_1086 (void); call_made_in_true_branch_on_line_1086(); } while (0); else do { extern void call_made_in_false_branch_on_line_1086 (void); call_made_in_false_branch_on_line_1086(); } while (0);
  if (strlen ((*pa5_7)[i]) < 6) do { extern void call_made_in_true_branch_on_line_1087 (void); call_made_in_true_branch_on_line_1087(); } while (0); else do { extern void call_made_in_false_branch_on_line_1087 (void); call_made_in_false_branch_on_line_1087(); } while (0);


  if (strlen ((*pa5_7)[0]) > sizeof *pa5_7) do { extern void call_made_in_true_branch_on_line_1090 (void); call_made_in_true_branch_on_line_1090(); } while (0); else do { extern void call_made_in_false_branch_on_line_1090 (void); call_made_in_false_branch_on_line_1090(); } while (0);
  if (strlen ((*pa5_7)[i]) > sizeof *pa5_7) do { extern void call_made_in_true_branch_on_line_1091 (void); call_made_in_true_branch_on_line_1091(); } while (0); else do { extern void call_made_in_false_branch_on_line_1091 (void); call_made_in_false_branch_on_line_1091(); } while (0);
 }

void keep_global_arrays_and_strings (int i)
{
  if (strlen (i < 0 ? a3 : "") < 2) do { extern void call_made_in_true_branch_on_line_1096 (void); call_made_in_true_branch_on_line_1096(); } while (0); else do { extern void call_made_in_false_branch_on_line_1096 (void); call_made_in_false_branch_on_line_1096(); } while (0);
  if (strlen (i < 0 ? a3 : "1") < 2) do { extern void call_made_in_true_branch_on_line_1097 (void); call_made_in_true_branch_on_line_1097(); } while (0); else do { extern void call_made_in_false_branch_on_line_1097 (void); call_made_in_false_branch_on_line_1097(); } while (0);
  if (strlen (i < 0 ? a3 : "12") < 2) do { extern void call_made_in_true_branch_on_line_1098 (void); call_made_in_true_branch_on_line_1098(); } while (0); else do { extern void call_made_in_false_branch_on_line_1098 (void); call_made_in_false_branch_on_line_1098(); } while (0);
  if (strlen (i < 0 ? a3 : "123") < 3) do { extern void call_made_in_true_branch_on_line_1099 (void); call_made_in_true_branch_on_line_1099(); } while (0); else do { extern void call_made_in_false_branch_on_line_1099 (void); call_made_in_false_branch_on_line_1099(); } while (0);

  if (strlen (i < 0 ? a7 : "") < 5) do { extern void call_made_in_true_branch_on_line_1101 (void); call_made_in_true_branch_on_line_1101(); } while (0); else do { extern void call_made_in_false_branch_on_line_1101 (void); call_made_in_false_branch_on_line_1101(); } while (0);
  if (strlen (i < 0 ? a7 : "1") < 5) do { extern void call_made_in_true_branch_on_line_1102 (void); call_made_in_true_branch_on_line_1102(); } while (0); else do { extern void call_made_in_false_branch_on_line_1102 (void); call_made_in_false_branch_on_line_1102(); } while (0);
  if (strlen (i < 0 ? a7 : "12") < 5) do { extern void call_made_in_true_branch_on_line_1103 (void); call_made_in_true_branch_on_line_1103(); } while (0); else do { extern void call_made_in_false_branch_on_line_1103 (void); call_made_in_false_branch_on_line_1103(); } while (0);
  if (strlen (i < 0 ? a7 : "123") < 5) do { extern void call_made_in_true_branch_on_line_1104 (void); call_made_in_true_branch_on_line_1104(); } while (0); else do { extern void call_made_in_false_branch_on_line_1104 (void); call_made_in_false_branch_on_line_1104(); } while (0);
  if (strlen (i < 0 ? a7 : "123456") < 6) do { extern void call_made_in_true_branch_on_line_1105 (void); call_made_in_true_branch_on_line_1105(); } while (0); else do { extern void call_made_in_false_branch_on_line_1105 (void); call_made_in_false_branch_on_line_1105(); } while (0);
  if (strlen (i < 0 ? a7 : "1234567") < 6) do { extern void call_made_in_true_branch_on_line_1106 (void); call_made_in_true_branch_on_line_1106(); } while (0); else do { extern void call_made_in_false_branch_on_line_1106 (void); call_made_in_false_branch_on_line_1106(); } while (0);




  if (strlen (i < 0 ? a7_3[0] : "") > 7) do { extern void call_made_in_true_branch_on_line_1111 (void); call_made_in_true_branch_on_line_1111(); } while (0); else do { extern void call_made_in_false_branch_on_line_1111 (void); call_made_in_false_branch_on_line_1111(); } while (0);
  if (strlen (i < 0 ? a7_3[i] : "") > 7) do { extern void call_made_in_true_branch_on_line_1112 (void); call_made_in_true_branch_on_line_1112(); } while (0); else do { extern void call_made_in_false_branch_on_line_1112 (void); call_made_in_false_branch_on_line_1112(); } while (0);
}

void keep_member_arrays_obj (int i)
{
  if (strlen (ma0_3_5_7[0][0][0].a3) < 2) do { extern void call_made_in_true_branch_on_line_1117 (void); call_made_in_true_branch_on_line_1117(); } while (0); else do { extern void call_made_in_false_branch_on_line_1117 (void); call_made_in_false_branch_on_line_1117(); } while (0);
  if (strlen (ma0_3_5_7[0][0][1].a3) < 2) do { extern void call_made_in_true_branch_on_line_1118 (void); call_made_in_true_branch_on_line_1118(); } while (0); else do { extern void call_made_in_false_branch_on_line_1118 (void); call_made_in_false_branch_on_line_1118(); } while (0);
  if (strlen (ma0_3_5_7[0][0][2].a3) < 2) do { extern void call_made_in_true_branch_on_line_1119 (void); call_made_in_true_branch_on_line_1119(); } while (0); else do { extern void call_made_in_false_branch_on_line_1119 (void); call_made_in_false_branch_on_line_1119(); } while (0);
  if (strlen (ma0_3_5_7[0][0][6].a3) < 2) do { extern void call_made_in_true_branch_on_line_1120 (void); call_made_in_true_branch_on_line_1120(); } while (0); else do { extern void call_made_in_false_branch_on_line_1120 (void); call_made_in_false_branch_on_line_1120(); } while (0);

  if (strlen (ma0_3_5_7[1][0][0].a3) < 2) do { extern void call_made_in_true_branch_on_line_1122 (void); call_made_in_true_branch_on_line_1122(); } while (0); else do { extern void call_made_in_false_branch_on_line_1122 (void); call_made_in_false_branch_on_line_1122(); } while (0);
  if (strlen (ma0_3_5_7[2][0][1].a3) < 2) do { extern void call_made_in_true_branch_on_line_1123 (void); call_made_in_true_branch_on_line_1123(); } while (0); else do { extern void call_made_in_false_branch_on_line_1123 (void); call_made_in_false_branch_on_line_1123(); } while (0);

  if (strlen (ma0_3_5_7[1][1][0].a3) < 2) do { extern void call_made_in_true_branch_on_line_1125 (void); call_made_in_true_branch_on_line_1125(); } while (0); else do { extern void call_made_in_false_branch_on_line_1125 (void); call_made_in_false_branch_on_line_1125(); } while (0);
  if (strlen (ma0_3_5_7[2][4][6].a3) < 2) do { extern void call_made_in_true_branch_on_line_1126 (void); call_made_in_true_branch_on_line_1126(); } while (0); else do { extern void call_made_in_false_branch_on_line_1126 (void); call_made_in_false_branch_on_line_1126(); } while (0);

  if (strlen (ma0_3_5_7[0][0][0].a5) < 4) do { extern void call_made_in_true_branch_on_line_1128 (void); call_made_in_true_branch_on_line_1128(); } while (0); else do { extern void call_made_in_false_branch_on_line_1128 (void); call_made_in_false_branch_on_line_1128(); } while (0);
  if (strlen (ma0_3_5_7[0][0][1].a5) < 4) do { extern void call_made_in_true_branch_on_line_1129 (void); call_made_in_true_branch_on_line_1129(); } while (0); else do { extern void call_made_in_false_branch_on_line_1129 (void); call_made_in_false_branch_on_line_1129(); } while (0);
  if (strlen (ma0_3_5_7[0][0][2].a5) < 4) do { extern void call_made_in_true_branch_on_line_1130 (void); call_made_in_true_branch_on_line_1130(); } while (0); else do { extern void call_made_in_false_branch_on_line_1130 (void); call_made_in_false_branch_on_line_1130(); } while (0);
  if (strlen (ma0_3_5_7[0][0][6].a5) < 4) do { extern void call_made_in_true_branch_on_line_1131 (void); call_made_in_true_branch_on_line_1131(); } while (0); else do { extern void call_made_in_false_branch_on_line_1131 (void); call_made_in_false_branch_on_line_1131(); } while (0);

  if (strlen (ma0_3_5_7[1][0][0].a5) < 4) do { extern void call_made_in_true_branch_on_line_1133 (void); call_made_in_true_branch_on_line_1133(); } while (0); else do { extern void call_made_in_false_branch_on_line_1133 (void); call_made_in_false_branch_on_line_1133(); } while (0);
  if (strlen (ma0_3_5_7[2][0][1].a5) < 4) do { extern void call_made_in_true_branch_on_line_1134 (void); call_made_in_true_branch_on_line_1134(); } while (0); else do { extern void call_made_in_false_branch_on_line_1134 (void); call_made_in_false_branch_on_line_1134(); } while (0);

  if (strlen (ma0_3_5_7[1][1][0].a5) < 4) do { extern void call_made_in_true_branch_on_line_1136 (void); call_made_in_true_branch_on_line_1136(); } while (0); else do { extern void call_made_in_false_branch_on_line_1136 (void); call_made_in_false_branch_on_line_1136(); } while (0);
  if (strlen (ma0_3_5_7[2][4][6].a5) < 4) do { extern void call_made_in_true_branch_on_line_1137 (void); call_made_in_true_branch_on_line_1137(); } while (0); else do { extern void call_made_in_false_branch_on_line_1137 (void); call_made_in_false_branch_on_line_1137(); } while (0);

  if (strlen (ma0_3_5_7[0][0][0].a7_3[0]) < 2) do { extern void call_made_in_true_branch_on_line_1139 (void); call_made_in_true_branch_on_line_1139(); } while (0); else do { extern void call_made_in_false_branch_on_line_1139 (void); call_made_in_false_branch_on_line_1139(); } while (0);
  if (strlen (ma0_3_5_7[2][4][6].a7_3[2]) < 2) do { extern void call_made_in_true_branch_on_line_1140 (void); call_made_in_true_branch_on_line_1140(); } while (0); else do { extern void call_made_in_false_branch_on_line_1140 (void); call_made_in_false_branch_on_line_1140(); } while (0);

  if (strlen (ma0_3_5_7[0][0][0].a5_7[0]) < 6) do { extern void call_made_in_true_branch_on_line_1142 (void); call_made_in_true_branch_on_line_1142(); } while (0); else do { extern void call_made_in_false_branch_on_line_1142 (void); call_made_in_false_branch_on_line_1142(); } while (0);
  if (strlen (ma0_3_5_7[2][4][6].a5_7[4]) < 6) do { extern void call_made_in_true_branch_on_line_1143 (void); call_made_in_true_branch_on_line_1143(); } while (0); else do { extern void call_made_in_false_branch_on_line_1143 (void); call_made_in_false_branch_on_line_1143(); } while (0);



  if (strlen (ma0_3_5_7[0][0][0].a3) > 2) do { extern void call_made_in_true_branch_on_line_1147 (void); call_made_in_true_branch_on_line_1147(); } while (0); else do { extern void call_made_in_false_branch_on_line_1147 (void); call_made_in_false_branch_on_line_1147(); } while (0);
  if (strlen (ma0_3_5_7[0][0][6].a3) > 2) do { extern void call_made_in_true_branch_on_line_1148 (void); call_made_in_true_branch_on_line_1148(); } while (0); else do { extern void call_made_in_false_branch_on_line_1148 (void); call_made_in_false_branch_on_line_1148(); } while (0);
  if (strlen (ma0_3_5_7[0][0][i].a3) > 2) do { extern void call_made_in_true_branch_on_line_1149 (void); call_made_in_true_branch_on_line_1149(); } while (0); else do { extern void call_made_in_false_branch_on_line_1149 (void); call_made_in_false_branch_on_line_1149(); } while (0);
}

void keep_member_arrays_ptr (struct MemArrays0 *ma0,
        struct MemArraysX *max,
        struct MemArrays7 *ma7,
        int i)
{
  if (strlen (ma0->a7_3[0]) > 0) do { extern void call_made_in_true_branch_on_line_1157 (void); call_made_in_true_branch_on_line_1157(); } while (0); else do { extern void call_made_in_false_branch_on_line_1157 (void); call_made_in_false_branch_on_line_1157(); } while (0);
  if (strlen (ma0->a7_3[0]) < 2) do { extern void call_made_in_true_branch_on_line_1158 (void); call_made_in_true_branch_on_line_1158(); } while (0); else do { extern void call_made_in_false_branch_on_line_1158 (void); call_made_in_false_branch_on_line_1158(); } while (0);
  if (strlen (ma0->a7_3[1]) < 2) do { extern void call_made_in_true_branch_on_line_1159 (void); call_made_in_true_branch_on_line_1159(); } while (0); else do { extern void call_made_in_false_branch_on_line_1159 (void); call_made_in_false_branch_on_line_1159(); } while (0);
  if (strlen (ma0->a7_3[6]) < 2) do { extern void call_made_in_true_branch_on_line_1160 (void); call_made_in_true_branch_on_line_1160(); } while (0); else do { extern void call_made_in_false_branch_on_line_1160 (void); call_made_in_false_branch_on_line_1160(); } while (0);
  if (strlen (ma0->a7_3[6]) < 2) do { extern void call_made_in_true_branch_on_line_1161 (void); call_made_in_true_branch_on_line_1161(); } while (0); else do { extern void call_made_in_false_branch_on_line_1161 (void); call_made_in_false_branch_on_line_1161(); } while (0);
  if (strlen (ma0->a7_3[i]) > 0) do { extern void call_made_in_true_branch_on_line_1162 (void); call_made_in_true_branch_on_line_1162(); } while (0); else do { extern void call_made_in_false_branch_on_line_1162 (void); call_made_in_false_branch_on_line_1162(); } while (0);
  if (strlen (ma0->a7_3[i]) < 2) do { extern void call_made_in_true_branch_on_line_1163 (void); call_made_in_true_branch_on_line_1163(); } while (0); else do { extern void call_made_in_false_branch_on_line_1163 (void); call_made_in_false_branch_on_line_1163(); } while (0);
  if (strlen (ma0->a7_3[i]) < 2) do { extern void call_made_in_true_branch_on_line_1164 (void); call_made_in_true_branch_on_line_1164(); } while (0); else do { extern void call_made_in_false_branch_on_line_1164 (void); call_made_in_false_branch_on_line_1164(); } while (0);



  if (strlen (ma0->a7_3[0]) > sizeof ma0->a7_3) do { extern void call_made_in_true_branch_on_line_1168 (void); call_made_in_true_branch_on_line_1168(); } while (0); else do { extern void call_made_in_false_branch_on_line_1168 (void); call_made_in_false_branch_on_line_1168(); } while (0);
  if (strlen (ma0->a7_3[i]) > sizeof ma0->a7_3) do { extern void call_made_in_true_branch_on_line_1169 (void); call_made_in_true_branch_on_line_1169(); } while (0); else do { extern void call_made_in_false_branch_on_line_1169 (void); call_made_in_false_branch_on_line_1169(); } while (0);

  if (strlen (ma0->a5_7[0]) < 5) do { extern void call_made_in_true_branch_on_line_1171 (void); call_made_in_true_branch_on_line_1171(); } while (0); else do { extern void call_made_in_false_branch_on_line_1171 (void); call_made_in_false_branch_on_line_1171(); } while (0);
  if (strlen (ma0[0].a5_7[0]) < 5) do { extern void call_made_in_true_branch_on_line_1172 (void); call_made_in_true_branch_on_line_1172(); } while (0); else do { extern void call_made_in_false_branch_on_line_1172 (void); call_made_in_false_branch_on_line_1172(); } while (0);
  if (strlen (ma0[1].a5_7[0]) < 5) do { extern void call_made_in_true_branch_on_line_1173 (void); call_made_in_true_branch_on_line_1173(); } while (0); else do { extern void call_made_in_false_branch_on_line_1173 (void); call_made_in_false_branch_on_line_1173(); } while (0);
  if (strlen (ma0[9].a5_7[0]) < 5) do { extern void call_made_in_true_branch_on_line_1174 (void); call_made_in_true_branch_on_line_1174(); } while (0); else do { extern void call_made_in_false_branch_on_line_1174 (void); call_made_in_false_branch_on_line_1174(); } while (0);
  if (strlen (ma0[9].a5_7[4]) < 5) do { extern void call_made_in_true_branch_on_line_1175 (void); call_made_in_true_branch_on_line_1175(); } while (0); else do { extern void call_made_in_false_branch_on_line_1175 (void); call_made_in_false_branch_on_line_1175(); } while (0);
  if (strlen (ma0[i].a5_7[4]) < 5) do { extern void call_made_in_true_branch_on_line_1176 (void); call_made_in_true_branch_on_line_1176(); } while (0); else do { extern void call_made_in_false_branch_on_line_1176 (void); call_made_in_false_branch_on_line_1176(); } while (0);
  if (strlen (ma0[i].a5_7[i]) < 5) do { extern void call_made_in_true_branch_on_line_1177 (void); call_made_in_true_branch_on_line_1177(); } while (0); else do { extern void call_made_in_false_branch_on_line_1177 (void); call_made_in_false_branch_on_line_1177(); } while (0);


  if (strlen (ma0[i].a5_7[i]) > sizeof ma0[i].a5_7) do { extern void call_made_in_true_branch_on_line_1180 (void); call_made_in_true_branch_on_line_1180(); } while (0); else do { extern void call_made_in_false_branch_on_line_1180 (void); call_made_in_false_branch_on_line_1180(); } while (0);

  if (strlen (ma0->a0) < 0x7fffffffffffffffL - 2) do { extern void call_made_in_true_branch_on_line_1182 (void); call_made_in_true_branch_on_line_1182(); } while (0); else do { extern void call_made_in_false_branch_on_line_1182 (void); call_made_in_false_branch_on_line_1182(); } while (0);
  if (strlen (ma0->a0) < 999) do { extern void call_made_in_true_branch_on_line_1183 (void); call_made_in_true_branch_on_line_1183(); } while (0); else do { extern void call_made_in_false_branch_on_line_1183 (void); call_made_in_false_branch_on_line_1183(); } while (0);
  if (strlen (ma0->a0) < 1) do { extern void call_made_in_true_branch_on_line_1184 (void); call_made_in_true_branch_on_line_1184(); } while (0); else do { extern void call_made_in_false_branch_on_line_1184 (void); call_made_in_false_branch_on_line_1184(); } while (0);

  if (strlen (max->ax) < 0x7fffffffffffffffL - 2) do { extern void call_made_in_true_branch_on_line_1186 (void); call_made_in_true_branch_on_line_1186(); } while (0); else do { extern void call_made_in_false_branch_on_line_1186 (void); call_made_in_false_branch_on_line_1186(); } while (0);
  if (strlen (max->ax) < 999) do { extern void call_made_in_true_branch_on_line_1187 (void); call_made_in_true_branch_on_line_1187(); } while (0); else do { extern void call_made_in_false_branch_on_line_1187 (void); call_made_in_false_branch_on_line_1187(); } while (0);
  if (strlen (max->ax) < 1) do { extern void call_made_in_true_branch_on_line_1188 (void); call_made_in_true_branch_on_line_1188(); } while (0); else do { extern void call_made_in_false_branch_on_line_1188 (void); call_made_in_false_branch_on_line_1188(); } while (0);

  if (strlen (ma7->a7) < 0x7fffffffffffffffL - 2) do { extern void call_made_in_true_branch_on_line_1190 (void); call_made_in_true_branch_on_line_1190(); } while (0); else do { extern void call_made_in_false_branch_on_line_1190 (void); call_made_in_false_branch_on_line_1190(); } while (0);
  if (strlen (ma7->a7) < 999) do { extern void call_made_in_true_branch_on_line_1191 (void); call_made_in_true_branch_on_line_1191(); } while (0); else do { extern void call_made_in_false_branch_on_line_1191 (void); call_made_in_false_branch_on_line_1191(); } while (0);
  if (strlen (ma7->a7) < 1) do { extern void call_made_in_true_branch_on_line_1192 (void); call_made_in_true_branch_on_line_1192(); } while (0); else do { extern void call_made_in_false_branch_on_line_1192 (void); call_made_in_false_branch_on_line_1192(); } while (0);
}

void keep_pointers (const char *s)
{
  if (strlen (ptr) < 0x7fffffffffffffffL - 2) do { extern void call_made_in_true_branch_on_line_1197 (void); call_made_in_true_branch_on_line_1197(); } while (0); else do { extern void call_made_in_false_branch_on_line_1197 (void); call_made_in_false_branch_on_line_1197(); } while (0);
  if (strlen (ptr) < 999) do { extern void call_made_in_true_branch_on_line_1198 (void); call_made_in_true_branch_on_line_1198(); } while (0); else do { extern void call_made_in_false_branch_on_line_1198 (void); call_made_in_false_branch_on_line_1198(); } while (0);
  if (strlen (ptr) < 1) do { extern void call_made_in_true_branch_on_line_1199 (void); call_made_in_true_branch_on_line_1199(); } while (0); else do { extern void call_made_in_false_branch_on_line_1199 (void); call_made_in_false_branch_on_line_1199(); } while (0);

  if (strlen (s) < 0x7fffffffffffffffL - 2) do { extern void call_made_in_true_branch_on_line_1201 (void); call_made_in_true_branch_on_line_1201(); } while (0); else do { extern void call_made_in_false_branch_on_line_1201 (void); call_made_in_false_branch_on_line_1201(); } while (0);
  if (strlen (s) < 999) do { extern void call_made_in_true_branch_on_line_1202 (void); call_made_in_true_branch_on_line_1202(); } while (0); else do { extern void call_made_in_false_branch_on_line_1202 (void); call_made_in_false_branch_on_line_1202(); } while (0);
  if (strlen (s) < 1) do { extern void call_made_in_true_branch_on_line_1203 (void); call_made_in_true_branch_on_line_1203(); } while (0); else do { extern void call_made_in_false_branch_on_line_1203 (void); call_made_in_false_branch_on_line_1203(); } while (0);
}
