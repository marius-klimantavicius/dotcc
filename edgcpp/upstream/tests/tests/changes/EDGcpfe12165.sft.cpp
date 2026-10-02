//type:fp
//options_all:--microsoft
//remark:[4.4] Microsoft compatibility: Attributes preceding linkage specifications
// 9/20/11  [EDGcpfe/12165]
//
// Microsoft compatibility: Attributes preceding linkage specifications
//
// In Microsoft mode, the front end now accepts Microsoft attributes (enclosed
// in square brackets) preceding a linkage specification.
[Any(1)] extern "C" void f(); // Now valid in Microsoft mode (if "Any"
                              // is an acceptable attribute).
