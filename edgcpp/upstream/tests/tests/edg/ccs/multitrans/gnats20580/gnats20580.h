template <class a, class b, class... c>
auto f(a &&d, b &&, c &&...) -> decltype((d));
template <class a, class b, class... c>
auto f(a &&, b &&e, c &&...) -> decltype((e));
