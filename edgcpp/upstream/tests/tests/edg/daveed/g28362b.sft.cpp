//remark:Matching partial specialization with concepts
//options:--c++20;fn

template<typename>
concept C = true;

template<typename T> struct B {
 template<typename U> struct N;
};

template<typename T> template<typename U> requires C<T> && C<U>
struct B<T>::N<U *>  // #1
{ using type1 = int; };

template<> template<typename U> requires C<int> && C<U>
struct B<int>::N<U *>  // #2
{ using type2 = int; };

B<int>::N<int *>::type2 i;

