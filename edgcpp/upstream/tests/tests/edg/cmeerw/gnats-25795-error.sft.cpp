//type:fn
//options:--c++17:--ms_c++17

namespace PR_test
{
  template <class AA> struct aaa { };
  template <class BA> using bbb = aaa<BA::xyz>;
  template <class CA, class CB> using ccc = CB;
  template <class DA, class DB> using ddd = aaa<bbb<decltype(ccc<DA, DB>())>>;
  template <class EA> using eee = ddd<int, EA>;
  template <class FA> eee<FA> fff(FA) { return {}; }
  void bar() {
    fff(1);
  }
}

namespace type_unknown_id
{
  template <class AA> struct aaa { };
  template <class BA> using bbb = aaa<X>;
  template <class DA, class DB> using ddd = aaa<bbb<decltype(DB())>>;

  template <class FA> ddd<int, FA> fff(FA) { return {}; }

  void bar() {
    fff(1);
  }
}

namespace type_unknown_member
{
  template <class AA> struct aaa { };
  template <class BA> using bbb = aaa<BA::X>;
  template <class DA, class DB> using ddd = aaa<bbb<decltype(DB())>>;

  template <class FA> ddd<int, FA> fff(FA) { return {}; }

  void bar() {
    fff(1);
  }
}

namespace type_unknown_typename_member
{
  template <class AA> struct aaa { };
  template <class BA> using bbb = aaa<typename BA::X>;
  template <class DA, class DB> using ddd = aaa<bbb<decltype(DB())>>;

  template <class FA> ddd<int, FA> fff(FA) { return {}; }

  void bar() {
    fff(1);
  }
}

namespace nontype_unknown_id
{
  template <int AA> struct aaa { };
  template <class BA> using bbb = aaa<X>;
  template <class DA, class DB> using ddd = aaa<bbb<decltype(DB())>::X>;

  template <class FA> ddd<int, FA> fff(FA) { return {}; }

  void bar() {
    fff(1);
  }
}

namespace nontype_unknown_member
{
  template <int AA> struct aaa { };
  template <class BA> using bbb = aaa<BA::X>;
  template <class DA, class DB> using ddd = aaa<bbb<decltype(DB())>::X>;

  template <class FA> ddd<int, FA> fff(FA) { return {}; }

  void bar() {
    fff(1);
  }
}
