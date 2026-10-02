//remark:GNU IA-64 ABI in C mode
//type:rp
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


// Should really add  --set_flag emulate_unsafe_gnu_abi_bugs
extern int printf(char const*, ...);

struct {
  short s1;
  int a1 : 17;
  int a2 : 17;
  int a3 : 30;
  short s2;
} s;

int main() {
      printf("sizeof(s) = %ld\n", sizeof(s));
}
