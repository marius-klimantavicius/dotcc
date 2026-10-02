//options_all:-r -x -tused
//options: --strict;cn

// Microsoft asm block
void f() {
  __asm {
    xxx;
    yyy;
    zzz;
  };
  __asm { xxx }
  __asm { xxx; yyy }
  __asm { xxx;
        yyy; }
  __asm { xxx;


  };
}

