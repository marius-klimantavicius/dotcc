//remark:Accessibility of member-using as elaborated name
//type:fp
//name:
//options:
//options_all:--strict
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

  struct B { struct S {}; };
  struct D: private B { using B::S; };
void f() { struct D::S s; }
  struct X: D{
    struct S s;
	void f() { struct S s; }
};
