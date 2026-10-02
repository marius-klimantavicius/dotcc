//remark:Microsoft dllimport/dllexport compatibility
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


#define EXP __declspec(dllexport)
#define IMP __declspec(dllimport)

struct S {
	void IMP f();
	static IMP void s();
};


void S::f() {}
void S::s() {}

S s;

int main() {
	s.f();
	s.s();
	return 1;
}
