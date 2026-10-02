//type:fp
//options:--c++26 -A

unsigned char arr[] = {
#embed __FILE__ prefix(1*) suffix(*1)
};

//cwg: 3014
//title: Comma-delimited vs. comma-separated output for #embed
//meeting: Sofia 6/25
//edg_status: Passes
