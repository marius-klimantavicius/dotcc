//remark:Cast to class template being defined
//options:--c++11 --gnu=80100;fp

template <class T>
class blah {
public:
  constexpr blah(int i) { }
 
#ifdef OK
  static constexpr blah a = 0;
#else
  static constexpr blah a = blah(0);
#endif
};
 
blah<int> w = blah<int>::a;
