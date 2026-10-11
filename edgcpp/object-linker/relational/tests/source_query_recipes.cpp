#include <typeinfo>

template<class T>
void source_query(T&) noexcept;

template<class T, bool Quiet = noexcept(source_query(*(T*)0))>
struct NoexceptRecipe {};

template<class T, bool Quiet = noexcept((*(T*)0).~T())>
struct DestructorRecipe {};

template<bool, class T> struct SelectedIf {};
template<class T> struct SelectedIf<true, T> { using type = T; };
template<bool Predicate, class T>
using SelectedIfT = typename SelectedIf<Predicate, T>::type;
template<class T> struct SelectedCondition { static constexpr bool value = sizeof(T) > 0; };

template<class T, bool Value = SelectedIfT<SelectedCondition<T>::value, bool>{true}>
struct BracedRecipe {};

template<class T>
constexpr const std::type_info* source_typeid_address = &typeid(T);

int concrete_query(int value) noexcept {
    return value;
}

constexpr bool folded_noexcept = noexcept(concrete_query(7));

int main() {
    return folded_noexcept ? 0 : 1;
}
