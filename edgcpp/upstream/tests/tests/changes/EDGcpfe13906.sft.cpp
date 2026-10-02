//type:fp
//options_all:--c++11
//remark:[4.7] Spurious error on range-based-for with const-qualified begin/end
// 4/16/13  [EDGcpfe/13906]
//
// Spurious error on range-based-for with const-qualified begin/end
//
// A spurious error had been issued on range-based-for (and for-each) statements
// when the begin/end member functions (or functions) had a const-qualified
// return type.  Now fixed.
struct Iterator {
  bool operator!=(const Iterator&) const;
  Iterator& operator++();
  const int& operator*() const;
};
struct Range {
  const Iterator begin();
  const Iterator end();
};
int main() {
  for (auto x : Range()) {}
}
