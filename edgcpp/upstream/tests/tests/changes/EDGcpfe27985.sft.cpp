//type:fp
//options_all:--gcc --gnu=150100
//remark:[6.8] GNU C compatibility: nullptr keyword
// 2/24/24  [EDGcpfe/27985]
//
// GNU C compatibility: nullptr keyword
//
// The C23 Standard adds support for the nullptr keyword and nullptr_t type
// (see the entry for EDGcpfe/25950,EDGcpfe/26287,EDGcpfe/27445).  The release
// candidate for gcc 15.1.0 recognizes the nullptr keyword (but defines
// nullptr_t as a typedef in its <stddef.h> header) in non-C23 C modes if
// nullptr has not been otherwise declared, and the front end has now been
// changed accordingly when gnu_version is at least 150000.
// --gcc --gnu_version=150100:
void *p = nullptr;   // Now accepted in non-C23 GNU C modes
