//remark:Pseudo-destructors
//options:--c++17 --gnu=69999 -tused -w;fp

namespace std {
template <typename a, a b> struct c {
  static constexpr a d = b;
  typedef c e;
};
template <bool, typename, typename> struct aa;
template <bool, typename bk, typename> struct aa { typedef bk e; };
template <typename bk, typename bl> struct aa<false, bk, bl> { typedef bl e; };

template <typename...> struct f;
template <typename g, typename h> struct f<g, h> : aa<1, g, h>::e {};

template <typename a> struct k : f<c<bool, false>, a> {};
template <typename a> a af;
struct ai {
  template <typename a, typename = decltype(af<a>.~aj)>
  static c<bool, true> ak(int);
  template <typename> static c<bool, false> ak(...);
};
template <typename a> struct al : ai { typedef decltype(ak<a>(0)) e; };
struct an : al<int>::e {};
struct ap {
  template <typename, typename> static c<bool, true> ak(int);
};
template <typename a, typename aq> struct ar : ap {
  typedef decltype(ak<a, aq>(0)) e;
};


template <typename> struct bd;
template <typename a> struct bd<a &> { typedef a e; };

//template <typename ay> struct az : aa<an::d, ar<int, int>, an>::e::e {};
template <typename ay> struct az : aa<al<int>::e::d, ar<int, int>, an>::e::e {};

template <typename a> class bh {
public:
  typedef typename bd<a>::e e;
};
template <typename a> struct bi { typedef typename bh<a>::e bg; };
template <bool, typename = void> struct bj;
template <typename a> struct bj<true, a> { typedef a e; };
template <typename> void bm();
template <typename _T1, typename _T2> struct pair {
  int bn;
  template <typename> 
    pair();
};
template <typename _T1, typename _T2>
pair<typename bi<_T1>::bg, typename bi<_T2>::bg> l(_T1 &&, _T2 &&);
struct bu {
  pair<int, int> *operator->();
};
template <typename, typename, typename, typename, typename = pair<int, int>>
class by {
public:
  typedef pair<int, int> cc;
  template <typename aq> pair<cc, bool> cf(aq &&);
};
template <typename bw, typename bx, typename cg, typename ch, typename ci>
template <typename aq>
pair<typename by<bw, bx, cg, ch, ci>::cc, bool>
by<bw, bx, cg, ch, ci>::cf(aq &&) {
  pair<int, int>();
}
template <typename T> class cj {
  by<int, pair<int, T>, pair<int, T>, int> m;

public:
  void cm(const pair<int, T> &); // = delete;
  template <typename U, typename V = typename bj<az<U>::d>::e>
  void cm(U &&) {
    m.cf(bm<U>);
  }
};
}
class co {
  std::cj<co> cq;
  void clone() {
    for (std::bu i;;) {
      co cp;
      std::pair<int, co> n = std::l(i->bn, cp);
      cq.cm(n);
    }
  }
};
int main() {}

