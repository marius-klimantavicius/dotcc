//type:cp
//options::--gnu_version 80000
//options_all:--c++14

template<typename _Signature> class function;
template<typename _Res, typename... _ArgTypes>
struct function<_Res(_ArgTypes...)>
{
  template<typename _Functor> function(_Functor);
};

struct Lambda
{
   function<void(const int&)> callback_ =
      [this](const auto v) { local = v; this->local = 7; };
   int local{};
};
