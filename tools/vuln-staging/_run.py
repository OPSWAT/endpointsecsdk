#!/usr/bin/env python3
###############################################################################################
##  Vuln Staging — _run.py
##
##  Runs another script under the portable (embeddable) Python that run-stage.cmd uses.
##  That Python ignores the script's own folder when importing (its python312._pth fixes
##  sys.path), so scripts that import siblings -- sdk-downloader's main.py, plan.py -- fail
##  when started directly. This puts the script's folder first on sys.path, then runs it as
##  __main__ with the remaining arguments.
##
##  Usage:  python _run.py <script.py> [args...]
##
##  Created by Chris Seiler — OPSWAT OEM Field CTO
###############################################################################################

import os
import runpy
import sys

if len(sys.argv) < 2:
    print("Usage: python _run.py <script.py> [args...]")
    sys.exit(2)

script = os.path.abspath(sys.argv[1])
sys.path.insert(0, os.path.dirname(script))
sys.argv = [script] + sys.argv[2:]
runpy.run_path(script, run_name="__main__")
