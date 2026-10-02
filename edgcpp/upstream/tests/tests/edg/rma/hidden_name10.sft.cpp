//options_all:-r -x -tused
//options: --strict;cp

typedef struct S { } T;
T x;
namespace N {
  typedef struct S T;
  static struct S x;
}

