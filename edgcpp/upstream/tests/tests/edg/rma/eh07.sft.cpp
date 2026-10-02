//options_all:-r -x -tused
//options: --strict;cn:;cn

void f() throw (int,char);
void f() throw (int) { };
void f() throw ();

