//type:cp
//options:--c++11
//options_all:-tused

template <typename T>
T&& declval();

struct true_type  { static constexpr bool value = true;  };
struct false_type { static constexpr bool value = false; };

template<class Ty, class... Args>
struct is_implicitly_constructible
{
  template<class Tx>
  static void test_func(const Tx &);

  template<class Tx = Ty>
  static auto evaluate(int) -> decltype(test_func<Tx>({declval<Args>()... }), true_type{});

  template<class = Ty>
  static auto evaluate(float) -> false_type;

  static constexpr bool value = decltype(evaluate(0))::value;
};

class B
{
  B() {}; //private implicit default constructor
};

struct C
{
  C() {}; //public implicit default constructor
};

struct D
{
  explicit D() {}; //public explicit default constructor
};

void f()
{
  static_assert(is_implicitly_constructible<B>::value == false, "");
  static_assert(is_implicitly_constructible<C>::value == true, "");
  static_assert(is_implicitly_constructible<D>::value == false, "");
}
