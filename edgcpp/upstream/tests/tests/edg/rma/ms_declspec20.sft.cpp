//options_all:-r -x -tused
//options: --microsoft_version=1400 -n;cp

template <class T> void f(T) { }
extern template __declspec(dllimport) void f(int);
main() {
  f(0);
}

