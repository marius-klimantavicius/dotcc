struct LongShape {
    unsigned long __is_long_ : 1;
    unsigned long __cap_ : 63;
    char* __data_;
};

struct ShortShape {
    unsigned char __is_long_ : 1;
    unsigned char __size_ : 7;
    unsigned char __data_[3];
};

struct __attribute__((packed)) Packed {
    unsigned char before;
    signed int signed_value : 5;
    unsigned int low : 3;
    unsigned int crossing : 9;
    bool enabled : 1;
    unsigned char after;
};

extern int effects;
extern int trace;
void update(LongShape*, ShortShape*, Packed*);
void ordered_store(Packed*);
