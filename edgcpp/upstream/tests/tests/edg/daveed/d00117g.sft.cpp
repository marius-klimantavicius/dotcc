//remark:GNU case ranges
//type:fp
//name:
//options:--gnu_version=40200:--parse;fp
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:
//script:

template<int I> void f() {
  switch (3) {
    case 1: break;
    case I: break;
  }
}

