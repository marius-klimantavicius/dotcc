//remark:GNU asm symbolic operands
//type:fp
//name:
//options:--gcc:--g++
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

void f() {
	asm ("xxx"::
	     "X"(0),
	     "X"(1),
	     "X"(2),
	     "X"(3),
	     "X"(4),
	     "X"(5),
	     "X"(6),
	     "X"(7),
	     "X"(8),
	     "X"(9),
	     "X"(10),
	     [N]"X"(11),
	     "[N]"(12));
}
