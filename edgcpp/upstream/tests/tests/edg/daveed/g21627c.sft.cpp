//remark:GNU bug regression
//options:--c++14 --gnu=80300;fp

template<typename> struct B {
    template<typename U> B& operator=(U v);
};

template<typename T> struct D: public B<T> {
    using B<T>::operator=;
    template<typename U> D& operator=(U v);
};

int main() {
    D<int> di;
    di = 2;
}
