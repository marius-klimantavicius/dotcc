//remark:break_position in switch clause
//type:fp
//name:
//options:
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

int a, d;
void f() 
{
  switch (a) {
  case 1: if (d) break;
  }
}
