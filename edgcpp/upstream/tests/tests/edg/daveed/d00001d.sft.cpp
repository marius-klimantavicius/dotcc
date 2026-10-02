/*
//remark:Ordinary designators
//type:fn
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


typedef struct X { int a, b, c; } X;
typedef struct Y { X k, l, m; } Y;
typedef struct Z { Y p, q, r; } Z;

Z p1 = { .p.l = { 1, 2 }};
Z p2 = { .p = { .l = { 1, 2 }}};
Z p3 = { .p.l.c = 1 };

Z n1 = { .p = .l = { 3, 4}};
Z n2 = { .p.l { 3, 4}};

