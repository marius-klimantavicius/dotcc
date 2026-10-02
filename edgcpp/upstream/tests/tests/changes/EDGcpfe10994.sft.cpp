//type:fp
//options_all:--microsoft
//remark:[4.12] Microsoft compatibility: __event __interface
// 6/2/16   [EDGcpfe/10994]
//
// Microsoft compatibility: __event __interface
//
// The "__event __interface" construct is now supported for COM event sources.
// No lowering of this construct is performed.
__interface IEvents {
  void MyEvent(int value);
};
class CSource {
  __event __interface IEvents;
};
