//remark:Abbreviated member templates
//options:--c++20;fp

template <class T>
struct D {
    void foo(auto);
};

template <class T>
void D<T>::foo(auto) {}
