//options_all:-r -x -tused
//options: --strict;cn:;cp

struct blah;
void f() throw(blah);
struct blah {};
void f() throw(blah){}

