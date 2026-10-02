//options_all:-r -x -tused
//options: --strict;cn

struct S;
struct T {
  static struct S sm1;
  static struct S sm2[2];
  static struct S sm3[];
  static void sm4;
  static int sm5[];
};
S T::sm1;
S T::sm2[];
S T::sm3[];
void T::sm4;
int T::sm5[];

