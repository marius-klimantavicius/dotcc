//options_all:-r -x -tused
//options: --strict;cn

void green () { }
enum color { blue, green, aqua };	/* ERROR */

void
test ()
{
	void blue ();
	enum color { red, blue };	/* ERROR */
}


