//options_all:-r -x -tused
//options: --strict;cn

struct X { };
struct Y { };
struct Z : public X, public Y { };

struct A {
	virtual void f() throw(X);
        virtual void f(int) throw(X);
        virtual void g() throw(X);
        virtual void g(int) throw(X);
        virtual void h() throw(Z);
};

struct B {
	virtual void f() throw(Y);
};

struct C : public A, public B {
	virtual void f() throw(Z);
        virtual void g() throw(Y);         // Error
        virtual void g(int) throw();
        virtual void f(int);               // Error
        virtual void h() throw(X);         // Error
};



