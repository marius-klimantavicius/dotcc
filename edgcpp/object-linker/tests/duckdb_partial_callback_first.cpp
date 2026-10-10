template<class T> struct Result { T value; ~Result() {} };
template<class T> struct Argument { T value; };
using Callback = Result<int>(*)(Argument<long>);
using RefCallback = Result<int>(*)(int&, Argument<long>);
using PtrCallback = Result<int>(*)(int*, Argument<long>);
struct Holder { Callback callback; RefCallback reference; PtrCallback pointer; };
Holder shared;
int result_width() { return sizeof(Result<int>); }
