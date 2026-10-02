//options_all:-r -x -tused
//options: --microsoft -n;cp

class C {
public:
   __declspec(dllexport) C();
   __declspec(dllexport) ~C();
};
__declspec(dllexport) C::C() {}
__declspec(dllexport) C::~C() {}

class D {
public:
   __inline D();
   __inline ~D();
};
__inline D::D() {}
__inline D::~D() {}

