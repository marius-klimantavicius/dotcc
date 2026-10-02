//options_all:--microsoft_version=1929
//type:fn
template <typename T>
struct S {
    ~S() noexcept;
};

template <typename T>
S<T>::~S() {} // now diagnoses a different exception specification
