//type:fn
//options:--c++26 -A

unsigned char arr[] = {
0,
#embed __FILE__ limit(defined(FOO))
};

//cwg: 3018
//title: Validity of defined in __has_embed
//meeting: Sofia 6/25
//edg_status: EDGcpfe/28248
