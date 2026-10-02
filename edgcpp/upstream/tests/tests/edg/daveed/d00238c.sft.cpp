//remark:Microsoft __interface support
//type:fn
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

__interface IB1 {};

__interface ID1: virtual private IB1 {};

struct SB {};
__interface ID2: SB {};

