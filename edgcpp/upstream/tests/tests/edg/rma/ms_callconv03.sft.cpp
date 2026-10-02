//options_all:-r -x -tused
//options: --microsoft -n;cn

typedef void __stdcall F(void);
typedef F __stdcall *PFN;
F fff;
void __stdcall fff() {}
void fff();  // error, different modifiers
extern PFN pf;  // anachronism used: modifiers on data are ignored

