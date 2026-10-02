//options_all:-r -x -tused
//options: --strict;cp:;rp

int x = 0;

int f() try { if (x>0) throw x; return 0; }
catch (int i) { if (i>0) throw; return 0; }
catch (...) {}

int main() try { return f(); }
catch (int i) { if (i>0) throw; }
catch (...) { }


