//type:fn
//options:--c++23
//options_all:-A

extern const int arr[];
constexpr const int *p = arr + 1;  // error
constexpr int arr[2] = {0, 1};
constexpr int k = *p;
