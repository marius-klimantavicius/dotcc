//type: fp
//options: 
# 0 "./strlenopt-73.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-73.c"
# 15 "./strlenopt-73.c"
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
# 16 "./strlenopt-73.c" 2
# 42 "./strlenopt-73.c"
void sink (void*);

const char a32[33] = "0123456789abcdef0123456789abcdef";
const char b32[33] = "fedcba9876543210fedcba9876543210";

const char a16[33] = "0123456789abcdef";
const char b16[33] = "fedcba9876543210";

int i0, i1, i2;

void test_copy_cond_equal_length (void)
{





  do { char arr_59[17]; char *pa = arr_59; memcpy (pa, i0 ? a16 : b16, 17); if ((!(16 == strlen (pa)))) do { extern void call_not_eliminated_on_line_59 (void); call_not_eliminated_on_line_59(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_60[17]; char *pa = arr_60; memcpy (pa, i0 ? a16 : b16, 17); if ((!(16 == strlen (pa)))) do { extern void call_not_eliminated_on_line_60 (void); call_not_eliminated_on_line_60(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_61[17]; char *pa = arr_61; memcpy (pa, (i0 ? a16 : b16) + 1, 16); if ((!(15 == strlen (pa)))) do { extern void call_not_eliminated_on_line_61 (void); call_not_eliminated_on_line_61(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_62[17]; char *pa = arr_62; memcpy (pa, (i0 ? a16 : b16) + 2, 15); if ((!(14 == strlen (pa)))) do { extern void call_not_eliminated_on_line_62 (void); call_not_eliminated_on_line_62(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_63[17]; char *pa = arr_63; memcpy (pa, (i0 ? a16 : b16) + 16, 1); if ((!(0 == strlen (pa)))) do { extern void call_not_eliminated_on_line_63 (void); call_not_eliminated_on_line_63(); } while (0); else (void)0; sink (pa); } while (0);

  do { char arr_65[33]; char *pa = arr_65; memcpy (pa, (i0 ? a32 : b32) + 1, 32); if ((!(31 == strlen (pa)))) do { extern void call_not_eliminated_on_line_65 (void); call_not_eliminated_on_line_65(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_66[33]; char *pa = arr_66; memcpy (pa, (i0 ? a32 : b32) + 2, 31); if ((!(30 == strlen (pa)))) do { extern void call_not_eliminated_on_line_66 (void); call_not_eliminated_on_line_66(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_67[33]; char *pa = arr_67; memcpy (pa, (i0 ? a32 : b32) + 3, 30); if ((!(29 == strlen (pa)))) do { extern void call_not_eliminated_on_line_67 (void); call_not_eliminated_on_line_67(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_68[33]; char *pa = arr_68; memcpy (pa, (i0 ? a32 : b32) + 31, 2); if ((!(1 == strlen (pa)))) do { extern void call_not_eliminated_on_line_68 (void); call_not_eliminated_on_line_68(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_69[33]; char *pa = arr_69; memcpy (pa, (i0 ? a32 : b32) + 32, 1); if ((!(0 == strlen (pa)))) do { extern void call_not_eliminated_on_line_69 (void); call_not_eliminated_on_line_69(); } while (0); else (void)0; sink (pa); } while (0);
}
# 80 "./strlenopt-73.c"
const char a4[16] = "0123";
const char b4[16] = "3210";

void test_copy_cond_unequal_length_i64 (void)
{
  do { char arr_85[16]; char *pa = arr_85; memcpy (pa, i0 ? a4 + 1 : b4 + 0, 8); if ((!(2 < strlen (pa)))) do { extern void call_not_eliminated_on_line_85 (void); call_not_eliminated_on_line_85(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_86[16]; char *pa = arr_86; memcpy (pa, i0 ? a4 + 1 : b4 + 2, 8); if ((!(1 < strlen (pa)))) do { extern void call_not_eliminated_on_line_86 (void); call_not_eliminated_on_line_86(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_87[16]; char *pa = arr_87; memcpy (pa, i0 ? a4 + 1 : b4 + 3, 8); if ((!(0 < strlen (pa)))) do { extern void call_not_eliminated_on_line_87 (void); call_not_eliminated_on_line_87(); } while (0); else (void)0; sink (pa); } while (0);

  do { char arr_89[16]; char *pa = arr_89; memcpy (pa, i0 ? a4 + 2 : b4 + 0, 8); if ((!(1 < strlen (pa)))) do { extern void call_not_eliminated_on_line_89 (void); call_not_eliminated_on_line_89(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_90[16]; char *pa = arr_90; memcpy (pa, i0 ? a4 + 2 : b4 + 1, 8); if ((!(1 < strlen (pa)))) do { extern void call_not_eliminated_on_line_90 (void); call_not_eliminated_on_line_90(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_91[16]; char *pa = arr_91; memcpy (pa, i0 ? a4 + 2 : b4 + 3, 8); if ((!(0 < strlen (pa)))) do { extern void call_not_eliminated_on_line_91 (void); call_not_eliminated_on_line_91(); } while (0); else (void)0; sink (pa); } while (0);
}
# 104 "./strlenopt-73.c"
const char a8[32] = "01234567";
const char b8[32] = "76543210";

void test_copy_cond_unequal_length_i128 (void)
{
  do { char arr_109[32]; char *pa = arr_109; memcpy (pa, i0 ? a8 + 1 : b8 + 0, 16); if ((!(6 < strlen (pa)))) do { extern void call_not_eliminated_on_line_109 (void); call_not_eliminated_on_line_109(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_110[32]; char *pa = arr_110; memcpy (pa, i0 ? a8 + 1 : b8 + 2, 16); if ((!(5 < strlen (pa)))) do { extern void call_not_eliminated_on_line_110 (void); call_not_eliminated_on_line_110(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_111[32]; char *pa = arr_111; memcpy (pa, i0 ? a8 + 1 : b8 + 3, 16); if ((!(4 < strlen (pa)))) do { extern void call_not_eliminated_on_line_111 (void); call_not_eliminated_on_line_111(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_112[32]; char *pa = arr_112; memcpy (pa, i0 ? a8 + 1 : b8 + 4, 16); if ((!(3 < strlen (pa)))) do { extern void call_not_eliminated_on_line_112 (void); call_not_eliminated_on_line_112(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_113[32]; char *pa = arr_113; memcpy (pa, i0 ? a8 + 1 : b8 + 5, 16); if ((!(2 < strlen (pa)))) do { extern void call_not_eliminated_on_line_113 (void); call_not_eliminated_on_line_113(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_114[32]; char *pa = arr_114; memcpy (pa, i0 ? a8 + 1 : b8 + 6, 16); if ((!(1 < strlen (pa)))) do { extern void call_not_eliminated_on_line_114 (void); call_not_eliminated_on_line_114(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_115[32]; char *pa = arr_115; memcpy (pa, i0 ? a8 + 1 : b8 + 7, 16); if ((!(0 < strlen (pa)))) do { extern void call_not_eliminated_on_line_115 (void); call_not_eliminated_on_line_115(); } while (0); else (void)0; sink (pa); } while (0);

  do { char arr_117[32]; char *pa = arr_117; memcpy (pa, i0 ? a8 + 2 : b8 + 0, 16); if ((!(5 < strlen (pa)))) do { extern void call_not_eliminated_on_line_117 (void); call_not_eliminated_on_line_117(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_118[32]; char *pa = arr_118; memcpy (pa, i0 ? a8 + 2 : b8 + 1, 16); if ((!(5 < strlen (pa)))) do { extern void call_not_eliminated_on_line_118 (void); call_not_eliminated_on_line_118(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_119[32]; char *pa = arr_119; memcpy (pa, i0 ? a8 + 2 : b8 + 3, 16); if ((!(3 < strlen (pa)))) do { extern void call_not_eliminated_on_line_119 (void); call_not_eliminated_on_line_119(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_120[32]; char *pa = arr_120; memcpy (pa, i0 ? a8 + 2 : b8 + 4, 16); if ((!(2 < strlen (pa)))) do { extern void call_not_eliminated_on_line_120 (void); call_not_eliminated_on_line_120(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_121[32]; char *pa = arr_121; memcpy (pa, i0 ? a8 + 2 : b8 + 5, 16); if ((!(1 < strlen (pa)))) do { extern void call_not_eliminated_on_line_121 (void); call_not_eliminated_on_line_121(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_122[32]; char *pa = arr_122; memcpy (pa, i0 ? a8 + 2 : b8 + 6, 16); if ((!(0 < strlen (pa)))) do { extern void call_not_eliminated_on_line_122 (void); call_not_eliminated_on_line_122(); } while (0); else (void)0; sink (pa); } while (0);

  do { char arr_124[32]; char *pa = arr_124; memcpy (pa, i0 ? a8 + 3 : b8 + 0, 16); if ((!(4 < strlen (pa)))) do { extern void call_not_eliminated_on_line_124 (void); call_not_eliminated_on_line_124(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_125[32]; char *pa = arr_125; memcpy (pa, i0 ? a8 + 3 : b8 + 1, 16); if ((!(4 < strlen (pa)))) do { extern void call_not_eliminated_on_line_125 (void); call_not_eliminated_on_line_125(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_126[32]; char *pa = arr_126; memcpy (pa, i0 ? a8 + 3 : b8 + 2, 16); if ((!(4 < strlen (pa)))) do { extern void call_not_eliminated_on_line_126 (void); call_not_eliminated_on_line_126(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_127[32]; char *pa = arr_127; memcpy (pa, i0 ? a8 + 3 : b8 + 4, 16); if ((!(3 < strlen (pa)))) do { extern void call_not_eliminated_on_line_127 (void); call_not_eliminated_on_line_127(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_128[32]; char *pa = arr_128; memcpy (pa, i0 ? a8 + 3 : b8 + 5, 16); if ((!(2 < strlen (pa)))) do { extern void call_not_eliminated_on_line_128 (void); call_not_eliminated_on_line_128(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_129[32]; char *pa = arr_129; memcpy (pa, i0 ? a8 + 3 : b8 + 6, 16); if ((!(1 < strlen (pa)))) do { extern void call_not_eliminated_on_line_129 (void); call_not_eliminated_on_line_129(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_130[32]; char *pa = arr_130; memcpy (pa, i0 ? a8 + 3 : b8 + 7, 16); if ((!(0 < strlen (pa)))) do { extern void call_not_eliminated_on_line_130 (void); call_not_eliminated_on_line_130(); } while (0); else (void)0; sink (pa); } while (0);

  do { char arr_132[32]; char *pa = arr_132; memcpy (pa, i0 ? a8 + 4 : b8 + 0, 16); if ((!(3 < strlen (pa)))) do { extern void call_not_eliminated_on_line_132 (void); call_not_eliminated_on_line_132(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_133[32]; char *pa = arr_133; memcpy (pa, i0 ? a8 + 4 : b8 + 1, 16); if ((!(3 < strlen (pa)))) do { extern void call_not_eliminated_on_line_133 (void); call_not_eliminated_on_line_133(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_134[32]; char *pa = arr_134; memcpy (pa, i0 ? a8 + 4 : b8 + 2, 16); if ((!(3 < strlen (pa)))) do { extern void call_not_eliminated_on_line_134 (void); call_not_eliminated_on_line_134(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_135[32]; char *pa = arr_135; memcpy (pa, i0 ? a8 + 4 : b8 + 3, 16); if ((!(3 < strlen (pa)))) do { extern void call_not_eliminated_on_line_135 (void); call_not_eliminated_on_line_135(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_136[32]; char *pa = arr_136; memcpy (pa, i0 ? a8 + 4 : b8 + 5, 16); if ((!(2 < strlen (pa)))) do { extern void call_not_eliminated_on_line_136 (void); call_not_eliminated_on_line_136(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_137[32]; char *pa = arr_137; memcpy (pa, i0 ? a8 + 4 : b8 + 6, 16); if ((!(1 < strlen (pa)))) do { extern void call_not_eliminated_on_line_137 (void); call_not_eliminated_on_line_137(); } while (0); else (void)0; sink (pa); } while (0);
  do { char arr_138[32]; char *pa = arr_138; memcpy (pa, i0 ? a8 + 4 : b8 + 7, 16); if ((!(0 < strlen (pa)))) do { extern void call_not_eliminated_on_line_138 (void); call_not_eliminated_on_line_138(); } while (0); else (void)0; sink (pa); } while (0);
}
