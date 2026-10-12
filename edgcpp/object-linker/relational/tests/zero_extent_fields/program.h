struct ShortShape {
    unsigned char header;
    char padding[0];
    char data[23];
};
struct Guarded {
    unsigned char before;
    ShortShape value;
    unsigned char after;
};
extern Guarded guarded;
Guarded *guarded_address();
unsigned long owner_size();
unsigned long padding_size();
char *padding_address(ShortShape *value);
char *dot_padding_address(ShortShape *value);
char *data_address(ShortShape *value);
int local_check();
