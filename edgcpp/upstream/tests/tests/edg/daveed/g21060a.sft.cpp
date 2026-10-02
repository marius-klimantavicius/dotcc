//remark:Deduction and empty pack expansions
//options:--c++14 --clang_v=50000 -tused;fn

template <class, long> struct b;
template <class, class> class c;
template <class k, long d, class = typename b<k, d>::e> class f;
template <int> struct o;
template <class k, class g> using h = c<k, g>;
template <class k, int d> using i = h<k, o<d>>;
template <bool, typename k> using j = k;
template <class k, int d> class c<k, o<d>> : f<k, 1> {};
template <typename, int> struct ad {
 using l = i<int, 0>;
 using m = i<int, 0>;
};
template <typename k, typename = k> class c;
struct p {
 using af = int;
 static constexpr long ag = sizeof(af);
};
template <typename k> class c<k> {
 static constexpr long ah() { return p::ag; }
};
template <unsigned long, class...> struct ai;
template <unsigned long d, class k> struct ai<d, k> { using e = k; };
template <class, int d> struct b : ai<d, int> {};
template <typename, int d, typename> class f {
 using aj = ad<int, d>;

public:
 using l = typename aj::l;
 using m = typename aj::m;
 static constexpr long ag = 0;
 f(l, m);
};
template <typename ak, long, typename al> j<0, ak> am(al);
template <typename ak, typename al> ak an(al);
template <typename, typename al, typename... ao> void an(ao... x, al y, al z);
template <typename, typename k, typename ap> j<0, f<double, 2>> aq(c<k, ap> n) {
 using ar = f<double, 2>::l;
 using as = f<double, 2>::m;
 return {an<ar>(n), am<as, ar::ag>(n)};
}
void a() {
 using av = c<int>;
 typedef f<double, av::ah()> aw;
 c<float> ax;
 aq<aw>(ax);
}
