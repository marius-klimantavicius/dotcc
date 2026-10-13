int __near *constant_near_pointer;
int __far *constant_far_pointer;
struct __near ConstantNearRecord { int value; };
struct __far ConstantFarRecord { int value; };
typedef void (__far *ConstantFarFunction)();
ConstantFarFunction constant_far_function;
const char *constant_native_function_name() { return __FUNCSIG__; }
