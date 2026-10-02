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

typedef struct X {
  int a;
  int b[3];
} X;

X x = { 1, 2, .a = 4 };

int main() {
	printf(" X = { .a = %d, .b = { %d, %d, %d } }\n",
          x.a, x.b[0], x.b[1], x.b[2]);
	return 0;
}

