template<class T> auto automatic(T value) { return value; }
template<class T> decltype(auto) reference(T& value) { return (value); }
template<class T> concept Sized = sizeof(T) > 0;
template<class T> Sized auto constrained(T value) { return value; }
template<class T> struct Holder { using R=decltype(automatic(T())); R value; };
template<class T> struct Holder2 { using R=decltype(reference(*(T*)0)); R value; };
template<class T> struct Holder3 { using R=decltype(constrained(T())); R value; };
int main() { int x=4; Holder<int> a{3}; Holder2<int> b{x}; Holder3<int> c{5}; return a.value+b.value+c.value==12?0:1; }
