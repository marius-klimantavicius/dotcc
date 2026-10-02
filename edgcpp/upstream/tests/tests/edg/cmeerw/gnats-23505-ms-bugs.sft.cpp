//type:fp
//options:--c++ --microsoft_version=1300
namespace ms_bugs
{
  template<int I>
  struct C
  {
    static int i;
  };

  struct C<0> {
    static const int i;
  };

  template<>
  const int C<0>::i;
}
