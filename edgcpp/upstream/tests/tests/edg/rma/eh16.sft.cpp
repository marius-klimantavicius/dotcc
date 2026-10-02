//options_all:-r -x -tused
//options: --strict;cn

int main(void)
{
	try {
	}
	catch ( ... ) {}	//should issue a diagnostic
	catch (char) {}
}


