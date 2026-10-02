template <class T> const T x = 1;
template <class T> T y = 1;

const int *j = &x<int>;
int *j2 = &y<int>;

