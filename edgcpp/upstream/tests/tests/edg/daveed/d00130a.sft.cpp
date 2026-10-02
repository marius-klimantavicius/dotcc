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
      a () { new class b; }
      class b {
            E *data;
      };
};

void foo(void)
{
  new a<int>();
}
