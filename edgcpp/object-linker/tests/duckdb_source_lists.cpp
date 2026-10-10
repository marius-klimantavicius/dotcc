namespace original_lists {
template<class T> void accept(const T&);
template<class T> concept Empty = requires { accept<T>({}); };
template<class T> concept Pair = requires { accept<T>({1, 2}); };
template<class... Ts> concept Left = (... && Empty<Ts>);
template<class... Ts> concept Right = (Empty<Ts> && ...);
template<class... Ts> concept BinaryLeft = (true && ... && Empty<Ts>);
template<class... Ts> concept BinaryRight = (Empty<Ts> && ... && true);
template<class T> concept Folded = Left<T,int> && Right<T,short> && BinaryLeft<T,int> && BinaryRight<T,short>;
template<Empty T> struct EmptyOwner { T value = {}; };
template<Pair T> struct PairOwner { T value = {}; };
template<Folded T> struct FoldOwner { T value = {}; };
struct Members { int first; int second; long other; };
template<class T> T source();
template<class T> using SelectedMember = decltype(((Members*)source<T>())->first);
template<class T> struct MemberOwner { SelectedMember<T> field = {}; };
static_assert(Left<int,short> && Right<int> && BinaryLeft<int,short> && BinaryRight<int>);
}
int main() { original_lists::FoldOwner<int> value{}; return value.value; }
