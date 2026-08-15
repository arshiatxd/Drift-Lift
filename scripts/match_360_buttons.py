import cv2
import numpy as np
from PIL import Image
import os

xb360_dir = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\srcimg\Controller Asset Pack\Xbox 360 Controller Images"
base_path = os.path.join(xb360_dir, r"Default Theme\Templates\Black\XB360_base_black.png")
press_dir = os.path.join(xb360_dir, r"Default Theme\Theme SVG\Theme Assets\Active Presses")

base = Image.open(base_path).convert("RGBA")
W, H = base.size

# Let's inspect coordinates of buttons on XB360_base_black.png (1545x955):
# Left stick: center ~ (355, 385), diameter ~ 190
# Right stick: center ~ (955, 595), diameter ~ 190
# D-pad center: ~ (555, 595), disc diameter ~ 200
# Guide: center ~ (755, 385), diameter ~ 140
# Back (arrow left): center ~ (635, 385)
# Start (arrow right): center ~ (875, 385)
# ABXY diamond center: ~ (1155, 385)
#   Y: center ~ (1155, 295)
#   X: center ~ (1065, 385)
#   B: center ~ (1245, 385)
#   A: center ~ (1155, 475)
# LB: Left upper bumper curve: x: 260..580, y: 155..300
# RB: Right upper bumper curve: x: 930..1220, y: 155..300
# LT: Left trigger top: x: 300..450, y: 0..160
# RT: Right trigger top: x: 1060..1210, y: 0..160

# Let's test placing each active press button on base
test_img = base.copy()

# Let's find exact coordinates by template matching or edge alignment
base_cv = cv2.imread(base_path, cv2.IMREAD_UNCHANGED)
base_gray = cv2.cvtColor(base_cv[:,:,:3], cv2.COLOR_BGR2GRAY)

positions_360 = {}
for f in os.listdir(press_dir):
    if f.endswith('.png'):
        p = os.path.join(press_dir, f)
        btn_cv = cv2.imread(p, cv2.IMREAD_UNCHANGED)
        alpha = btn_cv[:,:,3]
        # Match template
        res = cv2.matchTemplate(base_gray, cv2.cvtColor(btn_cv[:,:,:3], cv2.COLOR_BGR2GRAY), cv2.TM_CCOEFF_NORMED)
        min_val, max_val, min_loc, max_loc = cv2.minMaxLoc(res)
        print(f"{f:32s}: max_val={max_val:.3f}, loc={max_loc}")
        positions_360[f] = max_loc
