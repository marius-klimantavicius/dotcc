/*
//remark:Ordinary designators
//type:fp
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

typedef struct X { int a, b, c; } X;
typedef struct Y { X p, q, r; } Y;
int f1();
int f2();
int f3();
int f4();
X f5();

void ex1() {
   Y y1 = { .q = { f1(), f2(), f3() },
            f5(),
            .q.b = f4()};
}
