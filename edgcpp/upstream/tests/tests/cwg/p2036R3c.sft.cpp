//type:fp
//options_all:--c++20 -tused -A
int i;
auto counter = [i=0]() mutable -> decltype(i) { 
    return i++;
};
