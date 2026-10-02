//remark:Calling conventions on constructors and destructors
//type:fp
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
//script:

class C {
	C();
};
__stdcall C::C(){}
