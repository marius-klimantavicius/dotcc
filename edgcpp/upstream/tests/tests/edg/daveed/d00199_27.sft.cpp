//remark:VLA lowering code
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

typedef void F(int a[*][*]);

F *f;

