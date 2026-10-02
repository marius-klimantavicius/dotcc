//options_all:-r -x -tused
//options: --strict;cp

class A { friend void f(); };
inline void f() { }

