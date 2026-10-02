//options_all:-r -x -tused
//options: --microsoft -n;cp

class A { int i; };
class B { int j; };
class D;
class __multiple_inheritance D : A, B { char *pc; };  // no error

