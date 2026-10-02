//options_all:--c++20 -A
bool f() {
  constexpr int z = 42;
  return requires {
    sizeof(int [*&z]);
  } && requires (int x) {
    sizeof(int [*&z]);
  };
}

//cwg: 2910
//title: Effect of requirement-parameter-lists on odr-usability
//meeting: Wroclaw 11/24
//edg_status: Passes
