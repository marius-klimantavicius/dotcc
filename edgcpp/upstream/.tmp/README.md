# Mono Repository TMP

This directory is not intended for use as general temporary storage.  Its
purpose is to faciliate bridging temporary files from the host to docker (e.g.,
a file can be generated on the host, stored here, and then referenced from a
command executed in docker).

This allows for the host to pass non-trivial empherial information to Docker
for further processing (that if packaged entirely as command line arguments
would exceed the maximum command line argument limit -- primarily a problem
on Windows).
