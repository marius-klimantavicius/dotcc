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

__interface A {
	virtual void f() { printf("A::f\n"); }
};

__interface B {
	virtual void f() { printf("B::f\n"); }
};

struct D: A, B {
	virtual void A::f() { printf("D[A]::f\n"); }
	virtual void B::f() { printf("D[B]::f\n"); }
};

int main() {
	D *pd = new D;
	((A*)pd)->f();
	((B*)pd)->f();
}
