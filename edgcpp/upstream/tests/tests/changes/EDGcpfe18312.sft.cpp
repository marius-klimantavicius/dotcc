//type:fp
//options_all:--c++17
//remark:[5.0] C++17 compatibility: __is_aggregate type traits helper
// 1/2/18   [EDGcpfe/18312,EDGcpfe/18347,EDGcpfe/19018]
//
// C++17 compatibility: __is_aggregate type traits helper
//
// The front end now recognizes the __is_aggregate type traits helper, which
// supports the std::is_aggregate trait.  It returns TRUE if the type is an
// aggregate as described in the C++ Standard or if the type is a vector or
// complex type in modes that support those types.
typedef int arr[10];
bool b = __is_aggregate(arr);  // b is initialized to true
