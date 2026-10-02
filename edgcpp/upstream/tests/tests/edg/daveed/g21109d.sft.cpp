//remark:Explicit template argument substitution in deduction
//options:--c++17;fp

               struct V { static const int value = 0; };
               template<typename T> struct X { using type = T; };
               template<int I, typename U>
                 static inline X<U> g(X<U>*);
               struct Y: X<int> {
                 template<int I> using A = decltype(g<I>((Y*)(nullptr)));
                 template<typename T> typename A<T::value>::type& f(T);
               };
               auto r = Y{}.f(V{});
