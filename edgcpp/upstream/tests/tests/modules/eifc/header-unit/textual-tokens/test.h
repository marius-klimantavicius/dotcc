template<typename T>
inline bool has_nothrow_assign_proxy()
{
  return __has_nothrow_assign(T);
}


template<typename T>
inline bool has_nothrow_constructor_proxy()
{
  return __has_nothrow_constructor(T);
}

template<typename T>
inline bool is_trivial_proxy()
{
  return __is_trivial(T);
}

