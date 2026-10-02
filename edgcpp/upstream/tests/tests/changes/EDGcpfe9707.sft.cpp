//type:fp
//options_all:--microsoft
//remark:[4.2] Microsoft compatibility: both derived and virtual base classes have dllimport
// 8/7/09  [EDGcpfe/9707]
//
// Microsoft compatibility: both derived and virtual base classes have dllimport
// attribute
//
// In IA-64 ABI configurations when using Microsoft emulation mode, a derived
// class based upon a virtual class and requiring a virtual function table where
// both classes have the dllimport attribute specified had resulted in an
// assertion failure (in define_one_virtual_function_table).  This is now fixed.
//
// ------------------------------------------------------------------------------
struct __declspec(dllimport) B {
  virtual ~B();
  int i;
};
struct __declspec(dllimport) D : virtual B {};
D d;
