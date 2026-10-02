/*
//remark:Ordinary designators
//type:rp
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

#if defined __cplusplus
extern "C"
#endif
int printf(char const*, ...);

struct A { int i; int j; };
struct A a = {.j = 22, .i = 1, 2};

int main () {
  printf(".i = %d\n", a.i);
  printf(".j = %d\n", a.j);
  return 0;
}


