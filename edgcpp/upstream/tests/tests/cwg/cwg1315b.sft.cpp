//type:fn
//options_all:--c++17 -tused -A
//
template< int X, int (*array_ptr)[X] > class A {};
int array[5];
template< int X > class A<X,&array> { }; //error
