// Original, unevaluated concept definitions remain owned even when this
// default-member template is never instantiated. Mirrors libc++ subrange.
namespace DuckdbConstraints {
template<class A,class B> concept Same = __is_same(A,B);
template<class T> constexpr bool object_v = __is_class(T);
template<class T> concept Object = object_v<T>;
template<class T> concept Cursor = Object<T> && requires(T cursor) {
  typename T::value_type;
  *cursor;
  { ++cursor } noexcept -> Same<T&>;
  requires __is_class(T);
};
template<Cursor Iter> struct Range { Iter begin = Iter(); };
template<class T> concept Nested = requires(T outer) {
  requires requires(T inner) { outer + inner; };
};
template<Nested T> struct NestedOwner { T value = T(); };
template<class T> T&& original_forward(T&);
template<class T> concept Callable = requires(T& value) { original_forward<T>(value); };
template<Callable T> struct CallableOwner { T value = T(); };
// Keep both an empty pack boundary and an unevaluated expansion descriptor.
template<class T,class... Tail> concept FirstClass = __is_class(T);
template<class T,class... Tail> concept ForwardPack = FirstClass<T,Tail...>;
template<class T> concept EmptyPack = FirstClass<T>;
template<class T> concept FilledPack = ForwardPack<T,T,int>;
template<EmptyPack T> struct EmptyPackOwner { T value = T(); };
template<FilledPack T> struct FilledPackOwner { T value = T(); };
template<class... Types> struct Common {};
template<class T,class... Tail> struct PackRecordOwner { Common<T,Tail...> value = {}; };
template<class T> struct EmptyRecordOwner { Common<> value = {}; };


template<class T> concept Extent = sizeof(T) > 0 && alignof(T) > 0;
template<Extent T> struct ExtentOwner { T value = T(); };
// A class-valued constexpr customization point is a typed initializer, not a
// guessed boolean/integer constant or an executable source graph declaration.
struct FunctionObject {
  template<class T> constexpr int operator()(T&) const { return 1; }
};
inline constexpr FunctionObject invoke{};
template<class T> concept Invocable = requires(T& value) { invoke(value); };
template<Invocable T> struct InvocableOwner { T value = T(); };
// A reference to a variable template may also occur in a class argument's
// constant graph, independently of C++20 concepts (libc++ optional pattern).
template<bool Value> struct Gate {};
template<class A,class B> constexpr bool same_v=__is_same(A,B);
template<class T> struct OptionalLike { Gate<same_v<T,bool>> value={}; };

struct Iterator {
  using value_type = int;
  int value;
  int& operator*() noexcept { return value; }
  Iterator& operator++() noexcept { ++value;return *this; }
};
template<Cursor T> int selected(T& value) { return *value; }
template<class T> int selected(T&) { return -1; }
}
int main() {
  DuckdbConstraints::Iterator value{37};
  return DuckdbConstraints::selected(value)==37 ? 0:1;
}
