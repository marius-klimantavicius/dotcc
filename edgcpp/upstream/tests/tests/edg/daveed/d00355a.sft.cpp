//remark:Flexible array initializers
//type:fp
//name:
//options:--microsoft;fp:--gcc;fn:--c99;fn
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

struct S {
   int i, j ;
   int a[] ;
};

union U {
   struct S s1;
   struct S2 {
       int i;
       int j;
       int a[2];
   } s2;
} u = { 1, 2, 3, 4, 5 };
