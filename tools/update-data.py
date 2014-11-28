#!/usr/bin/env python
import sys
import os
import subprocess

TEMPLATE='''\
%(dir)sdir = @datadir@/@PACKAGE@/%(dir)s
nobase_dist_%(dir)s_DATA = %(files)s
'''



def main():
    home = sys.argv[1]
    for d in ['icons', 'theme', 'images']:
        di = os.path.join(home, 'data', d)
        files =  subprocess.check_output(["git", "ls-files", di])
        files = [x.replace(os.path.join('data', d) + '/', '') for x in files.split('\n')[:-1]]
        files = [x for x in files if not os.path.isdir(os.path.join(di, x))]
        files = " \\\n\t".join(files)
        am = os.path.join(di, 'Makefile.am')
        with open (am, "w+") as f:
            f.write (TEMPLATE % {'dir':d, 'files':files})

if __name__ == "__main__":
    main()

