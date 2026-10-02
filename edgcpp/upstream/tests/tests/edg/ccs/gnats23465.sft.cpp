//type:cp
//options:--c++20:--microsoft_version 1926
//options_all:-tused

struct data
{
    int x;
};

template<typename T>
struct X
{
    static constexpr data value = data{ 0 };
};

template<typename T>
struct Y
{
    static constexpr data value = X<T>::value;
};

template<typename T>
struct Z : Y<T> { };
