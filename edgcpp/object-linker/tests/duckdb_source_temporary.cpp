// Unevaluated source temporary recipes must not become executable lifetimes.
namespace source_temporaries {
template<class T> struct Box { int tag; };
template<class T> struct Indexer { using type = T; };
template<class T> Indexer<T> select_index(Indexer<T>);
template<class T> using Indexed = typename decltype(select_index(Indexer<T>{}))::type;
template<class T> struct Owner { Box<Indexed<T>> value = {}; };

struct Concrete { explicit Concrete(int); ~Concrete(); };
template<class T> T select_value(Concrete);
template<class T> using Selected = decltype(select_value<T>(Concrete(7)));
template<class T> struct SelectedOwner { Box<Selected<T>> value = {}; };

// These are ordinary members of a class template, not function templates.
template<class T> struct Wrapper { static T unwrap(T); };
template<class T> T source();
template<class T> using Unwrapped = decltype(Wrapper<T>::unwrap(source<T>()));
template<class T> struct MemberOwner { Box<Unwrapped<T>> value = {}; };
template<class T> concept MemberAvailable = requires { Wrapper<T>::unwrap(source<T>()); Wrapper<int>::unwrap(1); };
template<MemberAvailable T> struct MemberConstrained { T field = {}; };
static_assert(sizeof(Indexed<int>)==sizeof(int));
static_assert(sizeof(Selected<long>)==sizeof(long));
static_assert(sizeof(Unwrapped<int>)==sizeof(int));
}
int main() { return 0; }
