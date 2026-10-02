//remark:Diagnostic on missing return statement
//type:fp
//name:
//options:--strict;fp:--c99;fp
//options_all:-r
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

void f();
int g() { f(); }

