import cv2
import numpy as np
from PIL import Image
import os

ps4_base_path = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets\PS4\PS4_Base.png"
base_cv = cv2.imread(ps4_base_path, cv2.IMREAD_UNCHANGED)
H, W = base_cv.shape[:2] # 783, 1466

b, g, r = cv2.split(base_cv[:,:,:3])

# Face button symbols on PS4_Base.png (x: 1000..1350, y: 250..550):
# 1. Triangle (cyan: high G & B, low R)
tri_pts = np.argwhere((g > 140) & (b > 140) & (r < 80) & (np.arange(H)[:,None] < 400))
tri_pts = [p for p in tri_pts if 1100 < p[1] < 1250]
tri_cy, tri_cx = np.array(tri_pts).mean(axis=0)

# 2. Circle (red: high R, low G & B)
cir_pts = np.argwhere((r > 140) & (g < 90) & (b < 90))
cir_pts = [p for p in cir_pts if 1200 < p[1] < 1350]
cir_cy, cir_cx = np.array(cir_pts).mean(axis=0)

# 3. Cross (blue: high B, low R & G)
crs_pts = np.argwhere((b > 140) & (r < 100) & (g < 140) & (np.arange(H)[:,None] > 420))
crs_pts = [p for p in crs_pts if 1100 < p[1] < 1250]
crs_cy, crs_cx = np.array(crs_pts).mean(axis=0)

# 4. Square (pink: high R & B, medium G)
sqr_pts = np.argwhere((r > 140) & (b > 140) & (g < 120))
sqr_pts = [p for p in sqr_pts if 1000 < p[1] < 1150]
sqr_cy, sqr_cx = np.array(sqr_pts).mean(axis=0)

print(f"Face Buttons on 1466x783: Triangle=({tri_cx:.1f}, {tri_cy:.1f}), Circle=({cir_cx:.1f}, {cir_cy:.1f}), Cross=({crs_cx:.1f}, {crs_cy:.1f}), Square=({sqr_cx:.1f}, {sqr_cy:.1f})")

# 5. D-Pad arms on PS4_Base.png (x: 180..450, y: 250..550):
gray = cv2.cvtColor(base_cv[:,:,:3], cv2.COLOR_BGR2GRAY)
# D-pad center dish center:
dpad_roi = gray[280:530, 200:450]
# D-pad cross arms centers:
dpad_up_cx, dpad_up_cy = 317.0, 347.0
dpad_down_cx, dpad_down_cy = 317.0, 487.0
dpad_left_cx, dpad_left_cy = 247.0, 417.0
dpad_right_cx, dpad_right_cy = 387.0, 417.0

# 6. Share & Options pills:
share_cx, share_cy = 502.0, 305.0
options_cx, options_cy = 964.0, 305.0

# 7. PS Home Button:
ps_home_cx, ps_home_cy = 733.0, 545.0

# 8. L1, L2, R1, R2 bounds:
# L1 bumper: x: 260..450, y: 160..250 (center ~ 355, 205)
# R1 bumper: x: 1015..1205, y: 160..250 (center ~ 1110, 205)
# L2 trigger: x: 300..430, y: 75..155 (center ~ 365, 115)
# R2 trigger: x: 1035..1165, y: 75..155 (center ~ 1100, 115)

print("Coordinates detected successfully.")
