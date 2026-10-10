template<class T> struct Result { T value; ~Result() {} };
template<class T> struct Argument { T value; };
using Callback = Result<int>(*)(Argument<long>);
using RefCallback = Result<int>(*)(int&, Argument<long>);
using PtrCallback = Result<int>(*)(int*, Argument<long>);
struct Holder { Callback callback; RefCallback reference; PtrCallback pointer; };
extern Holder shared;
int storage_check();
Result<int> make_result(Argument<long> argument) { return {int(argument.value)}; }
int main() {
    if (storage_check()) return 1;
    shared.callback = &make_result;
    return shared.callback(Argument<long>{42}).value == 42 ? 0 : 2;
}
