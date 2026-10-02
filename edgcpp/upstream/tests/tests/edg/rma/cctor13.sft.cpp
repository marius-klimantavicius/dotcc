//options_all:-r -x -tused
//options: --strict;cn

struct T { T(T&); T(); };
struct X { volatile T t; X(); };
const X a;
X b(a);
X c = a;

