#! /bin/bash

TAG=$1
git log $TAG..  | grep '^Author' | cut -d' ' -f 2- | sort | uniq
