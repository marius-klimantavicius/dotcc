//remark:uuidof and explicit specializations
//options:--microsoft;fp

typedef struct _GUID {
   unsigned long  Data1;
   unsigned short Data2;
   unsigned short Data3;
   unsigned char  Data4[ 8 ];
} GUID;

template <typename T> class IFoo;

template <> class
   __declspec(uuid("73BD59D0-7FB0-45E4-A44E-A4494EABE793"))
   IFoo<int> { };

int main() {
   (void)__uuidof(IFoo<int>);
}

