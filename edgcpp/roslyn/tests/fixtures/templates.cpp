template<class T, int N=3> struct Holder { T value; T read() const { return value+N; } };
template<class T> using Alias = Holder<T, 4>;
template<class T> constexpr T shared_value = T(42);
template<class T> T identity(T value) { return value; }
template<class T> T identity(T *value) { return *value; }
template<> int identity<int>(int value) { return value+1; }
int template_use() { Alias<int> h{shared_value<int>}; return h.read()+identity(2); }
int scalar_static() { static int retained_scalar=42; return retained_scalar; }
namespace scope_owner { struct Item { int value; }; }
int local_class_owner() { struct Local { int value; int read() { return value; } }; Local x{3}; return x.read(); }
