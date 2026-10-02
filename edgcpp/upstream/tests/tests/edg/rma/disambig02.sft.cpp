//options_all:-r -x -tused
//options: --strict;cp

class A { public: A(int); operator int(); };
A a(1);
A b((int)a);
A c(int(a));

