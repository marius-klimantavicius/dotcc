char text[]="x"; static_assert(sizeof(text)==2);
constexpr char padded[3]="x"; static_assert(padded[2]==0);
