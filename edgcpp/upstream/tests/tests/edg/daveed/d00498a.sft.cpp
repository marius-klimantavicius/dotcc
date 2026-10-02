//remark:GNU C block-extern declaration check
//type:fp
//name:
//options:--gcc --gnu_version=30300:--c;fn
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

void foo() {
extern int a;
}

void bar() {
extern char a[];
}
