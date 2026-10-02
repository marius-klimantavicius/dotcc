//remark:GNU case ranges
//type:fp
//name:
//options:--gcc:;fn
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:-v 200000
//linker_options:
//execution_args:
//script:

void bar(int);

  int foo(int i)
  {
    switch (i) {
      case -1: bar(1); break;
      case 0 ... 2147483647: break;
    }
    return 0;
  }

