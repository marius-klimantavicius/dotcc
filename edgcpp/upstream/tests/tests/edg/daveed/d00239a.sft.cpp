//remark:Microsoft selective overriding
//type:fp
//name:
//options:
//options_all:--microsoft
//cases:
//source_files:
//input_files:  
//output_files:
//ulimit:
//linker_options:
//execution_args:

extern "C" int printf(char const*, ...);

struct A {
	virtual void f();
};

void A::f() { printf("A\n"); }

struct B {
	virtual void f() = 0;
};

void B::f() { printf("B\n"); }

struct C1: B {};
struct C2: B {};

struct D: A, C1, C2 {
	virtual void B::f() { printf("D's B::f\n"); }
};

int main() {
	B *pd1 = (C1*)new D;
	B *pd2 = (C2*)new D;
	A *pd3 = (A*)new D;
	pd1->f();
	pd2->f();
	pd3->f();
}

