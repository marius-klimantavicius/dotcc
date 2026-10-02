//remark:ext_vector_type conversions
//options:--clang_v=34200;fp

typedef signed short int16s;
typedef signed int int32s;
typedef int16s __vec16s __attribute__((ext_vector_type(32)));
typedef __vec16s vec16s __attribute__((aligned(2)));

void func(vec16s param){
  int32s p = 0;
  vec16s c = param+(vec16s)p;
}
