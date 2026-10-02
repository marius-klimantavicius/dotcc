//type:fp
//options_all:--microsoft_version=1916 -tused
//remark:[6.2] Abort on constexpr member variable in non-real instantiations
// 12/7/20  [EDGcpfe/23465]
//
// Abort on constexpr member variable in non-real instantiations
//
// When a class contains a constexpr member variable with a dependent initializer,
// the front end could previously abort when attempting a non-real instantiation
// of the class.
//
// This is now fixed.
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
struct Z : Y<T> { }; // Triggers non-real instantiation in Microsoft mode
