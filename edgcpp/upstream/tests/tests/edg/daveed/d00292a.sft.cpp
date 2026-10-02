//remark:Selectany constraints
//type:fn
//name:
//options:--microsoft_version=1200;fn:--microsoft_version=1300;fn
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

  struct A {
   int i; A() : i(42) { }
    static __declspec(selectany) int x;
  };
  A __declspec(selectany) a;
  static A __declspec(selectany) b;

main() {
	 volatile __declspec(selectany) double d;
}
