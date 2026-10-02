//type:rp
//options_all:--c++17 -tused -A
extern "C" int printf(const char*,...);

  struct X {
    X() { printf("X()\n"); }
    X(const X&) { printf("X(const X&)a\n"); }
    ~X() { printf("~X()\n"); }
  };

  struct Y { ~Y() noexcept(false) { throw 0; } };

  X f() {
    try {
      Y y;
      return {};
    } catch (...) {
    }
    return {};
  }

  int main() {
    f();   // Should print out X() twice and ~X() twice.
  }

//cwg: 2176
//title: Destroying the returned object when a destructor throws
//meeting: Jacksonville 2/16
//edg_status: EDGcpfe/22214
