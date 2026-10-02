//options_all:-r -x -tused
//options: --strict;cn

void f() throw(int(13)){}
void g() throw(float,int(13),int(0)) {}
void h() throw int(13) {}
void i() throw(int(13),int(0)){}

