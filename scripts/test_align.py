import cv2
import numpy as np
from PIL import Image
import os

base_dir = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\srcimg\Controller Asset Pack\Xbox Wireless Controller Images\Default Theme"
template_dir = os.path.join(base_dir, r"Template\Xbox Series X Controller\Black")
press_dir = os.path.join(base_dir, r"Theme Assets\Xbox Series X Active Presses")

overlay_path = os.path.join(template_dir, "Xbox Series X Controller Overlay.png")
base_path = os.path.join(template_dir, "XBSeries_base.png")

overlay = Image.open(overlay_path)
base = Image.open(base_path)
print(f"Overlay size: {overlay.size}, Base size: {base.size}")

# Let's inspect the active press files in press_dir
for f in sorted(os.listdir(press_dir)):
    if f.endswith(".png"):
        p = os.path.join(press_dir, f)
        img = Image.open(p)
        print(f"{f:32s}: {img.size}")
