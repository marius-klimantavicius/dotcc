//remark:GNU C zero length arrays
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

extern int printf(char const*, ...);

int main()
{
  typedef int T[1];
  printf("sizeof(int[0]) = %d\n", sizeof(int[0]));
  printf("sizeof(int[0]) = %d\n", sizeof(T[0]));
  return 0;
}
