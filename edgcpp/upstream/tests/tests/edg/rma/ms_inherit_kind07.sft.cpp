//options_all:-r -x -tused
//options: --microsoft -n;cn

class A { int i; };
class B { int j; };
class __single_inheritance D;
class D : A, B { char *pc; };  // error
class __multiple_inheritance E;
class E : virtual A, virtual B { };  // error

