#if TEST_NUMBER == 1
#define CLASS_KEY struct
#endif
#if TEST_NUMBER == 2
#define CLASS_KEY class
#endif
#if TEST_NUMBER == 3
#define CLASS_KEY union
#endif

template<typename T>
CLASS_KEY my_class {
public:
  T templ_value;

  my_class(T templ_value)
    : templ_value(templ_value)
  {}
};
