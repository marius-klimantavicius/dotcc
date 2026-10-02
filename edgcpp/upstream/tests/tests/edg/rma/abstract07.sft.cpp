//options_all:-r -x -tused
//options: --strict;cn

class A {
	virtual void f(int) = 0;
};

class B : public virtual A { };
class C : public virtual A { };
class D : public B, public C { };

D d;   // Error

class E : public virtual D { };
class F : public virtual D { };
class G : public E, public F { };


G g;   // Error


