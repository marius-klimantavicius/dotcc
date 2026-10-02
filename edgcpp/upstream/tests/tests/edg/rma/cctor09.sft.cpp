//options_all:-r -x -tused
//options: --strict;cn:;cn

        class Base {
        public:
            Base ( Base& );
            Base ( int );
        };

        void bar ()
        {
            Base b1 ( 9 );      // Okay, as before
            Base b2 = 9;        // Error, cannot create temporary for non-const
                                // reference argument to copy constructor.
            Base b3 = Base ( 9 );       // Okay, explicit generation of the
                                        // required temporary
            Base b4 = (Base)9;  // Same thing
        }

