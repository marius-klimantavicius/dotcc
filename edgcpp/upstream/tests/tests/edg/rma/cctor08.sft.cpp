//options_all:-r -x -tused
//options: --strict;cn:;cp

        class Base {
        private:
            Base ( const Base& );       // A 'private' copy constructor
        public:
            Base ( int );
        };

        class Derived : public Base {
        public:
            Derived ( int );
        };

        void foo ()
        {
            Base b1 ( 8 );      // Okay, uses 'Base::Base(int)'
            Base b2 = 8;        // Error, uses 'Base::Base(int)' to create
                                // an unnamed temporary, and then tries to
                                // call the 'private' copy constructor for
                                // Base.  Hence an illegal access to a
                                // 'private' member of 'Base'
 
            Derived d1 ( 7 );   // Okay, uses 'Derived::Derived(int)'
            Derived d2 = 7;     // Error, 'Derived' has NO copy constructor
                                // since it was not possible for the compiler
                                // to generate one on its behalf, as it would
                                // need access to 'Base's copy constructor
        }

