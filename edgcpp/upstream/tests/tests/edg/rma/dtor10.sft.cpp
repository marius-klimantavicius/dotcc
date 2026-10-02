//options_all:-r -x -tused
//options: --strict;cn

class A {
	A() {}
};

class B {
protected:
	~B() {}
};

//test 4
B b[] = {B()};


