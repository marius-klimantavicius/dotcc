//remark:GNU asm symbolic operands
//type:fp
//name:
//options:--gcc:--g++
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

int main()
{
    int a, b;
    b = 12; a = 0;
    asm( "movl %[source], %[result]":[result]"=X"(a):[source]"X"(b) );
    return 0;
}


