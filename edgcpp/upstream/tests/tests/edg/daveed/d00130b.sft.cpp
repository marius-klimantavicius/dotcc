//remark:Parsing of new expressions
//type:fp
//name:
//options:
//options_all:-tused
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

template <class E>
class a
{
  public:
      a () { new enum b; }
      enum b { xx };
};

void foo(void)
{
  new a<int>();
}
