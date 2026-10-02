//remark:Flexible array member layout
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

class C {
public:
};

struct S {
	C c[];
};

struct X {
        struct S s;
};
