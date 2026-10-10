// Original template-template references have their own formal parameter graph.
// None of these templates is instantiated: default member initializers retain
// the dependent declarations without requiring executable placeholder types.
template<class T, template<class, class> class Specifics>
struct DuckdbSourceTemplateParameterOwner {
    Specifics<T, int>* direct = nullptr;
    Specifics<int, T>* reverse = nullptr;
    DuckdbSourceTemplateParameterOwner<T, Specifics>* self = nullptr;
};
template<class T, template<class, class> class Specifics>
struct DuckdbOtherTemplateParameterOwner {
    Specifics<T, int>* other = nullptr;
};
template<template<class U = int, int Count = 7> class WithDefaults>
struct DuckdbSourceTemplateDefaultOwner {
    WithDefaults<>* defaults = nullptr;
};
template<template<class> class Inner,
         template<template<class> class, class> class Outer>
struct DuckdbSourceNestedTemplateOwner {
    Outer<Inner, int>* nested = nullptr;
};
int main() { return 0; }
