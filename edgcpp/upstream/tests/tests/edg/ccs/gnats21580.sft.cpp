//type:fn
//options:--c++17;fp:--c++20
//options_all:-W -r --diag_suppress 174

int f(int *a, int b, int c) {
  int r = a[b,c];   // deprecated
  r += a[(b,c)];    // OK
  return r;
}
