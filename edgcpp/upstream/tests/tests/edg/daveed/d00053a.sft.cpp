//remark:Microsoft compatibiltiy: allow multiple mem-initializers for members of the same anonymous union
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

struct X {
	X(): i(0), j(0.0) {}
	union {
		int i;
		double j;
	};
};

