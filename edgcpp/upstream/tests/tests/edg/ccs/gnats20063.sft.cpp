//type:cp
//options:--gcc:--g++
//options_all:--diag_error 1143
//require:GCC_IS_GENERATED_CODE_TARGET 1
void* f(void *p)
{
  void* p1 = __extension__({ p + 0; });
  p1 = __extension__(p + 0);
  return p1;
}
