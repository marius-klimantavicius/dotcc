//remark:Visibility of member using-declarations
//type:fp
//name:
//options:;fp:-DERROR;fn
//options_all:-A
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

class A
{
public:
  int f(int n) {return 1+n;}
#if ERROR
  int f(unsigned int n) { return 3+n; }
#endif
};

class B : public A
{
public:
  int f(unsigned int n) {return 2+n;}
  using A::f;
};

class C : public B
{
public:
  using A::f;
};
