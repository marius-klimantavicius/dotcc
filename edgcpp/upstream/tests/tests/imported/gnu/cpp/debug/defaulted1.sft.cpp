//type: fp
//options: --c++11
// PR c++/90674
// { dg-do compile { target c++11 } }

template<typename T>
struct C {
    C() {}
};

template<>
C<int>::C() = default;
