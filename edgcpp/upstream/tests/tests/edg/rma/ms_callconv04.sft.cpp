//options_all:-r -x -tused
//options: --microsoft -n;cp

           typedef void F(int);
           typedef const F CF;
           extern CF __stdcall f;
           typedef F FF;
           extern FF __stdcall g;

