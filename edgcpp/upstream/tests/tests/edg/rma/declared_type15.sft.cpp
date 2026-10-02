//options_all:-r -x -tused
//options: --strict;cn

extern "C" double cos(double);
extern "C" double cosh(double);
extern "C" double sin(double);

namespace std {
  template<class P> struct T {
    T(P p) : data(p) { } // default conversion ctor
    P imag() const { return data; }
    P data;
  };

  template<class P> inline T<P> cos(const T<P>& p) { return p; }
  template<class P> inline T<P> cosh(const T<P>& p) { return p; }
  template<class P> inline T<P> sin(const T<P>& p) {
    cos(p.imag());
    cosh(p.imag());
    return p;
  }
}

using namespace std;
main() {
  T<float> tf(1.0);
  float f = 1.0;
  cosh(tf);
  sin(tf);
  sin(f);
  return 0;
}

