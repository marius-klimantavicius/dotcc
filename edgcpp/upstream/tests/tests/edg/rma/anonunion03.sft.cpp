//options_all:-r -x -tused
//options: --strict;cp

class V { };
class A : virtual public V {
  union { int a, b; };
};

