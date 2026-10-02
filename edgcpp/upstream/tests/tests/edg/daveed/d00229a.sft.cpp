//remark:GNU C++: Member attributes and asm-names
//type:fp
//name:
//options:;fp:-DNEG;fn
//options_all:--g++
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

struct S {
	void n() __attribute__((noreturn));
	void f() __asm__("magic_f");
	static int sm __asm__("magic_sm");
#ifdef NEG
	int field __asm__("myfield");
#endif
};

void S::n() {}
void S::f() {}

int S::sm;
