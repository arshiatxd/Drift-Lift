import cv2
import numpy as np
from PIL import Image
import os

xb_black_overlay = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\srcimg\Controller Asset Pack\Xbox 360 Controller Images\Default Theme\Templates\Black\Xbox 360 Controller Overlay - Black.png"
xb_base = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets\Xbox360\XB360_Base.png"

img_overlay = Image.open(xb_black_overlay).convert("RGBA")
img_base = Image.open(xb_base).convert("RGBA")

print(f"Overlay: {img_overlay.size}")
print(f"Base: {img_base.size}")

# Let's inspect the active press images on 1545x955:
# Why are LB, LT, RB, RT, and analog sticks positioned where they are?
# Let's find where the bumpers and triggers are in img_overlay!
ov_cv = cv2.imread(xb_black_overlay, cv2.IMREAD_UNCHANGED)

# Let's find:
# 1. Left stick center on Base:
# 2. Right stick center on Base:
# 3. D-Pad center on Base:
# 4. LB & RB bounds on Base:
# 5. LT & RT bounds on Base:

# Left Stick Well:
# Let's find non-zero/outline pixels in region x: 200..600, y: 300..700
# Let's compute center of mass of the well circle:
well_l = ov_cv[350:600, 200:450]
# Right Stick Well:
well_r = ov_cv[550:850, 800:1100]

print("Overlay loaded successfully")
