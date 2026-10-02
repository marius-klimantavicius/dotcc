//remark:Using declarations with mix of tag and nontags
//type:fp
//name:
//options:
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

struct B {
  struct S {};
  void S(int, int);
};

struct D: private B {
	using B::S;
};

struct X: public D {
	void mf() { S(42, 7); }
	void mf2() { struct S s; }
};

