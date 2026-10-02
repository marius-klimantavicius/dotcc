//remark:GNU: size of arrays
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

extern int printf(const char *, ...);
typedef int ptrdiff_t;


struct sss {
  ptrdiff_t f;
} a[10];

int main() {
  printf ("sizeof(a)=%ld\n", sizeof (a));
  return 0;
}

