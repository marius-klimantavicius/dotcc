//options_all:-r -x -tused
//options: --strict;cn

void (*pf1)() throw();
void ((*pf2))() throw();
void (*(pf3))() throw();

void (**ppf1)() throw();		// error
void ((**ppf2))() throw();		// error
void (*(*ppf3))() throw();		// error
void (**(ppf4))() throw();		// error

void f(void (*)() throw (),
       void (**)() throw());		// error

class A;
void (A::*pmf1)() throw();		// error
void (A::**ppmf1)() throw();		// error

void (*pfa1[10])() throw();		// error
void (*(pfa1[10]))() throw();		// error

void (**ppfa1[10])() throw();           // error
void (*(*(ppfa2[10])))() throw();       // error

