//type:fp
//options_all:--c++20
//remark:[5.1] C++20: "likely" and "unlikely" standard attributes
// 10/12/18 [EDGcpfe/20025]
//
// C++20: "likely" and "unlikely" standard attributes
//
// The "likely" and "unlikely" attributes (meant to provide hints to a back end to
// aid in optimization) are now accepted on labels and statements (see P0479R5).
// The front end accepts and validates these attributes but takes no action on
// them.
void f(int i) {
  switch (i) {
    case 1: 
      goto done;
    default:
      [[unlikely]]
      break;
  }
[[likely]] done:;
}
