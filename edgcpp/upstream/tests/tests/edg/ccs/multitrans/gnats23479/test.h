namespace std {
  typedef unsigned long a;
  template <typename b, b c> struct d { static constexpr b e = c; };
  typedef d<bool, false> f;
  template <bool c> using g = d<bool, c>;
  template <bool, typename, typename> struct aa;
  template <typename...> struct h;
  template <typename> struct ab : g<!bool()> {};
  template <typename i, typename j> struct aa<false, i, j> { typedef j k; };
  template <typename l, typename, template <typename> typename> struct m {
    using k = l;
  };
  template <typename l, template <typename> class ac> using n = m<l, void, ac>;
  template <typename l, template <typename> class ac>
    using p = typename n<l, ac>::k;
  template <typename b> b ad;
  namespace {
    typedef char o;
  }
  template <typename, typename> struct ae {};
  template <typename> class ag {};
  template <class ai> class initializer_list {
    typedef a q;
    typedef ai *aj;
    aj r;
    q s;
  };
  struct t {};
  template <typename, typename> using ao = t;
  namespace aq {
    struct ar;
    struct as {};
    template <bool at, bool au, bool av> struct aw {
      using ax = g<at>;
      using ay = g<au>;
      using az = g<av>;
    };
    template <typename, bool> struct bb;
    struct bc {};
    struct bd {};
    struct be;
    template <typename, typename, typename, typename, typename, typename, typename,
      typename, typename, typename>
      struct bf {};
    template <typename, typename, typename, typename, typename, typename, typename,
      typename, typename, typename>
      struct bg {};
    template <typename, typename, typename, typename, typename, typename, typename,
      typename, typename, typename bh, bool = bh::ay::e>
      struct bi;
    template <typename bj, typename bk, typename an, typename bl, typename bm,
      typename bn, typename bo, typename x, typename bp, typename bh>
      struct bi<bj, bk, an, bl, bm, bn, bo, x, bp, bh, false>
      : bg<bj, bk, an, bl, bm, bn, bo, x, bp, bh> {};
    template <typename bq> using br = typename bq::br;
    template <typename, typename, typename, typename, typename, typename, typename,
      typename, typename, typename, typename = p<f, br>>
      struct bs;
    template <typename bj, typename bk, typename an, typename bl, typename bm,
      typename bn, typename bo, typename x, typename bp, typename bh>
      struct bs<bj, bk, an, bl, bm, bn, bo, x, bp, bh> {};
    template <int, typename b, bool = !__is_final(b)> struct bt;
    template <int bu, typename b> struct bt<bu, b, true> {
      template <typename bv> bt(bv);
    };
    template <typename, typename, typename, typename, typename, typename, bool>
      struct bw;
    template <typename bj, typename bk, typename bl, typename bn, typename bo>
      struct bw<bj, bk, bl, bn, bo, bd, true> : bt<0, bl>, bt<1, bn>, bt<2, bo> {
      using bx = bt<0, bl>;
      using by = bt<1, bn>;
      using bz = bt<2, bo>;
      typedef bn ca;
    bw(bl cb, bn cc, bo cd, bd) : bx(cb), by(cc), bz(cd) {}
    };
    template <typename bj, typename bk, typename bl, typename bm, typename bn,
      typename bo, typename x, typename bh>
      struct ce : bw<bj, bk, bl, bn, bo, x, bh::ax::e>, bt<0, bm> {
      typedef a q;
      using cf = bh;
      using ax = typename cf::ax;
      using cg = bw<bj, bk, bl, bn, bo, x, ax::e>;
      using ch = bt<0, bm>;
    ce(bl cb, bn cc, bo cd, x ci, bm cj) : cg(cb, cc, cd, ci), ch(cj) {}
    };
    template <typename, typename, typename, typename, typename, typename, typename,
      typename, typename, typename bh, bool = bh::az::e>
      struct ck;
    template <typename bj, typename bk, typename an, typename bl, typename bm,
      typename bn, typename bo, typename x, typename bp, typename bh>
      struct ck<bj, bk, an, bl, bm, bn, bo, x, bp, bh, true> {};
    template <typename cl> struct u : bt<0, cl> {
      using cm = bt<0, cl>;
      using cn = a;
      using co = cn *;
      template <typename an> u(an) : cm(ad<an>) {}
    };
  }
  template <typename, typename> using y = ab<h<>>;
  template <typename bj, typename bk, typename an, typename bl, typename bm,
    typename bn, typename bo, typename x, typename bp, typename bh>
    class cp : public aq::ce<bj, bk, bl, bm, bn, bo, x, bh>,
    aq::bf<bj, bk, an, bl, bm, bn, bo, x, bp, bh>,
    aq::bi<bj, bk, an, bl, bm, bn, bo, x, bp, bh>,
    aq::bs<bj, bk, an, bl, bm, bn, bo, x, bp, bh>,
    aq::ck<bj, bk, an, bl, bm, bn, bo, x, bp, bh>,
    aq::u<ao<an, aq::bb<bk, bh::ax::e>>> {
    using cf = bh;
    using ax = typename cf::ax;
    using cq = aq::bb<bk, ax::e>;
    using cr = ao<an, cq>;
    using cs = aq::u<cr>;
    using typename cs::co;

  public:
    typedef bk ct;
    typedef an cu;
    typedef bm cv;
    using ay = typename cf::ay;
    using cw = typename aa<ay::e, aq::ar, aq::as>::k;
    using cx = aq::ce<bj, bk, bl, bm, bn, bo, x, bh>;
    using typename cx::q;
    co cy = nullptr;
  cp(bn cc, bo cd, x cz, bm cj, bl da, cu) : cx(da, cc, cd, cz, cj), cs(cr()) {}
  cp(initializer_list<ct>, q, bn dc, cv dd, cu de = cu())
    : cp(dc, bo(), x(), dd, cw(), de) {}
  };
  template <bool df> using dg = aq::aw<df, false, true>;
  template <typename bj, typename b, typename x, typename dh = bj,
    typename an = ag<ae<bj, b>>, typename di = dg<y<bj, x>::e>>
    using dj = cp<bj, ae<bj, b>, an, aq::as, dh, x, aq::bc, aq::bd, aq::be, di>;
  template <typename bj, typename b, typename x = bj, typename dh = bj,
    typename an = ag<ae<bj, b>>>
    class dk {
	      typedef dj<x, dh, an> cp;
	      cp dl;

  public:
	      typedef typename cp::ct ct;
	      typedef typename cp::ca ca;
	      typedef typename cp::cv cv;
	      typedef typename cp::q q;
  dk(initializer_list<ct> dm, q dn = 0, ca dc = ca(), cv dd = cv())
	      : dl(dm, dn, dc, dd) {}
  };
  namespace {
    class v {
      dk<o, int> w{};
    };
  }
}
