//type:fp
//options_all:--c++17
//remark:[6.2] Spurious failure substituting parenthesized new-initializer
// 9/11/20  [EDGcpfe/22429,EDGcpfe/23337]
//
// Spurious failure substituting parenthesized new-initializer
//
// The front end previously sometimes failed to substitute a parenthesized
// new-initializer when the allocated type is not a class type and the
// parenthesized expression is a pack expansion.
//
// That problem is now fixed.
inline void *operator new(__EDG_SIZE_TYPE__, void *p) noexcept { return p; }
template<typename...> using void_t = void;
template<typename T> T makeT() noexcept;
template<typename T, typename... Args>
auto place_at(T *p, Args &&...args)
  -> decltype(::new ((void*)p) T(args...));
     // The substitution of args... was previously not handled correctly.
template<typename Void, typename, typename...> bool can_place_at = false;
template<typename T, typename... Args>
bool can_place_at<void_t<decltype(place_at(makeT<T*>(), makeT<Args>()...))>,
                  T, Args...> = true;
struct X {};
auto r = can_place_at<void, int, X>;
    // Previously triggered a spurious error above.  Now okay.
