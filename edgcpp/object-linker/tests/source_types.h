typedef long Count;
typedef Count ItemCount;
typedef unsigned long long Mask;
#ifdef __cplusplus
typedef bool Toggle;
#else
typedef _Bool Toggle;
#endif

Toggle is_enabled(Toggle input);
ItemCount source_shapes(const Count limit, Mask mask, Toggle enabled);
int alternate_parameter(int input);
