//options_all:-r -x -tused --microsoft_version=1200
//options: --microsoft -n;fn

template <class T> void f(const T&) { return; }
class X {
  friend void f<int>(const int&) { return; };
};
//template<>
void f<float>(const float&) { return; }

