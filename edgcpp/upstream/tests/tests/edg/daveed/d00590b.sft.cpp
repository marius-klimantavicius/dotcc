//remark:Designators into anonymous union fields
//type:cp
//name:
//options:
//options_all:--g++
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

struct S {
	union {
		int i;
	};
} s = {{ i:3 }};

