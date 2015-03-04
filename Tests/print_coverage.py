import sys

lines_count=0
matched_lines=0

reportfile = sys.argv[1]
with open(reportfile, 'r') as f:
    for l in f.readlines():
        if "Coverage=" in l:
            continue
        lines_count += 1
        if not l.endswith (' 0\n'):
            matched_lines += 1
print "Coverage %s%%" % (matched_lines * 100 / lines_count)
