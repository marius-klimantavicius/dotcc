//options_all:-r -x -tused
//options: --microsoft -n;cn

class A { int i; };
class B { int j; };
class __single_inheritance D : A, B { char *pc; };  // Error

