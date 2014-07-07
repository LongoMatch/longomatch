#!/usr/bin/env python
import sys
import os
import re

TEMPLATE = '''
ASSEMBLY = %s
TARGET = %s

LINK = $(REF_DEP_LONGOMATCH_ADDINS)

SOURCES = %s

RESOURCES = %s

include $(top_srcdir)/build/build.mk
'''

def mdptoam(mdp):
    with open (mdp) as f:
        l = f.read()
        try:
            assembly = re.findall('assembly="([^"]*)"', l)[0]
        except:
            assembly = ""
        try:
            target = re.findall('target="([^"]*)"', l)[0].lower()
        except:
            assembly = "library"
        try:
            files = re.findall('subtype="Code" buildaction="Compile" name="([^"]*)"', l)
        except:
            files = []
        try:
            resources = re.findall('subtype="Code" buildaction="EmbedAsResource" name="([^"]*)"', l)
            resources = [x.replace("../", "$(top_srcdir)/") for x in resources]
        except:
            resources = []
    files.sort()
    am = os.path.join(mdp.rsplit('/', 1)[0], 'Makefile.am')
    if not os.path.exists(am):
        with open (am, "w+") as f:
            f.write (TEMPLATE % (assembly, target,
                " \\\n\t".join(files),
                " \\\n\t".join(resources)))
    else:
        f = open(am)
        fr = f.readlines()
        f.close()
        insources = False
        inresources = False
        with open (am, "w+") as f:
            for l in fr:
                if l.startswith("SOURCES"):
                    insources = True
                if l.startswith("RESOURCES"):
                    inresources = True
                if insources and l == '\n':
                    insources = False
                    f.write ("SOURCES = %s" % (" \\\n\t".join(files)))
                    f.write ("\n")
                if inresources and l == '\n':
                    inresources = False
                    f.write ("RESOURCES = %s" % (" \\\n\t".join(resources)))
                    f.write ("\n")
                if not insources and not inresources:
                    f.write(l)

def main():
    mdps = []
    p = sys.argv[1]
    for d in os.listdir(p):
        if not os.path.isdir(d):
            continue
        for f in os.listdir(os.path.join(p, d)):
            if not f.endswith(".mdp"):
                continue
            mdps.append(os.path.join(d, f))
    for mdp in mdps:
        print "Updating " + mdp
        mdptoam(mdp)

if __name__ == "__main__":
    main()
