import cv2
import numpy as np
from PIL import Image
import os

xb360_dir = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\srcimg\Controller Asset Pack\Xbox 360 Controller Images"
base_path = os.path.join(xb360_dir, r"Default Theme\Templates\Black\XB360_base_black.png")
press_dir = os.path.join(xb360_dir, r"Default Theme\Theme SVG\Theme Assets\Active Presses")

base = Image.open(base_path).convert("RGBA")
W, H = base.size
print(f"XB360 Base size: {W}x{H}")

# Let's inspect where features are on XB360_base_black.png
# Let's detect button centers using OpenCV
base_cv = cv2.imread(base_path, cv2.IMREAD_UNCHANGED)
gray = cv2.cvtColor(base_cv[:,:,:3], cv2.COLOR_BGR2GRAY)

# Let's find:
# 1. Left Stick (x: 200..450, y: 250..500)
# 2. Right Stick (x: 800..1100, y: 450..700)
# 3. D-Pad center (x: 450..700, y: 450..700)
# 4. ABXY Diamond (x: 1000..1350, y: 250..550)
# 5. Guide (x: 650..850, y: 300..500)
# 6. Back (x: 550..700, y: 320..460)
# 7. Start (x: 800..950, y: 320..460)
# 8. Left Bumper / Trigger, Right Bumper / Trigger

# Let's test template matching for each active press button!
for f in sorted(os.listdir(press_dir)):
    if f.endswith('.png') and os.path.isfile(os.path.join(press_dir, f)):
        p = os.path.join(press_dir, f)
        btn = cv2.imread(p, cv2.IMREAD_UNCHANGED)
        print(f"{f:32s}: {btn.shape}")
