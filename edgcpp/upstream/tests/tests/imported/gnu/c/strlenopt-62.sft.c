//type: fp
//options: 
# 0 "./strlenopt-62.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./strlenopt-62.c"





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
# 7 "./strlenopt-62.c" 2

typedef short int int16_t;
typedef int int32_t;
# 31 "./strlenopt-62.c"
void sink (void*, ...);
# 42 "./strlenopt-62.c"
void test_char_vla_local (int n)
{
  do { char vla[n]; char *ptr = strcpy ((char*)vla, ""); if (!(0 == strlen (vla))) do { extern void call_in_true_branch_not_eliminated_on_line_44_0 (void); call_in_true_branch_not_eliminated_on_line_44_0(); } while (0); else (void)0; sink (ptr); } while (0);
  do { char vla[n]; char *ptr = strcpy ((char*)vla, "\0"); if (!(0 == strlen (vla))) do { extern void call_in_true_branch_not_eliminated_on_line_45_1 (void); call_in_true_branch_not_eliminated_on_line_45_1(); } while (0); else (void)0; sink (ptr); } while (0);
  do { char vla[n]; char *ptr = strcpy ((char*)vla, "1"); if (!(1 == strlen (vla))) do { extern void call_in_true_branch_not_eliminated_on_line_46_2 (void); call_in_true_branch_not_eliminated_on_line_46_2(); } while (0); else (void)0; sink (ptr); } while (0);
  do { char vla[n]; char *ptr = strcpy ((char*)vla, "12"); if (!(2 == strlen (vla))) do { extern void call_in_true_branch_not_eliminated_on_line_47_3 (void); call_in_true_branch_not_eliminated_on_line_47_3(); } while (0); else (void)0; sink (ptr); } while (0);
  do { char vla[n]; char *ptr = strcpy ((char*)vla, "123"); if (!(3 == strlen (vla))) do { extern void call_in_true_branch_not_eliminated_on_line_48_4 (void); call_in_true_branch_not_eliminated_on_line_48_4(); } while (0); else (void)0; sink (ptr); } while (0);

  do { char vla[n]; char *ptr = strcpy ((char*)vla, "123"); if (!(2 == strlen (vla + 1))) do { extern void call_in_true_branch_not_eliminated_on_line_50_5 (void); call_in_true_branch_not_eliminated_on_line_50_5(); } while (0); else (void)0; sink (ptr); } while (0);
  do { char vla[n]; char *ptr = strcpy ((char*)vla, "123"); if (!(1 == strlen (&vla[2]))) do { extern void call_in_true_branch_not_eliminated_on_line_51_6 (void); call_in_true_branch_not_eliminated_on_line_51_6(); } while (0); else (void)0; sink (ptr); } while (0);
  do { char vla[n]; char *ptr = strcpy ((char*)vla, "123"); if (!(0 == strlen (&vla[1] + 2))) do { extern void call_in_true_branch_not_eliminated_on_line_52_7 (void); call_in_true_branch_not_eliminated_on_line_52_7(); } while (0); else (void)0; sink (ptr); } while (0);

  do { char vla[n]; char *ptr = strcpy ((char*)vla, "123"); if (!(2 == strlen (&vla[2] - 1))) do { extern void call_in_true_branch_not_eliminated_on_line_54_8 (void); call_in_true_branch_not_eliminated_on_line_54_8(); } while (0); else (void)0; sink (ptr); } while (0);
  do { char vla[n]; char *ptr = strcpy ((char*)vla, "123"); if (!(3 == strlen (&vla[1] - 1))) do { extern void call_in_true_branch_not_eliminated_on_line_55_9 (void); call_in_true_branch_not_eliminated_on_line_55_9(); } while (0); else (void)0; sink (ptr); } while (0);

  do { char vla[n]; char *ptr = strcpy ((char*)vla, ""); if (!(0 == strlen (ptr))) do { extern void call_in_true_branch_not_eliminated_on_line_57_10 (void); call_in_true_branch_not_eliminated_on_line_57_10(); } while (0); else (void)0; sink (ptr); } while (0);
  do { char vla[n]; char *ptr = strcpy ((char*)vla, "\0"); if (!(0 == strlen (ptr))) do { extern void call_in_true_branch_not_eliminated_on_line_58_11 (void); call_in_true_branch_not_eliminated_on_line_58_11(); } while (0); else (void)0; sink (ptr); } while (0);
  do { char vla[n]; char *ptr = strcpy ((char*)vla, "1"); if (!(1 == strlen (ptr))) do { extern void call_in_true_branch_not_eliminated_on_line_59_12 (void); call_in_true_branch_not_eliminated_on_line_59_12(); } while (0); else (void)0; sink (ptr); } while (0);
  do { char vla[n]; char *ptr = strcpy ((char*)vla, "12"); if (!(2 == strlen (ptr))) do { extern void call_in_true_branch_not_eliminated_on_line_60_13 (void); call_in_true_branch_not_eliminated_on_line_60_13(); } while (0); else (void)0; sink (ptr); } while (0);
  do { char vla[n]; char *ptr = strcpy ((char*)vla, "123"); if (!(3 == strlen (ptr))) do { extern void call_in_true_branch_not_eliminated_on_line_61_14 (void); call_in_true_branch_not_eliminated_on_line_61_14(); } while (0); else (void)0; sink (ptr); } while (0);

  do { char vla[n]; char *ptr = strcpy ((char*)vla, "123"); if (!(2 == strlen (ptr + 1))) do { extern void call_in_true_branch_not_eliminated_on_line_63_15 (void); call_in_true_branch_not_eliminated_on_line_63_15(); } while (0); else (void)0; sink (ptr); } while (0);
  do { char vla[n]; char *ptr = strcpy ((char*)vla, "123"); if (!(1 == strlen (&ptr[2]))) do { extern void call_in_true_branch_not_eliminated_on_line_64_16 (void); call_in_true_branch_not_eliminated_on_line_64_16(); } while (0); else (void)0; sink (ptr); } while (0);
  do { char vla[n]; char *ptr = strcpy ((char*)vla, "123"); if (!(0 == strlen (&ptr[1] + 2))) do { extern void call_in_true_branch_not_eliminated_on_line_65_17 (void); call_in_true_branch_not_eliminated_on_line_65_17(); } while (0); else (void)0; sink (ptr); } while (0);
}

void test_int16_vla_local (int n)
{
  do { int16_t vla[n]; char *ptr = strcpy ((char*)vla, ""); if (!(0 == strlen ((char*)vla))) do { extern void call_in_true_branch_not_eliminated_on_line_70_18 (void); call_in_true_branch_not_eliminated_on_line_70_18(); } while (0); else (void)0; sink (ptr); } while (0);
  do { int16_t vla[n]; char *ptr = strcpy ((char*)vla, "\0"); if (!(0 == strlen ((char*)vla))) do { extern void call_in_true_branch_not_eliminated_on_line_71_19 (void); call_in_true_branch_not_eliminated_on_line_71_19(); } while (0); else (void)0; sink (ptr); } while (0);
  do { int16_t vla[n]; char *ptr = strcpy ((char*)vla, "1"); if (!(1 == strlen ((char*)vla))) do { extern void call_in_true_branch_not_eliminated_on_line_72_20 (void); call_in_true_branch_not_eliminated_on_line_72_20(); } while (0); else (void)0; sink (ptr); } while (0);
  do { int16_t vla[n]; char *ptr = strcpy ((char*)vla, "12"); if (!(2 == strlen ((char*)vla))) do { extern void call_in_true_branch_not_eliminated_on_line_73_21 (void); call_in_true_branch_not_eliminated_on_line_73_21(); } while (0); else (void)0; sink (ptr); } while (0);
  do { int16_t vla[n]; char *ptr = strcpy ((char*)vla, "123"); if (!(3 == strlen ((char*)vla))) do { extern void call_in_true_branch_not_eliminated_on_line_74_22 (void); call_in_true_branch_not_eliminated_on_line_74_22(); } while (0); else (void)0; sink (ptr); } while (0);

  do { int16_t vla[n]; char *ptr = strcpy ((char*)vla, "1234"); if (!(2 == strlen ((char*)(vla + 1)))) do { extern void call_in_true_branch_not_eliminated_on_line_76_23 (void); call_in_true_branch_not_eliminated_on_line_76_23(); } while (0); else (void)0; sink (ptr); } while (0);
  do { int16_t vla[n]; char *ptr = strcpy ((char*)vla, "123456"); if (!(2 == strlen ((char*)&vla[2]))) do { extern void call_in_true_branch_not_eliminated_on_line_77_24 (void); call_in_true_branch_not_eliminated_on_line_77_24(); } while (0); else (void)0; sink (ptr); } while (0);
  do { int16_t vla[n]; char *ptr = strcpy ((char*)vla, "123456"); if (!(1 == strlen ((char*)&vla[2] + 1))) do { extern void call_in_true_branch_not_eliminated_on_line_78_25 (void); call_in_true_branch_not_eliminated_on_line_78_25(); } while (0); else (void)0; sink (ptr); } while (0);
  do { int16_t vla[n]; char *ptr = strcpy ((char*)vla, "123456"); if (!(0 == strlen ((char*)&vla[2] + 2))) do { extern void call_in_true_branch_not_eliminated_on_line_79_26 (void); call_in_true_branch_not_eliminated_on_line_79_26(); } while (0); else (void)0; sink (ptr); } while (0);
  do { int16_t vla[n]; char *ptr = strcpy ((char*)vla, "123456"); if (!(0 == strlen ((char*)(&vla[1] + 2)))) do { extern void call_in_true_branch_not_eliminated_on_line_80_27 (void); call_in_true_branch_not_eliminated_on_line_80_27(); } while (0); else (void)0; sink (ptr); } while (0);

  do { int16_t vla[n]; char *ptr = strcpy ((char*)vla, "123456"); if (!(3 == strlen ((char*)&vla[2] - 1))) do { extern void call_in_true_branch_not_eliminated_on_line_82_28 (void); call_in_true_branch_not_eliminated_on_line_82_28(); } while (0); else (void)0; sink (ptr); } while (0);
  do { int16_t vla[n]; char *ptr = strcpy ((char*)vla, "123456"); if (!(4 == strlen ((char*)&vla[2] - 2))) do { extern void call_in_true_branch_not_eliminated_on_line_83_29 (void); call_in_true_branch_not_eliminated_on_line_83_29(); } while (0); else (void)0; sink (ptr); } while (0);

  do { int16_t vla[n]; char *ptr = strcpy ((char*)vla, ""); if (!(0 == strlen (ptr))) do { extern void call_in_true_branch_not_eliminated_on_line_85_30 (void); call_in_true_branch_not_eliminated_on_line_85_30(); } while (0); else (void)0; sink (ptr); } while (0);
  do { int16_t vla[n]; char *ptr = strcpy ((char*)vla, "\0"); if (!(0 == strlen (ptr))) do { extern void call_in_true_branch_not_eliminated_on_line_86_31 (void); call_in_true_branch_not_eliminated_on_line_86_31(); } while (0); else (void)0; sink (ptr); } while (0);
  do { int16_t vla[n]; char *ptr = strcpy ((char*)vla, "1"); if (!(1 == strlen (ptr))) do { extern void call_in_true_branch_not_eliminated_on_line_87_32 (void); call_in_true_branch_not_eliminated_on_line_87_32(); } while (0); else (void)0; sink (ptr); } while (0);
  do { int16_t vla[n]; char *ptr = strcpy ((char*)vla, "12"); if (!(2 == strlen (ptr))) do { extern void call_in_true_branch_not_eliminated_on_line_88_33 (void); call_in_true_branch_not_eliminated_on_line_88_33(); } while (0); else (void)0; sink (ptr); } while (0);
  do { int16_t vla[n]; char *ptr = strcpy ((char*)vla, "123"); if (!(3 == strlen (ptr))) do { extern void call_in_true_branch_not_eliminated_on_line_89_34 (void); call_in_true_branch_not_eliminated_on_line_89_34(); } while (0); else (void)0; sink (ptr); } while (0);

  do { int16_t vla[n]; char *ptr = strcpy ((char*)vla, "123"); if (!(2 == strlen (ptr + 1))) do { extern void call_in_true_branch_not_eliminated_on_line_91_35 (void); call_in_true_branch_not_eliminated_on_line_91_35(); } while (0); else (void)0; sink (ptr); } while (0);
  do { int16_t vla[n]; char *ptr = strcpy ((char*)vla, "123"); if (!(1 == strlen (&ptr[2]))) do { extern void call_in_true_branch_not_eliminated_on_line_92_36 (void); call_in_true_branch_not_eliminated_on_line_92_36(); } while (0); else (void)0; sink (ptr); } while (0);
  do { int16_t vla[n]; char *ptr = strcpy ((char*)vla, "123"); if (!(0 == strlen (&ptr[1] + 2))) do { extern void call_in_true_branch_not_eliminated_on_line_93_37 (void); call_in_true_branch_not_eliminated_on_line_93_37(); } while (0); else (void)0; sink (ptr); } while (0);
}

void test_int_vla_local (int n)
{
  do { int16_t vla[n]; char *ptr = strcpy ((char*)vla, ""); if (!(0 == strlen ((char*)vla))) do { extern void call_in_true_branch_not_eliminated_on_line_98_38 (void); call_in_true_branch_not_eliminated_on_line_98_38(); } while (0); else (void)0; sink (ptr); } while (0);
  do { int16_t vla[n]; char *ptr = strcpy ((char*)vla, "\0"); if (!(0 == strlen ((char*)vla))) do { extern void call_in_true_branch_not_eliminated_on_line_99_39 (void); call_in_true_branch_not_eliminated_on_line_99_39(); } while (0); else (void)0; sink (ptr); } while (0);
  do { int16_t vla[n]; char *ptr = strcpy ((char*)vla, "1"); if (!(1 == strlen ((char*)vla))) do { extern void call_in_true_branch_not_eliminated_on_line_100_40 (void); call_in_true_branch_not_eliminated_on_line_100_40(); } while (0); else (void)0; sink (ptr); } while (0);
  do { int16_t vla[n]; char *ptr = strcpy ((char*)vla, "12"); if (!(2 == strlen ((char*)vla))) do { extern void call_in_true_branch_not_eliminated_on_line_101_41 (void); call_in_true_branch_not_eliminated_on_line_101_41(); } while (0); else (void)0; sink (ptr); } while (0);
  do { int16_t vla[n]; char *ptr = strcpy ((char*)vla, "123"); if (!(3 == strlen ((char*)vla))) do { extern void call_in_true_branch_not_eliminated_on_line_102_42 (void); call_in_true_branch_not_eliminated_on_line_102_42(); } while (0); else (void)0; sink (ptr); } while (0);

  do { int16_t vla[n]; char *ptr = strcpy ((char*)vla, "1234"); if (!(2 == strlen ((char*)(vla + 1)))) do { extern void call_in_true_branch_not_eliminated_on_line_104_43 (void); call_in_true_branch_not_eliminated_on_line_104_43(); } while (0); else (void)0; sink (ptr); } while (0);
  do { int16_t vla[n]; char *ptr = strcpy ((char*)vla, "123456"); if (!(2 == strlen ((char*)&vla[2]))) do { extern void call_in_true_branch_not_eliminated_on_line_105_44 (void); call_in_true_branch_not_eliminated_on_line_105_44(); } while (0); else (void)0; sink (ptr); } while (0);
  do { int16_t vla[n]; char *ptr = strcpy ((char*)vla, "123456"); if (!(1 == strlen ((char*)&vla[2] + 1))) do { extern void call_in_true_branch_not_eliminated_on_line_106_45 (void); call_in_true_branch_not_eliminated_on_line_106_45(); } while (0); else (void)0; sink (ptr); } while (0);
  do { int16_t vla[n]; char *ptr = strcpy ((char*)vla, "123456"); if (!(0 == strlen ((char*)&vla[2] + 2))) do { extern void call_in_true_branch_not_eliminated_on_line_107_46 (void); call_in_true_branch_not_eliminated_on_line_107_46(); } while (0); else (void)0; sink (ptr); } while (0);
  do { int16_t vla[n]; char *ptr = strcpy ((char*)vla, "123456"); if (!(0 == strlen ((char*)(&vla[1] + 2)))) do { extern void call_in_true_branch_not_eliminated_on_line_108_47 (void); call_in_true_branch_not_eliminated_on_line_108_47(); } while (0); else (void)0; sink (ptr); } while (0);

  do { int vla[n]; char *ptr = strcpy ((char*)vla, ""); if (!(0 == strlen (ptr))) do { extern void call_in_true_branch_not_eliminated_on_line_110_48 (void); call_in_true_branch_not_eliminated_on_line_110_48(); } while (0); else (void)0; sink (ptr); } while (0);
  do { int vla[n]; char *ptr = strcpy ((char*)vla, "\0"); if (!(0 == strlen (ptr))) do { extern void call_in_true_branch_not_eliminated_on_line_111_49 (void); call_in_true_branch_not_eliminated_on_line_111_49(); } while (0); else (void)0; sink (ptr); } while (0);
  do { int vla[n]; char *ptr = strcpy ((char*)vla, "1"); if (!(1 == strlen (ptr))) do { extern void call_in_true_branch_not_eliminated_on_line_112_50 (void); call_in_true_branch_not_eliminated_on_line_112_50(); } while (0); else (void)0; sink (ptr); } while (0);
  do { int vla[n]; char *ptr = strcpy ((char*)vla, "12"); if (!(2 == strlen (ptr))) do { extern void call_in_true_branch_not_eliminated_on_line_113_51 (void); call_in_true_branch_not_eliminated_on_line_113_51(); } while (0); else (void)0; sink (ptr); } while (0);
  do { int vla[n]; char *ptr = strcpy ((char*)vla, "123"); if (!(3 == strlen (ptr))) do { extern void call_in_true_branch_not_eliminated_on_line_114_52 (void); call_in_true_branch_not_eliminated_on_line_114_52(); } while (0); else (void)0; sink (ptr); } while (0);

  do { int vla[n]; char *ptr = strcpy ((char*)vla, "123"); if (!(2 == strlen (ptr + 1))) do { extern void call_in_true_branch_not_eliminated_on_line_116_53 (void); call_in_true_branch_not_eliminated_on_line_116_53(); } while (0); else (void)0; sink (ptr); } while (0);
  do { int vla[n]; char *ptr = strcpy ((char*)vla, "123"); if (!(1 == strlen (&ptr[2]))) do { extern void call_in_true_branch_not_eliminated_on_line_117_54 (void); call_in_true_branch_not_eliminated_on_line_117_54(); } while (0); else (void)0; sink (ptr); } while (0);
  do { int vla[n]; char *ptr = strcpy ((char*)vla, "123"); if (!(0 == strlen (&ptr[1] + 2))) do { extern void call_in_true_branch_not_eliminated_on_line_118_55 (void); call_in_true_branch_not_eliminated_on_line_118_55(); } while (0); else (void)0; sink (ptr); } while (0);
}
# 134 "./strlenopt-62.c"
void test_char_array_ptr (char (**ppa)[])
{
  do { char (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, ""); (void)&ptr; if (!(0 == strlen (*parray))) do { extern void call_in_true_branch_not_eliminated_on_line_136_56 (void); call_in_true_branch_not_eliminated_on_line_136_56(); } while (0); else (void)0; } while (0);
  do { char (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, ""); (void)&ptr; if (!(0 == strlen (&(*parray)[0]))) do { extern void call_in_true_branch_not_eliminated_on_line_137_57 (void); call_in_true_branch_not_eliminated_on_line_137_57(); } while (0); else (void)0; } while (0);

  do { char (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "1"); (void)&ptr; if (!(1 == strlen (&(*parray)[0]))) do { extern void call_in_true_branch_not_eliminated_on_line_139_58 (void); call_in_true_branch_not_eliminated_on_line_139_58(); } while (0); else (void)0; } while (0);
  do { char (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "1"); (void)&ptr; if (!(0 == strlen (&(*parray)[1]))) do { extern void call_in_true_branch_not_eliminated_on_line_140_59 (void); call_in_true_branch_not_eliminated_on_line_140_59(); } while (0); else (void)0; } while (0);

  do { char (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "12"); (void)&ptr; if (!(2 == strlen (&(*parray)[0]))) do { extern void call_in_true_branch_not_eliminated_on_line_142_60 (void); call_in_true_branch_not_eliminated_on_line_142_60(); } while (0); else (void)0; } while (0);
  do { char (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "12"); (void)&ptr; if (!(1 == strlen (&(*parray)[1]))) do { extern void call_in_true_branch_not_eliminated_on_line_143_61 (void); call_in_true_branch_not_eliminated_on_line_143_61(); } while (0); else (void)0; } while (0);
  do { char (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "12"); (void)&ptr; if (!(0 == strlen (&(*parray)[2]))) do { extern void call_in_true_branch_not_eliminated_on_line_144_62 (void); call_in_true_branch_not_eliminated_on_line_144_62(); } while (0); else (void)0; } while (0);

  do { char (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "123"); (void)&ptr; if (!(3 == strlen (&(*parray)[0]))) do { extern void call_in_true_branch_not_eliminated_on_line_146_63 (void); call_in_true_branch_not_eliminated_on_line_146_63(); } while (0); else (void)0; } while (0);
  do { char (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "123"); (void)&ptr; if (!(2 == strlen (&(*parray)[1]))) do { extern void call_in_true_branch_not_eliminated_on_line_147_64 (void); call_in_true_branch_not_eliminated_on_line_147_64(); } while (0); else (void)0; } while (0);
  do { char (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "123"); (void)&ptr; if (!(1 == strlen (&(*parray)[2]))) do { extern void call_in_true_branch_not_eliminated_on_line_148_65 (void); call_in_true_branch_not_eliminated_on_line_148_65(); } while (0); else (void)0; } while (0);
  do { char (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "123"); (void)&ptr; if (!(0 == strlen (&(*parray)[3]))) do { extern void call_in_true_branch_not_eliminated_on_line_149_66 (void); call_in_true_branch_not_eliminated_on_line_149_66(); } while (0); else (void)0; } while (0);

  do { char (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "123"); (void)&ptr; if (!(3 == strlen (ptr))) do { extern void call_in_true_branch_not_eliminated_on_line_151_67 (void); call_in_true_branch_not_eliminated_on_line_151_67(); } while (0); else (void)0; } while (0);
  do { char (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "123"); (void)&ptr; if (!(2 == strlen (&ptr[1]))) do { extern void call_in_true_branch_not_eliminated_on_line_152_68 (void); call_in_true_branch_not_eliminated_on_line_152_68(); } while (0); else (void)0; } while (0);
  do { char (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "123"); (void)&ptr; if (!(1 == strlen (&ptr[2]))) do { extern void call_in_true_branch_not_eliminated_on_line_153_69 (void); call_in_true_branch_not_eliminated_on_line_153_69(); } while (0); else (void)0; } while (0);
  do { char (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "123"); (void)&ptr; if (!(0 == strlen (&ptr[3]))) do { extern void call_in_true_branch_not_eliminated_on_line_154_70 (void); call_in_true_branch_not_eliminated_on_line_154_70(); } while (0); else (void)0; } while (0);
}

void test_int16_array_ptr (int16_t (**ppa)[])
{
  do { int16_t (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, ""); (void)&ptr; if (!(0 == strlen ((char*)*parray))) do { extern void call_in_true_branch_not_eliminated_on_line_159_71 (void); call_in_true_branch_not_eliminated_on_line_159_71(); } while (0); else (void)0; } while (0);
  do { int16_t (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, ""); (void)&ptr; if (!(0 == strlen ((char*)&(*parray)[0]))) do { extern void call_in_true_branch_not_eliminated_on_line_160_72 (void); call_in_true_branch_not_eliminated_on_line_160_72(); } while (0); else (void)0; } while (0);

  do { int16_t (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "1"); (void)&ptr; if (!(1 == strlen ((char*)&(*parray)[0]))) do { extern void call_in_true_branch_not_eliminated_on_line_162_73 (void); call_in_true_branch_not_eliminated_on_line_162_73(); } while (0); else (void)0; } while (0);
  do { int16_t (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "12"); (void)&ptr; if (!(0 == strlen ((char*)&(*parray)[1]))) do { extern void call_in_true_branch_not_eliminated_on_line_163_74 (void); call_in_true_branch_not_eliminated_on_line_163_74(); } while (0); else (void)0; } while (0);

  do { int16_t (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "12"); (void)&ptr; if (!(2 == strlen ((char*)&(*parray)[0]))) do { extern void call_in_true_branch_not_eliminated_on_line_165_75 (void); call_in_true_branch_not_eliminated_on_line_165_75(); } while (0); else (void)0; } while (0);
  do { int16_t (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "12"); (void)&ptr; if (!(1 == strlen ((char*)&(*parray)[0] + 1))) do { extern void call_in_true_branch_not_eliminated_on_line_166_76 (void); call_in_true_branch_not_eliminated_on_line_166_76(); } while (0); else (void)0; } while (0);
  do { int16_t (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "1234"); (void)&ptr; if (!(2 == strlen ((char*)&(*parray)[1]))) do { extern void call_in_true_branch_not_eliminated_on_line_167_77 (void); call_in_true_branch_not_eliminated_on_line_167_77(); } while (0); else (void)0; } while (0);
  do { int16_t (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "1234"); (void)&ptr; if (!(1 == strlen ((char*)&(*parray)[1] + 1))) do { extern void call_in_true_branch_not_eliminated_on_line_168_78 (void); call_in_true_branch_not_eliminated_on_line_168_78(); } while (0); else (void)0; } while (0);
  do { int16_t (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "1234"); (void)&ptr; if (!(0 == strlen ((char*)&(*parray)[2]))) do { extern void call_in_true_branch_not_eliminated_on_line_169_79 (void); call_in_true_branch_not_eliminated_on_line_169_79(); } while (0); else (void)0; } while (0);

  do { int16_t (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "123456"); (void)&ptr; if (!(6 == strlen ((char*)&(*parray)[0]))) do { extern void call_in_true_branch_not_eliminated_on_line_171_80 (void); call_in_true_branch_not_eliminated_on_line_171_80(); } while (0); else (void)0; } while (0);
  do { int16_t (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "123456"); (void)&ptr; if (!(5 == strlen ((char*)&(*parray)[0] + 1))) do { extern void call_in_true_branch_not_eliminated_on_line_172_81 (void); call_in_true_branch_not_eliminated_on_line_172_81(); } while (0); else (void)0; } while (0);
  do { int16_t (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "123456"); (void)&ptr; if (!(0 == strlen ((char*)&(*parray)[0] + 6))) do { extern void call_in_true_branch_not_eliminated_on_line_173_82 (void); call_in_true_branch_not_eliminated_on_line_173_82(); } while (0); else (void)0; } while (0);
  do { int16_t (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "123456"); (void)&ptr; if (!(4 == strlen ((char*)&(*parray)[1]))) do { extern void call_in_true_branch_not_eliminated_on_line_174_83 (void); call_in_true_branch_not_eliminated_on_line_174_83(); } while (0); else (void)0; } while (0);
  do { int16_t (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "123456"); (void)&ptr; if (!(3 == strlen ((char*)&(*parray)[1] + 1))) do { extern void call_in_true_branch_not_eliminated_on_line_175_84 (void); call_in_true_branch_not_eliminated_on_line_175_84(); } while (0); else (void)0; } while (0);
  do { int16_t (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "123456"); (void)&ptr; if (!(0 == strlen ((char*)&(*parray)[1] + 4))) do { extern void call_in_true_branch_not_eliminated_on_line_176_85 (void); call_in_true_branch_not_eliminated_on_line_176_85(); } while (0); else (void)0; } while (0);
  do { int16_t (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "123456"); (void)&ptr; if (!(2 == strlen ((char*)&(*parray)[2]))) do { extern void call_in_true_branch_not_eliminated_on_line_177_86 (void); call_in_true_branch_not_eliminated_on_line_177_86(); } while (0); else (void)0; } while (0);
  do { int16_t (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "123456"); (void)&ptr; if (!(1 == strlen ((char*)&(*parray)[2] + 1))) do { extern void call_in_true_branch_not_eliminated_on_line_178_87 (void); call_in_true_branch_not_eliminated_on_line_178_87(); } while (0); else (void)0; } while (0);
  do { int16_t (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "123456"); (void)&ptr; if (!(0 == strlen ((char*)&(*parray)[2] + 2))) do { extern void call_in_true_branch_not_eliminated_on_line_179_88 (void); call_in_true_branch_not_eliminated_on_line_179_88(); } while (0); else (void)0; } while (0);
  do { int16_t (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "123456"); (void)&ptr; if (!(0 == strlen ((char*)&(*parray)[3]))) do { extern void call_in_true_branch_not_eliminated_on_line_180_89 (void); call_in_true_branch_not_eliminated_on_line_180_89(); } while (0); else (void)0; } while (0);

  do { int16_t (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "123"); (void)&ptr; if (!(3 == strlen ((char*)ptr))) do { extern void call_in_true_branch_not_eliminated_on_line_182_90 (void); call_in_true_branch_not_eliminated_on_line_182_90(); } while (0); else (void)0; } while (0);
  do { int16_t (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "123"); (void)&ptr; if (!(2 == strlen ((char*)&ptr[1]))) do { extern void call_in_true_branch_not_eliminated_on_line_183_91 (void); call_in_true_branch_not_eliminated_on_line_183_91(); } while (0); else (void)0; } while (0);
  do { int16_t (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "123"); (void)&ptr; if (!(1 == strlen ((char*)&ptr[2]))) do { extern void call_in_true_branch_not_eliminated_on_line_184_92 (void); call_in_true_branch_not_eliminated_on_line_184_92(); } while (0); else (void)0; } while (0);
  do { int16_t (*parray)[] = *ppa++; char *ptr = strcpy ((char*)*parray, "123"); (void)&ptr; if (!(0 == strlen ((char*)&ptr[3]))) do { extern void call_in_true_branch_not_eliminated_on_line_185_93 (void); call_in_true_branch_not_eliminated_on_line_185_93(); } while (0); else (void)0; } while (0);
}
