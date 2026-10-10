#ifndef SOURCE_TYPE_INFORMATION_C_API_H
#define SOURCE_TYPE_INFORMATION_C_API_H

#ifdef SOURCE_TYPE_C_REORDERED
enum NamedKind { NamedHigh = 6, NamedLow = -2 };
struct CEnumCatalog {
    enum { AnonymousHigh = 9, AnonymousMiddle = 5, AnonymousLow = 1 } value;
    enum NamedKind named;
};
#else
enum NamedKind { NamedLow = -2, NamedHigh = 6 };
struct CEnumCatalog {
    enum { AnonymousLow = 1, AnonymousMiddle = 5, AnonymousHigh = 9 } value;
    enum NamedKind named;
};
#endif

int read_c_enum(const struct CEnumCatalog* value);
#endif
