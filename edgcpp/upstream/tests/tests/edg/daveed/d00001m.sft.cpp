/*
//remark:Extended designators
//type:fn
//name:
//options:
//options_all:--c --extended_designators --microsoft
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
*/

struct A { int i; int j; };
int main() {
  int i = 1;
  struct A a[5] = { [2 ... 4] = { i++, i++ } };
  return 0;
}

