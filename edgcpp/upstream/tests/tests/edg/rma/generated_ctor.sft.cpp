//options_all:-r -x -tused
//options: --strict;cn

class A { A(); };
class B : public A {};
B::B() {}

