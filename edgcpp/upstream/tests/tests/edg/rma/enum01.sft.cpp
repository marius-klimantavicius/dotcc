//options_all:-r -x -tused --diag_suppress=102
//options: --strict;cn:;cn

enum E { e1, e2 };
class A {
  enum E;
  E e;
  enum EE;
  EE ee;
};
enum EE { ee1, ee2 };

