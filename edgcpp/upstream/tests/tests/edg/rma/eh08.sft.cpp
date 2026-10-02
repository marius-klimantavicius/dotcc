//options_all:-r -x -tused
//options: --strict;cp

void f(int) throw (int) { }
void f() throw () { }
void f(char) throw (int,char) { }
void f(int,int) { }

