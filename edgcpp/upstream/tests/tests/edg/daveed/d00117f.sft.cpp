//remark:GNU case ranges
//type:fp
//name:
//options:--gnu_version=40200:--gnu_version=30300 -tused:--parse;fn
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

template<int I, int J> void f(int i) {
        switch (i) {
                case 1 ... I: break;
                case I-1 ... J+1: break;
                case J ... 100: break;
        }
}
