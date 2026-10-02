//options_all:--c++20 -tused -A
  #include <compare>
  struct MyType {
    int i;
    double d;
    std::strong_ordering operator<=> (const MyType& c) const = default;
  };

//cwg: 2539
//title: Three-way comparison requiring strong ordering for floating-point types
//meeting: Issaquah 2/23
//edg_status: Passes
