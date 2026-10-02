//type:fn
//options: -A --c++20

constexpr bool b1 = +"foo" == "foo";  // error
constexpr bool b2 = "xfoo" + 1 == "foo\0y";  // error
