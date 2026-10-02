//type:fp
//options: -A --c++20

constexpr const char *f() {
  return "foo";
}

constexpr bool b5 = "foo" == "bar" + 0;
constexpr const char *p = f();
constexpr bool b3 = p == p;

static_assert(b5 == false);
static_assert(b3 == true);

//cwg: 2765
//title: Address comparisons between potentially non-unique objects during constant evaluation
//meeting: Croydon 3/26
