/*
//remark:Ordinary designators
//type:fp
//name:
//options:
//options_all:--c --designators
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
*/



typedef struct X {
  int a;
  int b[5];
} X;

X x1 = { 3, .a = 2 };
X x2 = { 3, 4, .a = 2 };
X x3 = { 3, 4, 5, 6, 7, 8, .a = 2};

