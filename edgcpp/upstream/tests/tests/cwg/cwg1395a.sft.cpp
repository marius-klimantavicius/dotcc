//type:rp
//options_all:--c++17 -A -tused

  template<typename ...T> int f(T*...) {
    return 1;
  }
  template<typename T> int f(const T&) {
    return 2;
  }
  int main() {
    if (f((int*)0) != 1) {
       return 1;
    }
    return 0;
  }

//cwg: 1395
//title: Partial ordering of variadic templates reconsidered
//meeting: Issaquah 11/16
//edg_status: Passes
