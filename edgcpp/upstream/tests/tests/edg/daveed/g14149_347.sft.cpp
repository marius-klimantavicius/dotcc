//remark:Complex type interpretation
//options:--gcc;fp

// GNU C mode:
#define cx()  (1.0 + 2.0i)
__complex__ double scx = cx()*cx();
