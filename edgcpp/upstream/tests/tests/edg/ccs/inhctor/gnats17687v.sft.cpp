//type:cp
//options_all:--c++17 -tused -A

struct a_ {
  int n;
  template <class T> a_(double d, T j) = delete;
  template <class T> a_(T i, T j, T k=3, T m=4) : n(i+j+k+m) { }
};
struct b_ : a_ {
  using a_::a_;
  template <class T> b_(T i, T j) : a_(i,j) { }
};

int main()
{
  // _CXX11 - Implements N2540 - 2008
  b_ ba (2, 1);
  if (ba.n != 10)
    return(1);
  return(0);
}
