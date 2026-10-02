//options_all:-r -x -tused
//options: --strict;cn:;rp

#include <stdio.h>

class W
{                             
public:
virtual	void f(void);
virtual	void g(void);
virtual	void h(void);
virtual	void k(void);
};


void W::f(void)  {printf ("In W::f\n");}

void W::g(void)  {printf ("In W::g\n");}

void W::h(void)  {printf ("In W::h\n");}

void W::k(void)  {printf ("In W::k\n");}

class A : public virtual W
{
public:
	void g(void);
};

void A::g(void)  {printf ("In A::g\n");}

class B : public virtual W
{
public:
	void f(void);
};

void B::f(void)  {printf ("In B::f\n");}

class C : public A, public B, public virtual W
{
public:
	void h(void);
	void f(void);
	void g(void);
};

void C::f(void)
{
	B::f();
}

void C::g(void)
{
	A::g();
}

void C::h(void)  {printf ("In C::h\n");}

main()
{
	C *pc = new C;

	printf ("\nOutput of pc\n\n");


	pc->f();
	pc->g();
	pc->h();
	((A*)pc)->f();
	((W*)pc)->f();

	B* pb = new B;

	printf ("\nOutput of pb\n\n");
	pb->f();
	pb->g();
	((W*)pb)->f();

	A* pa = new A;

	printf ("\nOutput of pa\n\n");
	pa->f();
	pa->g();
	((W*)pb)->g();

	return 0;
}

