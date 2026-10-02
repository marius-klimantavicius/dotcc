//remark:ext_vector_type conversions
//options:--clang_v=34200;fp

  typedef float VF4 __attribute__((ext_vector_type(4)));
  VF4 vec = (VF4)1;

