/*
//remark:Ordinary designators
//type:rp
//name:
//options:
//options_all:--c --designators --microsoft
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

struct Y { X p, q, r; };

int f1() { printf("f1()\n"); return 1; }
int f2() { printf("f2()\n"); return 2; }
int f3() { printf("f3()\n"); return 3; }
int f4() { printf("f4()\n"); return 4; }

int main() {
   struct Y y = { .q = { f1(), f2(), f3() }, .q.b = f4() };
	printf("y = { { %d, %d, %d }, { %d, %d, %d }, { %d, %d, %d } }\n",
          y.p.a, y.p.b, y.p.c, y.q.a, y.q.b, y.q.c, y.r.a, y.r.b, y.r.c);
	return 0;
}


