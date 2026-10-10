// Prototype enum values remain typed semantic expressions until substitution.
enum GlobalEnum { global_zero, global_one };
GlobalEnum retain_global_enum(GlobalEnum value) { return value; }
namespace original_enums {
template<class T> T source();
template<class T> struct Enums {
    enum E { first = sizeof(T), second, third = first + 8, conditional = sizeof(T)>2 ? sizeof(T)-1 : 2 };
    enum class Scoped { zero, next };
    enum { anonymous_zero, anonymous_next };
    enum class Declared : T { zero, next };
    static E value(T);
    static Scoped scoped();
    static decltype(anonymous_zero) anonymous();
    static Declared declared();
};
template<class T> struct Owners {
    decltype(Enums<T>::value(source<T>())) value = {};
};
template<class T> struct ScopedOwner { decltype(Enums<T>::scoped()) value = {}; };
template<class T> struct AnonymousOwner { decltype(Enums<T>::anonymous()) value = {}; };
template<class T> struct DeclaredOwner { decltype(Enums<T>::declared()) value = {}; };
template<class T> concept ConcreteSelected = requires { Enums<int>::value(1); Enums<int>::scoped(); Enums<int>::anonymous(); Enums<int>::declared(); };
template<ConcreteSelected T> struct Filter { T value = {}; };
static_assert(Enums<int>::first == sizeof(int));
static_assert(Enums<int>::second == sizeof(int)+1);
static_assert(Enums<int>::third == sizeof(int)+8);
static_assert(static_cast<int>(Enums<int>::Scoped::next)==1);
static_assert(static_cast<int>(Enums<int>::Declared::next)==1);
}
int main() { return original_enums::Enums<int>::third == sizeof(int)+8 ? 0 : 1; }
