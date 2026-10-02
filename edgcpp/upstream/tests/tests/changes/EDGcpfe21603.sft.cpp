//type:fp
//options_all:--c++17 --gnu_version=70300
//remark:[6.1] Spurious GNU C++-mode error on explicit specialization with noexcept specifier
// 5/22/20  [EDGcpfe/21603,EDGcpfe/22071,EDGcpfe/22228,EDGcpfe/22661]
//
// Spurious GNU C++-mode error on explicit specialization with noexcept specifier
//
// In GNU C++17 mode, the front end sometimes failed to match up an explicit
// specialization with the corresponding function template that includes a
// noexcept specifier.  An error was issued as a result of this failure.
//
// That is now fixed.
template<typename> void f() noexcept(true);
template<> void f<int>() noexcept;  // Previously an error in GNU C++17
                                    // mode.  Now okay.
