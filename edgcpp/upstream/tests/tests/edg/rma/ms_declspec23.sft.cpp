//options_all:-r -x -tused
//options: --microsoft -n;cp

template<class P> void swap(P& p1, P& p2) {
  P t; t = p1; p1 = p2; p2 = t;
}
template<class P> struct basic_string {
  basic_string();
  inline void swap(basic_string<P>& p2);
  friend void swap(basic_string<P>& p1, basic_string<P>& p2) {
    p1.swap(p2);
  }
};
extern template struct __declspec(dllimport) basic_string<char>;
struct __declspec(dllimport) client {
  basic_string<char> name;
};

