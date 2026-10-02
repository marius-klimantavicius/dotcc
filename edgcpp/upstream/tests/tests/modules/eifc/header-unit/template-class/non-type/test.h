#if TEST_NUMBER == 1
#define CLASS_KEY struct
#endif
#if TEST_NUMBER == 2
#define CLASS_KEY class
#endif
#if TEST_NUMBER == 3
#define CLASS_KEY union
#endif

template<int N>
CLASS_KEY add_ten_class {
public:
  add_ten_class()
    : result_value(N + 10)
  {}
  int result_value;
};
