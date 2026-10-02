//type:fp
//options_all:--microsoft
//remark:[5.1] Microsoft compatibility: __uuidof and constexpr
// 5/10/19  [EDGcpfe/15513,EDGcpfe/20180]
//
// Microsoft compatibility: __uuidof and constexpr
//
// In Microsoft modes, the front end now treats __uuidof expressions as full-
// fledged constant expressions.
typedef struct _GUID {
  unsigned long  Data1;
  unsigned short Data2;
  unsigned short Data3;
  unsigned char  Data4[8];
} GUID;
struct __declspec(uuid("{12345678-ABCD-EFFE-DCBA-987654321000}")) X {};
constexpr GUID guid = __uuidof(X);        // Now accepted in Microsoft mode.
static_assert(guid.Data3 == 0xEFFE, "");  // Ditto.
