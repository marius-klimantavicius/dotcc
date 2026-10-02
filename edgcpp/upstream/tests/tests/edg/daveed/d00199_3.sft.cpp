//remark:VLA type copies
//type:fn
//name:
//options:
//options_all:--c99
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

void g(float[2][2]);
void f(int i) {
  typedef float A[i][i];
  A const a;
  g(a);
}
