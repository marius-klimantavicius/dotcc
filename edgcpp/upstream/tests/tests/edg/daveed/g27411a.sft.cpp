//remark:C++20 generated comparisons
//options:--c++20;fp

class S {
    operator char*();
    operator void*();
    friend bool operator==(S, char);
} s;

bool r =  s != 0;
