//remark:GNU C old-style definitions following prototypes with unpromoted types
//type:cp
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

int f(int);

int f(x)
	short x;
{
	return 3;
}

int g(short);

int g(x)
	short x;
{
	return 3;
}

