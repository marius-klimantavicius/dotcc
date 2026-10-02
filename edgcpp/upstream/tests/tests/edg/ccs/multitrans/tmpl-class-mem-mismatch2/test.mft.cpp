//type:fn
//options_all:--multi_trans -tused
//source_files:tmpl-class-mem-mismatch2-p2.C
//require:COMPILE_MULTIPLE_TRANSLATION_UNITS 1

template<typename T>
struct X {
  enum E {};
  struct N;
};

struct Y {
  friend struct X<double>::N;
};
