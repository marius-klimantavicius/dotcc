//options_all:-r -x -tused
//options: --microsoft -n;cn

class A { int i; };
class B { int j; };
class D;
char *D:: *pmd;                 // should be an error according to doc
class __multiple_inheritance D : A, B { char *pc; };  // actual error

