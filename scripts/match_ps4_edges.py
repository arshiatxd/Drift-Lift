import cv2
import numpy as np
from PIL import Image
import os

ps4_base_path = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets\PS4\PS4_Base.png"
ps4_dir = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets\PS4"

base = cv2.imread(ps4_base_path, cv2.IMREAD_UNCHANGED)
H, W = base.shape[:2] # 783, 1466
print(f"PS4 Base: {W}x{H}")

# Let's inspect features in 1466x783 space
# 1. Triangle, Circle, Cross, Square centers:
# Let's find colored/tinted symbols in right face region (x: 1000..1350, y: 250..550)
# Triangle (cyan symbol at x ~ 1150, y ~ 340)
# Circle (red symbol at x ~ 1240, y ~ 420)
# Cross (blue symbol at x ~ 1150, y ~ 500)
# Square (pink symbol at x ~ 1060, y ~ 420)

# 2. D-pad cross arms:
# Up (x ~ 315, y ~ 340)
# Down (x ~ 315, y ~ 500)
# Left (x ~ 235, y ~ 420)
# Right (x ~ 395, y ~ 420)

# 3. Share & Options:
# Share (x ~ 475, y ~ 300)
# Options (x ~ 990, y ~ 300)

# 4. Home (PS):
# PS Logo (x ~ 733, y ~ 585)

# 5. L1, L2, R1, R2 bounds:
# L1 bumper (x ~ 270..470, y ~ 160..260)
# L2 trigger (x ~ 280..420, y ~ 80..160)
# R1 bumper (x ~ 1000..1200, y ~ 160..260)
# R2 trigger (x ~ 1050..1190, y ~ 80..160)

# 6. Left Stick & Right Stick:
# Left stick well (x ~ 500, y ~ 570)
# Right stick well (x ~ 965, y ~ 570)

# Let's run edge template match for each DS4 asset!
for f in sorted(os.listdir(ps4_dir)):
    if f.endswith('.png') and f != 'PS4_Base.png':
        p = os.path.join(ps4_dir, f)
        btn = cv2.imread(p, cv2.IMREAD_UNCHANGED)
        if btn is None: continue
        
        # Edge match
        btn_gray = cv2.cvtColor(btn[:,:,:3], cv2.COLOR_BGR2GRAY)
        base_gray = cv2.cvtColor(base[:,:,:3], cv2.COLOR_BGR2GRAY)
        
        btn_edge = cv2.Canny(btn_gray, 50, 150)
        base_edge = cv2.Canny(base_gray, 50, 150)
        
        res = cv2.matchTemplate(base_edge, btn_edge, cv2.TM_CCOEFF)
        min_v, max_v, min_loc, max_loc = cv2.minMaxLoc(res)
        print(f"{f:32s}: max_loc={max_loc}, size=({btn.shape[1]}, {btn.shape[0]})")
