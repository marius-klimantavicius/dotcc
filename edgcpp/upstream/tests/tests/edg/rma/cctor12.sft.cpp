//options_all:-r -x -tused
//options: --strict;cn

struct T { T(T&); T(); };
struct X { const T t; X(); };
X a;
X b(a);
X c = a;



