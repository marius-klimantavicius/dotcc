//options_all:-r -x -tused
//options: --strict;cn

class A {
	virtual void f(int) = 0;
};

class B : public virtual A { };
class C : public virtual A { };
class D : public virtual A { };

class E : public A, public B, public C, public D { };

E b;

