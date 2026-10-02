//options_all:-r -x -tused
//options: --strict;cp

class A {
	A(int&);
};

class B {
	friend A::A(int&);
};


