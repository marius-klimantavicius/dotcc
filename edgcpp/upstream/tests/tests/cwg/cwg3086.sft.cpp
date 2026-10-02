//type:fp
//options:--c++17:--c++26
//options_all:-A -W -tused

void f()
{
  _Pragma("pack(1)");
  _Pragma(u"pack(1)");
  _Pragma(u8"pack(1)");
  _Pragma(L"pack(1)");
  _Pragma(U"pack(1)");
}

//cwg: 3086
//title: Destringizing should consider all sorts of encoding-prefixes
//meeting: Kona 11/25
//edg_status: Passes
