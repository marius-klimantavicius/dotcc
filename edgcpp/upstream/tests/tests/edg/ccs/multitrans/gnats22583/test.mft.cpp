//type:fn
//options:-DINLINE= -DDEFAULT=:-DINLINE=inline -DDEFAULT=:-DINLINE= -DDEFAULT="= default";cp
//options_all:--gnu_version 80999 --multi_trans_unit
//source_files:gnats22583-p2.C
//require:COMPILE_MULTIPLE_TRANSLATION_UNITS 1

template<typename T> struct A {
  int var = 0;
  INLINE A() DEFAULT;
};
A<int> a;
