//type:fp
//options:--gn 110400;fn:--gn 120100;fn:--gn 110500:--gn 120400
//options_all:--c++ --target linux_x86_64

void f(const int ci, int i)
{
  __builtin_ia32_ldtilecfg(&ci);
  __builtin_ia32_ldtilecfg(&i);
  __builtin_ia32_sttilecfg(&i);
}
