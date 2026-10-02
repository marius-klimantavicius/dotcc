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

typedef struct X { int a, b, c; } X;

struct Y { X p, q, r; } y = { .p = { .a = 12, 13, 14 }, .q.b = 42 };

int main() {
	printf("y = { { %d, %d, %d }, { %d, %d, %d }, { %d, %d, %d } }\n",
          y.p.a, y.p.b, y.p.c, y.q.a, y.q.b, y.q.c, y.r.a, y.r.b, y.r.c);
	return 0;
}

