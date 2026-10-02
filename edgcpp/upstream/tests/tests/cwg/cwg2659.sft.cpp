//options_all:--c++23
#ifndef __cpp_range_based_for
#  error "__cpp_range_based_for"
#elif  __cpp_range_based_for != 202211L
#  error "__cpp_range_based_for != 202211L"
#endif

//cwg: 2659
//title: Missing feature-test macro for lifetime extension in range-for loop
//meeting: Issaquah 2/23
//edg_status: EDGcpfe/26070
//fixed_in: 6.8
