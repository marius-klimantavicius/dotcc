//type:fn
//options_all:--multi_trans -tused
//source_files:tmpl-class-mem-mismatch-p2.C
//require:COMPILE_MULTIPLE_TRANSLATION_UNITS 1

template <typename> class X {
  enum k {};
  union {
    double x;
  };
  union {
    double y;
  };
  union {
    double z;
  };
};
