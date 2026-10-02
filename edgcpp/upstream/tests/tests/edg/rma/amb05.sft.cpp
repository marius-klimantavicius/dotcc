//options_all:-r -x -tused
//options: --strict;cn

class A { public: typedef int X; float Y; };
class B { public: float X; typedef int Y; };
class C : public A, public B { X a; Y b; };

