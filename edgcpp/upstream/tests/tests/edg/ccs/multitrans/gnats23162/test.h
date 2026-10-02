template<class T>
struct time {
  time() = default;
  ~time() = default;
};

template<int N>
struct count_time {
};

using Time = time<count_time<9>>;

struct t {
  Time grantedTime;
  int x;
};
