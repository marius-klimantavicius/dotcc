//options_all:-r -x -tused
//options: --cfront_3.0;cp

void f(int i) {

DL01:  goto L1;     // error: jump over j
DL02:  goto L2;     // error: jump over j,k
DL03:  goto L3;     // error: jump over j
DL04:  goto L4;     // error: jump over j,l
DL05:  goto L5;     // error: jump over j,l,m
DL06:  goto L6;     // error: jump over j,l,m,n

  int j = 0;

DL11:  goto L1;     // okay
DL12:  goto L2;     // error: jump over k
DL13:  goto L3;     // okay
DL14:  goto L4;     // error: jump over l
DL15:  goto L5;     // error: jump over l,m
DL16:  goto L6;     // error: jump over l,m,n

  if (i != 0) {
L1:;

DL21:  goto L1;     // okay
DL22:  goto L2;     // error: jump over k
DL23:  goto L3;     // okay
DL24:  goto L4;     // error: jump over l
DL25:  goto L5;     // error: jump over l,m
DL26:  goto L6;     // error: jump over l,m,n

    int k = 0;
L2:;

DL31:  goto L1;     // okay
DL32:  goto L2;     // error: jump over k  ??? cfront: NO ERROR
DL33:  goto L3;     // okay
DL34:  goto L4;     // error: jump over l
DL35:  goto L5;     // error: jump over l,m
DL36:  goto L6;     // error: jump over l,m,n

  }
L3:
  int l = 0;
L4:;

DL41:  goto L1;     // okay
DL42:  goto L2;     // error: jump over k
DL43:  goto L3;     // okay
DL44:  goto L4;     // okay
DL45:  goto L5;     // error: jump over m
DL46:  goto L6;     // error: jump over m,n

  if (i != 0 ) {
    {
    int m = 0;

DL51:  goto L1;     // okay
DL52:  goto L2;     // error: jump over k
DL53:  goto L3;     // okay
DL54:  goto L4;     // okay
DL55:  goto L5;     // okay
DL56:  goto L6;     // error: jump over n

L5:
    int n = 0;

DL61:  goto L1;     // okay
DL62:  goto L2;     // error: jump over k
DL63:  goto L3;     // okay
DL64:  goto L4;     // okay
DL65:  goto L5;     // error: jump over m
DL66:  goto L6;     // okay

L6:;
    }
  }
}

