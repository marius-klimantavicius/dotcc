//type:fp
//options:--c++11:--c++11 --gn 130200:--c++11 --clang_version 180100
template <typename> class a;
class b;
template <typename c> struct B { typedef a<c> d; };
template <typename> class e {};
template <typename> class a : public e<int> {
public:
  template <typename f> a& operator=(const e<f> &);
};
template <int, int> class ar;
template <int, int, typename> class g;
template <int ao, int ap> class h : g<ao, ap, int> {
public:
  using g<ao, ap, int>::operator=;
  template <typename f> h& operator=(const e<f> &);
};
template <int ao, int ap> class g<ao, ap, int> : ar<ao, ap> {
public:
  using ar<ao, ap>::operator=;
};
template <int ao, int ap> class ar : B<h<ao, ap>>::d {
public:
  typedef typename B<h<ao, ap>>::d at;
  using at::operator=;
};
template <int av> class i : h<av, 1> {
public:
  using h<av, 1>::operator=;
};
template <class> class aw {
public:
  i<3> j();
  aw &operator=(const b &);
};
class k : aw<int> {
public:
  using aw ::operator=;
};
template <class c> aw<c> &aw<c>::operator=(const b &) {
  B<int>::d ba;
  j() = ba;
  return *this;
}
class b {
public:
  template <typename c> b(double, c);
};
int bb;
double bc;
void m() {
  k l;
  l = b(bc, bb);
}
