//options_all:-r -x -tused
//options: --cfront_3.0;cn

class A { int& i; };     // cfront: error     cpfe: okay
class B { A a; } x;      // cfront: okay      cpfe: warning
class C { B b[3]; } y;   // cfront: okay      cpfe: warning

