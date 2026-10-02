//options_all:-r -x -tused
//options: --strict;cn:--diag_warn=260;rp

                class alpha
                   {
                   public:
                    static const size;
                    static int array [];
                   };
                const alpha::size = 4;
                int alpha::array[       size] = { 0, 1, 2, 3 };  // error now
//              int alpha::array[alpha::size] = { 0, 1, 2, 3 };  // works
                main(){}


