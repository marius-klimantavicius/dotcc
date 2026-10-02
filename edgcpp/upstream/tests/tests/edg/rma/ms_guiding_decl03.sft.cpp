//options_all:-r -x -tused
//options: --microsoft_version=1400 -n;cp

template <class T> inline void f(T t) { }
/* A guiding declaration if --guiding_decls. */
void f(int);
/* An old-style specialization if --old_specializations. */
void f(float) { }

template <class T> void g(T t) { }
/* A guiding declaration if --guiding_decls. */
inline void g(int);
/* An old-style specialization if --old_specializations. */
inline void g(float) { }

main() {
  int i;
  float x;
  f(i);
  f(x);
  g(i);
  g(x);
}

