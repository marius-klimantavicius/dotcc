//remark:Duplicate definition
//options:--microsoft;fp

typedef struct _GUID {
    
    unsigned long  Data1;

} GUI;

 GUI IID_it;

extern "C" __declspec(selectany)  GUI IID_it = { 0x1df0111 };
