//remark:GNU attribute syntax in struct declarations
//type:fp
//name:
//options:
//options_all:--gcc
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

typedef void F(void);
typedef void G(void);
F f __attribute__((const));
F g;

void h(void);

int main() {
	F *p1 = f;
	F *p2 = g;
	G *p3 = f;
	G *p4 = g;
	G *p5 = h;
	F *p6 = h;
	return 0;
}
