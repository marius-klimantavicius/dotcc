//type: fp
//options:  --c++11: --c++11: --c++11
# 1 "SemaCXX/pragma-optimize.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 452 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/pragma-optimize.cpp" 2




#pragma clang optimize off
# 15 "SemaCXX/pragma-optimize.cpp"
extern int foo(int a, int b);



int bar(int x, int y) {
    for(int i = 0; i < x; ++i)
        y += x;
    return y + foo(x, y);
}






int created (int param) { return param; }


class MyClass {
    public:

        int method(int blah);
};


int MyClass::method(int blah) {
    return blah + 1;
}



template <typename T> T twice (T param);


template <typename T> T thrice (T param) {
    return 3 * param;
}



int __attribute__((always_inline)) baz(int z) {
    return foo(z, 2);
}




int __attribute__((minsize)) bax(int z) {
    return foo(z, 2);
}


#pragma clang optimize on





int wombat (int param) { return param; }





float container (float par) {
    return twice(par);
}






float container2 (float par) {
    return thrice(par);
}







template<> int thrice(int par) {
    return (par << 1) + par;
}
int container3 (int par) {
    return thrice(par);
}







#pragma clang optimize off

int another_optnone(int x) {
    return x << 1;
}


#pragma clang optimize on

int another_normal(int x) {
    return x << 2;
}







# 1 "SemaCXX/Inputs/header-with-pragma-optimize-off.h" 1


#pragma clang optimize off
# 131 "SemaCXX/pragma-optimize.cpp" 2

int yet_another_optnone(int x) {
    return x << 3;
}


#pragma clang optimize on

int yet_another_normal(int x) {
    return x << 4;
}
