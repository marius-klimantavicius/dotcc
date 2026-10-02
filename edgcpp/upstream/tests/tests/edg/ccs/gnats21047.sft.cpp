//type:cp
//options_all:--c++17 --egrep_c_pattern 'using.*f01,\\s*N02::f02;'
//script:edg-egrep-c
//require:BACK_END_IS_CP_GEN_BE 1

int f00();

namespace N01 {
  int f01();
}
namespace N02 {
  int f02();
  int f02(int);
  int f02(float);
}
namespace N {
  using ::f00, N01::f01, N02::f02;
}

int main(void)
{
  N::f02();
}
