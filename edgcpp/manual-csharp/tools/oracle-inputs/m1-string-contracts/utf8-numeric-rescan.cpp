// UTF8 result retains arbitrary numeric code units; separate from wide rescan.
static_assert((u8"" "\xff")[0]==255);
static_assert(sizeof(u8"" "\xff")==2);
static_assert(("\xff" u8"")[0]==255);
