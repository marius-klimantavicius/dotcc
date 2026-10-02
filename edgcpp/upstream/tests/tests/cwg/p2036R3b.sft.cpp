//options_all:--c++20 -tused -A
int x = 4;
auto y = [&r = x, x = x+1]()->int {
    r +=2;
    return x+2;
} ();
int i;
auto counter = [i=0]() mutable -> decltype(i) { 
    return i++;
};
