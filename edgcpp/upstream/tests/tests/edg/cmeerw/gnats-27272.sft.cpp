//type:fp
//options:--gn 130200;fn:--gn 130300:--gn 140100
//options_all:--c++ --target linux_x86_64

void f(const int ci, int i)
{
  __builtin_ia32_ldtilecfg(&ci);
  __builtin_ia32_ldtilecfg(&i);
  __builtin_ia32_sttilecfg(&i);
}
