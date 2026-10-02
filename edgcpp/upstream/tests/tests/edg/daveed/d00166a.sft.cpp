//remark:GNU C unused attribute
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

static void f(void) __attribute__((unused));

void f(void) {
}

//void g() { f(); }

