//remark:Microsoft alignment directives
//type:fp
//name:
//options:
//options_all:--microsoft
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

#pragma pack(8) // default value

__declspec(align(16))  struct A
{
  char ca ;
} ;

struct B
{
  char cb ;
  A sA ;
} ;

int tabB[ (sizeof(B)==32) ? 1 : -1] ; //compilation error here



