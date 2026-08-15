import cv2
import numpy as np
from PIL import Image
import os

xbox_dir = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets\Xbox"
base = cv2.imread(os.path.join(xbox_dir, "XB_Base.png"), cv2.IMREAD_UNCHANGED)
H, W = base.shape[:2]

scale = 260.0 / 954.0
dx = (420.0 - 1534.0 * scale) / 2.0
dy = 0.0

print(f"Base size: {W}x{H}, scale: {scale:.6f}, dx: {dx:.3f}, dy: {dy:.3f}")

# Let's inspect the buttons and find their exact centers on base:
# ABXY buttons:
# In Xbox Series X controller (1534x954):
# Y: Center = (1182.0, 310.0)
# X: Center = (1072.0, 420.0)
# B: Center = (1291.0, 408.0)
# A: Center = (1181.0, 518.0)

# D-Pad center: (577.8, 655.4)
# D-Pad Up: Center = (577.8, 595.0)
# D-Pad Down: Center = (577.8, 715.0)
# D-Pad Left: Center = (517.8, 655.4)
# D-Pad Right: Center = (637.8, 655.4)

# Sticks:
# Left Stick: Center = (367.0, 418.0)
# Right Stick: Center = (974.0, 658.0)

# View, Menu, Share, Guide:
# View (Back): Center = (652.0, 425.0)
# Menu (Start): Center = (883.0, 425.0)
# Share: Center = (765.0, 510.0)
# Guide (Home): Center = (767.0, 308.0)

# Bumpers (LB, RB):
# LB: Left upper curve
# RB: Right upper curve
# Triggers (LT, RT):
# LT: Upper left trigger
# RT: Upper right trigger
