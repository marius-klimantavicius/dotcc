//type: fp
//options: --c++14
// PR c++/67041
// { dg-do compile { target c++14 } }

template<typename T>
auto test = [](){
    return T{};
};

int main() {
    test<int>();
}
