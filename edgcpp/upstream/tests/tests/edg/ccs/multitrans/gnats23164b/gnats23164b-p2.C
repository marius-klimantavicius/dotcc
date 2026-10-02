namespace std {
  template <typename> struct hash;
  template <typename, typename> class vector;
  template <typename a> class vector<bool, a> {
    typedef a b;
    template <typename> friend struct hash;
  };
}
