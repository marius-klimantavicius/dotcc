//type:fp
//options_all:--c++17 -tused -A
template <class T> struct A {
    template <class U> static const U x = 1;
    static const int y = 2;
  };

  int main() {
    A<int> a;
    int y = a.y;         // OK
    int x = a.x<int>;    // ???
  }

//cwg: 2255
//title: Instantiated static data member templates
//meeting: Jacksonville 2/18
//edg_status: Passes
