//remark:__is_contructible recursion
//options:--c++17;fp

template <bool B, typename T>
struct enable_if { };

template <typename T>
struct enable_if<true, T> { using type = T; };

template <typename T, typename ...Ts>
constexpr bool is_constructible_v = __is_constructible(T, Ts...);

struct any
{
  template<class _Ty,
           class = typename enable_if<is_constructible_v<_Ty, const _Ty &>, void>::type >
  any(_Ty&& _Val) { }
};

template <typename _Ty>
struct optional {
  optional() { }

  template <typename _Uty,
            typename enable_if<!is_constructible_v<_Ty, optional<_Uty>&&>, int>::type = 0>
  optional(const optional<_Uty>& other) { }
};

int main()
{
  optional<any> oa2;
  optional<any> oa3(oa2);
}
