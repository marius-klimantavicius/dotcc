//options_all:-r -x -tused
//options: --strict;cn

void f();
void ff() throw(int);
void (&pf1)() = f;
void (&pf2)() = ff;
void (&pf3)() throw(int) = f;         // Error
void (&pf4)() throw(int) = ff;
void (&pf5)() throw() = f;            // Error
void (&pf6)() throw() = ff;           // Error


