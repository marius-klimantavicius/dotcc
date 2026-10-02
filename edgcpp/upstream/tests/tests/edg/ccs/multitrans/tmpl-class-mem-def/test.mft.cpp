//type:cp
//options_all:--multi_trans -tused -A -w --diag_warning=1055,768,2949 --diag_suppress=1204,961
//source_files:tmpl-class-mem-def-p2.C tmpl-class-mem-def-p3.C
//require:COMPILE_MULTIPLE_TRANSLATION_UNITS 1

template<typename T> struct X {
  struct Y;
};

X<bool> var1;
