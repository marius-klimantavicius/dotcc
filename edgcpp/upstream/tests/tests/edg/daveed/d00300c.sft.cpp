//remark:Named address space qualifiers
//type:fn
//name:
//options:
//options_all:--c99 --embedded_c
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

int main() {
   int *p = 0;
   _EDG_NAS_A int *pa = 0;
   _EDG_NAS_B int *pb = 0;
   _EDG_NAS_C int *pc = 0;

   p = pb;  // OK
   pa = pa; // OK
   pb = pb; // OK
   pa = pb; // OK
   pb = pa; // Error
   pc = pa; // Error
   pa = pc; // Error
   pa = p; // Error
   pb = p; // Error
}
