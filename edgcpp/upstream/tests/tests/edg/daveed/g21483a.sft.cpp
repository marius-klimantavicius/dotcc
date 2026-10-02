//remark:Dependent static data memnbers
//options:--c++11 --gnu_version  80100 -w;fp:--c++17;fp:--microsoft_v=1915;fp

template <class T>
class blah {
public:
  constexpr blah(int i) { }

  template <int I>
  class bar {
    public:
    constexpr static blah<T> a = I;
  };
};

blah<int> w = blah<int>::bar<5>::a;

