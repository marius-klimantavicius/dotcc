//type: fp
//options: 
# 0 "./tree-ssa/pr90356-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/pr90356-2.c"







# 1 "./tree-ssa/pr90356-1.c" 1







double f1 (double x) { return (x + 0.0) + 0.0; }
double f2 (double y) { return (y + (-0.0)) + (-0.0); }
double f3 (double y) { return (y - 0.0) - 0.0; }
double f4 (double x) { return (x - (-0.0)) - (-0.0); }
double f5 (double x) { return (x + 0.0) - 0.0; }
double f6 (double x) { return (x + (-0.0)) - (-0.0); }
double f7 (double x) { return (x - 0.0) + 0.0; }
double f8 (double x) { return (x - (-0.0)) + (-0.0); }
double f9 (double x) { double t = x + 0.0; return t + 0.0; }
double f10 (double y) { double t = y + (-0.0); return t + (-0.0); }
double f11 (double y) { double t = y - 0.0; return t - 0.0; }
double f12 (double x) { double t = x - (-0.0); return t - (-0.0); }
double f13 (double x) { double t = x + 0.0; return t - 0.0; }
double f14 (double x) { double t = x + (-0.0); return t - (-0.0); }
double f15 (double x) { double t = x - 0.0; return t + 0.0; }
double f16 (double x) { double t = x - (-0.0); return t + (-0.0); }
# 9 "./tree-ssa/pr90356-2.c" 2
