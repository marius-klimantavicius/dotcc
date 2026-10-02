//remark:Coroutines and prototype instantiations
//type:fp
//options_all:--microsoft_v=1914 --parse --set_flag coroutines -tused

template <typename T>
auto f(T t)
{
  co_yield t;
}
