#include "api.h"
// The original spelling and comments belong to the packaged source bytes.
struct Guard { int tag; Guard(int n) : tag(n) {} ~Guard() {} };
int choose(int x) { return x + 1; }
long choose(long x) { return x + 2; }
template<class T> T twice(T x) { return x + x; }
template<class T> struct Box { static int read() { return 1; } };
template<class T> struct Box<T*> { static int read() { return 2; } };
int work(int n) {
  Guard first(n);
  int total = 0;
  for (int i = 0; i < n; ++i) {
    Guard inner(i);
    if (i == 4) break;
    total += choose(twice<int>(OUTER(i)));
  }
  try {
    Guard conditional(n + 1);
    if (n < 0) throw n;
    total += conditional.tag;
  } catch (int error) {
    total -= error;
  }
  return total + Box<int*>::read();
}
#undef API_VERSION
#define API_VERSION 6
