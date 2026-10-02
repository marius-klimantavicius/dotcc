//remark:GNU C case ranges
//type:fn
//name:
//options:
//options_all:--gcc
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

void f(int x)
{
        switch (x) {
        case 1 ... 10:
        case 1 ... 10:
                ;
        }
}


