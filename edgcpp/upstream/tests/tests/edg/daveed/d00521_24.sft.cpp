//remark:Microsoft dllimport/dllexport compatibility
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

void func() {
    __declspec( dllexport ) int n;         // Error; implies external
                                           // definition in local scope.
}
