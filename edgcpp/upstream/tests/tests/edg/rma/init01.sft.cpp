//options_all:-r -x -tused
//options: --strict;cp

class V { public: int a; };
class A : public virtual V {public: int i, j; };
static A x;
static A y = x;

