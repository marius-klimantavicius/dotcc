//remark:Local variable declaration hiding
//type:fn
//name:
//options:
//options_all:--diag_err=1348
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

void foo()
{
   int i;
   {
      void i();
   }
   {
      static int i;
   }
   {
      char i;
   }
   {
      extern int i;
   }
}
