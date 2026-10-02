//remark:Ctor-initializers for flexible array members
//type:fp
//name:
//options:--g++;fp:--microsoft;fp
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

struct NA2
{
NA2(int i) : b(i), a()
   { }
int b ;
int a[] ;
};
