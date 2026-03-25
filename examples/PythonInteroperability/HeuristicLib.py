import os
import sys
from pythonnet import load

load("coreclr")
import clr  # noqa: E402

_here = os.path.dirname(os.path.abspath(__file__))
dll_dir = os.path.join(_here, "csProject", "bin", "Release", "net10.0", "publish")

if dll_dir not in sys.path:
    sys.path.append(dll_dir)

clr.AddReference("HEAL.HeuristicLib.Core")