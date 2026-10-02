//remark:Partial specialization
//options:--c++14;fp

template<int V> struct IntVal {
  static constexpr int val = V;
};

template<bool> struct BoolVal;

template<typename, typename, typename> struct Assign;

template<typename T, typename U>
  struct Assign<T, U, BoolVal<bool(1+(unsigned)T::val)>>;

template<typename T>
  struct Assign<T, T, BoolVal<bool(1+(unsigned)T::val)>> {};


Assign<IntVal<0>, IntVal<0>, BoolVal<1>> ai;

