//type:fn
//options_all:--microsoft_v 1916 --no_ms_permissive
template <typename ...Ts>
struct A {
public:
              template <typename U>
              using from_template_t = decltype(from_template(A<U>{}));
 
private:
              template <template <typename...> typename Type, typename ...Args>
              static constexpr A<Args...> from_template(A<Type<Args...>>);
};
 
A<>::from_template_t<A<int>> a;
