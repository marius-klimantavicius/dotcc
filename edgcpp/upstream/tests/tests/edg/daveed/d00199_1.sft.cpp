//remark:C++ VLAs
//type:fp
//name:
//options:
//options_all:--g++ -x
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

int ctor, dtor;

struct A {
	A() {ctor++;}
	~A() {dtor++;}
};

struct B {
	B();
	~B();
	int x;
};

B::B()
try
: x(37)
{
}
catch (...) {
}

int main()
{
	B b;

	if (ctor != 10 || dtor != 10)
		return 1;

	return 0;
}
