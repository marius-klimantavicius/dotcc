//options_all:--microsoft
template <class T> void f(T*) {}
template <class T> decltype(f(f(T()))) f(T&) {}

void g(int* p)
{
    f(p);
}
