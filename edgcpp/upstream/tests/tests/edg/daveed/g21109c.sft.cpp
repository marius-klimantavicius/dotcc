//remark:Explicit template argument substitution in deduction
//options:--c++17;fp

struct C
{
 static const int value = 0;
};

template <typename T>
struct store
{
  using elem_type = T;
};

template <int J, typename U>
static inline store<U> store_at_impl(store<U>*);

struct vector : store<int>
{
  template <int J>
  using store_at = decltype(store_at_impl<J>(static_cast<vector*>(nullptr)));
  template <typename J> typename store_at<J::value>::elem_type& at_impl(J);
};

void foo()
{
  vector v;
  C c;
  v.at_impl(c);
}
