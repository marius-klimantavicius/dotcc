//type:fp
//options:--c++23
//options_all:-A

extern const int arr[];
constexpr const int *p = arr + 0; // OK
constexpr int arr[2] = {0, 1};
constexpr int k = *p;           // OK

//cwg: 3004
//title: Pointer arithmetic on array of unknown bound
//meeting: Kona 11/25
//edg_status: EDGcpfe/28536
